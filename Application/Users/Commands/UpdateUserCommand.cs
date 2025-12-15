using System.ComponentModel.DataAnnotations;

namespace Application.Users.Commands;

/// <summary>
/// Comando para actualizar un usuario existente.
/// </summary>
public class UpdateUserCommand
{
    /// <summary>
    /// ID del usuario a actualizar.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nuevo nombre del usuario.
    /// </summary>
    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 200 caracteres.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Nuevo email del usuario.
    /// </summary>
    [Required(ErrorMessage = "El email es requerido.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    [StringLength(256, ErrorMessage = "El email no puede exceder 256 caracteres.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Códigos de roles a asignar al usuario (reemplaza los roles existentes).
    /// Si es null o vacío, no se modifican los roles.
    /// </summary>
    public IEnumerable<string>? RoleCodes { get; set; }
}
