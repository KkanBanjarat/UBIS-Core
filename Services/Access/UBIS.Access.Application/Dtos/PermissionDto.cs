namespace UBIS.Access.Application.Dtos;
public class PermissionDto
{
    public Guid Id { get; set; }

    public string Code { get; set; }

    public string? Description { get; set; }
    public string? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class CreatePermissionDto
{

    public string Code { get; set; }

    public string? Description { get; set; }
}

public class PermissionSummaryDto
{
     public Guid Id { get; set; }
     public string Code { get; set; }

    public string? Description { get; set; }
}