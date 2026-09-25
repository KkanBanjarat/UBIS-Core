using Microsoft.EntityFrameworkCore;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Repositories.Interfaces;

public class ReasonApproveRepos : BaseRepos<TbReasonApprove>, IReasonApproveRepos
{
    public ReasonApproveRepos(HrDbContext context) : base(context)
    {
    }

    public async Task<List<TbReasonApprove>> GetByDocumentAsync(string docType, string docNumber, int docRev)
    {
        return await _dbSet
            .Where(x => x.DocType == docType && x.DocNumber == docNumber && x.DocRev == docRev)
            .OrderByDescending(x => x.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }
}