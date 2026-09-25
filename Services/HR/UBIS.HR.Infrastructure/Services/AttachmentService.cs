using Microsoft.AspNetCore.Http;
using UBIS.HR.Application.Dtos;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Domain.Entities;
using UBIS.HR.Infrastructure.Repositories;

namespace UBIS.HR.Infrastructure.Services;

public class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepos _repos;
    private readonly ICurrentUserService _currentUser;
    private readonly string _rootPath;

    private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".xlsx", ".xls", ".docx", ".doc" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public AttachmentService(IAttachmentRepos repos, ICurrentUserService currentUser)
    {
        _repos = repos;
        _currentUser = currentUser;
        _rootPath = "/app/uploads";
    }

    public async Task<List<AttachmentDto>> GetByDocumentAsync(string docType, string docNumber)
    {
        var items = await _repos.GetByDocumentAsync(docType, docNumber);
        return items.Select(MapToDto).ToList();
    }

    public async Task<AttachmentDto> UploadAsync(string docType, string docNumber, IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new InvalidOperationException("กรุณาเลือกไฟล์ที่ต้องการอัปโหลด");

        if (file.Length > MaxFileSize)
            throw new InvalidOperationException("ไฟล์มีขนาดเกิน 10 MB");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException($"ไม่รองรับไฟล์ประเภท {ext}");


        var safeDocType = SanitizePathSegment(docType);
        var safeDocNumber = SanitizePathSegment(docNumber);

        var folder = Path.Combine(_rootPath, safeDocType, safeDocNumber);
        Directory.CreateDirectory(folder);

        var storedFileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(folder, storedFileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var entity = new TbAttachment
        {
            DocType = docType,
            DocNumber = docNumber,
            FileName = file.FileName,
            FilePath = fullPath,
            ContentType = file.ContentType,
            FileSize = file.Length,
            CreatedAt = DateTime.Now,
            CreatedBy = _currentUser.GetCurrentUserEmail(),
            IsDelete = false
        };

        await _repos.AddAsync(entity);
        await _repos.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task<(byte[] Content, string ContentType, string FileName)?> DownloadAsync(Guid id)
    {
        var entity = await _repos.GetByIdAsync(id);
        if (entity == null || entity.IsDelete || !File.Exists(entity.FilePath))
            return null;

        var bytes = await File.ReadAllBytesAsync(entity.FilePath);
        return (bytes, entity.ContentType, entity.FileName);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _repos.GetByIdAsync(id);
        if (entity == null) return false;

        entity.IsDelete = true;
        entity.DeletedAt = DateTime.Now;
        entity.DeletedBy = _currentUser.GetCurrentUserEmail();
        await _repos.SaveChangesAsync();

        // ลบไฟล์จริงทิ้งด้วย (Hard Delete เฉพาะไฟล์บน Disk เท่านั้น Record ยัง Soft Delete ไว้เป็นประวัติ)
        if (File.Exists(entity.FilePath))
        {
            try { File.Delete(entity.FilePath); } catch { /* ไม่ให้ล้ม Transaction ถ้าลบไฟล์ไม่สำเร็จ */ }
        }

        return true;
    }

    private static AttachmentDto MapToDto(TbAttachment e)
    {
        return new AttachmentDto
        {
            Id = e.Id,
            DocType = e.DocType,
            DocNumber = e.DocNumber,
            FileName = e.FileName,
            ContentType = e.ContentType,
            FileSize = e.FileSize,
            Url = $"/Attachment/{e.Id}/download",
            CreatedBy = e.CreatedBy,
            CreatedAt = e.CreatedAt,
        };
    }

    private static string SanitizePathSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("ข้อมูลอ้างอิงเอกสารไม่ถูกต้อง");

        if (value.Contains("..") || value.Contains('/') || value.Contains('\\'))
            throw new InvalidOperationException("ข้อมูลอ้างอิงเอกสารมีอักขระที่ไม่อนุญาต");

        var invalidChars = Path.GetInvalidFileNameChars();
        if (value.Any(c => invalidChars.Contains(c)))
            throw new InvalidOperationException("ข้อมูลอ้างอิงเอกสารมีอักขระที่ไม่อนุญาต");

        return value;
    }
}