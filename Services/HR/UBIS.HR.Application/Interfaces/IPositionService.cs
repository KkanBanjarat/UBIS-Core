using UBIS.HR.Application.Dtos;

namespace UBIS.HR.Application.Interfaces;

public interface IPositionService
{
    Task<IEnumerable<PositionDto>> GetAllAsync();
    Task<PositionDto?> GetByIdAsync(Guid id);
    Task<(IEnumerable<PositionDto> Items, int TotalCount)> GetFilteredPagedAsync(PositionFilterDto filter);
    Task<PositionDto> CreateAsync(CreatePositionDto data);
    Task<PositionDto?> UpdateAsync(Guid id, CreatePositionDto data);
    Task<bool> DeleteAsync(Guid id);
}