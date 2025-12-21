namespace Infrastructure.Persistence.Entities.Ppa;

/// <summary>
/// Entity de EF Core que representa un estudiante asociado a un PPA en la base de datos.
/// Mapea a la tabla PpaStudents.
/// </summary>
public class PpaStudentEntity
{
    /// <summary>
    /// Identificador único del estudiante en el contexto del PPA.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador del PPA al que pertenece este estudiante.
    /// </summary>
    public Guid PpaId { get; set; }

    /// <summary>
    /// Nombre completo del estudiante.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    // Navegación
    /// <summary>
    /// Navegación hacia el PPA al que pertenece este estudiante.
    /// </summary>
    public PpaEntity? Ppa { get; set; }
}
