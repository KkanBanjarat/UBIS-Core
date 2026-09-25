using UBIS.HR.Application.Dtos;

namespace UBIS.HR.Application.Interfaces;

public interface IPrettyCashService : IApprovalDocumentService
{
    Task<PagedResultDto<PrettyCashDto>> GetAllAsync(PrettyCashFilterDto filter);
    Task<PrettyCashDto?> GetByIdAsync(Guid id);
    Task<PrettyCashDto> CreateAsync(CreatePrettyCashDto data);
    Task<PrettyCashDto?> UpdateAsync(Guid id, CreatePrettyCashDto data);
    Task<bool> DeleteAsync(Guid id);
    Task<PrettyCashDto?> SubmitAsync(Guid id);
    Task<PrettyCashDto?> RecallAsync(Guid id);
    Task<PrettyCashDto?> GetByDocNumAsync(string docNum);
}