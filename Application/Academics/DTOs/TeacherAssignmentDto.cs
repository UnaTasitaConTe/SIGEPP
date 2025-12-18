namespace Application.Academics.DTOs;

/// <summary>
/// DTO de asignación docente para respuestas de la API.
/// Representa la asignación de un docente a una asignatura en un período académico.
/// </summary>
public sealed record TeacherAssignmentDto
{
    /// <summary>
    /// ID de la asignación.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// ID del docente asignado.
    /// </summary>
    public required Guid TeacherId { get; init; }

    /// <summary>
    /// ID de la asignatura asignada.
    /// </summary>
    public required Guid SubjectId { get; init; }

    /// <summary>
    /// ID del período académico en el que se realiza la asignación.
    /// </summary>
    public required Guid AcademicPeriodId { get; init; }

    /// <summary>
    /// Indica si la asignación está activa.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Nombre del docente asignado (opcional, útil para mostrar en listas).
    /// </summary>
    public string? TeacherName { get; init; }

    /// <summary>
    /// Código de la asignatura (opcional, útil para mostrar en listas).
    /// </summary>
    public string? SubjectCode { get; init; }

    /// <summary>
    /// Nombre de la asignatura (opcional, útil para mostrar en listas).
    /// </summary>
    public string? SubjectName { get; init; }

    /// <summary>
    /// Código del período académico (opcional, útil para mostrar en listas).
    /// </summary>
    public string? AcademicPeriodCode { get; init; }

    /// <summary>
    /// Nombre del período académico (opcional, útil para mostrar en listas).
    /// </summary>
    public string? AcademicPeriodName { get; init; }
}
