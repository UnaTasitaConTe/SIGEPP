namespace Infrastructure.Persistence.Entities.Academics;

/// <summary>
/// Entity de EF Core que representa una asignatura en la base de datos.
/// Mapea a la tabla Subjects.
/// </summary>
public class SubjectEntity
{
    /// <summary>
    /// Identificador único de la asignatura.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Código único de la asignatura (ej: "ISW-101", "MAT-201").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de la asignatura (ej: "Ingeniería de Software I").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descripción opcional de la asignatura.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indica si la asignatura está activa en el sistema.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Fecha de creación de la asignatura.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha de última actualización de la asignatura.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    // Navegación: Asignaciones docentes para esta asignatura
    public ICollection<TeacherAssignmentEntity> TeacherAssignments { get; set; } = new List<TeacherAssignmentEntity>();
}
