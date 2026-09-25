public class UserListDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public Guid? EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public bool IsActive { get; set; }
    public bool IsEntra { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class CreateLocalUserDto
{
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string? EmployeeCode { get; set; }
}

public class UpdateUserDto
{
    public string? Email { get; set; }             // ใช้เฉพาะ User local
    public string? DisplayName { get; set; }       // ใช้เฉพาะ User local
    public string? EmployeeCode { get; set; }
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
    public string? EntraObjectId { get; set; }

    public string Email { get; set; }

    public string DisplayName { get; set; }

    public string? EmployeeCode { get; set; }

    public bool IsActive { get; set; }
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