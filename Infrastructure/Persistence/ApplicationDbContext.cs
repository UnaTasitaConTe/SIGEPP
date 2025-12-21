using Infrastructure.Persistence.Configurations;
using Infrastructure.Persistence.Configurations.Academics;
using Infrastructure.Persistence.Configurations.Ppa;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Entities.Academics;
using Infrastructure.Persistence.Entities.Ppa;
using Infrastructure.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// Contexto de base de datos de EF Core para la aplicación FESC-PPA Hub.
/// Maneja todas las entidades del sistema y sus configuraciones.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets - Seguridad
    public DbSet<RoleEntity> Roles => Set<RoleEntity>();
    public DbSet<PermissionEntity> Permissions => Set<PermissionEntity>();
    public DbSet<RolePermissionEntity> RolePermissions => Set<RolePermissionEntity>();

    // DbSets - Usuarios
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<UserRoleEntity> UserRoles => Set<UserRoleEntity>();

    // DbSets - Académico
    public DbSet<AcademicPeriodEntity> AcademicPeriods => Set<AcademicPeriodEntity>();
    public DbSet<SubjectEntity> Subjects => Set<SubjectEntity>();
    public DbSet<TeacherAssignmentEntity> TeacherAssignments => Set<TeacherAssignmentEntity>();

    // DbSets - PPA
    public DbSet<PpaEntity> Ppas => Set<PpaEntity>();
    public DbSet<PpaAttachmentEntity> PpaAttachments => Set<PpaAttachmentEntity>();
    public DbSet<PpaTeacherAssignmentEntity> PpaTeacherAssignments => Set<PpaTeacherAssignmentEntity>();
    public DbSet<PpaStudentEntity> PpaStudents => Set<PpaStudentEntity>();
    public DbSet<Domain.Ppa.Entities.PpaHistoryEntry> PpaHistoryEntries => Set<Domain.Ppa.Entities.PpaHistoryEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configuraciones Fluent API - Seguridad
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());

        // Aplicar configuraciones Fluent API - Usuarios
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());

        // Aplicar configuraciones Fluent API - Académico
        modelBuilder.ApplyConfiguration(new AcademicPeriodConfiguration());
        modelBuilder.ApplyConfiguration(new SubjectConfiguration());
        modelBuilder.ApplyConfiguration(new TeacherAssignmentConfiguration());

        // Aplicar configuraciones Fluent API - PPA
        modelBuilder.ApplyConfiguration(new PpaConfiguration());
        modelBuilder.ApplyConfiguration(new PpaAttachmentConfiguration());
        modelBuilder.ApplyConfiguration(new PpaTeacherAssignmentConfiguration());
        modelBuilder.ApplyConfiguration(new PpaStudentConfiguration());
        modelBuilder.ApplyConfiguration(new PpaHistoryEntryConfiguration());

        // Aplicar seeds
        SecuritySeed.Seed(modelBuilder);
        UserSeed.Seed(modelBuilder);
    }
}
