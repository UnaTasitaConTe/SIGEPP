namespace Application.Ppa.DTOs;

/// <summary>
/// DTO que representa una asignación docente-asignatura dentro de un PPA,
/// con información detallada de la asignatura y el docente.
/// </summary>
public sealed record PpaTeacherAssignmentDetailDto
{
    /// <summary>
    /// ID de la asignación docente-asignatura.
    /// </summary>
    public required Guid TeacherAssignmentId { get; init; }

    /// <summary>
    /// ID del docente asignado.
    /// </summary>
    public required Guid TeacherId { get; init; }

    /// <summary>
    /// Nombre completo del docente.
    /// </summary>
    public string? TeacherName { get; init; }

    /// <summary>
    /// ID de la asignatura.
    /// </summary>
    public required Guid SubjectId { get; init; }

    /// <summary>
    /// Código de la asignatura.
    /// </summary>
    public string? SubjectCode { get; init; }

    /// <summary>
    /// Nombre de la asignatura.
    /// </summary>
    public string? SubjectName { get; init; }

    /// <summary>
    /// Semestre de la asignatura.
    /// </summary>
   // public int? SubjectSemester { get; init; }
}
