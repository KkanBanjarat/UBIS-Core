using Microsoft.AspNetCore.Http;
using UBIS.HR.Application.Dtos;

namespace UBIS.HR.Application.Interfaces;

public interface IAttachmentService
{
    Task<List<AttachmentDto>> GetByDocumentAsync(string docType, string docNumber);
    Task<AttachmentDto> UploadAsync(string docType, string docNumber, IFormFile file);
    Task<(byte[] Content, string ContentType, string FileName)?> DownloadAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
}