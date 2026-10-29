using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories.Interfaces;

namespace UBIS.HR.Infrastructure.Repositories;

public interface IDocLimitRepos : IBaseRepos<TbDocLimit>
{
    Task<TbDocLimit?> GetEffectiveAsync(string docType, DateTime date);
}