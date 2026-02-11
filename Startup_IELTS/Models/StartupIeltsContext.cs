using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Startup_IELTS.Models;

public partial class StartupIeltsContext : DbContext
{
    public StartupIeltsContext()
    {
    }

    public StartupIeltsContext(DbContextOptions<StartupIeltsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Writing> Writings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=startup_ielts;Username=postgres;Password=quenmkroi000;Ssl Mode=Disable;Trust Server Certificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Name, "roles_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(20)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("users_role_id_fkey");
        });

        modelBuilder.Entity<Writing>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("writings_pkey");

            entity.ToTable("writings");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.GradedAt).HasColumnName("graded_at");
            entity.Property(e => e.Hide)
                .HasDefaultValue(false)
                .HasColumnName("hide");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.Question).HasColumnName("question");
            entity.Property(e => e.Source).HasColumnName("source");
            entity.Property(e => e.SubmittedAt).HasColumnName("submitted_at");
            entity.Property(e => e.TaskType)
                .HasMaxLength(20)
                .HasColumnName("task_type");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Type).HasColumnName("type");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
