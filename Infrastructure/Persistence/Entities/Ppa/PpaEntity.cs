using Infrastructure.Persistence.Entities.Academics;

namespace Infrastructure.Persistence.Entities.Ppa;

/// <summary>
/// Entity de EF Core que representa un PPA (Proyecto Pedagógico de Aula) en la base de datos.
/// Mapea a la tabla Ppas.
/// </summary>
public class PpaEntity
{
    /// <summary>
    /// Identificador único del PPA.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Título del PPA (identificador principal del proyecto).
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Objetivo general del PPA.
    /// </summary>
    public string? GeneralObjective { get; set; }

    /// <summary>
    /// Objetivos específicos del PPA (texto libre).
    /// </summary>
    public string? SpecificObjectives { get; set; }

    /// <summary>
    /// Descripción general del PPA.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Estado del PPA (mapeado al enum PpaStatus: 0=Proposal, 1=InProgress, 2=Completed, 3=Archived).
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Identificador del período académico en el que se desarrolla el PPA.
    /// </summary>
    public Guid AcademicPeriodId { get; set; }

    /// <summary>
    /// Identificador del docente principal responsable del PPA.
    /// </summary>
    public Guid PrimaryTeacherId { get; set; }

    /// <summary>
    /// Fecha y hora de creación del PPA.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha y hora de última actualización del PPA.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // Navegaciones
    /// <summary>
    /// Navegación hacia el período académico del PPA.
    /// </summary>
    public AcademicPeriodEntity? AcademicPeriod { get; set; }

    /// <summary>
    /// Navegación hacia el docente principal del PPA.
    /// </summary>
    public UserEntity? PrimaryTeacher { get; set; }

    /// <summary>
    /// Anexos (archivos) asociados al PPA.
    /// </summary>
    public ICollection<PpaAttachmentEntity> Attachments { get; set; } = new List<PpaAttachmentEntity>();

    /// <summary>
    /// Relación many-to-many con asignaciones docente-asignatura a través de tabla intermedia.
    /// </summary>
    public ICollection<PpaTeacherAssignmentEntity> PpaTeacherAssignments { get; set; } = new List<PpaTeacherAssignmentEntity>();

    /// <summary>
    /// Estudiantes asociados al PPA.
    /// </summary>
    public ICollection<PpaStudentEntity> Students { get; set; } = new List<PpaStudentEntity>();

    /// <summary>
    /// ID del PPA original del cual este PPA es una continuación.
    /// Null si este PPA no es una continuación de otro.
    /// </summary>
    public Guid? ContinuationOfPpaId { get; set; }

    /// <summary>
    /// ID del PPA que continúa este PPA en otro periodo académico.
    /// Null si este PPA no ha sido continuado aún.
    /// </summary>
    public Guid? ContinuedByPpaId { get; set; }
}
