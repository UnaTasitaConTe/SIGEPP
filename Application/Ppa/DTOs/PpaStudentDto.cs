namespace Application.Ppa.DTOs;

/// <summary>
/// DTO para representar un estudiante asociado a un PPA en respuestas de lectura.
/// </summary>
public class PpaStudentDto
{
    /// <summary>
    /// Identificador único del estudiante en el contexto del PPA.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre completo del estudiante.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
