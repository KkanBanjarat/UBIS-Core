using Microsoft.EntityFrameworkCore;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Repositories.Interfaces;

public class PrefixRepos : BaseRepos<TbPrefix>, IPrefixRepos
{
    // private readonly HrDbContext _context;

    public PrefixRepos(HrDbContext context) : base(context)
    {
        // _context = context;
    }

    public async Task<string> GenerateDocNumberAsync(string docType, string generatedBy)
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                var prefix = await _context.TbPrefixes
                    .FirstOrDefaultAsync(x => x.DocType == docType);

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
                prefix.UpdatedBy = generatedBy;

                var docNumber = $"{prefix.Prefix}{currentKey}{runningNumber.ToString().PadLeft(prefix.RunningLength, '0')}";

                _context.TbDocNumberLogs.Add(new TbDocNumberLog
                {
                    DocType = docType,
                    DocNumber = docNumber,
                    GeneratedAt = DateTime.Now,
                    GeneratedBy = generatedBy
                });

                await _context.SaveChangesAsync();
                return docNumber;
            }
            catch (DbUpdateConcurrencyException) when (attempt < 4)
            {
                // มี Request อื่นแก้ Row เดียวกันไปพร้อมกัน → ล้าง Tracking แล้วลองใหม่
                foreach (var entry in _context.ChangeTracker.Entries())
                    entry.State = EntityState.Detached;
            }
        }

        throw new InvalidOperationException("ไม่สามารถออกเลขที่เอกสารได้ กรุณาลองใหม่อีกครั้ง");
    }
}