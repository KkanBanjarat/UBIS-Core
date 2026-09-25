using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UBIS.HR.Domain.Entities;

namespace UBIS.HR.Infrastructure.Data;

public partial class HrDbContext : DbContext
{
    public HrDbContext()
    {
    }

    public HrDbContext(DbContextOptions<HrDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<FdwTbUser> FdwTbUsers { get; set; }

    public virtual DbSet<TbAttachment> TbAttachments { get; set; }

    public virtual DbSet<TbBenefit> TbBenefits { get; set; }

    public virtual DbSet<TbBenefitPlan> TbBenefitPlans { get; set; }

    public virtual DbSet<TbBenefitPlanItem> TbBenefitPlanItems { get; set; }

    public virtual DbSet<TbBranch> TbBranches { get; set; }

    public virtual DbSet<TbCompany> TbCompanies { get; set; }

    public virtual DbSet<TbDocNumberLog> TbDocNumberLogs { get; set; }

    public virtual DbSet<TbEmployee> TbEmployees { get; set; }

    public virtual DbSet<TbEmployeeBenefitPlan> TbEmployeeBenefitPlans { get; set; }

    public virtual DbSet<TbEmployeeType> TbEmployeeTypes { get; set; }

    public virtual DbSet<TbOrganizationLevelType> TbOrganizationLevelTypes { get; set; }

    public virtual DbSet<TbOrganizationUnit> TbOrganizationUnits { get; set; }

    public virtual DbSet<TbPodAdminBranch> TbPodAdminBranches { get; set; }

    public virtual DbSet<TbPosition> TbPositions { get; set; }

    public virtual DbSet<TbPositionLevel> TbPositionLevels { get; set; }

    public virtual DbSet<TbPrefix> TbPrefixes { get; set; }

    public virtual DbSet<TbPrettyCashLine> TbPrettyCashLines { get; set; }

    public virtual DbSet<TbPrettyCashRequest> TbPrettyCashRequests { get; set; }

    public virtual DbSet<TbReasonApprove> TbReasonApproves { get; set; }

    public virtual DbSet<TbRouteApprove> TbRouteApproves { get; set; }

    public virtual DbSet<TbTransApprove> TbTransApproves { get; set; }

    public virtual DbSet<VwPodAdminBranch> VwPodAdminBranches { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Name=ConnectionStrings:HrDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("postgres_fdw");

        modelBuilder.Entity<FdwTbUser>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("fdw_tb_user");
        });

        modelBuilder.Entity<TbAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_attachment_pkey");

            entity.ToTable("tb_attachment");

