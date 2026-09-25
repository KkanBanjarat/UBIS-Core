using UBIS.HR.Application.Dtos;
namespace UBIS.HR.Application.Interfaces;

public interface ICurrentUserService
{
    string GetCurrentUserEmail();
    Guid GetCurrentUserId();
    string GetCurrentUserEmployeeCode();
    string? GetScope(string permissionCode);
}