using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeds;

/// <summary>
/// Seeder para roles del sistema.
/// Define los roles iniciales basados en el catálogo de Domain.
/// Los códigos deben coincidir exactamente con Domain.Security.Catalogs.Roles.
/// </summary>
public static class RoleSeed
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoleEntity>().HasData(
            // ============================================
            // ROL: ADMIN
            // ============================================
            new RoleEntity
            {
                Id = 1,
                Code = "ADMIN",
                Name = "Administrador",
                Description = "Acceso completo a gestión académica (períodos, materias, asignaciones), supervisión de PPAs y dashboard completo.",
                IsSystemRole = true
            },

            // ============================================
            // ROL: DOCENTE
            // ============================================
            new RoleEntity
            {
                Id = 2,
                Code = "DOCENTE",
                Name = "Docente",
                Description = "Gestiona sus propios PPAs y recursos académicos, visualiza materias asignadas.",
                IsSystemRole = true
            },

            // ============================================
            // ROL: CONSULTA_INTERNA
            // ============================================
            new RoleEntity
            {
                Id = 3,
                Code = "CONSULTA_INTERNA",
                Name = "Consulta Interna",
                Description = "Acceso de solo lectura para consulta y auditoría de PPAs, recursos y dashboard.",
                IsSystemRole = true
            }
        );
    }
}
