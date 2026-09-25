namespace UBIS.HR.Application.Dtos;

public class OrganizationLevelTypeDto
{
     public Guid Id { get; set; }
    public string NameTh { get; set; }   // บริษัท, สายงาน, ฝ่าย, แผนก, ส่วนงาน
    public string NameEn { get; set; }   // Company, Group, Department, Division, Section
    public int Sequence { get; set; }    // 1=บริษัท, 2=สายงาน, 3=ฝ่าย, 4=แผนก, 5=ส่วนงาน (ใช้เรียงลำดับ/ตรวจสอบ)
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

}
public class CreateOrganizationLevelTypeDto
{
    public string NameTh { get; set; }
    public string NameEn { get; set;}
    public int Sequence { get; set; }
    
}