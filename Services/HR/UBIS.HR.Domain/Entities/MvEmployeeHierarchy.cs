using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class MvEmployeeHierarchy
{
    public Guid? AncestorId { get; set; }

    public Guid? DescendantId { get; set; }

    public int? Depth { get; set; }
}
