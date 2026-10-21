using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Infrastructure.Repositories;

namespace UBIS.HR.Infrastructure.Services;

public class DocLimitService : IDocLimitService
{
    private readonly IDocLimitRepos _repos;

    public DocLimitService(IDocLimitRepos repos) => _repos = repos;

    public async Task<DocLimitDto> GetEffectiveAsync(string docType, DateTime docDate)
    {
        var row = await _repos.GetEffectiveAsync(docType, docDate.Date);
        return new DocLimitDto
        {
            LimitAmount = row?.LimitAmount,
            EffectiveFrom = row?.EffectiveFrom
        };
    }

    public async Task EnsureWithinLimitAsync(string docType, DateTime docDate, decimal totalAmount)
    {
        var limit = await GetEffectiveAsync(docType, docDate);
        if (limit.LimitAmount.HasValue && totalAmount > limit.LimitAmount.Value)
            throw new InvalidOperationException(
                $"ยอดรวมทั้งเอกสารต้องไม่เกิน {limit.LimitAmount.Value:N2} บาท (ยอดปัจจุบัน {totalAmount:N2} บาท)");
    }
}