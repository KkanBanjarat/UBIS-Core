using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories.Interfaces;

namespace UBIS.HR.Infrastructure.Repositories;

public interface IAttachmentRepos : IBaseRepos<TbAttachment>
{
    Task<List<TbAttachment>> GetByDocumentAsync(string docType, string docNumber);
}