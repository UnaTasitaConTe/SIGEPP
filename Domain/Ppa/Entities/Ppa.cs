namespace Domain.Ppa.Entities;

/// <summary>
/// Representa el registro lógico de un PPA (Proyecto Pedagógico de Aula).
///
/// El PPA es principalmente un documento académico identificado por su título y metadatos.
/// Los archivos físicos (incluyendo el documento formal del PPA en PDF) se modelan
/// como anexos separados (ver <see cref="PpaAttachment"/>).
/// </summary>
public sealed class Ppa
{
    /// <summary>
    /// Identificador único del PPA.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Título del PPA. Es el identificador principal del proyecto académico.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Objetivo general del PPA.
    /// </summary>
    public string? GeneralObjective { get; private set; }

    /// <summary>
    /// Objetivos específicos del PPA.
    /// Por ahora se maneja como texto libre.
    /// </summary>
    public string? SpecificObjectives { get; private set; }

    /// <summary>
    /// Descripción general del PPA.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Estado actual del PPA en su ciclo de vida.
    /// </summary>
    public PpaStatus Status { get; private set; }

    /// <summary>
    /// Identificador del período académico en el que se desarrolla el PPA.
    /// </summary>
    public Guid AcademicPeriodId { get; private set; }

    /// <summary>
    /// Identificador del docente principal responsable del PPA.
    /// </summary>
    public Guid PrimaryTeacherId { get; private set; }

    private readonly List<Guid> _teacherAssignmentIds = new();

    /// <summary>
    /// Identificadores de las asignaciones docente-asignatura relacionadas con este PPA.
    /// Un PPA puede involucrar múltiples asignaturas y docentes.
    /// </summary>
    public IReadOnlyCollection<Guid> TeacherAssignmentIds => _teacherAssignmentIds.AsReadOnly();

    /// <summary>
    /// Fecha y hora de creación del registro del PPA.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Fecha y hora de última actualización del PPA.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    // Constructor privado para control total sobre la creación
    private Ppa(
        Guid id,
        string title,
        Guid academicPeriodId,
        Guid primaryTeacherId,
        PpaStatus status,
        DateTime createdAt)
    {
        Id = id;
        Title = title;
        AcademicPeriodId = academicPeriodId;
        PrimaryTeacherId = primaryTeacherId;
        Status = status;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Crea un nuevo PPA en estado Proposal.
    /// </summary>
    /// <param name="title">Título del PPA (obligatorio).</param>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="primaryTeacherId">ID del docente principal.</param>
    /// <param name="generalObjective">Objetivo general (opcional).</param>
    /// <param name="specificObjectives">Objetivos específicos (opcional).</param>
    /// <param name="description">Descripción (opcional).</param>
    /// <returns>Nueva instancia de PPA.</returns>
    /// <exception cref="ArgumentException">Si el título está vacío o los IDs son inválidos.</exception>
    public static Ppa Create(
        string title,
        Guid academicPeriodId,
        Guid primaryTeacherId,
        string? generalObjective = null,
        string? specificObjectives = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título del PPA es obligatorio.", nameof(title));

        if (academicPeriodId == Guid.Empty)
            throw new ArgumentException("El ID del período académico no puede estar vacío.", nameof(academicPeriodId));

        if (primaryTeacherId == Guid.Empty)
            throw new ArgumentException("El ID del docente principal no puede estar vacío.", nameof(primaryTeacherId));

        var ppa = new Ppa(
            Guid.NewGuid(),
            title.Trim(),
            academicPeriodId,
            primaryTeacherId,
            PpaStatus.Proposal,
            DateTime.UtcNow)
        {
            GeneralObjective = generalObjective?.Trim(),
            SpecificObjectives = specificObjectives?.Trim(),
            Description = description?.Trim()
        };

        return ppa;
    }

    /// <summary>
    /// Factory para reconstruir un PPA existente desde persistencia.
    /// </summary>
    public static Ppa CreateWithId(
        Guid id,
        string title,
        Guid academicPeriodId,
        Guid primaryTeacherId,
        PpaStatus status,
        DateTime createdAt,
        string? generalObjective = null,
        string? specificObjectives = null,
        string? description = null,
        DateTime? updatedAt = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El ID del PPA no puede estar vacío.", nameof(id));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título del PPA es obligatorio.", nameof(title));

        if (academicPeriodId == Guid.Empty)
            throw new ArgumentException("El ID del período académico no puede estar vacío.", nameof(academicPeriodId));

        if (primaryTeacherId == Guid.Empty)
            throw new ArgumentException("El ID del docente principal no puede estar vacío.", nameof(primaryTeacherId));

        var ppa = new Ppa(id, title.Trim(), academicPeriodId, primaryTeacherId, status, createdAt)
        {
            GeneralObjective = generalObjective?.Trim(),
            SpecificObjectives = specificObjectives?.Trim(),
            Description = description?.Trim(),
            UpdatedAt = updatedAt
        };

        return ppa;
    }

