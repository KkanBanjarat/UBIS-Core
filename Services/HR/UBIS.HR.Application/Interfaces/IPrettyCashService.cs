using UBIS.HR.Application.Dtos;

namespace UBIS.HR.Application.Interfaces;

public interface IPettyCashService : IApprovalDocumentService
{
    Task<PagedResultDto<PettyCashDto>> GetAllAsync(PettyCashFilterDto filter);
    Task<PettyCashDto?> GetByIdAsync(Guid id);
    Task<PettyCashDto> CreateAsync(CreatePettyCashDto data);
    Task<PettyCashDto?> UpdateAsync(Guid id, CreatePettyCashDto data);
    Task<bool> DeleteAsync(Guid id);
    Task<PettyCashDto?> SubmitAsync(Guid id);
    Task<PettyCashDto?> RecallAsync(Guid id);
    Task<PettyCashDto?> GetByDocNumAsync(string docNum);
}