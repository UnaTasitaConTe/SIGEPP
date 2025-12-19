using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeds;

/// <summary>
/// Seeder para relaciones Role-Permission.
/// Define qué permisos tiene cada rol basándose en la matriz de permisos del dominio.
/// Referencia: Domain.Security.Catalogs.Roles para la asignación de permisos.
/// </summary>
public static class RolePermissionSeed
{
    // Constantes para IDs de permisos (deben coincidir con PermissionSeed)
    private const long PeriodsView = 1;
    private const long PeriodsCreate = 2;
    private const long PeriodsUpdate = 3;
    private const long PeriodsDeactivate = 4;
    private const long SubjectsView = 5;
    private const long SubjectsCreate = 6;
    private const long SubjectsUpdate = 7;
    private const long SubjectsDeactivate = 8;
    private const long TeacherSubjectsManage = 9;
    private const long PpaViewAll = 10;
    private const long PpaViewOwn = 11;
    private const long PpaCreate = 12;
    private const long PpaUpdate = 13;
    private const long PpaChangeStatus = 14;
    private const long PpaUploadFile = 15;
    private const long ResourcesViewAll = 16;
    private const long ResourcesViewOwn = 17;
    private const long ResourcesCreate = 18;
    private const long ResourcesUpdate = 19;
    private const long ResourcesDelete = 20;
    private const long DashboardView = 21;
    private const long DashboardViewDetails = 22;

    // Constantes para IDs de roles (deben coincidir con RoleSeed)
    private const long AdminRoleId = 1;
    private const long DocenteRoleId = 2;
    private const long ConsultaInternaRoleId = 3;

    public static void Seed(ModelBuilder modelBuilder)
    {
        var rolePermissions = new List<RolePermissionEntity>();

        // ============================================
        // ROL: ADMIN - 14 permisos
        // ============================================
        // Gestión Académica completa
        rolePermissions.Add(new RolePermissionEntity { RoleId = AdminRoleId, PermissionId = PeriodsView });
        rolePermissions.Add(new RolePermissionEntity { RoleId = AdminRoleId, PermissionId = PeriodsCreate });
        rolePermissions.Add(new RolePermissionEntity { RoleId = AdminRoleId, PermissionId = PeriodsUpdate });
        rolePermissions.Add(new RolePermissionEntity { RoleId = AdminRoleId, PermissionId = PeriodsDeactivate });
        rolePermissions.Add(new RolePermissionEntity { RoleId = AdminRoleId, PermissionId = SubjectsView });
        rolePermissions.Add(new RolePermissionEntity { RoleId = AdminRoleId, PermissionId = SubjectsCreate });
        rolePermissions.Add(new RolePermissionEntity { RoleId = AdminRoleId, PermissionId = SubjectsUpdate });
        rolePermissions.Add(new RolePermissionEntity { RoleId = AdminRoleId, PermissionId = SubjectsDeactivate });
        rolePermissions.Add(new RolePermissionEntity { RoleId = AdminRoleId, PermissionId = TeacherSubjectsManage });

        // PPA - Supervisión
        rolePermissions.Add(new RolePermissionEntity { RoleId = AdminRoleId, PermissionId = PpaViewAll });
        rolePermissions.Add(new RolePermissionEntity { RoleId = AdminRoleId, PermissionId = PpaUpdate });
        rolePermissions.Add(new RolePermissionEntity { RoleId = AdminRoleId, PermissionId = PpaChangeStatus });
        rolePermissions.Add(new RolePermissionEntity { RoleId = AdminRoleId, PermissionId = PpaUploadFile });

        // Recursos - Solo lectura
        rolePermissions.Add(new RolePermissionEntity { RoleId = AdminRoleId, PermissionId = ResourcesViewAll });

        // Dashboard - Completo
        rolePermissions.Add(new RolePermissionEntity { RoleId = AdminRoleId, PermissionId = DashboardView });
        rolePermissions.Add(new RolePermissionEntity { RoleId = AdminRoleId, PermissionId = DashboardViewDetails });

        // ============================================
        // ROL: DOCENTE - 10 permisos
        // ============================================
        // Materias - Solo lectura
        rolePermissions.Add(new RolePermissionEntity { RoleId = DocenteRoleId, PermissionId = SubjectsView });

        // PPA - Gestión propia
        rolePermissions.Add(new RolePermissionEntity { RoleId = DocenteRoleId, PermissionId = PpaViewOwn });
        rolePermissions.Add(new RolePermissionEntity { RoleId = DocenteRoleId, PermissionId = PpaCreate });
        rolePermissions.Add(new RolePermissionEntity { RoleId = DocenteRoleId, PermissionId = PpaUpdate });
        rolePermissions.Add(new RolePermissionEntity { RoleId = DocenteRoleId, PermissionId = PpaChangeStatus });
        rolePermissions.Add(new RolePermissionEntity { RoleId = DocenteRoleId, PermissionId = PpaUploadFile });

        // Recursos - CRUD propio
        rolePermissions.Add(new RolePermissionEntity { RoleId = DocenteRoleId, PermissionId = ResourcesViewOwn });
        rolePermissions.Add(new RolePermissionEntity { RoleId = DocenteRoleId, PermissionId = ResourcesCreate });
        rolePermissions.Add(new RolePermissionEntity { RoleId = DocenteRoleId, PermissionId = ResourcesUpdate });
        rolePermissions.Add(new RolePermissionEntity { RoleId = DocenteRoleId, PermissionId = ResourcesDelete });

        // ============================================
        // ROL: CONSULTA_INTERNA - 3 permisos
        // ============================================
        // PPA - Solo lectura completa
        rolePermissions.Add(new RolePermissionEntity { RoleId = ConsultaInternaRoleId, PermissionId = PpaViewAll });

        // Recursos - Solo lectura completa
        rolePermissions.Add(new RolePermissionEntity { RoleId = ConsultaInternaRoleId, PermissionId = ResourcesViewAll });

        // Dashboard - Básico
        rolePermissions.Add(new RolePermissionEntity { RoleId = ConsultaInternaRoleId, PermissionId = DashboardView });

        rolePermissions.Add(new RolePermissionEntity { RoleId = AdminRoleId, PermissionId = PpaCreate });
        // ✅ Catálogos académicos - Solo lectura
        rolePermissions.Add(new RolePermissionEntity { RoleId = ConsultaInternaRoleId, PermissionId = PeriodsView });
        rolePermissions.Add(new RolePermissionEntity { RoleId = ConsultaInternaRoleId, PermissionId = SubjectsView });

        // ✅ Períodos - Solo lectura (útil para formularios/selecciones)
        rolePermissions.Add(new RolePermissionEntity { RoleId = DocenteRoleId, PermissionId = PeriodsView });

        // ✅ Dashboard - Básico (si el docente ve panel)
        rolePermissions.Add(new RolePermissionEntity { RoleId = DocenteRoleId, PermissionId = DashboardView });

        // ✅ Recursos - Gestión completa (opcional si Admin debe administrar anexos)
        rolePermissions.Add(new RolePermissionEntity { RoleId = AdminRoleId, PermissionId = ResourcesCreate });
        rolePermissions.Add(new RolePermissionEntity { RoleId = AdminRoleId, PermissionId = ResourcesUpdate });
        rolePermissions.Add(new RolePermissionEntity { RoleId = AdminRoleId, PermissionId = ResourcesDelete });


        modelBuilder.Entity<RolePermissionEntity>().HasData(rolePermissions);
    }
}
