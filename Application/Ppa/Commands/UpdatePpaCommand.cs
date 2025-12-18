using System.ComponentModel.DataAnnotations;

namespace Application.Ppa.Commands;

/// <summary>
/// Comando para actualizar los datos de un PPA existente.
/// </summary>
public class UpdatePpaCommand
{
    /// <summary>
    /// ID del PPA a actualizar.
    /// </summary>
    [Required(ErrorMessage = "El ID del PPA es requerido.")]
    public Guid Id { get; set; }

    /// <summary>
    /// Nuevo título del PPA.
    /// </summary>
    [Required(ErrorMessage = "El título del PPA es requerido.")]
    [StringLength(300, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 300 caracteres.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Nueva descripción del PPA.
    /// </summary>
    [StringLength(3000, ErrorMessage = "La descripción no puede exceder los 3000 caracteres.")]
    public string? Description { get; set; }

    /// <summary>
    /// Nuevo objetivo general del PPA.
    /// </summary>
    [StringLength(1000, ErrorMessage = "El objetivo general no puede exceder los 1000 caracteres.")]
    public string? GeneralObjective { get; set; }

    /// <summary>
    /// Nuevos objetivos específicos del PPA.
    /// </summary>
    [StringLength(2000, ErrorMessage = "Los objetivos específicos no pueden exceder los 2000 caracteres.")]
    public string? SpecificObjectives { get; set; }
}
