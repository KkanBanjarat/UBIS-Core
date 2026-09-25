using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class FdwTbUser
{
    public Guid? Id { get; set; }

    public string? Email { get; set; }

    public string? DisplayName { get; set; }
}
