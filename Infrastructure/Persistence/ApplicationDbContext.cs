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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configuraciones Fluent API
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());

        // Aplicar seeds
        SecuritySeed.Seed(modelBuilder);
    }
}
