using Domain.Security.ValueObjects;

namespace Domain.Security.Catalogs;

/// <summary>
/// Catálogo de permisos del sistema FESC-PPA Hub.
/// Define todos los permisos disponibles organizados por módulo.
/// Esta clase actúa como parte del lenguaje ubicuo del dominio.
/// </summary>
public static class Permissions
{
    // ========================================
    // MÓDULO: Períodos Académicos
    // ========================================
    public static class Periods
    {
        public static readonly Permission View = Permission.Create("periods.view");
        public static readonly Permission Create = Permission.Create("periods.create");
        public static readonly Permission Update = Permission.Create("periods.update");
        public static readonly Permission Deactivate = Permission.Create("periods.deactivate");

        public static IReadOnlyList<Permission> All => new[]
        {
            View, Create, Update, Deactivate
        };
    }

    // ========================================
    // MÓDULO: Asignaturas/Materias
    // ========================================
    public static class Subjects
    {
        public static readonly Permission View = Permission.Create("subjects.view");
        public static readonly Permission Create = Permission.Create("subjects.create");
        public static readonly Permission Update = Permission.Create("subjects.update");
        public static readonly Permission Deactivate = Permission.Create("subjects.deactivate");

        public static IReadOnlyList<Permission> All => new[]
        {
            View, Create, Update, Deactivate
        };
    }

    // ========================================
    // MÓDULO: Asignación Docente-Materia
    // ========================================
    public static class TeacherSubjects
    {
        public static readonly Permission Manage = Permission.Create("teacherSubjects.manage");

        public static IReadOnlyList<Permission> All => new[]
        {
            Manage
        };
    }

    // ========================================
    // MÓDULO: PPA (Proyectos Académicos)
    // ========================================
    public static class PPA
    {
        public static readonly Permission ViewAll = Permission.Create("ppa.view_all");
        public static readonly Permission ViewOwn = Permission.Create("ppa.view_own");
        public static readonly Permission Create = Permission.Create("ppa.create");
        public static readonly Permission Update = Permission.Create("ppa.update");
        public static readonly Permission ChangeStatus = Permission.Create("ppa.change_status");
        public static readonly Permission UploadFile = Permission.Create("ppa.upload_file");

        public static IReadOnlyList<Permission> All => new[]
        {
            ViewAll, ViewOwn, Create, Update, ChangeStatus, UploadFile
        };
    }

    // ========================================
    // MÓDULO: Recursos/Anexos
    // ========================================
    public static class Resources
    {
        public static readonly Permission ViewAll = Permission.Create("resources.view_all");
        public static readonly Permission ViewOwn = Permission.Create("resources.view_own");
        public static readonly Permission Create = Permission.Create("resources.create");
        public static readonly Permission Update = Permission.Create("resources.update");
        public static readonly Permission Delete = Permission.Create("resources.delete");

        public static IReadOnlyList<Permission> All => new[]
        {
            ViewAll, ViewOwn, Create, Update, Delete
        };
    }

    // ========================================
    // MÓDULO: Panel/Dashboard
    // ========================================
    public static class Dashboard
    {
        public static readonly Permission View = Permission.Create("dashboard.view");
        public static readonly Permission ViewDetails = Permission.Create("dashboard.view_details");

        public static IReadOnlyList<Permission> All => new[]
        {
            View, ViewDetails
        };
    }

    // ========================================
    // Colección de todos los permisos del sistema
    // ========================================
    public static IReadOnlyList<Permission> All
    {
        get
        {
            var allPermissions = new List<Permission>();
            allPermissions.AddRange(Periods.All);
            allPermissions.AddRange(Subjects.All);
            allPermissions.AddRange(TeacherSubjects.All);
            allPermissions.AddRange(PPA.All);
            allPermissions.AddRange(Resources.All);
            allPermissions.AddRange(Dashboard.All);
            return allPermissions.AsReadOnly();
        }
    }

    /// <summary>
    /// Obtiene todos los permisos de un módulo específico
    /// </summary>
    public static IReadOnlyList<Permission> GetByModule(string moduleName)
    {
        return moduleName.ToLowerInvariant() switch
        {
            "periods" => Periods.All,
            "subjects" => Subjects.All,
            "teachersubjects" => TeacherSubjects.All,
            "ppa" => PPA.All,
            "resources" => Resources.All,
            "dashboard" => Dashboard.All,
            _ => Array.Empty<Permission>()
        };
    }
}
