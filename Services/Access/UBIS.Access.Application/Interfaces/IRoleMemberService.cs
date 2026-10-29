using UBIS.Access.Application.Dtos;

namespace UBIS.Access.Application.Interfaces;

public interface IRoleMemberService
{
    Task<List<RoleMemberDto>> GetByRoleIdAsync(Guid roleId);
}