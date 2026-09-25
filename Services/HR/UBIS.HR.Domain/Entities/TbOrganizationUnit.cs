using System;
using System.Collections.Generic;

namespace UBIS.HR.Domain.Entities;

public partial class TbOrganizationUnit
{
    public Guid Id { get; set; }

    public string? Code { get; set; }

    public string NameTh { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public bool IsDelete { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public string? ShortName { get; set; }

    public string? Type { get; set; }

    public virtual ICollection<TbEmployee> TbEmployeeDepartments { get; set; } = new List<TbEmployee>();

    public virtual ICollection<TbEmployee> TbEmployeeDivisions { get; set; } = new List<TbEmployee>();

    public virtual ICollection<TbEmployee> TbEmployeeGroups { get; set; } = new List<TbEmployee>();

    public virtual ICollection<TbEmployee> TbEmployeeSections { get; set; } = new List<TbEmployee>();
}
