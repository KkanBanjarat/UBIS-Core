

using UBIS.HR.Application.Dtos;
namespace UBIS.HR.Application.Interfaces;

public interface IEmployeeTypeService
{
    Task<IEnumerable<EmployeeTypeDto>> GetAllAsync();
    Task<EmployeeTypeDto?> GetByIdAsync(Guid id);
}