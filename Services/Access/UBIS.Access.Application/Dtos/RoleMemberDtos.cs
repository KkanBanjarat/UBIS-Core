namespace UBIS.Access.Application.Dtos;

public class RoleMemberDto
{
    public Guid UserRoleId { get; set; }   // ใช้ตอนลบ (DELETE api/UserRoles/{id})
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? EmployeeCode { get; set; }
    public bool IsActive { get; set; }
    public string Scope { get; set; } = "";
}