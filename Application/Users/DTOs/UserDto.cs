namespace Application.Users.DTOs;

/// <summary>
/// DTO básico de usuario para respuestas de la API.
/// Contiene información esencial sin exponer el modelo de dominio.
/// </summary>
public sealed record UserDto
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
    /// Códigos de roles asignados al usuario.
    /// </summary>
    public required IReadOnlyCollection<string> Roles { get; init; }
}
