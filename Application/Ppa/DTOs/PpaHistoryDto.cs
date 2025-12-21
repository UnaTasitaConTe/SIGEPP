using Domain.Ppa;

namespace Application.Ppa.DTOs;

/// <summary>
/// DTO para una entrada del historial de un PPA.
/// </summary>
public class PpaHistoryDto
{
    /// <summary>
    /// ID de la entrada de historial.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del PPA al que pertenece esta entrada.
    /// </summary>
    public Guid PpaId { get; set; }

    /// <summary>
    /// ID del usuario que realizó la acción.
    /// </summary>
    public Guid PerformedByUserId { get; set; }

    /// <summary>
    /// Nombre del usuario que realizó la acción.
    /// </summary>
    public string? PerformedByUserName { get; set; }

    /// <summary>
    /// Fecha y hora en que se realizó la acción.
    /// </summary>
    public DateTime PerformedAt { get; set; }

    /// <summary>
    /// Tipo de acción realizada.
    /// </summary>
    public PpaHistoryActionType ActionType { get; set; }

    /// <summary>
    /// Descripción legible del tipo de acción.
    /// </summary>
    public string ActionTypeDescription { get; set; } = string.Empty;

    /// <summary>
    /// Valor anterior antes del cambio.
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// Nuevo valor después del cambio.
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// Notas adicionales sobre la acción.
    /// </summary>
    public string? Notes { get; set; }
}
