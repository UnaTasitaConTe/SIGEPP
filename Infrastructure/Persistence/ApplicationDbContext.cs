using Infrastructure.Persistence.Configurations;
using Infrastructure.Persistence.Entities;
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

        // Aplicar seeds
        SecuritySeed.Seed(modelBuilder);
        UserSeed.Seed(modelBuilder);
    }
}
