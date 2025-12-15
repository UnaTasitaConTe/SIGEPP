using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeds;

/// <summary>
/// Seed para datos iniciales de usuarios.
/// IMPORTANTE: Este seed es solo para entornos de desarrollo.
/// NO debe usarse en producción sin cambiar las credenciales.
/// </summary>
public static class UserSeed
{
    /// <summary>
    /// ID del usuario administrador inicial
    /// </summary>
    private static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    /// <summary>
    /// ID del rol ADMIN (debe coincidir con el ID del seed de RoleSeed)
    /// </summary>
    private const long AdminRoleId = 1L;

    public static void Seed(ModelBuilder modelBuilder)
    {
        SeedUsers(modelBuilder);
        SeedUserRoles(modelBuilder);
    }

    /// <summary>
    /// Crea el usuario administrador inicial para desarrollo.
    /// </summary>
    private static void SeedUsers(ModelBuilder modelBuilder)
    {
        // NOTA IMPORTANTE: Este hash corresponde a la contraseña "Admin123!"
        // Generado con BCrypt usando WorkFactor 12
        // DEBE SER CAMBIADO EN PRODUCCIÓN por el usuario después del primer login
        const string defaultPasswordHash = "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIr.5gPyOu";

        var adminUser = new UserEntity
        {
            Id = AdminUserId,
            Name = "Administrador SIGEPP",
            Email = "admin@sigepp.local",
            PasswordHash = defaultPasswordHash,
            IsActive = true,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        modelBuilder.Entity<UserEntity>().HasData(adminUser);
    }

    /// <summary>
    /// Asigna el rol ADMIN al usuario administrador inicial.
    /// </summary>
    private static void SeedUserRoles(ModelBuilder modelBuilder)
    {
        var adminUserRole = new UserRoleEntity
        {
            UserId = AdminUserId,
            RoleId = AdminRoleId
        };

        modelBuilder.Entity<UserRoleEntity>().HasData(adminUserRole);
    }
}

/*
 * ===============================================================
 * INFORMACIÓN DE SEGURIDAD - SOLO PARA DESARROLLO
 * ===============================================================
 *
 * Usuario por defecto:
 *   Email: admin@sigepp.local
 *   Password: Admin123!
 *
 * ⚠️ ADVERTENCIAS IMPORTANTES:
 *
 * 1. Este usuario y contraseña son SOLO para desarrollo/testing
 * 2. NUNCA usar estas credenciales en producción
 * 3. En producción, crear usuarios mediante la API con contraseñas seguras
 * 4. El primer paso en producción debe ser cambiar o eliminar este usuario
 * 5. Considerar deshabilitar este seed en appsettings.Production.json
 *
 * Generación del hash (para referencia):
 *   - Algoritmo: BCrypt
 *   - Work Factor: 12
 *   - Comando (usando BCrypt.Net-Next):
 *     var hash = BCrypt.Net.BCrypt.HashPassword("Admin123!", 12);
 *
 * ===============================================================
 */
