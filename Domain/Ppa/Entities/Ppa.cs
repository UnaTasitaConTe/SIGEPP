using Domain.Ppa.ValueObjects;

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

    /// <summary>
    /// Alias para PrimaryTeacherId. Representa al docente responsable del PPA.
    /// </summary>
    public Guid ResponsibleTeacherId => PrimaryTeacherId;

    /// <summary>
    /// ID del PPA original del cual este PPA es una continuación.
    /// Null si este PPA no es una continuación de otro.
    /// </summary>
    public Guid? ContinuationOfPpaId { get; private set; }

    /// <summary>
    /// ID del PPA que continúa este PPA en otro periodo académico.
    /// Null si este PPA no ha sido continuado aún.
    /// </summary>
    public Guid? ContinuedByPpaId { get; private set; }

    private readonly List<Guid> _teacherAssignmentIds = new();
    private readonly List<PpaStudent> _students = new();

    /// <summary>
    /// Identificadores de las asignaciones docente-asignatura relacionadas con este PPA.
    /// Un PPA puede involucrar múltiples asignaturas y docentes.
    /// </summary>
    public IReadOnlyCollection<Guid> TeacherAssignmentIds => _teacherAssignmentIds.AsReadOnly();

    /// <summary>
    /// Estudiantes asociados al PPA.
    /// </summary>
    public IReadOnlyCollection<PpaStudent> Students => _students.AsReadOnly();

    /// <summary>
    /// Fecha y hora de creación del registro del PPA.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Fecha y hora de última actualización del PPA.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    public string? TeacherPrimaryName { get; private set; }

    // Constructor privado para control total sobre la creación
    private Ppa(
        Guid id,
        string title,
        Guid academicPeriodId,
        Guid primaryTeacherId,
        PpaStatus status,
        DateTime createdAt,
        string? teacherPrimaryName,
         Guid? continuationOfPpaId = null,
        Guid? continuedByPpaId = null
        )
    {
        Id = id;
        Title = title;
        AcademicPeriodId = academicPeriodId;
        PrimaryTeacherId = primaryTeacherId;
        Status = status;
        CreatedAt = createdAt;
        TeacherPrimaryName = teacherPrimaryName;
        ContinuationOfPpaId = continuationOfPpaId;
        ContinuedByPpaId = continuedByPpaId;
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
        string? description = null,
        string? teacherPrimaryName = null,
         Guid? continuationOfPpaId = null,
        Guid? continuedByPpaId = null
        )
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
            DateTime.UtcNow,
            teacherPrimaryName,
            continuationOfPpaId,
            continuedByPpaId
            )
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
        DateTime? updatedAt = null,
        string? teacherPrimaryName = null,
        Guid? continuationOfPpaId = null,
        Guid? continuedByPpaId = null
        )
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El ID del PPA no puede estar vacío.", nameof(id));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título del PPA es obligatorio.", nameof(title));

        if (academicPeriodId == Guid.Empty)
            throw new ArgumentException("El ID del período académico no puede estar vacío.", nameof(academicPeriodId));

        if (primaryTeacherId == Guid.Empty)
            throw new ArgumentException("El ID del docente principal no puede estar vacío.", nameof(primaryTeacherId));

        var ppa = new Ppa(id, title.Trim(), academicPeriodId, primaryTeacherId, status, createdAt, teacherPrimaryName)
        {
            GeneralObjective = generalObjective?.Trim(),
            SpecificObjectives = specificObjectives?.Trim(),
            Description = description?.Trim(),
            UpdatedAt = updatedAt,
            ContinuedByPpaId =continuedByPpaId,
            ContinuationOfPpaId = continuationOfPpaId
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

    /// <summary>
    /// Método interno para reconstruir la colección de estudiantes desde persistencia.
    /// </summary>
    internal void RestoreStudents(IEnumerable<PpaStudent> students)
    {
        _students.Clear();
        _students.AddRange(students);
    }

    /// <summary>
    /// Establece todas las asignaciones docente-asignatura del PPA de una sola vez,
    /// reemplazando las existentes.
    /// </summary>
    /// <param name="teacherAssignmentIds">Nuevos IDs de asignaciones.</param>
    /// <exception cref="ArgumentException">Si hay IDs duplicados o vacíos.</exception>
    public void SetTeacherAssignments(IEnumerable<Guid> teacherAssignmentIds)
    {
        var assignmentsList = teacherAssignmentIds.ToList();

        // Validar que no haya IDs vacíos
        if (assignmentsList.Any(id => id == Guid.Empty))
            throw new ArgumentException(
                "La lista de asignaciones contiene IDs vacíos.",
                nameof(teacherAssignmentIds));

        // Validar que no haya duplicados
        if (assignmentsList.Count != assignmentsList.Distinct().Count())
            throw new ArgumentException(
                "La lista de asignaciones contiene IDs duplicados.",
                nameof(teacherAssignmentIds));

        _teacherAssignmentIds.Clear();
        _teacherAssignmentIds.AddRange(assignmentsList);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cambia el docente responsable del PPA.
    /// </summary>
    /// <param name="newTeacherId">ID del nuevo docente responsable.</param>
    /// <exception cref="ArgumentException">Si el ID es vacío.</exception>
    /// <exception cref="InvalidOperationException">Si el nuevo docente ya es el responsable actual.</exception>
    public void ChangeResponsibleTeacher(Guid newTeacherId)
    {
        if (newTeacherId == Guid.Empty)
            throw new ArgumentException(
                "El ID del nuevo docente responsable no puede estar vacío.",
                nameof(newTeacherId));

        if (PrimaryTeacherId == newTeacherId)
            throw new InvalidOperationException(
                "El docente especificado ya es el responsable actual del PPA.");

        PrimaryTeacherId = newTeacherId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Establece los estudiantes asociados al PPA a partir de una lista de nombres.
    /// Reemplaza la lista existente de estudiantes.
    /// </summary>
    /// <param name="studentNames">Nombres de los estudiantes.</param>
    /// <exception cref="ArgumentException">Si hay nombres vacíos o duplicados.</exception>
    public void SetStudents(IEnumerable<string> studentNames)
    {
        var namesList = studentNames.Select(n => n?.Trim()).Where(n => !string.IsNullOrEmpty(n)).ToList();

        // Validar que no haya duplicados (case-insensitive)
        if (namesList.Count != namesList.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            throw new ArgumentException(
                "La lista de estudiantes contiene nombres duplicados.",
                nameof(studentNames));

        _students.Clear();
        foreach (var name in namesList)
        {
            _students.Add(PpaStudent.Create(name));
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sincroniza la lista de estudiantes del PPA basándose en una lista de estudiantes con sus IDs.
    /// - Los estudiantes con Id existente se actualizarán (por nombre).
    /// - Los estudiantes sin Id (null o Guid.Empty) se crearán.
    /// - Los estudiantes existentes que no aparezcan en la lista se eliminarán.
    /// Esta operación es idempotente.
    /// </summary>
    /// <param name="studentsToSync">Lista de estudiantes con sus IDs y nombres.</param>
    /// <exception cref="ArgumentException">
    /// Si hay nombres vacíos, IDs duplicados, nombres duplicados, o IDs que no existen.
    /// </exception>
    public void SyncStudents(IEnumerable<(Guid? Id, string Name)> studentsToSync)
    {
        var syncList = studentsToSync
            .Select(s => (Id: s.Id, Name: s.Name?.Trim()))
            .Where(s => !string.IsNullOrEmpty(s.Name))
            .ToList();

        // Validar que no haya IDs duplicados en la lista de sincronización
        var idsToUpdate = syncList
            .Where(s => s.Id.HasValue && s.Id.Value != Guid.Empty)
            .Select(s => s.Id!.Value)
            .ToList();

        if (idsToUpdate.Count != idsToUpdate.Distinct().Count())
            throw new ArgumentException(
                "La lista de estudiantes contiene IDs duplicados.",
                nameof(studentsToSync));

        // Validar que no haya nombres duplicados (case-insensitive)
        var names = syncList.Select(s => s.Name!).ToList();
        if (names.Count != names.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            throw new ArgumentException(
                "La lista de estudiantes contiene nombres duplicados.",
                nameof(studentsToSync));

        // Validar que todos los IDs a actualizar existan en la lista actual
        var currentStudentIds = _students.Select(s => s.Id).ToHashSet();
        var invalidIds = idsToUpdate.Where(id => !currentStudentIds.Contains(id)).ToList();
        if (invalidIds.Any())
            throw new ArgumentException(
                $"Los siguientes IDs de estudiantes no existen en el PPA: {string.Join(", ", invalidIds)}",
                nameof(studentsToSync));

        // Separar estudiantes a crear, actualizar y eliminar
        var toCreate = syncList.Where(s => !s.Id.HasValue || s.Id.Value == Guid.Empty).ToList();
        var toUpdate = syncList.Where(s => s.Id.HasValue && s.Id.Value != Guid.Empty).ToList();
        var idsInRequest = toUpdate.Select(s => s.Id!.Value).ToHashSet();
        var toDelete = _students.Where(s => !idsInRequest.Contains(s.Id)).ToList();

        // Eliminar estudiantes que no vienen en la lista
        foreach (var student in toDelete)
        {
            _students.Remove(student);
        }

        // Actualizar estudiantes existentes (reemplazar con nueva instancia del Value Object)
        foreach (var updateItem in toUpdate)
        {
            var existingStudent = _students.FirstOrDefault(s => s.Id == updateItem.Id!.Value);
            if (existingStudent != null)
            {
                // Remover el viejo
                _students.Remove(existingStudent);

                // Agregar el nuevo con el mismo Id pero nombre actualizado
                _students.Add(PpaStudent.CreateWithId(updateItem.Id!.Value, updateItem.Name!));
            }
        }

        // Crear nuevos estudiantes
        foreach (var createItem in toCreate)
        {
            _students.Add(PpaStudent.Create(createItem.Name!));
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca este PPA como continuación de otro PPA.
    /// </summary>
    /// <param name="originalPpaId">ID del PPA original que se está continuando.</param>
    /// <exception cref="ArgumentException">Si el ID es vacío o es el mismo ID de este PPA.</exception>
    /// <exception cref="InvalidOperationException">Si el PPA ya tiene configurada una continuación de otro PPA.</exception>
    public void SetContinuationOf(Guid originalPpaId)
    {
        if (originalPpaId == Guid.Empty)
            throw new ArgumentException(
                "El ID del PPA original no puede estar vacío.",
                nameof(originalPpaId));

        if (originalPpaId == Id)
            throw new ArgumentException(
                "Un PPA no puede ser continuación de sí mismo.",
                nameof(originalPpaId));

        if (ContinuationOfPpaId.HasValue)
            throw new InvalidOperationException(
                $"Este PPA ya está configurado como continuación del PPA '{ContinuationOfPpaId.Value}'.");

        ContinuationOfPpaId = originalPpaId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca que este PPA ha sido continuado por otro PPA en un período académico diferente.
    /// </summary>
    /// <param name="newPpaId">ID del nuevo PPA que continúa este.</param>
    /// <param name="targetAcademicPeriodId">ID del periodo académico del PPA que continúa.</param>
    /// <exception cref="ArgumentException">Si los IDs son vacíos o el nuevo PPA es el mismo que este.</exception>
    /// <exception cref="InvalidOperationException">Si el PPA ya ha sido continuado.</exception>
    public void MarkContinuedBy(Guid newPpaId, Guid targetAcademicPeriodId)
    {
        if (newPpaId == Guid.Empty)
            throw new ArgumentException(
                "El ID del nuevo PPA no puede estar vacío.",
                nameof(newPpaId));

        if (targetAcademicPeriodId == Guid.Empty)
            throw new ArgumentException(
                "El ID del periodo académico del nuevo PPA no puede estar vacío.",
                nameof(targetAcademicPeriodId));

        if (newPpaId == Id)
            throw new ArgumentException(
                "Un PPA no puede ser continuado por sí mismo.",
                nameof(newPpaId));

        if (targetAcademicPeriodId == AcademicPeriodId)
            throw new InvalidOperationException(
                "El PPA de continuación debe estar en un periodo académico diferente.");

        if (ContinuedByPpaId.HasValue)
            throw new InvalidOperationException(
                $"Este PPA ya ha sido continuado por el PPA '{ContinuedByPpaId.Value}'.");

        ContinuedByPpaId = newPpaId;
        UpdatedAt = DateTime.UtcNow;
    }
}
