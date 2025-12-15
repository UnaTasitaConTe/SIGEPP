namespace Infrastructure.Persistence.Entities;

/// <summary>
/// Entity de EF Core que representa la relación many-to-many entre Roles y Permissions.
/// Mapea a la tabla RolePermissions.
/// </summary>
public class RolePermissionEntity
{
    /// <summary>
    /// ID del rol (FK)
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    /// ID del permiso (FK)
    /// </summary>
    public long PermissionId { get; set; }

    // Navegación
    public RoleEntity Role { get; set; } = null!;
    public PermissionEntity Permission { get; set; } = null!;
}
