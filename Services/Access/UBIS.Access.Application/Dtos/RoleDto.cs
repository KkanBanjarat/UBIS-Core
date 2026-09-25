namespace UBIS.Access.Application.Dtos;

public class RoleDto
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }
    public string? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class CreateRoleDto
{
    public string Name { get; set; }

    public string? Description { get; set; }
}

public class RoleScopeSummaryDto
{
    public Guid RoleId { get; set; }
    public string? RoleName { get; set; }
    public string Scope { get; set; }
}