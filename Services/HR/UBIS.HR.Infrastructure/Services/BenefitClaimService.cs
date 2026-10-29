using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories;
using UBIS.HR.Infrastructure.Excel;

namespace UBIS.HR.Infrastructure.Services;

public class BenefitClaimService : IBenefitClaimService, IApprovalDocumentService
{
    private readonly IDocNumberService _docNumberService;
    private readonly IApprovalRouteResolverService _routeResolver;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITransactionService _tx;
    private readonly IBenefitClaimRepos _repos;
    private readonly IEmployeeRepos _employeeRepos;
    private readonly IPodAdminBranchRepos _podAdminBranchRepos;
    private readonly IEmployeeBenefitPlanRepos _employeeBenefitPlanRepos;

    private readonly ILogger<BenefitClaimService> _logger;
    private readonly ICurrentUserService _currentUser;

    public string DocType => "BenefitClaim";

    private const string DefaultFuelType = "Gasohol 91";

    public BenefitClaimService(IBenefitClaimRepos repos,
        IDocNumberService docNumberService,
        ITransactionService tx,
        IEmployeeRepos employeeRepos,
        IPodAdminBranchRepos podAdminBranchRepos,
        IEmployeeBenefitPlanRepos employeeBenefitPlanRepos,
        IApprovalRouteResolverService routeResolver,
        IServiceProvider serviceProvider,
        ICurrentUserService currentUser,
        ILogger<BenefitClaimService> logger)
    {
        _repos = repos;
        _docNumberService = docNumberService;
        _tx = tx;
        _employeeRepos = employeeRepos;
        _podAdminBranchRepos = podAdminBranchRepos;
        _employeeBenefitPlanRepos = employeeBenefitPlanRepos;
        _routeResolver = routeResolver;
        _serviceProvider = serviceProvider;
        _currentUser = currentUser;
        _logger = logger;
    }

    private IApprovalService ApprovalService => _serviceProvider.GetRequiredService<IApprovalService>();

    private static BenefitClaimDto MapToDto(TbBenefitClaim s)
    {
        return new BenefitClaimDto
        {
            Id = s.Id,
            DocNum = s.DocNum,
            DocStatus = s.DocStatus,
            DocDate = s.DocDate.Date,
            EmployeeId = s.EmployeeId,
            EmployeeNameTh = s.Employee != null ? $"{s.Employee.FnameTh} {s.Employee.LnameTh}" : null,
            PositionTh = s.Employee?.Position?.NameTh,
            PositionEn = s.Employee?.Position?.NameEn,
            PositionLevel = s.Employee?.PositionLevel != null ? $"L{s.Employee.PositionLevel.Level}" : null,
            PositionLevelNameTh = s.Employee?.PositionLevel?.NameTh,
            PositionLevelNameEn = s.Employee?.PositionLevel?.NameEn,
            Branch = $"{s.Employee?.Branch.NameTh} ({s.Employee?.Branch.Code})",
            Company = s.Employee?.Company.NameTh,
            CompanyCode = s.Employee?.Company.Code,
            Remark = s.Remark,
            TotalAmount = s.TotalAmount,
            LineCount = s.TbBenefitClaimLines.Count,
            DocFormId = s.DocFormId,
            DocFormCode = s.DocForm?.Code,
            DocFormNameTh = s.DocForm?.NameTh,
            DocFormLayout = s.DocForm?.Layout,
            BankAccountNo = s.BankAccountNo,
            PhoneNo = s.PhoneNo,
            TripPurpose = s.TripPurpose,
            Destination = s.Destination,
            TripType = s.TripType,
            DepartAt = s.DepartAt,
            ReturnAt = s.ReturnAt,
            IsOvernight = s.IsOvernight,
            Lines = s.TbBenefitClaimLines.Select(l => new BenefitClaimLineDto
            {
                Id = l.Id,
                BenefitId = l.BenefitId,
                BenefitNameTh = l.Benefit?.NameTh,
                Detail = l.Detail,
                LimitAmount = l.LimitAmount,
                Amount = l.Amount,
                Qty = l.Qty,
                AccountCode = l.AccountCode,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Route = l.Route,
                FuelPrice = l.FuelPrice,
                Rate = l.Rate,
                Region = l.Region,
                CalcType = l.Benefit?.CalcType,
                Breakdown = string.IsNullOrWhiteSpace(l.CalcBreakdown)
                    ? new()
                    : JsonSerializer.Deserialize<List<BenefitClaimBreakdownDto>>(l.CalcBreakdown) ?? new(),
            }).ToList(),
            CreatedBy = s.CreatedBy,
            CreatedAt = s.CreatedAt,
            UpdatedBy = s.UpdatedBy,
            UpdatedAt = s.UpdatedAt,
        };
    }

