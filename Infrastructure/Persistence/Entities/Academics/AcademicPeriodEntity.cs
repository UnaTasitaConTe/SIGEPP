namespace Infrastructure.Persistence.Entities.Academics;

/// <summary>
/// Entity de EF Core que representa un período académico en la base de datos.
/// Mapea a la tabla AcademicPeriods.
/// </summary>
public class AcademicPeriodEntity
{
    /// <summary>
    /// Identificador único del período académico.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Código único del período académico (ej: "2024-1", "2024-2").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Nombre descriptivo del período académico (ej: "Periodo 2024-1").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de inicio del período académico (opcional).
    /// </summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>
    /// Fecha de fin del período académico (opcional).
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Indica si el período académico está activo en el sistema.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Fecha de creación del período académico.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha de última actualización del período académico.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    // Navegación: Asignaciones docentes en este período
    public ICollection<TeacherAssignmentEntity> TeacherAssignments { get; set; } = new List<TeacherAssignmentEntity>();
}
