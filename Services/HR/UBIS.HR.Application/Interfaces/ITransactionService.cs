namespace UBIS.HR.Application.Interfaces;

public interface ITransactionService
{
    Task<T> RunAsync<T>(Func<Task<T>> action);
    Task RunAsync(Func<Task> action);

    /// ถ้าอยู่ใน transaction จะรอจน commit สำเร็จแล้วค่อยรัน ถ้าไม่อยู่จะรันทันที
    /// ถ้า rollback จะไม่รัน และ error ในงานหลัง commit จะไม่ถูกโยนออกไป
    Task OnCommittedAsync(Func<Task> callback);
}