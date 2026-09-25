using Microsoft.EntityFrameworkCore;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Services;

public class DocNumberService : IDocNumberService
{
    private readonly HrDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DocNumberService(HrDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<string> GenerateAsync(string docType)
    {
        for (int attempt = 0; attempt < 3; attempt++)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var prefix = await _context.TbPrefixes
                    .FromSqlInterpolated($"SELECT * FROM tb_prefix WHERE \"DocType\" = {docType} FOR UPDATE")
                    .FirstOrDefaultAsync();

                if (prefix == null)
                    throw new InvalidOperationException($"ไม่พบการตั้งค่าเลขที่เอกสารสำหรับ DocType: {docType}");

                var currentKey = string.IsNullOrEmpty(prefix.DateFormat)
                    ? ""
                    : DateTime.Now.ToString(prefix.DateFormat);

                var runningNumber = prefix.LastResetKey == currentKey
                    ? prefix.LastRunningNumber + 1
                    : 1;

                prefix.LastResetKey = currentKey;
                prefix.LastRunningNumber = runningNumber;
                prefix.UpdatedAt = DateTime.Now;
                prefix.UpdatedBy = _currentUser.GetCurrentUserEmail();

                var docNumber = $"{prefix.Prefix}{currentKey}{runningNumber.ToString().PadLeft(prefix.RunningLength, '0')}";

                _context.TbDocNumberLogs.Add(new TbDocNumberLog
                {
                    DocType = docType,
                    DocNumber = docNumber,
                    GeneratedAt = DateTime.Now,
                    GeneratedBy = _currentUser.GetCurrentUserEmail()
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return docNumber;
            }
            catch (DbUpdateException) when (attempt < 2)
            {
                await transaction.RollbackAsync();
            }
        }

        throw new InvalidOperationException("ไม่สามารถออกเลขที่เอกสารได้ กรุณาลองใหม่อีกครั้ง");
    }
}