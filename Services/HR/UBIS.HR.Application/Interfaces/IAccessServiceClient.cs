namespace UBIS.HR.Application.Interfaces;

/// <summary>
/// ตัวกลางเรียก Access Service ผ่าน API (ไม่แตะ Database ของ Access โดยตรง)
/// รักษา Service Boundary ตามหลัก Microservice
/// </summary>
public interface IAccessServiceClient
{
    /// <summary>
    /// หา UserId จาก Access Service โดยไล่ตามลำดับ: EmployeeCode → EmployeeId → Email
    /// คืน null ถ้าไม่พบ หรือเรียก Service ไม่สำเร็จ
    /// </summary>
    Task<Guid?> LookupUserIdAsync(Guid employeeId, string? employeeCode, string? email);
}