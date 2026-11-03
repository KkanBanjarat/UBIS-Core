using Microsoft.EntityFrameworkCore;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;
using UBIS.HR.Infrastructure.Repositories;

namespace UBIS.HR.Infrastructure.Services;

public class DocNumberService : IDocNumberService
{
    private readonly HrDbContext _context;
    private readonly IPrefixRepos _prefixRepos;
    private readonly ICurrentUserService _currentUser;

    public DocNumberService(HrDbContext context, IPrefixRepos prefixRepos, ICurrentUserService currentUser)
    {
        _context = context;
        _prefixRepos = prefixRepos;
        _currentUser = currentUser;
    }

    public async Task<string> GenerateAsync(string docType)
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                var prefix = await _prefixRepos.GetByDocTypeAsync(docType);

                if (prefix == null)
                    throw new InvalidOperationException($"ไม่พบการตั้งค่าเลขที่เอกสารสำหรับ DocType: {docType}");

                var currentKey = string.IsNullOrEmpty(prefix.DateFormat)
                    ? ""
                    : DateTime.Now.ToString(prefix.DateFormat);

                var runningNumber = prefix.LastResetKey == currentKey
                    ? prefix.LastRunningNumber + 1
                    : 1;

                var user = _currentUser.GetCurrentUserEmail();

                prefix.LastResetKey = currentKey;
                prefix.LastRunningNumber = runningNumber;
                prefix.UpdatedAt = DateTime.Now;
                prefix.UpdatedBy = user;

                var docNumber = $"{prefix.Prefix}{currentKey}{runningNumber.ToString().PadLeft(prefix.RunningLength, '0')}";

                _prefixRepos.AddDocNumberLog(new TbDocNumberLog
                {
                    DocType = docType,
                    DocNumber = docNumber,
                    GeneratedAt = DateTime.Now,
                    GeneratedBy = user
                });

                await _context.SaveChangesAsync();
                return docNumber;
            }
            catch (DbUpdateException) when (attempt < 4)
            {
                _context.ChangeTracker.Clear();
            }
        }

        throw new InvalidOperationException("ไม่สามารถออกเลขที่เอกสารได้ กรุณาลองใหม่อีกครั้ง");
    }
}