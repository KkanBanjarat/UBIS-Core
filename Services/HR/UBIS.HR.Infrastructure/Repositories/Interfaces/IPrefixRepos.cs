using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories.Interfaces;

namespace UBIS.HR.Infrastructure.Repositories;

public interface IPrefixRepos : IBaseRepos<TbPrefix>
{
    Task<string> GenerateDocNumberAsync(string docType, string generatedBy);
}