using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories.Interfaces;

namespace UBIS.HR.Infrastructure.Repositories;

public interface IRouteApproveRepos : IBaseRepos<TbRouteApprove>
{
    Task<List<TbRouteApprove>> GetActiveRouteAsync(string docType);
    Task<List<TbRouteApprove>> GetAllRoutesAsync();
    Task<List<string>> GetDocTypesAsync();
    Task<bool> ExistsStepNoAsync(string docType, int stepNo, Guid? excludeId = null);
}