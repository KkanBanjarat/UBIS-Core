using Microsoft.Extensions.Logging;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Infrastructure.Data;

namespace UBIS.HR.Infrastructure.Services;

public class TransactionService : ITransactionService
{
    private readonly HrDbContext _db;
    private readonly ILogger<TransactionService> _logger;
    private readonly List<Func<Task>> _callbacks = new();
    private int _depth;

    public TransactionService(HrDbContext db, ILogger<TransactionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<T> RunAsync<T>(Func<Task<T>> action)
    {
        if (_depth > 0) return await action();   // อยู่ใน transaction แล้ว ใช้อันเดิม

        _depth++;
        T result;
        try
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            result = await action();
            await tx.CommitAsync();
        }
        catch
        {
            _callbacks.Clear();   // rollback แล้ว ไม่ต้องแจ้งอะไร
            throw;
        }
        finally
        {
            _depth--;
        }

        await FlushCallbacksAsync();
        return result;
    }

    public async Task RunAsync(Func<Task> action)
        => await RunAsync<bool>(async () => { await action(); return true; });

    public async Task OnCommittedAsync(Func<Task> callback)
    {
        if (_depth > 0) _callbacks.Add(callback);
        else await SafeRunAsync(callback);
    }

    private async Task FlushCallbacksAsync()
    {
        var list = _callbacks.ToList();
        _callbacks.Clear();
        foreach (var cb in list) await SafeRunAsync(cb);
    }

    private async Task SafeRunAsync(Func<Task> cb)
    {
        try { await cb(); }
        catch (Exception ex) { _logger.LogWarning(ex, "งานหลัง commit ล้มเหลว (ข้ามไป)"); }
    }
}