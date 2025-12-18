using System.ComponentModel.DataAnnotations;
using Domain.Ppa;

namespace Application.Ppa.Commands;

/// <summary>
/// Comando para cambiar el estado de un PPA.
/// </summary>
public class ChangePpaStatusCommand
{
    /// <summary>
    /// ID del PPA cuyo estado se desea cambiar.
    /// </summary>
    [Required(ErrorMessage = "El ID del PPA es requerido.")]
    public Guid Id { get; set; }

    /// <summary>
    /// Nuevo estado del PPA.
    /// </summary>
    [Required(ErrorMessage = "El nuevo estado es requerido.")]
    public PpaStatus NewStatus { get; set; }
}
