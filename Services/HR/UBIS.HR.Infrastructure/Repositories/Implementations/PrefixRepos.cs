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
    public Task<TbPrefix?> GetByDocTypeAsync(string docType) =>
    _context.TbPrefixes.FirstOrDefaultAsync(x => x.DocType == docType);

    public void AddDocNumberLog(TbDocNumberLog log) => _context.TbDocNumberLogs.Add(log);
}