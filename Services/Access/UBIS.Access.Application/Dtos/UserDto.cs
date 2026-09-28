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

public class CreateUserDto            // สร้าง User local เท่านั้น (ไม่รับ EntraObjectId)
{
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string? EmployeeCode { get; set; }
    public Guid? EmployeeId { get; set; }
    public string Password { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public class EntraSyncResultDto
{
    public int Total { get; set; }
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Linked { get; set; }
    public int Unchanged { get; set; }
    public int Skipped { get; set; }            // เพิ่ม
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



public class UpdateUserDto
{
    public string? Email { get; set; }          // ใช้เฉพาะ User local
    public string? DisplayName { get; set; }    // ใช้เฉพาะ User local
    public string? EmployeeCode { get; set; }
    public Guid? EmployeeId { get; set; }
    public bool IsActive { get; set; }
}

public class ResetPasswordDto
{
    public string NewPassword { get; set; } = "";
}

public class UserFilterDto
{
    public string? Search { get; set; }
    public string? Status { get; set; }   // "active" | "inactive"
    public string? Source { get; set; }   // "entra" | "local"
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class EntraUserInputDto
{
    public string EntraObjectId { get; set; } = "";
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string? EmployeeCode { get; set; }
    public bool IsActive { get; set; }
}