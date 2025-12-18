using Infrastructure.Persistence.Entities.Academics;

namespace Infrastructure.Persistence.Entities.Ppa;

/// <summary>
/// Tabla intermedia que relaciona PPAs con TeacherAssignments (many-to-many).
/// Permite que un PPA involucre múltiples asignaciones docente-asignatura.
/// Mapea a la tabla PpaTeacherAssignments.
/// </summary>
public class PpaTeacherAssignmentEntity
{
    /// <summary>
    /// Identificador del PPA.
    /// </summary>
    public Guid PpaId { get; set; }

    /// <summary>
    /// Identificador de la asignación docente-asignatura.
    /// </summary>
    public Guid TeacherAssignmentId { get; set; }

    // Navegaciones
    /// <summary>
    /// Navegación hacia el PPA.
    /// </summary>
    public PpaEntity? Ppa { get; set; }

    /// <summary>
    /// Navegación hacia la asignación docente-asignatura.
    /// </summary>
    public TeacherAssignmentEntity? TeacherAssignment { get; set; }
}