            entity.HasIndex(e => new { e.DocType, e.DocNumber, e.DocRev }, "idx_attachment_doc").HasFilter("(\"IsDelete\" = false)");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasDefaultValueSql("'System'::text");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DocRev).HasDefaultValue(1);
        });

        modelBuilder.Entity<TbBenefit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_benefit_pkey");

            entity.ToTable("tb_benefit");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasDefaultValueSql("'System'::text");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedBy).HasDefaultValueSql("'System'::text");
        });

        modelBuilder.Entity<TbBenefitPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_benefit_plan_pkey");

            entity.ToTable("tb_benefit_plan");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasDefaultValueSql("'System'::text");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedBy).HasDefaultValueSql("'System'::text");
        });

        modelBuilder.Entity<TbBenefitPlanItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_benefit_plan_item_pkey");

            entity.ToTable("tb_benefit_plan_item");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasDefaultValueSql("'System'::text");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LimitAmount).HasPrecision(18, 2);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedBy).HasDefaultValueSql("'System'::text");

            entity.HasOne(d => d.Benefit).WithMany(p => p.TbBenefitPlanItems)
                .HasForeignKey(d => d.BenefitId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_BenefitPlanItem_Benefit");

            entity.HasOne(d => d.BenefitPlan).WithMany(p => p.TbBenefitPlanItems)
                .HasForeignKey(d => d.BenefitPlanId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_BenefitPlanItem_BenefitPlan");
        });

        modelBuilder.Entity<TbBranch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_branch_pkey");

            entity.ToTable("tb_branch");

            entity.HasIndex(e => e.CompanyId, "idx_branch_company").HasFilter("(\"IsDelete\" = false)");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasDefaultValueSql("'System'::text");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedBy).HasDefaultValueSql("'System'::text");

            entity.HasOne(d => d.Company).WithMany(p => p.TbBranches)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Branch_Company");
        });

        modelBuilder.Entity<TbCompany>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_company_pkey");

            entity.ToTable("tb_company");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasDefaultValueSql("'System'::text");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedBy).HasDefaultValueSql("'System'::text");
        });

        modelBuilder.Entity<TbDocNumberLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_doc_number_log_pkey");

            entity.ToTable("tb_doc_number_log");

            entity.HasIndex(e => new { e.DocType, e.GeneratedAt }, "idx_doc_number_log_type");

            entity.HasIndex(e => new { e.DocType, e.DocNumber }, "ux_doc_number_log_docnum").IsUnique();

            entity.Property(e => e.GeneratedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.GeneratedBy).HasDefaultValueSql("'System'::text");
        });

        modelBuilder.Entity<TbEmployee>(entity =>
        {
            entity.ToTable("tb_employee");

            entity.HasIndex(e => e.EmployeeTypeId, "IX_tb_employee_EmployeeTypeId");

            entity.HasIndex(e => new { e.IsDelete, e.Status }, "IX_tb_employee_IsDelete_Status");

            entity.HasIndex(e => e.PositionId, "IX_tb_employee_PositionId");

            entity.HasIndex(e => e.PositionLevelId, "IX_tb_employee_PositionLevelId");

            entity.HasIndex(e => e.BranchId, "idx_employee_branch").HasFilter("(\"IsDelete\" = false)");

            entity.HasIndex(e => e.CompanyId, "idx_employee_company").HasFilter("(\"IsDelete\" = false)");

            entity.HasIndex(e => e.DepartmentId, "idx_employee_department").HasFilter("(\"IsDelete\" = false)");

            entity.HasIndex(e => e.DivisionId, "idx_employee_division").HasFilter("(\"IsDelete\" = false)");

            entity.HasIndex(e => e.Email, "idx_employee_email_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.EmpId, "idx_employee_empid_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.EmployeeTypeId, "idx_employee_emptype").HasFilter("(\"IsDelete\" = false)");

            entity.HasIndex(e => e.FnameEn, "idx_employee_fnameen_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.FnameTh, "idx_employee_fnameth_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.GroupId, "idx_employee_group").HasFilter("(\"IsDelete\" = false)");

            entity.HasIndex(e => new { e.IsDelete, e.UpdatedAt }, "idx_employee_isdelete_updatedat").IsDescending(false, true);

            entity.HasIndex(e => e.LnameEn, "idx_employee_lnameen_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.LnameTh, "idx_employee_lnameth_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.SectionId, "idx_employee_section").HasFilter("(\"IsDelete\" = false)");

            entity.HasIndex(e => e.Status, "idx_employee_status").HasFilter("(\"IsDelete\" = false)");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.BranchId).HasDefaultValueSql("'00000000-0000-0000-0000-000000000000'::uuid");
            entity.Property(e => e.CompanyId).HasDefaultValueSql("'00000000-0000-0000-0000-000000000000'::uuid");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedAt)
                .HasDefaultValueSql("'-infinity'::timestamp with time zone")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedBy).HasDefaultValueSql("''::text");
            entity.Property(e => e.EmployeeTypeId).HasDefaultValueSql("'00000000-0000-0000-0000-000000000000'::uuid");
            entity.Property(e => e.FnameEn)
                .HasDefaultValueSql("''::text")
                .HasColumnName("FNameEn");
            entity.Property(e => e.FnameTh).HasColumnName("FNameTh");
            entity.Property(e => e.HireDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.LnameEn)
                .HasDefaultValueSql("''::text")
                .HasColumnName("LNameEn");
            entity.Property(e => e.LnameTh).HasColumnName("LNameTh");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Branch).WithMany(p => p.TbEmployees)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Company).WithMany(p => p.TbEmployees)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_employee_company");

            entity.HasOne(d => d.Department).WithMany(p => p.TbEmployeeDepartments)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK_employee_dept");

            entity.HasOne(d => d.Division).WithMany(p => p.TbEmployeeDivisions)
                .HasForeignKey(d => d.DivisionId)
                .HasConstraintName("FK_employee_div");

            entity.HasOne(d => d.EmployeeType).WithMany(p => p.TbEmployees)
                .HasForeignKey(d => d.EmployeeTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Group).WithMany(p => p.TbEmployeeGroups)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK_employee_group");

            entity.HasOne(d => d.Position).WithMany(p => p.TbEmployees)
                .HasForeignKey(d => d.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.PositionLevel).WithMany(p => p.TbEmployees)
                .HasForeignKey(d => d.PositionLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.ReportTo).WithMany(p => p.InverseReportTo)
                .HasForeignKey(d => d.ReportToId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Section).WithMany(p => p.TbEmployeeSections)
                .HasForeignKey(d => d.SectionId)
                .HasConstraintName("FK_employee_sec");
        });

        modelBuilder.Entity<TbEmployeeBenefitPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_employee_benefit_plan_pkey");

            entity.ToTable("tb_employee_benefit_plan");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasDefaultValueSql("'System'::text");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedBy).HasDefaultValueSql("'System'::text");

            entity.HasOne(d => d.BenefitPlan).WithMany(p => p.TbEmployeeBenefitPlans)
                .HasForeignKey(d => d.BenefitPlanId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_EmployeeBenefitPlan_BenefitPlan");

            entity.HasOne(d => d.Employee).WithMany(p => p.TbEmployeeBenefitPlans)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_EmployeeBenefitPlan_Employee");
        });

        modelBuilder.Entity<TbEmployeeType>(entity =>
        {
            entity.ToTable("tb_employee_type");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TbOrganizationLevelType>(entity =>
        {
            entity.ToTable("tb_organization_level_type");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("'-infinity'::timestamp with time zone")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasDefaultValueSql("''::text");
            entity.Property(e => e.DeletedAt)
                .HasDefaultValueSql("'-infinity'::timestamp with time zone")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedBy).HasDefaultValueSql("''::text");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("'-infinity'::timestamp with time zone")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedBy).HasDefaultValueSql("''::text");
        });

        modelBuilder.Entity<TbOrganizationUnit>(entity =>
        {
            entity.ToTable("tb_organization_unit");

            entity.HasIndex(e => e.Type, "idx_orgunit_type").HasFilter("(\"IsDelete\" = false)");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Type).HasDefaultValueSql("'Group'::text");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TbPodAdminBranch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_pod_admin_branch_pkey");

            entity.ToTable("tb_pod_admin_branch");

            entity.HasIndex(e => e.BranchId, "ux_pod_admin_branch_primary")
                .IsUnique()
                .HasFilter("((\"IsPrimary\" = true) AND (\"IsDelete\" = false))");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("'System'::character varying");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("'System'::character varying");

            entity.HasOne(d => d.Branch).WithOne(p => p.TbPodAdminBranch)
                .HasForeignKey<TbPodAdminBranch>(d => d.BranchId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_tb_pod_admin_branch_branch");
        });

        modelBuilder.Entity<TbPosition>(entity =>
        {
            entity.ToTable("tb_position");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TbPositionLevel>(entity =>
        {
            entity.ToTable("tb_position_level");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TbPrefix>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_prefix_pkey");

            entity.ToTable("tb_prefix");

            entity.HasIndex(e => e.DocType, "ux_prefix_doctype").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasDefaultValueSql("'System'::text");
            entity.Property(e => e.ResetPeriod).HasDefaultValueSql("'Daily'::text");
            entity.Property(e => e.RunningLength).HasDefaultValue(4);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedBy).HasDefaultValueSql("'System'::text");
        });

        modelBuilder.Entity<TbPrettyCashLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_pretty_cash_line_pkey");

            entity.ToTable("tb_pretty_cash_line");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasDefaultValueSql("'System'::text");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.LimitAmount).HasPrecision(18, 2);
            entity.Property(e => e.Qty)
                .HasPrecision(18, 2)
                .HasDefaultValue(1m);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedBy).HasDefaultValueSql("'System'::text");

            entity.HasOne(d => d.Benefit).WithMany(p => p.TbPrettyCashLines)
                .HasForeignKey(d => d.BenefitId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_pcline_benefit");

            entity.HasOne(d => d.PrettyCash).WithMany(p => p.TbPrettyCashLines)
                .HasForeignKey(d => d.PrettyCashId)
                .HasConstraintName("FK_pcline_request");
        });

        modelBuilder.Entity<TbPrettyCashRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_pretty_cash_request_pkey");

            entity.ToTable("tb_pretty_cash_request");

            entity.HasIndex(e => e.DocNum, "ux_pretty_cash_docnum").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasDefaultValueSql("'System'::text");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DocDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DocStatus).HasDefaultValueSql("'Draft'::text");
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedBy).HasDefaultValueSql("'System'::text");

            entity.HasOne(d => d.Employee).WithMany(p => p.TbPrettyCashRequests)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_pretty_cash_employee");
        });

        modelBuilder.Entity<TbReasonApprove>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_reason_approve_pkey");

            entity.ToTable("tb_reason_approve");

            entity.HasIndex(e => new { e.DocType, e.DocNumber, e.DocRev, e.Round }, "idx_reasonapprove_doc");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasDefaultValueSql("'System'::text");
            entity.Property(e => e.DocRev).HasDefaultValue(1);
            entity.Property(e => e.Round).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedBy).HasDefaultValueSql("'System'::text");
        });

        modelBuilder.Entity<TbRouteApprove>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_route_approve_pkey");

            entity.ToTable("tb_route_approve");

            entity.HasIndex(e => new { e.DocType, e.StepNo }, "ux_route_approve_step")
                .IsUnique()
                .HasFilter("(\"IsActive\" = true)");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasDefaultValueSql("'System'::text");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedBy).HasDefaultValueSql("'System'::text");
        });

        modelBuilder.Entity<TbTransApprove>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_trans_approve_pkey");

            entity.ToTable("tb_trans_approve");

            entity.HasIndex(e => new { e.DocType, e.DocNumber, e.DocRev, e.Round }, "idx_transapprove_doc");

            entity.Property(e => e.ApprovedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasDefaultValueSql("'System'::text");
            entity.Property(e => e.DocRev).HasDefaultValue(1);
            entity.Property(e => e.Round).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedBy).HasDefaultValueSql("'System'::text");
        });

        modelBuilder.Entity<VwPodAdminBranch>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_pod_admin_branch");

            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
