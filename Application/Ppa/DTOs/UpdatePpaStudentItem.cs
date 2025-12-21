using System.ComponentModel.DataAnnotations;

namespace Application.Ppa.DTOs;

/// <summary>
/// DTO para actualizar o crear un estudiante asociado a un PPA.
/// Soporta operaciones de sincronización (create/update/delete).
/// </summary>
public class UpdatePpaStudentItem
{
    /// <summary>
    /// Identificador único del estudiante.
    /// Null o Guid.Empty indica que es un nuevo estudiante a crear.
    /// Si tiene valor, se actualizará el estudiante existente con ese ID.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Nombre completo del estudiante.
    /// </summary>
    [Required(ErrorMessage = "El nombre del estudiante es obligatorio.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre del estudiante debe tener entre 2 y 200 caracteres.")]
    public string Name { get; set; } = string.Empty;
}
