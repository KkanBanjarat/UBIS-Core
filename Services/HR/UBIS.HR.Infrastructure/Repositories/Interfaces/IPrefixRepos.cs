using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories.Interfaces;

namespace UBIS.HR.Infrastructure.Repositories;

public interface IPrefixRepos : IBaseRepos<TbPrefix>
{
    Task<TbPrefix?> GetByDocTypeAsync(string docType);
    void AddDocNumberLog(TbDocNumberLog log);
}