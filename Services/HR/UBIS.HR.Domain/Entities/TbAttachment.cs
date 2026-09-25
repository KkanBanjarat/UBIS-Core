using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbAttachment
{
    public Guid Id { get; set; }

    public string DocType { get; set; } = null!;

    public string DocNumber { get; set; } = null!;

    public int DocRev { get; set; }

    public string FileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long FileSize { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsDelete { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }
}
