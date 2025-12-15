namespace Application.Security;

/// <summary>
/// Representa el usuario autenticado actualmente en el sistema.
/// Contiene la información del usuario extraída del token JWT.
/// </summary>
public sealed record CurrentUser
{
    /// <summary>
    /// ID único del usuario autenticado.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Email del usuario.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Códigos de roles asignados al usuario (ej: "ADMIN", "DOCENTE").
    /// </summary>
    public required IReadOnlyCollection<string> Roles { get; init; }

    /// <summary>
    /// Permisos efectivos del usuario (unión de permisos de todos sus roles).
    /// Formato: "module.action" (ej: "ppa.create", "subjects.view").
    /// </summary>
    public required IReadOnlyCollection<string> Permissions { get; init; }
}
