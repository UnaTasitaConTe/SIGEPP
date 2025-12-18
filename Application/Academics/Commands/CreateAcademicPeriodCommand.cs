using System.ComponentModel.DataAnnotations;

namespace Application.Academics.Commands;

/// <summary>
/// Comando para crear un nuevo período académico en el sistema.
/// </summary>
public class CreateAcademicPeriodCommand
{
    /// <summary>
    /// Código único del período académico (ej: "2024-1", "2024-2").
    /// </summary>
    [Required(ErrorMessage = "El código del período académico es requerido.")]
    [StringLength(20, MinimumLength = 1, ErrorMessage = "El código debe tener entre 1 y 20 caracteres.")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Nombre descriptivo del período académico (ej: "Periodo 2024-1").
    /// </summary>
    [Required(ErrorMessage = "El nombre del período académico es requerido.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 200 caracteres.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de inicio del período académico (opcional).
    /// </summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>
    /// Fecha de fin del período académico (opcional).
    /// </summary>
    public DateOnly? EndDate { get; set; }
}