    // ===== ตรวจสิทธิ์ + คำนวณ =====

    private async Task<Dictionary<Guid, AvailableBenefitDto>> ValidateAsync(CreateBenefitClaimDto data)
    {
        if (data.DocFormId <= 0)
            throw new InvalidOperationException("กรุณาเลือกประเภทเอกสาร");
        if (data.Lines.Count == 0)
            throw new InvalidOperationException("กรุณาเพิ่มรายการเบิกอย่างน้อย 1 รายการ");
        if (data.Lines.Any(l => l.BenefitId == Guid.Empty))
            throw new InvalidOperationException("ทุกรายการต้องระบุสวัสดิการ");

        var available = (await _employeeBenefitPlanRepos
                .GetAvailableBenefitsAsync(data.EmployeeId, data.DocFormId))
            .ToDictionary(a => a.BenefitId);

        foreach (var l in data.Lines)
        {
            if (!available.TryGetValue(l.BenefitId, out var b))
                throw new InvalidOperationException(
                    "มีสวัสดิการที่ไม่อยู่ในสิทธิ์ของพนักงาน หรือไม่ใช่ของแบบฟอร์มนี้");
            if (b.CalcType == "Actual" && l.Amount <= 0)
                throw new InvalidOperationException($"ยอดเบิกของ '{b.NameTh}' ต้องมากกว่า 0");
        }
        return available;
    }

    private record LineCalc(decimal Amount, decimal Qty, decimal? Rate, string? Region,
        List<BenefitClaimBreakdownDto>? Breakdown = null);

    private TbBenefitClaimLine NewLine(CreateBenefitClaimLineDto l, decimal limitAmount, LineCalc c)
    {
        var email = _currentUser.GetCurrentUserEmail();
        var now = DateTime.Now;
        return new TbBenefitClaimLine
        {
            BenefitId = l.BenefitId,
            Detail = l.Detail,
            LimitAmount = limitAmount,
            Amount = c.Amount,
            Qty = c.Qty,
            Rate = c.Rate,
            Region = c.Region,
            CalcBreakdown = c.Breakdown == null ? null : JsonSerializer.Serialize(c.Breakdown),
            AccountCode = l.AccountCode,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            Route = l.Route,
            FuelPrice = l.FuelPrice,
            CreatedAt = now,
            CreatedBy = email,
            UpdatedAt = now,
            UpdatedBy = email,
            IsDelete = false,
        };
    }

    private async Task<(List<TbBenefitClaimLine> Lines, decimal Total)> BuildLinesAsync(
        CreateBenefitClaimDto data, Dictionary<Guid, AvailableBenefitDto> available, int level)
    {
        var lines = new List<TbBenefitClaimLine>();
        foreach (var l in data.Lines)
        {
            var b = available[l.BenefitId];
            var calc = await CalculateAsync(l, b, level, data.DocDate);
            lines.Add(NewLine(l, b.LimitAmount, calc));
        }
        return (lines, lines.Sum(x => x.Amount));
    }

