using System.ComponentModel.DataAnnotations;

namespace Application.Auth.Commands;

/// <summary>
/// Comando para autenticar un usuario en el sistema.
/// </summary>
public sealed record LoginCommand
{
    /// <summary>
    /// Email del usuario.
    /// </summary>
    [Required(ErrorMessage = "El email es requerido.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    public required string Email { get; init; }

    /// <summary>
    /// Contraseña en texto plano.
    /// </summary>
    [Required(ErrorMessage = "La contraseña es requerida.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "La contraseña es requerida.")]
    public required string Password { get; init; }
}
