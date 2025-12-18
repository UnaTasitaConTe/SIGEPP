using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Entities.Academics;

/// <summary>
/// Entity de EF Core que representa la asignación de un docente a una asignatura en un período académico.
/// Mapea a la tabla TeacherAssignments.
/// </summary>
public class TeacherAssignmentEntity
{
    /// <summary>
    /// Identificador único de la asignación.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del docente (User con rol DOCENTE).
    /// </summary>
    public Guid TeacherId { get; set; }

    /// <summary>
    /// ID de la asignatura asignada.
    /// </summary>
    public Guid SubjectId { get; set; }

    /// <summary>
    /// ID del período académico en el que se realiza la asignación.
    /// </summary>
    public Guid AcademicPeriodId { get; set; }

    /// <summary>
    /// Indica si la asignación está activa.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Fecha de creación de la asignación.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha de última actualización de la asignación.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    // Navegación: Docente asignado
    public UserEntity? Teacher { get; set; }

    // Navegación: Asignatura asignada
    public SubjectEntity? Subject { get; set; }

    // Navegación: Período académico
    public AcademicPeriodEntity? AcademicPeriod { get; set; }
}
