using System.ComponentModel.DataAnnotations;

namespace Application.Ppa.Commands;

/// <summary>
/// Comando para crear un nuevo PPA en el sistema.
/// </summary>
public class CreatePpaCommand
{
    /// <summary>
    /// Título del PPA (identificador principal del proyecto).
    /// </summary>
    [Required(ErrorMessage = "El título del PPA es requerido.")]
    [StringLength(300, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 300 caracteres.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Descripción general del PPA.
    /// </summary>
    [StringLength(3000, ErrorMessage = "La descripción no puede exceder los 3000 caracteres.")]
    public string? Description { get; set; }

    /// <summary>
    /// Objetivo general del PPA.
    /// </summary>
    [StringLength(1000, ErrorMessage = "El objetivo general no puede exceder los 1000 caracteres.")]
    public string? GeneralObjective { get; set; }

    /// <summary>
    /// Objetivos específicos del PPA.
    /// </summary>
    [StringLength(2000, ErrorMessage = "Los objetivos específicos no pueden exceder los 2000 caracteres.")]
    public string? SpecificObjectives { get; set; }

    /// <summary>
    /// ID del período académico en el que se desarrolla el PPA.
    /// </summary>
    [Required(ErrorMessage = "El ID del período académico es requerido.")]
    public Guid AcademicPeriodId { get; set; }

    /// <summary>
    /// ID del docente principal responsable del PPA.
    /// </summary>
    [Required(ErrorMessage = "El ID del docente principal es requerido.")]
    public Guid PrimaryTeacherId { get; set; }

    /// <summary>
    /// IDs de las asignaciones docente-asignatura relacionadas con este PPA.
    /// </summary>
    [Required(ErrorMessage = "Debe especificar al menos una asignación docente-asignatura.")]
    [MinLength(1, ErrorMessage = "Debe especificar al menos una asignación docente-asignatura.")]
    public IReadOnlyCollection<Guid> TeacherAssignmentIds { get; set; } = Array.Empty<Guid>();
}
