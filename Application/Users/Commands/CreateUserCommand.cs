using System.ComponentModel.DataAnnotations;

namespace Application.Users.Commands;

/// <summary>
/// Comando para crear un nuevo usuario en el sistema.
/// </summary>
public class CreateUserCommand
{
    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 200 caracteres.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email del usuario (debe ser único en el sistema).
    /// </summary>
    [Required(ErrorMessage = "El email es requerido.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    [StringLength(256, ErrorMessage = "El email no puede exceder 256 caracteres.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña en texto plano (será hasheada antes de guardar).
    /// </summary>
    [Required(ErrorMessage = "La contraseña es requerida.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres.")]
    public string RawPassword { get; set; } = string.Empty;

    /// <summary>
    /// Códigos de roles a asignar al usuario al momento de crearlo.
    /// Ejemplo: ["ADMIN", "DOCENTE"]
    /// </summary>
    public IEnumerable<string> RoleCodes { get; set; } = [];

    /// <summary>
    /// Indica si el usuario debe crearse activo.
    /// Por defecto es true.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
