using System.ComponentModel.DataAnnotations;

namespace Application.Ppa.Commands;

/// <summary>
/// Comando para que un administrador cree un nuevo PPA en el sistema,
/// especificando el docente responsable.
/// </summary>
public class CreatePpaAsAdminCommand
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
    /// ID del docente responsable del PPA.
    /// </summary>
    /// <remarks>
    /// A diferencia del comando estándar, aquí el administrador especifica
    /// explícitamente quién será el docente responsable del PPA.
    /// </remarks>
    [Required(ErrorMessage = "El ID del docente responsable es requerido.")]
    public Guid ResponsibleTeacherId { get; set; }

    /// <summary>
    /// IDs de las asignaciones docente-asignatura relacionadas con este PPA.
    /// </summary>
    /// <remarks>
    /// Las asignaciones deben:
    /// - Existir en el sistema.
    /// - Pertenecer al mismo período académico especificado.
    /// - Estar activas.
    /// </remarks>
    [Required(ErrorMessage = "Debe especificar al menos una asignación docente-asignatura.")]
    [MinLength(1, ErrorMessage = "Debe especificar al menos una asignación docente-asignatura.")]
    public IReadOnlyCollection<Guid> TeacherAssignmentIds { get; set; } = Array.Empty<Guid>();

    /// <summary>
    /// Nombres de los estudiantes asociados al PPA.
    /// </summary>
    public IReadOnlyCollection<string> StudentNames { get; set; } = Array.Empty<string>();
}