    private async Task<LineCalc> CalculateAsync(
        CreateBenefitClaimLineDto l, AvailableBenefitDto b, int level, DateTime docDate)
    {
        switch (b.CalcType)
        {
            case "Fuel":
                {
                    if (l.FuelPrice is null or <= 0)
                        throw new InvalidOperationException($"'{b.NameTh}': กรุณาระบุราคาน้ำมัน");
                    if (l.Qty <= 0)
                        throw new InvalidOperationException($"'{b.NameTh}': กรุณาระบุระยะทาง (กม.)");

                    var rate = await _repos.GetFuelRateAsync(DefaultFuelType, l.FuelPrice.Value, docDate)
                        ?? throw new InvalidOperationException(
                            $"ไม่พบอัตราค่าน้ำมันที่ราคา {l.FuelPrice:N2} บาท/ลิตร");

                    var fuelAmount = Math.Round(l.Qty * rate.RatePerKm, 2);
                    return new LineCalc(fuelAmount, l.Qty, rate.RatePerKm, b.Region,
                        new() { new() { Qty = l.Qty, Rate = rate.RatePerKm, Amount = fuelAmount } });
                }

            case "PerDiem":
            case "Daily":
                {
                    var days = b.CalcType == "Daily"
                       ? CountDailyQty(l, b.NameTh)
                       : CountDays(l, b.NameTh, inclusive: true);
                    var bands = await _repos.GetPerDiemRatesAsync(
                        b.Region ?? throw new InvalidOperationException($"'{b.NameTh}' ยังไม่ได้ตั้งค่า Region"),
                        docDate);

                    decimal amount = 0;
                    var counted = 0;
                    var rows = new List<BenefitClaimBreakdownDto>();
                    foreach (var band in bands)
                    {
                        var to = band.DayTo ?? int.MaxValue;
                        var overlap = Math.Min(days, to) - band.DayFrom + 1;
                        if (overlap <= 0) continue;
                        var rowAmount = overlap * band.RatePerDay;
                        amount += rowAmount;
                        counted += overlap;
                        rows.Add(new BenefitClaimBreakdownDto
                        {
                            From = band.DayFrom,
                            To = band.DayTo,
                            Qty = overlap,
                            Rate = band.RatePerDay,
                            Amount = rowAmount
                        });
                    }
                    if (counted != days)
                        throw new InvalidOperationException($"ตั้งค่าอัตราเบี้ยเลี้ยงของ '{b.NameTh}' ไม่ครอบคลุม {days} วัน");

                    return new LineCalc(amount, days, Math.Round(amount / days, 2), b.Region, rows);
                }

            case "Accommodation":
                {
                    var nights = CountDays(l, b.NameTh, inclusive: false);
                    var rate = await _repos.GetAccommodationRateAsync(level, docDate)
                        ?? throw new InvalidOperationException($"ไม่พบอัตราค่าที่พักสำหรับระดับ L{level}");

                    var perNight = b.Region == "International" ? rate.InternationalRate : rate.DomesticRate;
                    var accAmount = nights * perNight;
                    return new LineCalc(accAmount, nights, perNight, b.Region,
                        new() { new() { Qty = nights, Rate = perNight, Amount = accAmount } });
                }

            default: // Actual
                return new LineCalc(l.Amount, l.Qty, null, b.Region);
        }
    }

    private static int CountDays(CreateBenefitClaimLineDto l, string name, bool inclusive)
    {
        if (l.StartDate is null || l.EndDate is null)
            throw new InvalidOperationException($"'{name}': กรุณาระบุวันที่เริ่มต้นและสิ้นสุด");
        var diff = (l.EndDate.Value.Date - l.StartDate.Value.Date).Days;
        var n = inclusive ? diff + 1 : diff;
        if (n <= 0)
            throw new InvalidOperationException($"'{name}': ช่วงวันที่ไม่ถูกต้อง");
        return n;
    }
    private static int CountDailyQty(CreateBenefitClaimLineDto l, string name)
    {
        if (l.Qty <= 0 || l.Qty != Math.Floor(l.Qty))
            throw new InvalidOperationException($"'{name}': กรุณาระบุจำนวนวันเป็นจำนวนเต็ม");
        return (int)l.Qty;
    }
    private static void ApplyHeader(TbBenefitClaim e, CreateBenefitClaimDto d,
        (int Level, string? BankAccountNo, string? PhoneNo) snap, decimal total)
    {
        e.DocDate = d.DocDate.Date;
        e.DocFormId = d.DocFormId;
        e.EmployeeId = d.EmployeeId;
        e.PositionLevel = snap.Level;
        e.BankAccountNo = snap.BankAccountNo;
        e.PhoneNo = snap.PhoneNo;
        e.TripPurpose = d.TripPurpose;
        e.Destination = d.Destination;
        e.TripType = d.TripType;
        e.DepartAt = d.DepartAt;
        e.ReturnAt = d.ReturnAt;
        e.IsOvernight = d.IsOvernight;
        e.Remark = d.Remark;
        e.TotalAmount = total;
    }

    private async Task<(int Level, string? BankAccountNo, string? PhoneNo)> GetSnapshotAsync(Guid employeeId)
    {
        var snap = await _employeeRepos.GetClaimSnapshotAsync(employeeId);
        if (snap == null) throw new InvalidOperationException("ไม่พบข้อมูลพนักงาน");
        return snap.Value;
    }

