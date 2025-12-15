using Domain.Security.Entities;

namespace Domain.Security.Catalogs;

/// <summary>
/// Catálogo de roles predefinidos del sistema FESC-PPA Hub.
/// Define los roles estándar con sus permisos correspondientes.
/// Esta clase actúa como parte del lenguaje ubicuo del dominio.
/// </summary>
public static class Roles
{
    // ========================================
    // IDs de Roles (constantes para referencia)
    // ========================================
    public const string AdminId = "ADMIN";
    public const string DocenteId = "DOCENTE";
    public const string ConsultaInternaId = "CONSULTA_INTERNA";

    // ========================================
    // ADMIN
    // Acceso completo a gestión académica y supervisión
    // ========================================
    public static readonly Role Admin = Role.Create(
        code: AdminId,
        name: "Administrador",
        description: "Acceso completo a gestión académica (períodos, materias, asignaciones), supervisión de PPAs y dashboard completo.",
        isSystemRole: true,
        permissions: new[]
        {
            // Gestión Académica: Períodos (CRUD completo)
            Permissions.Periods.View,
            Permissions.Periods.Create,
            Permissions.Periods.Update,
            Permissions.Periods.Deactivate,

            // Gestión Académica: Materias (CRUD completo)
            Permissions.Subjects.View,
            Permissions.Subjects.Create,
            Permissions.Subjects.Update,
            Permissions.Subjects.Deactivate,

            // Gestión Académica: Asignación Docente-Materia
            Permissions.TeacherSubjects.Manage,

            // PPA: Supervisión y control
            Permissions.PPA.ViewAll,
            Permissions.PPA.Update,
            Permissions.PPA.ChangeStatus,
            Permissions.PPA.UploadFile,

            // Recursos: Solo visualización
            Permissions.Resources.ViewAll,

            // Dashboard: Acceso completo
            Permissions.Dashboard.View,
            Permissions.Dashboard.ViewDetails
        }
    );

    // ========================================
    // DOCENTE
    // Gestión de sus propios PPAs y recursos
    // ========================================
    public static readonly Role Docente = Role.Create(
        code: DocenteId,
        name: "Docente",
        description: "Gestiona sus propios PPAs y recursos académicos, visualiza materias asignadas.",
        isSystemRole: true,
        permissions: new[]
        {
            // Gestión Académica: Solo visualizar materias
            Permissions.Subjects.View,

            // PPA: Gestión de sus propios proyectos
            Permissions.PPA.ViewOwn,
            Permissions.PPA.Create,
            Permissions.PPA.Update,
            Permissions.PPA.ChangeStatus,
            Permissions.PPA.UploadFile,

            // Recursos: CRUD de sus propios recursos
            Permissions.Resources.ViewOwn,
            Permissions.Resources.Create,
            Permissions.Resources.Update,
            Permissions.Resources.Delete
        }
    );

    // ========================================
    // CONSULTA_INTERNA
    // Solo lectura de PPAs, recursos y dashboard
    // ========================================
    public static readonly Role ConsultaInterna = Role.Create(
        code: ConsultaInternaId,
        name: "Consulta Interna",
        description: "Acceso de solo lectura para consulta y auditoría de PPAs, recursos y dashboard.",
        isSystemRole: true,
        permissions: new[]
        {
            // PPA: Solo visualización completa
            Permissions.PPA.ViewAll,

            // Recursos: Solo visualización completa
            Permissions.Resources.ViewAll,

            // Dashboard: Solo visualización básica
            Permissions.Dashboard.View
        }
    );

    // ========================================
    // Colección de todos los roles predefinidos
    // ========================================
    public static IReadOnlyList<Role> All => new[]
    {
        Admin,
        Docente,
        ConsultaInterna
    };

    /// <summary>
    /// Obtiene un rol por su ID
    /// </summary>
    public static Role? GetById(long roleId)
    {
        return All.FirstOrDefault(r => r.Id.Equals(roleId));
    }

    /// <summary>
    /// Verifica si un ID de rol existe en el catálogo
    /// </summary>
    public static bool Exists(long roleId)
    {
        return All.Any(r => r.Id.Equals(roleId));
    }

    /// <summary>
    /// Obtiene los nombres de todos los roles
    /// </summary>
    public static IReadOnlyList<string> GetAllNames()
    {
        return All.Select(r => r.Name).ToList().AsReadOnly();
    }

    /// <summary>
    /// Obtiene los IDs de todos los roles
    /// </summary>
    public static IReadOnlyList<long> GetAllIds()
    {
        return All.Select(r => r.Id).ToList().AsReadOnly();
    }
}
