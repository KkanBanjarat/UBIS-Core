using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UBIS.Access.Domain.Entities;

namespace UBIS.Access.Infrastructure.Data;

public partial class AccessDbContext : DbContext
{
    public AccessDbContext()
    {
    }

    public AccessDbContext(DbContextOptions<AccessDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TbPermission> TbPermissions { get; set; }

    public virtual DbSet<TbRole> TbRoles { get; set; }

    public virtual DbSet<TbRolePermission> TbRolePermissions { get; set; }

    public virtual DbSet<TbUser> TbUsers { get; set; }

    public virtual DbSet<TbUserRole> TbUserRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Name=ConnectionStrings:AccessDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TbPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_permission_pkey");

            entity.ToTable("tb_permission");

            entity.HasIndex(e => e.Code, "uq_tb_permission_code").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedBy).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedBy).HasMaxLength(255);
        });

        modelBuilder.Entity<TbRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_role_pkey");

            entity.ToTable("tb_role");

            entity.HasIndex(e => e.Name, "uq_tb_role_name").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedBy).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedBy).HasMaxLength(255);
        });

        modelBuilder.Entity<TbRolePermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_role_permission_pkey");

            entity.ToTable("tb_role_permission");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedBy).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedBy).HasMaxLength(255);

            entity.HasOne(d => d.Permission).WithMany(p => p.TbRolePermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_tb_role_permission_permission");

            entity.HasOne(d => d.Role).WithMany(p => p.TbRolePermissions)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_tb_role_permission_role");
        });

        modelBuilder.Entity<TbUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_user_pkey");

            entity.ToTable("tb_user");

            entity.HasIndex(e => e.Email, "tb_user_email_unique").IsUnique();

            entity.HasIndex(e => e.EntraObjectId, "tb_user_entra_object_id_unique").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("'System'::character varying");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedBy).HasMaxLength(100);
            entity.Property(e => e.DisplayName).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.EmployeeCode).HasMaxLength(100);
            entity.Property(e => e.EntraObjectId).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastLoginAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("'System'::character varying");
        });

        modelBuilder.Entity<TbUserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tb_user_role_pkey");

            entity.ToTable("tb_user_role");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedBy).HasMaxLength(255);
            entity.Property(e => e.Scope).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedBy).HasMaxLength(255);

            entity.HasOne(d => d.Role).WithMany(p => p.TbUserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_tb_user_role_role");

            entity.HasOne(d => d.User).WithMany(p => p.TbUserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_tb_user_role_user");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