    // ===== Query =====

    public async Task<PagedResultDto<BenefitClaimDto>> GetAllAsync(BenefitClaimFilterDto filter)
    {
        var employeeCode = _currentUser.GetCurrentUserEmployeeCode();
        var currentEmployeeId = await _employeeRepos.GetIdByEmpIdAsync(employeeCode) ?? Guid.Empty;

        var adminBranches = await _podAdminBranchRepos.GetByUserIdWithBranchAsync(_currentUser.GetCurrentUserId());
        var adminBranchIds = adminBranches.Select(b => b.BranchId).ToList();

        filter.currentEmployeeId = currentEmployeeId;
        filter.currentUserEmail = _currentUser.GetCurrentUserEmail();

        var (items, totalCount) = await _repos.GetFilteredPagedAsync(filter, adminBranchIds);
        return new PagedResultDto<BenefitClaimDto> { Items = items.ToList(), TotalCount = totalCount };
    }

    public async Task<BenefitClaimDto?> GetByIdAsync(Guid id)
    {
        var s = await _repos.GetDetailByIdAsync(id);
        return s == null ? null : await WithAffiliationAsync(MapToDto(s));
    }

    public async Task<BenefitClaimDto?> GetByDocNumAsync(string docNum)
    {
        var s = await _repos.GetByDocNumAsync(docNum);
        return s == null ? null : await WithAffiliationAsync(MapToDto(s));
    }

    public async Task<BenefitClaimSummaryDto> GetSummaryAsync(BenefitClaimSummaryFilterDto filter)
    {
        var adminBranches = await _podAdminBranchRepos.GetByUserIdWithBranchAsync(_currentUser.GetCurrentUserId());
        var adminBranchIds = adminBranches.Select(b => b.BranchId).ToList();

        if (adminBranchIds.Count == 0)
            throw new UnauthorizedAccessException("เฉพาะ HR Admin เท่านั้นที่ดูรายงานสรุปได้");

        return await _repos.GetSummaryAsync(filter, adminBranchIds);
    }

    public async Task<List<BenefitClaimExportRowDto>> GetDetailRowsAsync(BenefitClaimSummaryFilterDto filter)
    {
        var adminBranchIds = await GetAdminBranchIdsOrThrowAsync();
        return await _repos.GetExportRowsAsync(filter, adminBranchIds);
    }

    public async Task<PagedResultDto<EmployeeSummaryDto>> GetEmployeeSummaryPagedAsync(
        BenefitClaimEmployeeFilterDto filter)
    {
        var adminBranchIds = await GetAdminBranchIdsOrThrowAsync();
        var (items, total) = await _repos.GetEmployeeSummaryPagedAsync(filter, adminBranchIds);
        return new PagedResultDto<EmployeeSummaryDto> { Items = items, TotalCount = total };
    }

    public async Task<List<BenefitClaimExportRowDto>> GetEmployeeRowsAsync(
        BenefitClaimEmployeeRowsFilterDto filter)
    {
        var adminBranchIds = await GetAdminBranchIdsOrThrowAsync();
        var rows = await _repos.GetExportRowsAsync(filter, adminBranchIds, filter.EmployeeId);
        return rows.OrderBy(x => x.DocDate).ThenBy(x => x.DocNum).ToList();
    }

    private async Task<List<Guid>> GetAdminBranchIdsOrThrowAsync()
    {
        var adminBranches = await _podAdminBranchRepos.GetByUserIdWithBranchAsync(_currentUser.GetCurrentUserId());
        var ids = adminBranches.Select(b => b.BranchId).ToList();
        if (ids.Count == 0)
            throw new UnauthorizedAccessException("เฉพาะ HR Admin เท่านั้นที่ดูรายงานสรุปได้");
        return ids;
    }

    public async Task<byte[]> ExportSummaryAsync(BenefitClaimSummaryFilterDto filter)
    {
        var adminBranchIds = await GetAdminBranchIdsOrThrowAsync();
        var summary = await _repos.GetSummaryAsync(filter, adminBranchIds);
        var rows = await _repos.GetExportRowsAsync(filter, adminBranchIds);
        return BenefitClaimSummaryExcelBuilder.Build(summary, rows, filter);
    }

