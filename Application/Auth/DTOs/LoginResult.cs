namespace Application.Auth.DTOs;

/// <summary>
/// Resultado de una autenticación exitosa.
/// </summary>
public sealed record LoginResult
{
    /// <summary>
    /// Token JWT generado.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// ID del usuario autenticado.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Email del usuario.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Roles del usuario.
    /// </summary>
    public required IReadOnlyCollection<string> Roles { get; init; }

    /// <summary>
    /// Fecha de expiración del token.
    /// </summary>
    public required DateTime ExpiresAt { get; init; }
}
