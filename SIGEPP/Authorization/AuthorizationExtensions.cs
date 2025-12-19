using Microsoft.AspNetCore.Authorization;

namespace SIGEPP.Authorization;

/// <summary>
/// Extensiones para configurar autorización basada en permisos en SIGEPP.
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Registra las políticas de autorización basadas en permisos para SIGEPP.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <returns>Colección de servicios para encadenamiento.</returns>
    /// <remarks>
    /// Este método registra:
    /// - El handler de autorización por permisos (<see cref="PermissionAuthorizationHandler"/>).
    /// - Políticas de autorización predefinidas para los módulos del sistema.
    ///
    /// Las políticas están alineadas con los permisos definidos en Domain.Security.Catalogs.Permissions.
    ///
    /// Uso en controllers:
    /// <code>
    /// [Authorize(Policy = "Periods.View")]
    /// public async Task&lt;IActionResult&gt; GetAll() { ... }
    ///
    /// [Authorize(Policy = "Ppa.Create")]
    /// public async Task&lt;IActionResult&gt; Create([FromBody] CreatePpaCommand command) { ... }
    /// </code>
    /// </remarks>
    public static IServiceCollection AddSigeppAuthorizationPolicies(this IServiceCollection services)
    {
        // Registrar el handler de autorización por permisos
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // Configurar políticas de autorización
        services.AddAuthorization(options =>
        {
            // ============================================
            // MÓDULO: Períodos Académicos
            // ============================================

            /// <summary>
            /// Política: Permite ver/listar períodos académicos.
            /// Permiso requerido: "periods.view"
            /// Uso: [Authorize(Policy = "Periods.View")]
            /// </summary>
            options.AddPolicy("Periods.View", policy =>
                policy.Requirements.Add(new PermissionRequirement("periods.view")));

            /// <summary>
            /// Política: Permite crear períodos académicos.
            /// Permiso requerido: "periods.create"
            /// Uso: [Authorize(Policy = "Periods.Create")]
            /// </summary>
            options.AddPolicy("Periods.Create", policy =>
                policy.Requirements.Add(new PermissionRequirement("periods.create")));

            /// <summary>
            /// Política: Permite actualizar períodos académicos.
            /// Permiso requerido: "periods.update"
            /// Uso: [Authorize(Policy = "Periods.Update")]
            /// </summary>
            options.AddPolicy("Periods.Update", policy =>
                policy.Requirements.Add(new PermissionRequirement("periods.update")));

            /// <summary>
            /// Política: Permite desactivar períodos académicos.
            /// Permiso requerido: "periods.deactivate"
            /// Uso: [Authorize(Policy = "Periods.Deactivate")]
            /// </summary>
            options.AddPolicy("Periods.Deactivate", policy =>
                policy.Requirements.Add(new PermissionRequirement("periods.deactivate")));

            // ============================================
            // MÓDULO: Asignaturas/Materias
            // ============================================

            /// <summary>
            /// Política: Permite ver/listar asignaturas.
            /// Permiso requerido: "subjects.view"
            /// Uso: [Authorize(Policy = "Subjects.View")]
            /// </summary>
            options.AddPolicy("Subjects.View", policy =>
                policy.Requirements.Add(new PermissionRequirement("subjects.view")));

            /// <summary>
            /// Política: Permite crear asignaturas.
            /// Permiso requerido: "subjects.create"
            /// Uso: [Authorize(Policy = "Subjects.Create")]
            /// </summary>
            options.AddPolicy("Subjects.Create", policy =>
                policy.Requirements.Add(new PermissionRequirement("subjects.create")));

            /// <summary>
            /// Política: Permite actualizar asignaturas.
            /// Permiso requerido: "subjects.update"
            /// Uso: [Authorize(Policy = "Subjects.Update")]
            /// </summary>
            options.AddPolicy("Subjects.Update", policy =>
                policy.Requirements.Add(new PermissionRequirement("subjects.update")));

            /// <summary>
            /// Política: Permite desactivar asignaturas.
            /// Permiso requerido: "subjects.deactivate"
            /// Uso: [Authorize(Policy = "Subjects.Deactivate")]
            /// </summary>
            options.AddPolicy("Subjects.Deactivate", policy =>
                policy.Requirements.Add(new PermissionRequirement("subjects.deactivate")));

            // ============================================
            // MÓDULO: Asignación Docente-Materia
            // ============================================

            /// <summary>
            /// Política: Permite gestionar asignaciones docente-materia.
            /// Permiso requerido: "teacherSubjects.manage"
            /// Uso: [Authorize(Policy = "TeacherSubjects.Manage")]
            /// </summary>
            options.AddPolicy("TeacherSubjects.Manage", policy =>
                policy.Requirements.Add(new PermissionRequirement("teacherSubjects.manage")));

            // ============================================
            // MÓDULO: PPA (Proyectos Académicos)
            // ============================================

            /// <summary>
            /// Política: Permite ver todas las PPAs del sistema.
            /// Permiso requerido: "ppa.view_all"
            /// Uso: [Authorize(Policy = "Ppa.ViewAll")]
            /// </summary>
            options.AddPolicy("Ppa.ViewAll", policy =>
                policy.Requirements.Add(new PermissionRequirement("ppa.view_all")));

            /// <summary>
            /// Política: Permite ver PPAs propias (solo las del docente autenticado).
            /// Permiso requerido: "ppa.view_own"
            /// Uso: [Authorize(Policy = "Ppa.ViewOwn")]
            /// </summary>
            options.AddPolicy("Ppa.ViewOwn", policy =>
                policy.Requirements.Add(new PermissionRequirement("ppa.view_own")));

            /// <summary>
            /// Política: Permite crear nuevas PPAs.
            /// Permiso requerido: "ppa.create"
            /// Uso: [Authorize(Policy = "Ppa.Create")]
            /// </summary>
            options.AddPolicy("Ppa.Create", policy =>
                policy.Requirements.Add(new PermissionRequirement("ppa.create")));

            /// <summary>
            /// Política: Permite actualizar PPAs.
            /// Permiso requerido: "ppa.update"
            /// Uso: [Authorize(Policy = "Ppa.Update")]
            /// </summary>
            options.AddPolicy("Ppa.Update", policy =>
                policy.Requirements.Add(new PermissionRequirement("ppa.update")));

            /// <summary>
            /// Política: Permite cambiar el estado de PPAs.
            /// Permiso requerido: "ppa.change_status"
            /// Uso: [Authorize(Policy = "Ppa.ChangeStatus")]
            /// </summary>
            options.AddPolicy("Ppa.ChangeStatus", policy =>
                policy.Requirements.Add(new PermissionRequirement("ppa.change_status")));

            /// <summary>
            /// Política: Permite subir archivos a PPAs.
            /// Permiso requerido: "ppa.upload_file"
            /// Uso: [Authorize(Policy = "Ppa.UploadFile")]
            /// </summary>
            options.AddPolicy("Ppa.UploadFile", policy =>
                policy.Requirements.Add(new PermissionRequirement("ppa.upload_file")));

            // ============================================
            // MÓDULO: Recursos/Anexos Académicos
            // ============================================

            /// <summary>
            /// Política: Permite ver todos los recursos del sistema.
            /// Permiso requerido: "resources.view_all"
            /// Uso: [Authorize(Policy = "Resources.ViewAll")]
            /// </summary>
            options.AddPolicy("Resources.ViewAll", policy =>
                policy.Requirements.Add(new PermissionRequirement("resources.view_all")));

            /// <summary>
            /// Política: Permite ver recursos propios.
            /// Permiso requerido: "resources.view_own"
            /// Uso: [Authorize(Policy = "Resources.ViewOwn")]
            /// </summary>
            options.AddPolicy("Resources.ViewOwn", policy =>
                policy.Requirements.Add(new PermissionRequirement("resources.view_own")));

            /// <summary>
            /// Política: Permite crear/subir nuevos recursos.
            /// Permiso requerido: "resources.create"
            /// Uso: [Authorize(Policy = "Resources.Create")]
            /// </summary>
            options.AddPolicy("Resources.Create", policy =>
                policy.Requirements.Add(new PermissionRequirement("resources.create")));

            /// <summary>
            /// Política: Permite actualizar recursos existentes.
            /// Permiso requerido: "resources.update"
            /// Uso: [Authorize(Policy = "Resources.Update")]
            /// </summary>
            options.AddPolicy("Resources.Update", policy =>
                policy.Requirements.Add(new PermissionRequirement("resources.update")));

            /// <summary>
            /// Política: Permite eliminar recursos.
            /// Permiso requerido: "resources.delete"
            /// Uso: [Authorize(Policy = "Resources.Delete")]
            /// </summary>
            options.AddPolicy("Resources.Delete", policy =>
                policy.Requirements.Add(new PermissionRequirement("resources.delete")));

            // ============================================
            // MÓDULO: Panel/Dashboard
            // ============================================

            /// <summary>
            /// Política: Permite ver el dashboard básico.
            /// Permiso requerido: "dashboard.view"
            /// Uso: [Authorize(Policy = "Dashboard.View")]
            /// </summary>
            options.AddPolicy("Dashboard.View", policy =>
                policy.Requirements.Add(new PermissionRequirement("dashboard.view")));

            /// <summary>
            /// Política: Permite ver detalles completos del dashboard.
            /// Permiso requerido: "dashboard.view_details"
            /// Uso: [Authorize(Policy = "Dashboard.ViewDetails")]
            /// </summary>
            options.AddPolicy("Dashboard.ViewDetails", policy =>
                policy.Requirements.Add(new PermissionRequirement("dashboard.view_details")));

            options.AddPolicy("Ppa.View", policy =>
                policy.RequireAssertion(ctx =>
                    ctx.User.HasClaim("permission", "ppa.view_all") ||
                    ctx.User.HasClaim("permission", "ppa.view_own")));

            options.AddPolicy("Resources.View", policy =>
                policy.RequireAssertion(ctx =>
                    ctx.User.HasClaim("permission", "resources.view_all") ||
                    ctx.User.HasClaim("permission", "resources.view_own")));


        });

        return services;
    }
}