    private async Task<BenefitClaimDto> WithAffiliationAsync(BenefitClaimDto dto)
    {
        var aff = await _employeeRepos.GetAffiliationAsync(dto.EmployeeId);
        if (aff != null)
        {
            dto.Affiliation = aff.Value.Name;
            dto.AffiliationCode = aff.Value.Code;
        }
        return dto;
    }

    public Task<List<DocFormDto>> GetDocFormsAsync()
        => _repos.GetDocFormsAsync(DocType);

    public Task<List<AvailableBenefitDto>> GetAvailableBenefitsAsync(Guid employeeId, int docFormId)
        => _employeeBenefitPlanRepos.GetAvailableBenefitsAsync(employeeId, docFormId);

    public async Task<CalculateLineResultDto> CalculateLineAsync(CalculateLineRequestDto req)
    {
        if (req.Line.BenefitId == Guid.Empty)
            throw new InvalidOperationException("กรุณาเลือกสวัสดิการ");

        var available = await _employeeBenefitPlanRepos
            .GetAvailableBenefitsAsync(req.EmployeeId, req.DocFormId);
        var b = available.FirstOrDefault(x => x.BenefitId == req.Line.BenefitId)
            ?? throw new InvalidOperationException(
                "สวัสดิการนี้ไม่อยู่ในสิทธิ์ของพนักงาน หรือไม่ใช่ของแบบฟอร์มนี้");

        // ระดับตำแหน่งใช้เฉพาะค่าที่พัก ไม่ต้อง query ทุกครั้ง
        var level = b.CalcType == "Accommodation"
            ? (await GetSnapshotAsync(req.EmployeeId)).Level
            : 0;

        var line = new CreateBenefitClaimLineDto
        {
            BenefitId = req.Line.BenefitId,
            Detail = "",
            Amount = req.Line.Amount,
            Qty = req.Line.Qty,
            StartDate = req.Line.StartDate,
            EndDate = req.Line.EndDate,
            FuelPrice = req.Line.FuelPrice,
        };
        var calc = await CalculateAsync(line, b, level, req.DocDate);

        return new CalculateLineResultDto
        {
            Amount = calc.Amount,
            Qty = calc.Qty,
            Rate = calc.Rate,
            Region = calc.Region,
            Breakdown = calc.Breakdown ?? new(),
        };
    }
    // ===== Command =====

