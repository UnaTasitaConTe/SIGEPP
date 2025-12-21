using System.ComponentModel.DataAnnotations;
using Application.Ppa.DTOs;

namespace Application.Ppa.Commands;

/// <summary>
/// Comando para que un administrador actualice un PPA existente,
/// con la capacidad de cambiar el docente responsable.
/// </summary>
public class UpdatePpaAsAdminCommand
{
    /// <summary>
    /// ID del PPA a actualizar.
    /// </summary>
    [Required(ErrorMessage = "El ID del PPA es requerido.")]
    public Guid Id { get; set; }

    /// <summary>
    /// Nuevo título del PPA.
    /// </summary>
    [Required(ErrorMessage = "El título del PPA es requerido.")]
    [StringLength(300, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 300 caracteres.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Nueva descripción del PPA.
    /// </summary>
    [StringLength(3000, ErrorMessage = "La descripción no puede exceder los 3000 caracteres.")]
    public string? Description { get; set; }

    /// <summary>
    /// Nuevo objetivo general del PPA.
    /// </summary>
    [StringLength(1000, ErrorMessage = "El objetivo general no puede exceder los 1000 caracteres.")]
    public string? GeneralObjective { get; set; }

    /// <summary>
    /// Nuevos objetivos específicos del PPA.
    /// </summary>
    [StringLength(2000, ErrorMessage = "Los objetivos específicos no pueden exceder los 2000 caracteres.")]
    public string? SpecificObjectives { get; set; }

    /// <summary>
    /// ID del docente responsable del PPA.
    /// </summary>
    /// <remarks>
    /// El administrador puede cambiar el docente responsable del PPA.
    /// </remarks>
    [Required(ErrorMessage = "El ID del docente responsable es requerido.")]
    public Guid ResponsibleTeacherId { get; set; }

    /// <summary>
    /// Nuevos IDs de asignaciones docente-asignatura.
    /// </summary>
    [Required(ErrorMessage = "Debe especificar al menos una asignación docente-asignatura.")]
    [MinLength(1, ErrorMessage = "Debe especificar al menos una asignación docente-asignatura.")]
    public IReadOnlyCollection<Guid> TeacherAssignmentIds { get; set; } = Array.Empty<Guid>();

    /// <summary>
    /// Lista de estudiantes a sincronizar.
    /// Los estudiantes con Id existente se actualizarán.
    /// Los estudiantes sin Id (null o Guid.Empty) se crearán.
    /// Los estudiantes existentes que no aparezcan en esta lista se eliminarán.
    /// </summary>
    public IReadOnlyCollection<UpdatePpaStudentItem> Students { get; set; } = Array.Empty<UpdatePpaStudentItem>();
}
