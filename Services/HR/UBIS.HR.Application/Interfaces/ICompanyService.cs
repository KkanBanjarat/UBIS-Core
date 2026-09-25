

using UBIS.HR.Application.Dtos;
namespace UBIS.HR.Application.Interfaces;

public interface ICompanyService
{
    Task<IEnumerable<CompanyDto>> GetAllAsync();
    Task<CompanyDto?> GetByIdAsync(Guid id);
}