    public async Task<BenefitClaimDto> CreateAsync(CreateBenefitClaimDto data)
    {
        try
        {
            var available = await ValidateAsync(data);
            var snap = await GetSnapshotAsync(data.EmployeeId);
            var (lines, total) = await BuildLinesAsync(data, available, snap.Level);

            var email = _currentUser.GetCurrentUserEmail();
            var now = DateTime.Now;

            var entity = new TbBenefitClaim
            {
                DocNum = await _docNumberService.GenerateAsync(DocType),
                DocStatus = "Draft",
                CreatedAt = now,
                CreatedBy = email,
                UpdatedAt = now,
                UpdatedBy = email,
                IsDelete = false,
                TbBenefitClaimLines = lines,
            };
            ApplyHeader(entity, data, snap, total);

            await _repos.AddAsync(entity);
            await _repos.SaveChangesAsync();

            return (await GetByIdAsync(entity.Id))!;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มใบเบิกสวัสดิการ: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<BenefitClaimDto?> UpdateAsync(Guid id, CreateBenefitClaimDto data)
    {
        try
        {
            var existing = await _repos.GetByIdAsync(id);
            if (existing == null) return null;

            if (existing.DocStatus != "Draft")
                throw new InvalidOperationException("แก้ไขได้เฉพาะเอกสารสถานะ Draft เท่านั้น กรุณาตรวจสอบ");

            var available = await ValidateAsync(data);
            var snap = await GetSnapshotAsync(data.EmployeeId);
            var (lines, total) = await BuildLinesAsync(data, available, snap.Level);

            ApplyHeader(existing, data, snap, total);
            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedBy = _currentUser.GetCurrentUserEmail();

            await _repos.DeleteLinesAsync(id);

            foreach (var line in lines)
                existing.TbBenefitClaimLines.Add(line);

            await _repos.SaveChangesAsync();
            return await GetByIdAsync(id);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการแก้ไขใบเบิกสวัสดิการ: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var existing = await _repos.GetByIdAsync(id);
            if (existing == null) return false;

            if (existing.DocStatus != "Draft")
                throw new InvalidOperationException("ลบได้เฉพาะเอกสารสถานะ Draft เท่านั้น กรุณาตรวจสอบ");

            existing.IsDelete = true;
            existing.DeletedBy = _currentUser.GetCurrentUserEmail();
            existing.DeletedAt = DateTime.Now;

            await _repos.SaveChangesAsync();
            return true;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการลบใบเบิกสวัสดิการ: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<BenefitClaimDto?> SubmitAsync(Guid id)
    {
        try
        {
            var existing = await _repos.GetByIdAsync(id);
            if (existing == null) return null;

            if (existing.DocStatus != "Draft")
                throw new InvalidOperationException("ส่งอนุมัติได้เฉพาะเอกสารสถานะ Draft เท่านั้น");

            var resolvedApprovers = await _routeResolver.ResolveAsync(DocType, existing.EmployeeId);

            return await _tx.RunAsync<BenefitClaimDto?>(async () =>
            {
                existing.DocStatus = "WaitApprove";
                existing.UpdatedAt = DateTime.Now;
                existing.UpdatedBy = _currentUser.GetCurrentUserEmail();
                await _repos.SaveChangesAsync();

                await ApprovalService.CreateApprovalAsync(
                    DocType,
                    existing.DocNum,
                    1,
                    resolvedApprovers.OrderBy(x => x.StepNo).ToList());

                return await GetByIdAsync(id);
            });
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการส่งอนุมัติใบเบิกสวัสดิการ: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<BenefitClaimDto?> RecallAsync(Guid id)
    {
        try
        {
            var existing = await _repos.GetByIdAsync(id);
            if (existing == null) return null;

            if (existing.DocStatus != "WaitApprove" && existing.DocStatus != "Rejected")
                throw new InvalidOperationException("ดึงกลับได้เฉพาะเอกสารสถานะ รออนุมัติ หรือ ตีกลับ เท่านั้น");

            return await _tx.RunAsync<BenefitClaimDto?>(async () =>
            {
                existing.DocStatus = "Draft";
                existing.UpdatedAt = DateTime.Now;
                existing.UpdatedBy = _currentUser.GetCurrentUserEmail();
                await _repos.SaveChangesAsync();

                await ApprovalService.RecallAsync(DocType, existing.DocNum, 1);

                return await GetByIdAsync(id);
            });
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงใบเบิกสวัสดิการกลับ: {Message}", ex.Message);
            throw;
        }
    }

    // ===== IApprovalDocumentService =====

    public async Task<(DateTime DocDate, string EmployeeNameTh, string? DocumentDetail)?> GetSummaryAsync(string docNumber)
    {
        var entity = await _repos.GetByDocNumAsync(docNumber);
        if (entity == null) return null;

        var name = entity.Employee != null ? $"{entity.Employee.FnameTh} {entity.Employee.LnameTh}" : "-";
        return (entity.DocDate, name, $"เอกสารเบิกสวัสดิการ จำนวนเงินรวม {entity.TotalAmount} บาท");
    }

    public async Task<Dictionary<string, (DateTime DocDate, string EmployeeNameTh, string? DocumentDetail)>> GetSummariesAsync(
        IEnumerable<string> docNumbers, MyApprovalFilterDto? filter = null)
    {
        var entities = await _repos.GetSummariesByDocNumsAsync(docNumbers, filter);

        return entities.ToDictionary(
            e => e.DocNum,
            e => (
                e.DocDate,
                e.Employee != null ? $"{e.Employee.FnameTh} {e.Employee.LnameTh}" : "-",
                (string?)$"เอกสารเบิกสวัสดิการ จำนวนเงินรวม {e.TotalAmount} บาท"
            ));
    }

    public async Task MarkApprovedAsync(string docNumber)
    {
        var entity = await _repos.GetByDocNumAsync(docNumber);
        if (entity == null) return;

        entity.DocStatus = "Approved";
        entity.UpdatedAt = DateTime.Now;
        entity.UpdatedBy = _currentUser.GetCurrentUserEmail();
        await _repos.SaveChangesAsync();
    }

    public async Task MarkDeniedAsync(string docNumber, bool isDisapprove)
    {
        var entity = await _repos.GetByDocNumAsync(docNumber);
        if (entity == null) return;

        entity.DocStatus = isDisapprove ? "Disapproved" : "Rejected";
        entity.UpdatedAt = DateTime.Now;
        entity.UpdatedBy = _currentUser.GetCurrentUserEmail();
        await _repos.SaveChangesAsync();
    }
}