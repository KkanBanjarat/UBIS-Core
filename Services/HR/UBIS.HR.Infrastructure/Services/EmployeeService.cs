using Microsoft.Extensions.Logging;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories;

namespace UBIS.HR.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepos _repos;
    private readonly IEmployeeBenefitPlanRepos _benefitPlanRepos;
    private readonly IPodAdminBranchRepos _podAdminBranchRepos;

    private readonly ILogger<EmployeeService> _logger;
    private readonly IOrganizationUnitService _serviceOrgUnit;
    private readonly ICurrentUserService _currentUser;

    public EmployeeService(IEmployeeRepos repos,
        IEmployeeBenefitPlanRepos benefitPlanRepos,
        IPodAdminBranchRepos podAdminBranchRepos,
        IOrganizationUnitService serviceOrgUnit,
        ICurrentUserService currentUser,
        ILogger<EmployeeService> logger)
    {
        _repos = repos;
        _benefitPlanRepos = benefitPlanRepos;
        _podAdminBranchRepos = podAdminBranchRepos;
        _logger = logger;
        _serviceOrgUnit = serviceOrgUnit;
        _currentUser = currentUser;
    }

    public async Task<PagedResultDto<EmployeeDto>> GetAllAsync(EmployeeFilterDto filter)
    {
        try
        {
            var (emps, totalCount) = await _repos.GetFilteredPagedAsync(filter);

            var items = emps.Select(s => new EmployeeDto
            {
                Id = s.Id,
                EmpId = s.EmpId,
                FNameTh = s.FnameTh,
                FNameEn = s.FnameEn,
                LNameTh = s.LnameTh,
                LNameEn = s.LnameEn,
                Email = s.Email,
                HireDate = s.HireDate,
                PositionId = s.PositionId,
                PositionNameTh = s.Position!.NameTh,
                PositionNameEn = s.Position!.NameEn,
                PositionLevelId = s.PositionLevelId,
                PositionLevel = s.PositionLevel.Level,
                PositionLevelNameTh = s.PositionLevel!.NameTh,
                PositionLevelNameEn = s.PositionLevel!.NameEn,
                CompanyId = s.CompanyId,
                CompanyNameTh = s.Company!.NameTh,
                CompanyNameEn = s.Company!.NameEn,
                CompanyCode = s.Company.Code,
                GroupId = s.GroupId,
                GroupNameTh = s.Group != null ? s.Group.NameTh : null,
                GroupNameEn = s.Group != null ? s.Group.NameEn : null,
                DepartmentId = s.DepartmentId,
                DepartmentNameTh = s.Department != null ? s.Department.NameTh : null,
                DepartmentNameEn = s.Department != null ? s.Department.NameEn : null,
                DivisionId = s.DivisionId,
                DivisionNameTh = s.Division != null ? s.Division.NameTh : null,
                DivisionNameEn = s.Division != null ? s.Division.NameEn : null,
                SectionId = s.SectionId,
                SectionNameTh = s.Section != null ? s.Section.NameTh : null,
                SectionNameEn = s.Section != null ? s.Section.NameEn : null,
                EmployeeTypeId = s.EmployeeTypeId,
                EmployeeTypeNameTh = s.EmployeeType!.NameTh,
                EmployeeTypeNameEn = s.EmployeeType!.NameEn,
                BranchId = s.BranchId,
                BranchNameEn = s.Branch!.NameEn,
                BranchNameTh = s.Branch!.NameTh,
                BranchCode = s.Branch!.Code,
                Status = s.Status,
                ReportToId = s.ReportToId,
                ReportToNameEn = s.ReportToId != null ? $"{s.ReportTo!.FnameEn} {s.ReportTo.LnameEn}" : null,
                ReportToNameTh = s.ReportToId != null ? $"{s.ReportTo!.FnameTh} {s.ReportTo.LnameTh}" : null,
                PrefixNameEn = s.PrefixNameEn,
                PrefixNameTh = s.PrefixNameTh,
                Gender = s.Gender,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,
            }).ToList();
            return new PagedResultDto<EmployeeDto> { Items = items, TotalCount = totalCount };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Employee: {Message}", ex.Message);
            throw;
        }
    }
    private async Task SyncBenefitPlansAsync(Guid employeeId, List<Guid>? benefitPlanIds)
    {
        var newIds = (benefitPlanIds ?? new List<Guid>()).ToHashSet();
        var existing = await _benefitPlanRepos.GetByEmployeeIdAsync(employeeId);
        var existingIds = existing.Select(x => x.BenefitPlanId).ToHashSet();

        foreach (var link in existing.Where(x => !newIds.Contains(x.BenefitPlanId)))
        {
            link.IsDelete = true;
            link.DeletedBy = _currentUser.GetCurrentUserEmail();
            link.DeletedAt = DateTime.Now;
            await _benefitPlanRepos.UpdateAsync(link);
        }

        foreach (var planId in newIds.Except(existingIds))
        {
            await _benefitPlanRepos.AddAsync(new TbEmployeeBenefitPlan
            {
                EmployeeId = employeeId,
                BenefitPlanId = planId,
                IsActive = true,
                CreatedBy = _currentUser.GetCurrentUserEmail(),
                CreatedAt = DateTime.Now,
                UpdatedBy = _currentUser.GetCurrentUserEmail(),
                UpdatedAt = DateTime.Now,
                IsDelete = false
            });
        }

        await _benefitPlanRepos.SaveChangesAsync();
    }
    public async Task<EmployeeDetailDto?> GetByIdAsync(Guid id, bool includeOrgChart = true)
    {
        try
        {
            var s = await _repos.GetDetailByIdAsync(id);
            if (s == null) return null;

            var dto = MapToDetailDto(s);

            if (includeOrgChart)
                dto.OrgChart = await BuildOrgChartAsync(id);

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Employee: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<EmployeeDetailDto?> GetByEmployeeCodeAsync(string employeeCode)
    {
        try
        {
            var s = await _repos.GetDetailByEmpIdAsync(employeeCode);
            if (s == null) return null;

            var dto = MapToDetailDto(s);
            dto.OrgChart = await BuildOrgChartAsync(s.Id);
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล Employee: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<PagedResultDto<EmployeeSearchDto>> SearchEmployeesAsync(SearchEmployeeRequestDto request)
    {
        var (emps, totalCount) = await _repos.SearchAsync(request);

        var items = emps.Select(e => new EmployeeSearchDto
        {
            Id = e.Id,
            EmpId = e.EmpId,
            FirstNameTh = e.FnameTh,
            LastNameTh = e.LnameTh,
            FirstNameEn = e.FnameEn,
            LastNameEn = e.LnameEn,
            PositionNameTh = e.Position.NameTh,
            PositionNameEn = e.Position.NameEn,
            Status = e.Status
        }).ToList();

        return new PagedResultDto<EmployeeSearchDto> { Items = items, TotalCount = totalCount };
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto data)
    {
        try
        {
            if (await _repos.ExistsByEmpIdAsync(data.EmpId))
                throw new InvalidOperationException("ข้อมูลซ้ำในช่อง: EmpId");

            var newEntity = new TbEmployee
            {
                EmpId = data.EmpId,
                FnameTh = data.FNameTh,
                FnameEn = data.FNameEn,
                LnameEn = data.LNameEn,
                LnameTh = data.LNameTh,
                Email = data.Email,
                HireDate = data.HireDate.Date,
                EmployeeTypeId = data.EmployeeTypeId,
                Status = data.Status,
                ReportToId = data.ReportToId,
                PositionId = data.PositionId,
                PositionLevelId = data.PositionLevelId,
                CompanyId = data.CompanyId,
                BranchId = data.BranchId,
                GroupId = data.GroupId,
                DepartmentId = data.DepartmentId,
                DivisionId = data.DivisionId,
                SectionId = data.SectionId,
                Gender = data.Gender,
                PrefixNameEn = data.PrefixNameEn,
                PrefixNameTh = data.PrefixNameTh,
                CreatedAt = DateTime.Now,
                CreatedBy = _currentUser.GetCurrentUserEmail(),
                UpdatedAt = DateTime.Now,
                UpdatedBy = _currentUser.GetCurrentUserEmail(),
                IsDelete = false
            };

            await _repos.AddAsync(newEntity);
            await _repos.SaveChangesAsync();

            await SyncBenefitPlansAsync(newEntity.Id, data.BenefitPlanIds);   // ← เพิ่ม

            // หลังบันทึกไม่ต้องคำนวณ OrgChart (หน้าบ้านโหลด List ใหม่อยู่แล้ว)
            return (await GetByIdAsync(newEntity.Id))!;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มข้อมูล Employee : {Message}", ex.Message);
            throw;
        }
    }

    public async Task<EmployeeDto?> UpdateAsync(Guid id, CreateEmployeeDto data)
    {
        try
        {
            var existingEntity = await _repos.GetByIdAsync(id); // Tracked entity, ไม่ใช้ตัว Include
            if (existingEntity == null)
                return null;

            if (await _repos.ExistsByEmpIdAsync(data.EmpId, id))
                throw new InvalidOperationException("ข้อมูลซ้ำในช่อง: EmpId");

            existingEntity.EmpId = data.EmpId;
            existingEntity.FnameTh = data.FNameTh;
            existingEntity.FnameEn = data.FNameEn;
            existingEntity.LnameEn = data.LNameEn;
            existingEntity.LnameTh = data.LNameTh;
            existingEntity.Email = data.Email;
            existingEntity.HireDate = data.HireDate.Date;
            existingEntity.EmployeeTypeId = data.EmployeeTypeId;
            existingEntity.Status = data.Status;
            existingEntity.ReportToId = data.ReportToId;
            existingEntity.PositionId = data.PositionId;
            existingEntity.PositionLevelId = data.PositionLevelId;
            existingEntity.CompanyId = data.CompanyId;
            existingEntity.BranchId = data.BranchId;
            existingEntity.GroupId = data.GroupId;
            existingEntity.DepartmentId = data.DepartmentId;
            existingEntity.DivisionId = data.DivisionId;
            existingEntity.SectionId = data.SectionId;
            existingEntity.UpdatedAt = DateTime.Now;
            existingEntity.PrefixNameTh = data.PrefixNameTh;
            existingEntity.PrefixNameEn = data.PrefixNameEn;
            existingEntity.Gender = data.Gender;
            existingEntity.UpdatedBy = _currentUser.GetCurrentUserEmail();

            await _repos.SaveChangesAsync();

            await SyncBenefitPlansAsync(id, data.BenefitPlanIds);   // ← เพิ่ม

            return await GetByIdAsync(id, includeOrgChart: false);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มข้อมูล Employee: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var existingEntity = await _repos.GetByIdAsync(id); // Tracked entity
            if (existingEntity == null)
                return false;

            existingEntity.IsDelete = true;
            existingEntity.DeletedBy = _currentUser.GetCurrentUserEmail();
            existingEntity.DeletedAt = DateTime.Now;

            await _repos.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการลบข้อมูล Employee: {Message}", ex.Message);
            throw;
        }
    }

    private static EmployeeDetailDto MapToDetailDto(TbEmployee s)
    {
        return new EmployeeDetailDto
        {
            Id = s.Id,
            EmpId = s.EmpId,
            FNameTh = s.FnameTh,
            LNameTh = s.LnameTh,
            FNameEn = s.FnameEn,
            LNameEn = s.LnameEn,
            Email = s.Email,
            HireDate = s.HireDate,
            PositionId = s.PositionId,
            PositionNameTh = s.Position!.NameTh,
            PositionNameEn = s.Position!.NameEn,
            PositionLevelId = s.PositionLevelId,
            PositionLevel = s.PositionLevel.Level,
            PositionLevelNameTh = s.PositionLevel!.NameTh,
            PositionLevelNameEn = s.PositionLevel!.NameEn,
            CompanyId = s.CompanyId,
            CompanyNameTh = s.Company!.NameTh,
            CompanyNameEn = s.Company!.NameEn,
            CompanyCode = s.Company.Code,
            GroupId = s.GroupId,
            GroupNameTh = s.Group != null ? s.Group.NameTh : null,
            GroupNameEn = s.Group != null ? s.Group.NameEn : null,
            DepartmentId = s.DepartmentId,
            DepartmentNameTh = s.Department != null ? s.Department.NameTh : null,
            DepartmentNameEn = s.Department != null ? s.Department.NameEn : null,
            DivisionId = s.DivisionId,
            DivisionNameTh = s.Division != null ? s.Division.NameTh : null,
            DivisionNameEn = s.Division != null ? s.Division.NameEn : null,
            SectionId = s.SectionId,
            SectionNameTh = s.Section != null ? s.Section.NameTh : null,
            SectionNameEn = s.Section != null ? s.Section.NameEn : null,
            EmployeeTypeId = s.EmployeeTypeId,
            EmployeeTypeNameTh = s.EmployeeType!.NameTh,
            EmployeeTypeNameEn = s.EmployeeType!.NameEn,
            PrefixNameEn = s.PrefixNameEn,
            PrefixNameTh = s.PrefixNameTh,
            Gender = s.Gender,
            Status = s.Status,
            ReportToId = s.ReportToId,
            BranchId = s.BranchId,
            BranchNameEn = s.Branch!.NameEn,
            BranchNameTh = s.Branch!.NameTh,
            BranchCode = s.Branch!.Code,
            ReportToNameEn = s.ReportToId != null ? $"{s.ReportTo!.FnameEn} {s.ReportTo.LnameEn}" : null,
            ReportToNameTh = s.ReportToId != null ? $"{s.ReportTo!.FnameTh} {s.ReportTo.LnameTh}" : null,
            BenefitPlans = s.TbEmployeeBenefitPlans
                .Select(bp => new BenefitPlanSummaryDto
                {
                    Id = bp.BenefitPlanId,
                    NameTh = bp.BenefitPlan.NameTh,
                    NameEn = bp.BenefitPlan.NameEn,
                    Items = bp.BenefitPlan.TbBenefitPlanItems
                        .Where(i => i.IsActive)
                        .Select(i => new BenefitPlanItemSummaryDto
                        {
                            BenefitId = i.BenefitId,
                            BenefitNameTh = i.Benefit.NameTh,
                            BenefitNameEn = i.Benefit.NameEn,
                            LimitAmount = i.LimitAmount,
                            Description = i.Description,
                        })
                        .ToList(),
                })
                .ToList(),
            CreatedAt = s.CreatedAt,
            CreatedBy = s.CreatedBy,
            UpdatedAt = s.UpdatedAt,
            UpdatedBy = s.UpdatedBy,
            //OrganizationUnitPath = new List<OrganizationUnitPathItemDto>()
        };
    }
    private async Task<EmployeeOrgChartNodeDto?> BuildOrgChartAsync(Guid employeeId)
    {
        var flat = await _repos.GetOrgChartFlatDataAsync();
        var byId = flat.ToDictionary(x => x.Id);
        var childrenLookup = flat
            .Where(x => x.ReportToId.HasValue)
            .GroupBy(x => x.ReportToId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        if (!byId.ContainsKey(employeeId))
            return null;

        // 1) ไล่ขึ้นไปหาสายบังคับบัญชาทั้งหมด จนถึงจุดสูงสุด (Root)
        //    เก็บเป็น List เรียงจาก "เรา" ไปหา "สูงสุด" ก่อน (ใกล้ตัวสุดอยู่ Index 0)
        var upChain = new List<EmployeeOrgChartNodeDto> { CloneNode(byId[employeeId]) };
        var visitedUp = new HashSet<Guid> { employeeId };
        var currentReportToId = byId[employeeId].ReportToId;

        while (currentReportToId.HasValue && byId.TryGetValue(currentReportToId.Value, out var manager))
        {
            if (!visitedUp.Add(manager.Id)) break; // กันข้อมูลวนลูป
            upChain.Add(CloneNode(manager));
            currentReportToId = manager.ReportToId;
        }

        // 2) เติมลูกน้องเต็มสาย (Subtree) ให้เฉพาะ Node ที่เป็น "เรา" เท่านั้น
        //    (สายบังคับบัญชาชั้นอื่นไม่เอาลูกน้องของเขามาด้วย จะได้ไม่บวมข้อมูลเกินจำเป็น)
        var selfNode = upChain[0];
        selfNode.IsSelf = true;
        AttachSubtree(selfNode, childrenLookup, new HashSet<Guid> { employeeId });

        // 3) ประกอบร่าง: ต่อ Node แต่ละชั้นเป็นสายเดียวจาก Root ลงมาถึงเรา
        //    upChain[last] คือ Root (สูงสุด) → ... → upChain[0] คือ "เรา"
        EmployeeOrgChartNodeDto? root = null;
        EmployeeOrgChartNodeDto? previous = null;

        for (int i = upChain.Count - 1; i >= 0; i--)
        {
            var node = upChain[i];
            if (root == null)
                root = node;
            else
                previous!.Children.Add(node);

            previous = node;
        }

        return root;
    }
    public async Task<List<AllowedEmployeeDto>> GetAllowedEmployeesForDocumentAsync()
    {
        var employeeCode = _currentUser.GetCurrentUserEmployeeCode();
        var flat = await _repos.GetOrgChartFlatDataAsync();

        var self = flat.FirstOrDefault(x => x.EmpId == employeeCode);
        if (self == null) return new List<AllowedEmployeeDto>();

        var childrenLookup = flat
            .Where(x => x.ReportToId.HasValue)
            .GroupBy(x => x.ReportToId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var allowedIds = new HashSet<Guid> { self.Id };

        // ลูกน้องทุกชั้น (BFS/DFS)
        void CollectSubordinates(Guid managerId)
        {
            if (!childrenLookup.TryGetValue(managerId, out var children)) return;
            foreach (var child in children)
            {
                if (allowedIds.Add(child.Id))
                    CollectSubordinates(child.Id);
            }
        }
        CollectSubordinates(self.Id);

        // พนักงานในสาขาที่ตนดูแล (ถ้ามี)
        var currentUserId = _currentUser.GetCurrentUserId();
        var adminBranches = await _podAdminBranchRepos.GetByUserIdWithBranchAsync(currentUserId);
        if (adminBranches.Count > 0)
        {
            var branchIds = adminBranches.Select(b => b.BranchId).ToHashSet();
            foreach (var emp in flat.Where(x => branchIds.Contains(x.BranchId)))
                allowedIds.Add(emp.Id);
        }

        return flat
            .Where(x => allowedIds.Contains(x.Id))
            .OrderBy(x => x.FullNameTh)
            .Select(x => new AllowedEmployeeDto
            {
                Id = x.Id,
                EmpId = x.EmpId,
                FullNameTh = x.FullNameTh,
                FullNameEn = x.FullNameEn,
                PositionNameTh = x.PositionNameTh,
            })
            .ToList();
    }
    private static EmployeeOrgChartNodeDto CloneNode(EmployeeOrgChartNodeDto source)
    {
        return new EmployeeOrgChartNodeDto
        {
            Id = source.Id,
            EmpId = source.EmpId,
            FullNameTh = source.FullNameTh,
            FullNameEn = source.FullNameEn,
            PositionNameTh = source.PositionNameTh,
            PositionNameEn = source.PositionNameEn,
            ReportToId = source.ReportToId,
        };
    }

    private static void AttachSubtree(EmployeeOrgChartNodeDto node, Dictionary<Guid, List<EmployeeOrgChartNodeDto>> childrenLookup, HashSet<Guid> visitedDown)
    {
        if (!childrenLookup.TryGetValue(node.Id, out var children))
            return;

        foreach (var child in children)
        {
            if (!visitedDown.Add(child.Id)) continue; // กันข้อมูลวนลูป
            var childNode = CloneNode(child);
            node.Children.Add(childNode);
            AttachSubtree(childNode, childrenLookup, visitedDown);
        }
    }
}