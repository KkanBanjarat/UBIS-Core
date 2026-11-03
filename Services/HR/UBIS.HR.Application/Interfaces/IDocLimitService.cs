using UBIS.HR.Application.Dtos;

namespace UBIS.HR.Application.Interfaces;

public interface IDocLimitService
{
    /// limit ที่มีผล ณ วันที่เอกสาร (LimitAmount เป็น null ถ้าไม่มีกำหนด)
    Task<DocLimitDto> GetEffectiveAsync(string docType, DateTime docDate);

    /// ตรวจยอดรวมทั้งเอกสาร ถ้าเกินจะ throw InvalidOperationException
    Task EnsureWithinLimitAsync(string docType, DateTime docDate, decimal totalAmount);
}