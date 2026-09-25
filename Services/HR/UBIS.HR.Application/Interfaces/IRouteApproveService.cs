using UBIS.HR.Application.Dtos;

namespace UBIS.HR.Application.Interfaces;

public interface IRouteApproveService
{
    Task<List<RouteApproveDto>> GetAllAsync();
    Task<List<string>> GetDocTypesAsync();
    Task<RouteApproveDto> CreateAsync(CreateRouteApproveDto data);
    Task<RouteApproveDto?> UpdateAsync(Guid id, CreateRouteApproveDto data);
    Task<bool> DeleteAsync(Guid id);
}