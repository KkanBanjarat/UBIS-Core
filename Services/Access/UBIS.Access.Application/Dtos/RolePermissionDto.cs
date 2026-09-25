namespace UBIS.Access.Application.Dtos;

public class RolePermissionDto
{
    public Guid RoleId { get; set; }
    public string? RoleName { get; set; }
    public List<PermissionSummaryDto> Permissions { get; set; }
}

public class CreateRolePermissionDto
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }


}