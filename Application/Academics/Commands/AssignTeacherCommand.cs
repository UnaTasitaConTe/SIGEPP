using System.ComponentModel.DataAnnotations;

namespace Application.Academics.Commands;

/// <summary>
/// Comando para asignar un docente a una asignatura en un período académico.
/// </summary>
public class AssignTeacherCommand
{
    /// <summary>
    /// ID del docente a asignar.
    /// </summary>
    [Required(ErrorMessage = "El ID del docente es requerido.")]
    public Guid TeacherId { get; set; }

    /// <summary>
    /// ID de la asignatura a asignar.
    /// </summary>
    [Required(ErrorMessage = "El ID de la asignatura es requerido.")]
    public Guid SubjectId { get; set; }

    /// <summary>
    /// ID del período académico en el que se realiza la asignación.
    /// </summary>
    [Required(ErrorMessage = "El ID del período académico es requerido.")]
    public Guid AcademicPeriodId { get; set; }
}
