namespace Infrastructure.Persistence.Entities;

/// <summary>
/// Entity de EF Core para la relación many-to-many entre Users y Roles.
/// Mapea a la tabla UserRoles.
/// </summary>
public class UserRoleEntity
{
    /// <summary>
    /// ID del usuario (FK a Users.Id)
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// ID del rol (FK a Roles.Id)
    /// </summary>
    public long RoleId { get; set; }

    // Navegación hacia User
    public UserEntity User { get; set; } = null!;

    // Navegación hacia Role
    public RoleEntity Role { get; set; } = null!;
}
