using UBIS.HR.Application.Dtos;

namespace UBIS.HR.Application.Interfaces;

public interface IPositionLevelService
{
    Task<IEnumerable<PositionLevelDto>> GetAllAsync();
    Task<PositionLevelDto?> GetByIdAsync(Guid id);
    Task<PagedResultDto<PositionLevelDto>> GetPagedAsync(PositionLevelFilterDto filter);
    Task<PositionLevelDto> CreateAsync(CreatePositionLevelDto data);
    Task<PositionLevelDto?> UpdateAsync(Guid id, CreatePositionLevelDto data);
    Task<bool> DeleteAsync(Guid id);
}