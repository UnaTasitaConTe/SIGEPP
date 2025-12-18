using System.ComponentModel.DataAnnotations;

namespace Application.Academics.Commands;

/// <summary>
/// Comando para crear una nueva asignatura en el sistema.
/// </summary>
public class CreateSubjectCommand
{
    /// <summary>
    /// Código único de la asignatura (ej: "ISW-101", "MAT-201").
    /// </summary>
    [Required(ErrorMessage = "El código de la asignatura es requerido.")]
    [StringLength(20, MinimumLength = 1, ErrorMessage = "El código debe tener entre 1 y 20 caracteres.")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de la asignatura (ej: "Ingeniería de Software I").
    /// </summary>
    [Required(ErrorMessage = "El nombre de la asignatura es requerido.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 200 caracteres.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descripción opcional de la asignatura.
    /// </summary>
    [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres.")]
    public string? Description { get; set; }
}
