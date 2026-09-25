namespace UBIS.HR.Application.Dtos;

public class OrganizationUnitDto
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string NameTh { get; set; }
    public string NameEn { get; set; }
    public string? ShortName { get; set; }
    public string? Type { get; set; }  // Group, Department, Division, Section
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateOrganizationUnitDto
{
    public string? Code { get; set; }
    public string NameTh { get; set; }
    public string NameEn { get; set; }
    public string? ShortName { get; set; }
    public string Type { get; set; }  // Group, Department, Division, Section
}

public class OrganizationUnitPathItemDto
{
    public Guid Id { get; set; }
    public string NameTh { get; set; }
    public string NameEn { get; set; }
    public string? LevelTypeNameTh { get; set; }
    public string? LevelTypeNameEn { get; set; }
}

public class OrganizationUnitFilterDto
{
    public string? Search { get; set; }
    public string? Type { get; set; }   // Filter ตามประเภท (Group/Department/Division/Section) — Optional
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}