    /// <summary>
    /// Cambia el estado del PPA siguiendo las reglas del ciclo de vida.
    /// </summary>
    /// <param name="newStatus">Nuevo estado.</param>
    /// <exception cref="InvalidOperationException">
    /// Si el cambio de estado no es válido según las reglas de negocio.
    /// </exception>
    /// <remarks>
    /// Transiciones válidas:
    /// - Proposal → InProgress → Completed → Archived
    /// - No se puede salir del estado Archived (es terminal).
    /// - Se permiten regresiones en estados no terminales (ej: InProgress → Proposal).
    /// </remarks>
    public void ChangeStatus(PpaStatus newStatus)
    {
        if (Status == PpaStatus.Archived)
            throw new InvalidOperationException(
                "No se puede cambiar el estado de un PPA archivado. El estado Archived es terminal.");

        if (newStatus == Status)
            return; // No hay cambio

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Actualiza el título del PPA.
    /// </summary>
    /// <param name="title">Nuevo título (obligatorio).</param>
    /// <exception cref="ArgumentException">Si el título está vacío.</exception>
    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título del PPA no puede estar vacío.", nameof(title));

        Title = title.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Actualiza el objetivo general del PPA.
    /// </summary>
    public void UpdateGeneralObjective(string? generalObjective)
    {
        GeneralObjective = generalObjective?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Actualiza los objetivos específicos del PPA.
    /// </summary>
    public void UpdateSpecificObjectives(string? specificObjectives)
    {
        SpecificObjectives = specificObjectives?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Actualiza la descripción del PPA.
    /// </summary>
    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Asocia una asignación docente-asignatura al PPA.
    /// </summary>
    /// <param name="teacherAssignmentId">ID de la asignación.</param>
    /// <exception cref="ArgumentException">Si el ID es vacío o ya existe.</exception>
    public void AttachTeacherAssignment(Guid teacherAssignmentId)
    {
        if (teacherAssignmentId == Guid.Empty)
            throw new ArgumentException(
                "El ID de la asignación docente-asignatura no puede estar vacío.",
                nameof(teacherAssignmentId));

        if (_teacherAssignmentIds.Contains(teacherAssignmentId))
            throw new ArgumentException(
                $"La asignación docente-asignatura con ID '{teacherAssignmentId}' ya está asociada a este PPA.",
                nameof(teacherAssignmentId));

        _teacherAssignmentIds.Add(teacherAssignmentId);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Remueve una asignación docente-asignatura del PPA.
    /// </summary>
    /// <param name="teacherAssignmentId">ID de la asignación a remover.</param>
    public void RemoveTeacherAssignment(Guid teacherAssignmentId)
    {
        if (_teacherAssignmentIds.Remove(teacherAssignmentId))
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Método interno para reconstruir la colección de asignaciones desde persistencia.
    /// </summary>
    internal void RestoreTeacherAssignmentIds(IEnumerable<Guid> assignmentIds)
    {
        _teacherAssignmentIds.Clear();
        _teacherAssignmentIds.AddRange(assignmentIds.Where(id => id != Guid.Empty));
    }
}
