namespace UBIS.HR.Application.Interfaces;

public interface IDocNumberService
{
    Task<string> GenerateAsync(string docType);
}