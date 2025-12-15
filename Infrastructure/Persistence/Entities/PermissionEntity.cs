namespace Infrastructure.Persistence.Entities;

/// <summary>
/// Entity de EF Core que representa un permiso en la base de datos.
/// Mapea a la tabla Permissions.
/// </summary>
public class PermissionEntity
{
    /// <summary>
    /// Identificador único del permiso
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Código único del permiso (ej: "ppa.create", "subjects.view")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Módulo al que pertenece el permiso (ej: "ppa", "subjects")
    /// </summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// Acción que representa el permiso (ej: "create", "view")
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Descripción opcional del permiso
    /// </summary>
    public string? Description { get; set; }

    // Navegación many-to-many con Roles
    public ICollection<RolePermissionEntity> RolePermissions { get; set; } = new List<RolePermissionEntity>();
}
