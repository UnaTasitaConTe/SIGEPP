namespace Application.Users.DTOs;

/// <summary>
/// DTO detallado de usuario con información completa.
/// Usado para consultas individuales donde se necesita más detalle.
/// </summary>
public sealed record UserDetailDto
{
    /// <summary>
    /// ID del usuario.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Email del usuario.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Indica si el usuario está activo.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Fecha de creación del usuario.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Fecha de última actualización.
    /// </summary>
    public required DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Información detallada de roles asignados.
    /// </summary>
    public required IReadOnlyCollection<UserRoleDto> Roles { get; init; }

    /// <summary>
    /// Permisos efectivos del usuario (unión de todos sus roles).
    /// </summary>
    public required IReadOnlyCollection<string> Permissions { get; init; }
}

/// <summary>
/// DTO de rol para el detalle de usuario.
/// </summary>
public sealed record UserRoleDto
{
    /// <summary>
    /// Código del rol (ej: "ADMIN", "DOCENTE").
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Nombre descriptivo del rol.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Descripción del rol.
    /// </summary>
    public required string Description { get; init; }
}
