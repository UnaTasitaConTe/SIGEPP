using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeds;

/// <summary>
/// Seed principal para datos de seguridad (Roles, Permissions, RolePermissions).
/// Orquesta los seeders específicos y mantiene la responsabilidad única.
/// </summary>
public static class SecuritySeed
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        PermissionSeed.Seed(modelBuilder);
        RoleSeed.Seed(modelBuilder);
        RolePermissionSeed.Seed(modelBuilder);
        SubjectSeeder.Seed(modelBuilder);
    }
}
