using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeds;

/// <summary>
/// Seeder para permisos del sistema.
/// Define todos los permisos iniciales basados en el catálogo de Domain.
/// Los códigos deben coincidir exactamente con Domain.Security.Catalogs.Permissions.
/// </summary>
public static class PermissionSeed
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        var permissions = new List<PermissionEntity>();
        long id = 1;

        // ============================================
        // GESTIÓN ACADÉMICA - Períodos
        // ============================================
        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "periods.view",
            Module = "periods",
            Action = "view",
            Description = "Ver períodos académicos"
        });

        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "periods.create",
            Module = "periods",
            Action = "create",
            Description = "Crear períodos académicos"
        });

        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "periods.update",
            Module = "periods",
            Action = "update",
            Description = "Actualizar períodos académicos"
        });

        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "periods.deactivate",
            Module = "periods",
            Action = "deactivate",
            Description = "Desactivar períodos académicos"
        });

        // ============================================
        // GESTIÓN ACADÉMICA - Materias/Asignaturas
        // ============================================
        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "subjects.view",
            Module = "subjects",
            Action = "view",
            Description = "Ver materias/asignaturas"
        });

        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "subjects.create",
            Module = "subjects",
            Action = "create",
            Description = "Crear materias/asignaturas"
        });

        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "subjects.update",
            Module = "subjects",
            Action = "update",
            Description = "Actualizar materias/asignaturas"
        });

        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "subjects.deactivate",
            Module = "subjects",
            Action = "deactivate",
            Description = "Desactivar materias/asignaturas"
        });

        // ============================================
        // GESTIÓN ACADÉMICA - Asignación Docente-Materia
        // ============================================
        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "teacherSubjects.manage",
            Module = "teacherSubjects",
            Action = "manage",
            Description = "Gestionar asignación docente-materia"
        });

        // ============================================
        // PPA - Proyectos Académicos
        // ============================================
        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "ppa.view_all",
            Module = "ppa",
            Action = "view_all",
            Description = "Ver todos los PPAs"
        });

        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "ppa.view_own",
            Module = "ppa",
            Action = "view_own",
            Description = "Ver sus propios PPAs"
        });

        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "ppa.create",
            Module = "ppa",
            Action = "create",
            Description = "Crear PPAs"
        });

        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "ppa.update",
            Module = "ppa",
            Action = "update",
            Description = "Actualizar PPAs"
        });

        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "ppa.change_status",
            Module = "ppa",
            Action = "change_status",
            Description = "Cambiar estado de PPAs"
        });

        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "ppa.upload_file",
            Module = "ppa",
            Action = "upload_file",
            Description = "Subir archivos a PPAs"
        });

        // ============================================
        // RECURSOS/ANEXOS
        // ============================================
        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "resources.view_all",
            Module = "resources",
            Action = "view_all",
            Description = "Ver todos los recursos"
        });

        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "resources.view_own",
            Module = "resources",
            Action = "view_own",
            Description = "Ver sus propios recursos"
        });

        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "resources.create",
            Module = "resources",
            Action = "create",
            Description = "Crear recursos"
        });

        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "resources.update",
            Module = "resources",
            Action = "update",
            Description = "Actualizar recursos"
        });

        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "resources.delete",
            Module = "resources",
            Action = "delete",
            Description = "Eliminar recursos"
        });

        // ============================================
        // DASHBOARD/PANEL
        // ============================================
        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "dashboard.view",
            Module = "dashboard",
            Action = "view",
            Description = "Ver dashboard"
        });

        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "dashboard.view_details",
            Module = "dashboard",
            Action = "view_details",
            Description = "Ver detalles del dashboard"
        });

        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "teacherSubjects.view",
            Module = "teacherSubjects",
            Action = "view_all",
            Description = "Ver detalles del Gestion asignación docente-materia"
        });

        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "teacherSubjects.create",
            Module = "teacherSubjects",
            Action = "create",
            Description = "Crear asignación docente-materia"
        });

        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "teacherSubjects.update",
            Module = "teacherSubjects",
            Action = "update",
            Description = "Actualizar asignación docente-materia"
        });

        permissions.Add(new PermissionEntity
        {
            Id = id++,
            Code = "teacherSubjects.deactivate",
            Module = "teacherSubjects",
            Action = "deactivate",
            Description = "desactivar asignación docente-materia"
        });

        modelBuilder.Entity<PermissionEntity>().HasData(permissions);
    }
}
