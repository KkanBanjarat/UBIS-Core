public class AttachmentDto
{
    public Guid Id { get; set; }
    public string DocType { get; set; }
    public string DocNumber { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public string Url { get; set; }   // ← เพิ่ม
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}