using System.ComponentModel.DataAnnotations;

namespace Application.Ppa.Commands;

/// <summary>
/// Comando para continuar un PPA existente en otro período académico.
/// </summary>
/// <remarks>
/// Permite dar continuidad a un proyecto pedagógico de un periodo a otro,
/// creando un nuevo PPA vinculado al original. Esto es útil cuando un proyecto
/// se extiende por varios semestres o años.
/// </remarks>
public class ContinuePpaCommand
{
    /// <summary>
    /// ID del PPA original que se desea continuar.
    /// </summary>
    [Required(ErrorMessage = "El ID del PPA origen es requerido.")]
    public Guid SourcePpaId { get; set; }

    /// <summary>
    /// ID del período académico en el que se continuará el PPA.
    /// </summary>
    /// <remarks>
    /// El periodo destino debe ser diferente al periodo del PPA origen
    /// y debe ser cronológicamente posterior.
    /// </remarks>
    [Required(ErrorMessage = "El ID del período académico destino es requerido.")]
    public Guid TargetAcademicPeriodId { get; set; }

    /// <summary>
    /// Nuevo título para el PPA de continuación (opcional).
    /// </summary>
    /// <remarks>
    /// Si no se proporciona, se usará el título del PPA original.
    /// Si se proporciona, debe ser único en el periodo destino.
    /// </remarks>
    [StringLength(300, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 300 caracteres.")]
    public string? NewTitle { get; set; }

    /// <summary>
    /// ID del nuevo docente responsable del PPA de continuación (opcional).
    /// </summary>
    /// <remarks>
    /// Si no se proporciona, se usará el mismo responsable del PPA original.
    /// Esto permite transferir la responsabilidad del proyecto a otro docente
    /// en el nuevo periodo.
    /// </remarks>
    public Guid? NewResponsibleTeacherId { get; set; }

    /// <summary>
    /// IDs de las asignaciones docente-asignatura del periodo destino
    /// que estarán asociadas al PPA de continuación.
    /// </summary>
    /// <remarks>
    /// Las asignaciones deben:
    /// - Existir en el sistema.
    /// - Pertenecer al período académico destino.
    /// - Estar activas.
    /// </remarks>
    [Required(ErrorMessage = "Debe especificar al menos una asignación docente-asignatura para el nuevo periodo.")]
    [MinLength(1, ErrorMessage = "Debe especificar al menos una asignación docente-asignatura para el nuevo periodo.")]
    public IReadOnlyCollection<Guid> TeacherAssignmentIds { get; set; } = Array.Empty<Guid>();

    /// <summary>
    /// Nombres de los estudiantes que participarán en el PPA de continuación.
    /// </summary>
    /// <remarks>
    /// Los estudiantes pueden ser los mismos del PPA original o diferentes.
    /// Los cambios en la lista de estudiantes quedarán registrados en el historial.
    /// </remarks>
    public IReadOnlyCollection<string> StudentNames { get; set; } = Array.Empty<string>();
}
