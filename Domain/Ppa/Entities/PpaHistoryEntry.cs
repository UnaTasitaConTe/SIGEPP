namespace Domain.Ppa.Entities;

/// <summary>
/// Representa una entrada en el historial de cambios de un PPA.
/// Registra cada acción significativa realizada sobre el PPA para auditoría y trazabilidad.
/// </summary>
public sealed class PpaHistoryEntry
{
    /// <summary>
    /// Identificador único de la entrada de historial.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Identificador del PPA al que pertenece esta entrada de historial.
    /// </summary>
    public Guid PpaId { get; private set; }

    /// <summary>
    /// Identificador del usuario que realizó la acción.
    /// </summary>
    public Guid PerformedByUserId { get; private set; }

    /// <summary>
    /// Fecha y hora en que se realizó la acción.
    /// </summary>
    public DateTime PerformedAt { get; private set; }

    /// <summary>
    /// Tipo de acción realizada (creación, actualización, cambio de estado, etc.).
    /// </summary>
    public PpaHistoryActionType ActionType { get; private set; }

    /// <summary>
    /// Valor anterior antes del cambio (formato JSON o texto libre).
    /// Puede ser null si no aplica (ej: creación del PPA).
    /// </summary>
    public string? OldValue { get; private set; }

    /// <summary>
    /// Nuevo valor después del cambio (formato JSON o texto libre).
    /// Puede ser null si no aplica (ej: eliminación de un anexo).
    /// </summary>
    public string? NewValue { get; private set; }

    /// <summary>
    /// Notas adicionales sobre la acción (opcional).
    /// Por ejemplo: razón del cambio, comentarios del usuario, etc.
    /// </summary>
    public string? Notes { get; private set; }

    // Constructor privado para control total sobre la creación
    private PpaHistoryEntry(
        Guid id,
        Guid ppaId,
        Guid performedByUserId,
        DateTime performedAt,
        PpaHistoryActionType actionType)
    {
        Id = id;
        PpaId = ppaId;
        PerformedByUserId = performedByUserId;
        PerformedAt = performedAt;
        ActionType = actionType;
    }

    /// <summary>
    /// Crea una nueva entrada de historial para un PPA.
    /// </summary>
    /// <param name="ppaId">ID del PPA.</param>
    /// <param name="performedByUserId">ID del usuario que realiza la acción.</param>
    /// <param name="actionType">Tipo de acción realizada.</param>
    /// <param name="oldValue">Valor anterior (opcional).</param>
    /// <param name="newValue">Nuevo valor (opcional).</param>
    /// <param name="notes">Notas adicionales (opcional).</param>
    /// <returns>Nueva instancia de PpaHistoryEntry.</returns>
    /// <exception cref="ArgumentException">Si los IDs son inválidos.</exception>
    public static PpaHistoryEntry Create(
        Guid ppaId,
        Guid performedByUserId,
        PpaHistoryActionType actionType,
        string? oldValue = null,
        string? newValue = null,
        string? notes = null)
    {
        if (ppaId == Guid.Empty)
            throw new ArgumentException("El ID del PPA no puede estar vacío.", nameof(ppaId));

        if (performedByUserId == Guid.Empty)
            throw new ArgumentException("El ID del usuario no puede estar vacío.", nameof(performedByUserId));

        var entry = new PpaHistoryEntry(
            Guid.NewGuid(),
            ppaId,
            performedByUserId,
            DateTime.UtcNow,
            actionType)
        {
            OldValue = oldValue?.Trim(),
            NewValue = newValue?.Trim(),
            Notes = notes?.Trim()
        };

        return entry;
    }

    /// <summary>
    /// Factory para reconstruir una entrada de historial existente desde persistencia.
    /// </summary>
    public static PpaHistoryEntry CreateWithId(
        Guid id,
        Guid ppaId,
        Guid performedByUserId,
        DateTime performedAt,
        PpaHistoryActionType actionType,
        string? oldValue = null,
        string? newValue = null,
        string? notes = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El ID de la entrada de historial no puede estar vacío.", nameof(id));

        if (ppaId == Guid.Empty)
            throw new ArgumentException("El ID del PPA no puede estar vacío.", nameof(ppaId));

        if (performedByUserId == Guid.Empty)
            throw new ArgumentException("El ID del usuario no puede estar vacío.", nameof(performedByUserId));

        var entry = new PpaHistoryEntry(
            id,
            ppaId,
            performedByUserId,
            performedAt,
            actionType)
        {
            OldValue = oldValue?.Trim(),
            NewValue = newValue?.Trim(),
            Notes = notes?.Trim()
        };

        return entry;
    }
}
