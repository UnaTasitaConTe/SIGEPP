namespace Infrastructure.Persistence.Entities;

/// <summary>
/// Entity de EF Core que representa un usuario en la base de datos.
/// Mapea a la tabla Users.
/// </summary>
public class UserEntity
{
    /// <summary>
    /// Identificador único del usuario
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email del usuario (único en el sistema)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hash de la contraseña del usuario (nunca en texto plano)
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el usuario está activo en el sistema
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Fecha de creación del usuario
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha de última actualización del usuario
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    // Navegación many-to-many con Roles
    public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
}
