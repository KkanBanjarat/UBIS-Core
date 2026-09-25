public class UserListDto
{
    public Guid Id { get; set; }
    public string? EntraObjectId { get; set; }
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public Guid? EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public bool IsActive { get; set; }
    public bool IsEntra { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateUserDto
{
    public string? EntraObjectId { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public string EmployeeCode { get; set; }
    public Guid EmployeeId { get; set; }
    public bool IsActive { get; set; }
}

public class EntraSyncResultDto
{
    public int Total { get; set; }
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Linked { get; set; }
    public int Unchanged { get; set; }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string? EntraObjectId { get; set; }

    public string Email { get; set; }

    public string DisplayName { get; set; }

    public string? EmployeeCode { get; set; }

    public bool IsActive { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public class UserLookupDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public bool IsActive { get; set; }
}