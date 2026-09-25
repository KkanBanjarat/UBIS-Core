using Microsoft.EntityFrameworkCore;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Repositories.Interfaces;

public class AttachmentRepos : BaseRepos<TbAttachment>, IAttachmentRepos
{
    public AttachmentRepos(HrDbContext context) : base(context)
    {
    }

    public async Task<List<TbAttachment>> GetByDocumentAsync(string docType, string docNumber)
    {
        return await _dbSet
            .Where(x => x.DocType == docType && x.DocNumber == docNumber && !x.IsDelete)
            .OrderByDescending(x => x.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }
}