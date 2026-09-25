using Microsoft.EntityFrameworkCore;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Repositories.Interfaces;

public class ApprovalRepos : BaseRepos<TbTransApprove>, IApprovalRepos
{
    public ApprovalRepos(HrDbContext context) : base(context)
    {
    }

    public async Task<List<TbTransApprove>> GetByDocumentAsync(string docType, string docNumber, int docRev, int round)
    {
        return await _dbSet
            .Where(x => x.DocType == docType && x.DocNumber == docNumber && x.DocRev == docRev && x.Round == round)
            .OrderBy(x => x.StepNo)
            .ToListAsync();
    }

    public async Task<int> GetLatestRoundAsync(string docType, string docNumber, int docRev)
    {
        var hasAny = await _dbSet.AnyAsync(x => x.DocType == docType && x.DocNumber == docNumber && x.DocRev == docRev);
        if (!hasAny) return 0;

        return await _dbSet
            .Where(x => x.DocType == docType && x.DocNumber == docNumber && x.DocRev == docRev)
            .MaxAsync(x => x.Round);
    }

    public async Task<List<TbTransApprove>> GetPendingByApproverAsync(Guid approverId)
    {
        return await _dbSet
            .Where(x => x.ApproverId == approverId && x.Status == "WaitApprove")
            .OrderBy(x => x.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<TbTransApprove?> GetByIntIdAsync(int id)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
    }
}