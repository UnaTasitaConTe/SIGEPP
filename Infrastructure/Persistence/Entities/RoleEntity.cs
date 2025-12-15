namespace Infrastructure.Persistence.Entities;

/// <summary>
/// Entity de EF Core que representa un rol en la base de datos.
/// Mapea a la tabla Roles.
/// </summary>
public class RoleEntity
{
    /// <summary>
    /// Identificador único del rol
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Código único del rol (ej: "ADMIN", "DOCENTE", "CONSULTA_INTERNA")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Nombre descriptivo del rol
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del propósito y alcance del rol
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Indica si es un rol del sistema (no puede ser eliminado)
    /// </summary>
    public bool IsSystemRole { get; set; }

    // Navegación many-to-many con Permissions
    public ICollection<RolePermissionEntity> RolePermissions { get; set; } = new List<RolePermissionEntity>();

    // Navegación many-to-many con Users
    public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
}
