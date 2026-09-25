namespace UBIS.Access.Application.Dtos;
public class UserRoleDto
{
    public Guid UserId { get; set; }
    public List<RoleScopeSummaryDto> Roles { get; set; }
}

public class CreateUserRoleDto
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public string Scope { get; set; }
}