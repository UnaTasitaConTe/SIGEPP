using Application.Ppa.Commands;
using Application.Ppa.DTOs;
using Application.Security;
using Domain.Academics.Repositories;
using Domain.Common;
using Domain.Dictionaries;
using Domain.Ppa;
using Domain.Ppa.Entities;
using Domain.Ppa.Repositories;
using Domain.Users;
using System.Text.Json;
using static Domain.Security.Catalogs.Permissions;

namespace Application.Ppa;

/// <summary>
/// Servicio de aplicación para gestión de PPAs en SIGEPP.
/// Orquesta los casos de uso relacionados con Proyectos Pedagógicos de Aula.
/// </summary>
public sealed class PpaAppService
{
    private readonly IPpaRepository _ppaRepository;
    private readonly IAcademicPeriodRepository _periodRepository;
    private readonly ITeacherAssignmentRepository _teacherAssignmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPpaHistoryRepository _historyRepository;
    private readonly IPpaAttachmentRepository _ppaAttachmentRepository;

    public PpaAppService(
        IPpaRepository ppaRepository,
        IAcademicPeriodRepository periodRepository,
        ITeacherAssignmentRepository teacherAssignmentRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IPpaHistoryRepository historyRepository,
        IPpaAttachmentRepository ppaAttachmentRepository)
    {
        _ppaRepository = ppaRepository ?? throw new ArgumentNullException(nameof(ppaRepository));
        _periodRepository = periodRepository ?? throw new ArgumentNullException(nameof(periodRepository));
        _teacherAssignmentRepository = teacherAssignmentRepository ?? throw new ArgumentNullException(nameof(teacherAssignmentRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
        _ppaAttachmentRepository = ppaAttachmentRepository;
    }

    /// <summary>
    /// Obtiene un PPA por su ID con información detallada.
    /// </summary>
    /// <param name="id">ID del PPA.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>PPA detallado o null si no existe.</returns>
    public async Task<PpaDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var ppa = await _ppaRepository.GetByIdAsync(id, ct);
        if (ppa == null)
            return null;

        // Obtener información agregada
        var period = await _periodRepository.GetByIdAsync(ppa.AcademicPeriodId, ct);
        var teacher = await _userRepository.GetByIdAsync(ppa.PrimaryTeacherId, ct);

        // Obtener detalles de las asignaciones docente-asignatura
        var assignmentDetails = new List<PpaTeacherAssignmentDetailDto>();
        foreach (var assignmentId in ppa.TeacherAssignmentIds)
        {
            var assignment = await _teacherAssignmentRepository.GetByIdAsync(assignmentId, ct);
            if (assignment != null)
            {
                var assignmentTeacher = await _userRepository.GetByIdAsync(assignment.TeacherId, ct);

                assignmentDetails.Add(new PpaTeacherAssignmentDetailDto
                {
                    TeacherAssignmentId = assignment.Id,
                    TeacherId = assignment.TeacherId,
                    TeacherName = assignmentTeacher?.Name,
                    SubjectId = assignment.SubjectId,
                    SubjectCode = assignment.SubjectCode,
                    SubjectName = assignment.SubjectName,
                    //SubjectSemester = assignment.Semester
                });
            }
        }

        var students = ppa.Students.Select(s => new PpaStudentDto
        {
            Id = s.Id,
            Name = s.Name
        }).ToList();

        return ToDetailDto(ppa, period?.Code, teacher?.Name, assignmentDetails, students);
    }

    /// <summary>
    /// Obtiene todos los PPAs de un período académico específico.
    /// </summary>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de PPAs del período.</returns>
    public async Task<IReadOnlyCollection<PpaDto>> GetByAcademicPeriodAsync(
        Guid academicPeriodId,
        CancellationToken ct = default)
    {
        var ppas = await _ppaRepository.GetByAcademicPeriodAsync(academicPeriodId, ct);
        return ppas.Select(ToDto).ToList().AsReadOnly();
    }

    /// <summary>
    /// Obtiene todos los PPAs de un docente en un período académico específico.
    /// </summary>
    /// <param name="teacherId">ID del docente.</param>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de PPAs del docente en el período.</returns>
    public async Task<IReadOnlyCollection<PpaDto>> GetByTeacherAsync(
        Guid teacherId,
        Guid academicPeriodId,
        CancellationToken ct = default)
    {
        var ppas = await _ppaRepository.GetByTeacherAsync(teacherId, academicPeriodId, ct);
        return ppas.Select(ToDto).ToList().AsReadOnly();
    }

    /// <summary>
    /// Obtiene todos los PPAs en los que un docente está involucrado
    /// (como responsable o a través de asignaciones docente-asignatura).
    /// </summary>
    /// <param name="teacherId">ID del docente.</param>
    /// <param name="academicPeriodId">ID del período académico (opcional para filtrar).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista resumida de PPAs del docente.</returns>
    public async Task<IReadOnlyCollection<PpaSummaryDto>> GetPpasForTeacherAsync(
        Guid teacherId,
        Guid? academicPeriodId = null,
        CancellationToken ct = default)
    {
        // Obtener PPAs donde el docente es responsable
        IReadOnlyCollection<Domain.Ppa.Entities.Ppa> ppasList;

        if (academicPeriodId.HasValue)
        {
            ppasList = await _ppaRepository.GetByTeacherAsync(teacherId, academicPeriodId.Value, ct);
        }
        else
        {
            // Si no se especifica periodo, obtener de todos los periodos
            // (necesitaríamos un método en el repositorio, por ahora usaremos una alternativa)
            ppasList = new List<Domain.Ppa.Entities.Ppa>().AsReadOnly();
        }

        // Obtener PPAs donde el docente tiene asignaciones
        // Primero obtener todas las asignaciones del docente
        var teacherAssignments = await _teacherAssignmentRepository.GetByTeacherAsync(teacherId, ct);

        if (academicPeriodId.HasValue)
        {
            teacherAssignments = teacherAssignments
                .Where(a => a.AcademicPeriodId == academicPeriodId.Value)
                .ToList()
                .AsReadOnly();
        }

        // Para cada asignación, obtener PPAs que la incluyan
        var ppasFromAssignments = new List<Domain.Ppa.Entities.Ppa>();
        foreach (var assignment in teacherAssignments)
        {
            var ppasForAssignment = await _ppaRepository.GetByTeacherAssignmentAsync(assignment.Id, ct);
            ppasFromAssignments.AddRange(ppasForAssignment);
        }

        // Combinar ambas listas y eliminar duplicados
        var allPpas = ppasList.Concat(ppasFromAssignments)
            .GroupBy(p => p.Id)
            .Select(g => g.First())
            .OrderByDescending(p => p.CreatedAt)
            .ToList();

        // Mapear a DTOs resumidos
        var result = new List<PpaSummaryDto>();
        foreach (var ppa in allPpas)
        {
            var period = await _periodRepository.GetByIdAsync(ppa.AcademicPeriodId, ct);
            var teacher = await _userRepository.GetByIdAsync(ppa.ResponsibleTeacherId, ct);

            result.Add(new PpaSummaryDto
            {
                Id = ppa.Id,
                Title = ppa.Title,
                Status = ppa.Status,
                AcademicPeriodId = ppa.AcademicPeriodId,
                AcademicPeriodCode = period?.Code,
                ResponsibleTeacherId = ppa.ResponsibleTeacherId,
                ResponsibleTeacherName = teacher?.Name,
                AssignmentsCount = ppa.TeacherAssignmentIds.Count,
                StudentsCount = ppa.Students.Count,
                IsContinuation = ppa.ContinuationOfPpaId != null,
                HasContinuation = ppa.ContinuedByPpaId != null,
                CreatedAt = ppa.CreatedAt,
                UpdatedAt = ppa.UpdatedAt
            });
        }

        return result.AsReadOnly();
    }

    /// <summary>
    /// Obtiene una lista paginada de PPAs con filtros avanzados.
    /// </summary>
    /// <param name="query">Consulta con parámetros de paginación y filtros.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resultado paginado con PPAs resumidos.</returns>
    public async Task<PagedResult<PpaSummaryDto>> GetPpasPagedAsync(
        PpaPagedQuery query,
        CancellationToken ct = default)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        // Llamar al repositorio con todos los filtros
        var pagedResult = await _ppaRepository.GetPagedAsync(
            page: query.Page,
            pageSize: query.PageSize,
            search: query.Search,
            academicPeriodId: query.AcademicPeriodId,
            status: query.Status,
            responsibleTeacherId: query.ResponsibleTeacherId,
            teacherId: query.TeacherId,
            ct: ct);

        // Mapear cada PPA a PpaSummaryDto
        var dtos = new List<PpaSummaryDto>();
        foreach (var ppa in pagedResult.Items)
        {
            var period = await _periodRepository.GetByIdAsync(ppa.AcademicPeriodId, ct);
            var teacher = await _userRepository.GetByIdAsync(ppa.ResponsibleTeacherId, ct);

            dtos.Add(new PpaSummaryDto
            {
                Id = ppa.Id,
                Title = ppa.Title,
                Status = ppa.Status,
                AcademicPeriodId = ppa.AcademicPeriodId,
                AcademicPeriodCode = period?.Code,
                ResponsibleTeacherId = ppa.ResponsibleTeacherId,
                ResponsibleTeacherName = teacher?.Name,
                AssignmentsCount = ppa.TeacherAssignmentIds.Count,
                StudentsCount = ppa.Students.Count,
                IsContinuation = ppa.ContinuationOfPpaId != null,
                HasContinuation = ppa.ContinuedByPpaId != null,
                CreatedAt = ppa.CreatedAt,
                UpdatedAt = ppa.UpdatedAt
            });
        }

        return new PagedResult<PpaSummaryDto>(
            items: dtos.AsReadOnly(),
            page: pagedResult.Page,
            pageSize: pagedResult.PageSize,
            totalItems: pagedResult.TotalItems);
    }

    /// <summary>
    /// Obtiene el historial completo de un PPA.
    /// </summary>
    /// <param name="ppaId">ID del PPA.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de entradas de historial ordenadas por fecha descendente.</returns>
    public async Task<IReadOnlyCollection<PpaHistoryDto>> GetHistoryAsync(
        Guid ppaId,
        CancellationToken ct = default)
    {
        // Verificar que el PPA existe
        var ppa = await _ppaRepository.GetByIdAsync(ppaId, ct);
        if (ppa == null)
            throw new InvalidOperationException($"PPA con ID '{ppaId}' no encontrado.");

        // Obtener historial
        var history = await _historyRepository.GetByPpaAsync(ppaId, ct);

        // Mapear a DTOs
        var result = new List<PpaHistoryDto>();
        foreach (var entry in history)
        {
            var user = await _userRepository.GetByIdAsync(entry.PerformedByUserId, ct);

            result.Add(new PpaHistoryDto
            {
                Id = entry.Id,
                PpaId = entry.PpaId,
                PerformedByUserId = entry.PerformedByUserId,
                PerformedByUserName = user?.Name,
                PerformedAt = entry.PerformedAt,
                ActionType = entry.ActionType,
                ActionTypeDescription = GetActionTypeDescription(entry.ActionType),
                OldValue = entry.OldValue,
                NewValue = entry.NewValue,
                Notes = entry.Notes
            });
        }

        return result.AsReadOnly();
    }

    /// <summary>
    /// Crea un nuevo PPA en el sistema.
    /// </summary>
    /// <param name="command">Comando con los datos del PPA a crear.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>ID del PPA creado.</returns>
    /// <exception cref="InvalidOperationException">
    /// Si el período no existe/no está activo,
    /// las asignaciones no existen/no pertenecen al período/no pertenecen al docente autenticado,
    /// o ya existe un PPA con el mismo nombre en el periodo.
    /// </exception>
    public async Task<Guid> CreateAsync(CreatePpaCommand command, CancellationToken ct = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // 1. Obtener el usuario autenticado (será el responsable del PPA)
        var currentUserId = _currentUserService.GetCurrentUserId();

        // 2. Validar que el período académico exista y esté activo
        var period = await _periodRepository.GetByIdAsync(command.AcademicPeriodId, ct);
        if (period == null)
            throw new InvalidOperationException($"El período académico con ID '{command.AcademicPeriodId}' no existe.");

        if (!period.IsActive)
            throw new InvalidOperationException($"El período académico '{period.Code}' no está activo.");

        // 3. Validar unicidad del título en el periodo
        var titleExists = await _ppaRepository.ExistsWithTitleAsync(
            command.Title,
            command.AcademicPeriodId,
            excludePpaId: null,
            ct);

        if (titleExists)
            throw new InvalidOperationException(
                $"Ya existe un PPA con el título '{command.Title}' en el período académico '{period.Code}'. " +
                "Los títulos deben ser únicos dentro del mismo período.");

        // 4. Validar que todas las asignaciones docente-asignatura existan, pertenezcan al período
        // y pertenezcan al docente autenticado
        foreach (var assignmentId in command.TeacherAssignmentIds)
        {
            var assignment = await _teacherAssignmentRepository.GetByIdAsync(assignmentId, ct);
            if (assignment == null)
                throw new InvalidOperationException($"La asignación docente-asignatura con ID '{assignmentId}' no existe.");

            if (assignment.AcademicPeriodId != command.AcademicPeriodId)
                throw new InvalidOperationException(
                    $"La asignación docente-asignatura con ID '{assignmentId}' no pertenece al período académico especificado.");

            if (assignment.TeacherId != currentUserId)
                throw new InvalidOperationException(
                    $"La asignación docente-asignatura con ID '{assignmentId}' no pertenece al docente autenticado. " +
                    "Solo puede crear PPAs con sus propias asignaciones.");

            if (!assignment.IsActive)
                throw new InvalidOperationException(
                    $"La asignación docente-asignatura con ID '{assignmentId}' no está activa.");
        }

        // 5. Crear el PPA usando el método de fábrica del dominio
        // El docente autenticado será el responsable
        var ppa = Domain.Ppa.Entities.Ppa.Create(
            title: command.Title,
            academicPeriodId: command.AcademicPeriodId,
            primaryTeacherId: currentUserId,
            generalObjective: command.GeneralObjective,
            specificObjectives: command.SpecificObjectives,
            description: command.Description);

        // 6. Establecer las asignaciones docente-asignatura usando el método de dominio
        ppa.SetTeacherAssignments(command.TeacherAssignmentIds);

        // 7. Establecer los estudiantes si se proporcionaron
        if (command.StudentNames.Any())
        {
            ppa.SetStudents(command.StudentNames);
        }

        // 8. Guardar el PPA
        await _ppaRepository.AddAsync(ppa, ct);

        // 9. Registrar entrada de historial: PPA creado
        var historyEntry = PpaHistoryEntry.Create(
            ppaId: ppa.Id,
            performedByUserId: currentUserId,
            actionType: PpaHistoryActionType.Created,
            oldValue: null,
            newValue: $"Título: {ppa.Title}, Período: {period.Code}",
            notes: $"PPA creado con {command.TeacherAssignmentIds.Count} asignaciones y {command.StudentNames.Count} estudiantes.");

        await _historyRepository.AddAsync(historyEntry, ct);

        return ppa.Id;
    }

    /// <summary>
    /// Crea un nuevo PPA como administrador, especificando el docente responsable.
    /// </summary>
    /// <param name="command">Comando con los datos del PPA a crear.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>ID del PPA creado.</returns>
    /// <exception cref="InvalidOperationException">
    /// Si el período no existe/no está activo,
    /// el docente responsable no existe,
    /// las asignaciones no existen/no pertenecen al período,
    /// o ya existe un PPA con el mismo nombre en el periodo.
    /// </exception>
    public async Task<Guid> CreateAsAdminAsync(CreatePpaAsAdminCommand command, CancellationToken ct = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // 1. Obtener el usuario autenticado (para el historial)
        var currentUserId = _currentUserService.GetCurrentUserId();

        // 2. Validar que el período académico exista y esté activo
        var period = await _periodRepository.GetByIdAsync(command.AcademicPeriodId, ct);
        if (period == null)
            throw new InvalidOperationException($"El período académico con ID '{command.AcademicPeriodId}' no existe.");

        if (!period.IsActive)
            throw new InvalidOperationException($"El período académico '{period.Code}' no está activo.");

        // 3. Validar que el docente responsable exista
        var responsibleTeacher = await _userRepository.GetByIdAsync(command.ResponsibleTeacherId, ct);
        if (responsibleTeacher == null)
            throw new InvalidOperationException($"El docente con ID '{command.ResponsibleTeacherId}' no existe.");

        // 4. Validar unicidad del título en el periodo
        var titleExists = await _ppaRepository.ExistsWithTitleAsync(
            command.Title,
            command.AcademicPeriodId,
            excludePpaId: null,
            ct);

        if (titleExists)
            throw new InvalidOperationException(
                $"Ya existe un PPA con el título '{command.Title}' en el período académico '{period.Code}'. " +
                "Los títulos deben ser únicos dentro del mismo período.");

        // 5. Validar que todas las asignaciones docente-asignatura existan y pertenezcan al período
        foreach (var assignmentId in command.TeacherAssignmentIds)
        {
            var assignment = await _teacherAssignmentRepository.GetByIdAsync(assignmentId, ct);
            if (assignment == null)
                throw new InvalidOperationException($"La asignación docente-asignatura con ID '{assignmentId}' no existe.");

            if (assignment.AcademicPeriodId != command.AcademicPeriodId)
                throw new InvalidOperationException(
                    $"La asignación docente-asignatura con ID '{assignmentId}' no pertenece al período académico especificado.");

            if (!assignment.IsActive)
                throw new InvalidOperationException(
                    $"La asignación docente-asignatura con ID '{assignmentId}' no está activa.");
        }

        // 6. Crear el PPA usando el método de fábrica del dominio
        var ppa = Domain.Ppa.Entities.Ppa.Create(
            title: command.Title,
            academicPeriodId: command.AcademicPeriodId,
            primaryTeacherId: command.ResponsibleTeacherId,
            generalObjective: command.GeneralObjective,
            specificObjectives: command.SpecificObjectives,
            description: command.Description);

        // 7. Establecer las asignaciones docente-asignatura
        ppa.SetTeacherAssignments(command.TeacherAssignmentIds);

        // 8. Establecer los estudiantes si se proporcionaron
        if (command.StudentNames.Any())
        {
            ppa.SetStudents(command.StudentNames);
        }

        // 9. Guardar el PPA
        await _ppaRepository.AddAsync(ppa, ct);

        // 10. Registrar entrada de historial: PPA creado por admin
        var historyEntry = PpaHistoryEntry.Create(
            ppaId: ppa.Id,
            performedByUserId: currentUserId,
            actionType: PpaHistoryActionType.Created,
            oldValue: null,
            newValue: $"Título: {ppa.Title}, Período: {period.Code}",
            notes: $"PPA creado por administrador. Responsable: {responsibleTeacher.Name}, " +
                   $"Asignaciones: {command.TeacherAssignmentIds.Count}, Estudiantes: {command.StudentNames.Count}");

        await _historyRepository.AddAsync(historyEntry, ct);

        return ppa.Id;
    }

    /// <summary>
    /// Actualiza un PPA como administrador, permitiendo cambiar el docente responsable.
    /// </summary>
    /// <param name="command">Comando con los datos a actualizar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">
    /// Si el PPA no existe o se intenta hacer cambios no permitidos.
    /// </exception>
    public async Task UpdateAsAdminAsync(UpdatePpaAsAdminCommand command, CancellationToken ct = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // 1. Obtener el PPA
        var ppa = await _ppaRepository.GetByIdAsync(command.Id, ct);
        if (ppa == null)
            throw new InvalidOperationException($"PPA con ID '{command.Id}' no encontrado.");

        // 2. Obtener usuario autenticado
        var currentUser = _currentUserService.GetCurrentUser();
        if (currentUser == null)
            throw new InvalidOperationException("No se pudo obtener el usuario autenticado.");

        var currentUserId = currentUser.UserId;

        // 3. Preparar lista de entradas de historial
        var historyEntries = new List<PpaHistoryEntry>();

        // 4. Actualizar título si cambió
        if (command.Title != ppa.Title)
        {
            // Validar unicidad del nuevo título
            var titleExists = await _ppaRepository.ExistsWithTitleAsync(
                command.Title,
                ppa.AcademicPeriodId,
                excludePpaId: ppa.Id,
                ct);

            if (titleExists)
                throw new InvalidOperationException(
                    $"Ya existe otro PPA con el título '{command.Title}' en este período académico. " +
                    "Los títulos deben ser únicos dentro del mismo período.");

            var oldTitle = ppa.Title;
            ppa.UpdateTitle(command.Title);

            historyEntries.Add(PpaHistoryEntry.Create(
                ppaId: ppa.Id,
                performedByUserId: currentUserId,
                actionType: PpaHistoryActionType.UpdatedTitle,
                oldValue: oldTitle,
                newValue: command.Title));
        }

        // 5. Actualizar descripción si cambió
        if (command.Description != ppa.Description)
        {
            ppa.UpdateDescription(command.Description);

            historyEntries.Add(PpaHistoryEntry.Create(
                ppaId: ppa.Id,
                performedByUserId: currentUserId,
                actionType: PpaHistoryActionType.UpdatedDescription,
                oldValue: ppa.Description,
                newValue: command.Description));
        }

        // 6. Actualizar objetivo general si cambió
        if (command.GeneralObjective != ppa.GeneralObjective)
        {
            ppa.UpdateGeneralObjective(command.GeneralObjective);

            historyEntries.Add(PpaHistoryEntry.Create(
                ppaId: ppa.Id,
                performedByUserId: currentUserId,
                actionType: PpaHistoryActionType.UpdatedGeneralObjective,
                oldValue: ppa.GeneralObjective,
                newValue: command.GeneralObjective));
        }

        // 7. Actualizar objetivos específicos si cambió
        if (command.SpecificObjectives != ppa.SpecificObjectives)
        {
            ppa.UpdateSpecificObjectives(command.SpecificObjectives);

            historyEntries.Add(PpaHistoryEntry.Create(
                ppaId: ppa.Id,
                performedByUserId: currentUserId,
                actionType: PpaHistoryActionType.UpdatedSpecificObjectives,
                oldValue: ppa.SpecificObjectives,
                newValue: command.SpecificObjectives));
        }

        // 8. Cambiar docente responsable si es diferente
        if (command.ResponsibleTeacherId != ppa.ResponsibleTeacherId)
        {
            // Validar que el nuevo docente exista
            var newTeacher = await _userRepository.GetByIdAsync(command.ResponsibleTeacherId, ct);
            if (newTeacher == null)
                throw new InvalidOperationException(
                    $"El docente con ID '{command.ResponsibleTeacherId}' no existe.");

            var oldTeacherId = ppa.ResponsibleTeacherId;
            ppa.ChangeResponsibleTeacher(command.ResponsibleTeacherId);

            historyEntries.Add(PpaHistoryEntry.Create(
                ppaId: ppa.Id,
                performedByUserId: currentUserId,
                actionType: PpaHistoryActionType.ChangedResponsibleTeacher,
                oldValue: oldTeacherId.ToString(),
                newValue: command.ResponsibleTeacherId.ToString(),
                notes: $"Docente anterior: {oldTeacherId}, Nuevo docente: {newTeacher.Name}"));
        }

        // 9. Cambiar asignaciones
        var newAssignmentsList = command.TeacherAssignmentIds.ToList();

        // Validar que todas las asignaciones existan y pertenezcan al mismo período
        foreach (var assignmentId in newAssignmentsList)
        {
            var assignment = await _teacherAssignmentRepository.GetByIdAsync(assignmentId, ct);
            if (assignment == null)
                throw new InvalidOperationException(
                    $"La asignación docente-asignatura con ID '{assignmentId}' no existe.");

            if (assignment.AcademicPeriodId != ppa.AcademicPeriodId)
                throw new InvalidOperationException(
                    $"La asignación docente-asignatura con ID '{assignmentId}' no pertenece al período académico del PPA.");

            if (!assignment.IsActive)
                throw new InvalidOperationException(
                    $"La asignación docente-asignatura con ID '{assignmentId}' no está activa.");
        }

        // Verificar si realmente cambió la lista
        var currentAssignments = ppa.TeacherAssignmentIds.OrderBy(x => x).ToList();
        var newAssignments = newAssignmentsList.OrderBy(x => x).ToList();

        if (!currentAssignments.SequenceEqual(newAssignments))
        {
            var oldAssignmentsStr = string.Join(", ", currentAssignments);
            var newAssignmentsStr = string.Join(", ", newAssignments);

            ppa.SetTeacherAssignments(newAssignmentsList);

            historyEntries.Add(PpaHistoryEntry.Create(
                ppaId: ppa.Id,
                performedByUserId: currentUserId,
                actionType: PpaHistoryActionType.UpdatedAssignments,
                oldValue: oldAssignmentsStr,
                newValue: newAssignmentsStr,
                notes: $"Total asignaciones: {newAssignmentsList.Count}"));
        }

        // 10. Sincronizar estudiantes usando el nuevo método de sincronización
        if (command.Students.Any())
        {
            // Guardar estado anterior para historial
            var currentStudents = ppa.Students.Select(s => (s.Id, s.Name)).OrderBy(x => x.Name).ToList();

            // Preparar datos para sincronización
            var studentsToSyncx = command.Students
                .Select(s => (Id: s.Id, Name: s.Name))
                .ToList();

            // Ejecutar sincronización (update/create/delete)
            ppa.SyncStudents(studentsToSyncx);

            // Comparar estados para registrar en historial
            var newStudents = ppa.Students.Select(s => (s.Id, s.Name)).OrderBy(x => x.Name).ToList();

            // Registrar en historial si hubo cambios
            var studentsChanged =
                currentStudents.Count != newStudents.Count ||
                !currentStudents.Select(s => s.Name).SequenceEqual(
                    newStudents.Select(s => s.Name),
                    StringComparer.OrdinalIgnoreCase);

            if (studentsChanged)
            {
                var oldStudentsStr = currentStudents.Any()
                    ? string.Join(", ", currentStudents.Select(s => s.Name))
                    : "Sin estudiantes";
                var newStudentsStr = newStudents.Any()
                    ? string.Join(", ", newStudents.Select(s => s.Name))
                    : "Sin estudiantes";

                var created = command.Students.Count(s => !s.Id.HasValue || s.Id.Value == Guid.Empty);
                var updated = command.Students.Count(s => s.Id.HasValue && s.Id.Value != Guid.Empty);
                var deleted = currentStudents.Count - updated;

                historyEntries.Add(PpaHistoryEntry.Create(
                    ppaId: ppa.Id,
                    performedByUserId: currentUserId,
                    actionType: PpaHistoryActionType.UpdatedStudents,
                    oldValue: oldStudentsStr,
                    newValue: newStudentsStr,
                    notes: $"Total: {newStudents.Count} estudiantes. Creados: {created}, Actualizados: {updated}, Eliminados: {deleted}"));
            }
        }
        else
        {
            // Si no se proporcionan estudiantes, eliminar todos los existentes
            var currentStudents = ppa.Students.ToList();
            if (currentStudents.Count != 0)
            {
                var oldStudentsStr = string.Join(", ", currentStudents.Select(s => s.Name));

                // Sincronizar con lista vacía elimina todos
                ppa.SyncStudents([]);

                historyEntries.Add(PpaHistoryEntry.Create(
                    ppaId: ppa.Id,
                    performedByUserId: currentUserId,
                    actionType: PpaHistoryActionType.UpdatedStudents,
                    oldValue: oldStudentsStr,
                    newValue: "Sin estudiantes",
                    notes: $"Todos los estudiantes ({currentStudents.Count}) fueron eliminados"));
            }
        }

        await _ppaRepository.UpdateAsync(
           ppa,
            ct);

        // 12. Guardar todas las entradas de historial
        if (historyEntries.Any())
        {
            await _historyRepository.AddRangeAsync(historyEntries, ct);
        }


    }

    /// <summary>
    /// Actualiza los datos de un PPA existente.
    /// </summary>
    /// <param name="command">Comando con los datos a actualizar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">
    /// Si el PPA no existe, el usuario no tiene permisos,
    /// o se intenta hacer cambios no permitidos según el estado del PPA.
    /// </exception>
    public async Task UpdateAsync(UpdatePpaCommand command, CancellationToken ct = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // 1. Obtener el PPA
        var ppa = await _ppaRepository.GetByIdAsync(command.Id, ct);
        if (ppa == null)
            throw new InvalidOperationException($"PPA con ID '{command.Id}' no encontrado.");

        // 2. Obtener usuario autenticado
        var currentUser = _currentUserService.GetCurrentUser();
        if (currentUser == null)
            throw new InvalidOperationException("No se pudo obtener el usuario autenticado.");

        var currentUserId = currentUser.UserId;

        // 3. Validar permisos: debe ser ADMIN o el docente responsable actual
        var isAdmin = currentUser.Roles.Contains("ADMIN");
        var isResponsible = ppa.ResponsibleTeacherId == currentUserId;

        if (!isAdmin && !isResponsible)
            throw new InvalidOperationException(
                "No tiene permisos para actualizar este PPA. " +
                "Solo el docente responsable o un administrador pueden actualizar el PPA.");

        // 4. Determinar si el PPA está en un estado editable
        var isInEditableState = ppa.Status != PpaStatus.Completed && ppa.Status != PpaStatus.Archived;

        // 5. Validar que no se cambien ciertos campos si no está en estado editable
        if (!isInEditableState)
        {
            if (command.NewResponsibleTeacherId.HasValue)
                throw new InvalidOperationException(
                    "No se puede cambiar el docente responsable de un PPA en estado Completed o Archived.");

            if (command.NewTeacherAssignmentIds != null)
                throw new InvalidOperationException(
                    "No se pueden cambiar las asignaciones de un PPA en estado Completed o Archived.");

            if (command.NewStudents != null)
                throw new InvalidOperationException(
                    "No se pueden cambiar los estudiantes de un PPA en estado Completed o Archived.");
        }

        // 6. Preparar lista de entradas de historial
        var historyEntries = new List<PpaHistoryEntry>();
        // 7. Actualizar título si cambió
        if (command.Title != ppa.Title)
        {
            // Validar unicidad del nuevo título
            var titleExists = await _ppaRepository.ExistsWithTitleAsync(
                command.Title,
                ppa.AcademicPeriodId,
                excludePpaId: ppa.Id,
                ct);

            if (titleExists)
                throw new InvalidOperationException(
                    $"Ya existe otro PPA con el título '{command.Title}' en este período académico. " +
                    "Los títulos deben ser únicos dentro del mismo período.");

            var oldTitle = ppa.Title;
            ppa.UpdateTitle(command.Title);

            historyEntries.Add(PpaHistoryEntry.Create(
                ppaId: ppa.Id,
                performedByUserId: currentUserId,
                actionType: PpaHistoryActionType.UpdatedTitle,
                oldValue: oldTitle,
                newValue: command.Title));
        }

        // 8. Actualizar descripción si cambió
        if (command.Description != ppa.Description)
        {
            ppa.UpdateDescription(command.Description);

            historyEntries.Add(PpaHistoryEntry.Create(
                ppaId: ppa.Id,
                performedByUserId: currentUserId,
                actionType: PpaHistoryActionType.UpdatedDescription,
                oldValue: ppa.Description,
                newValue: command.Description));
        }

        // 9. Actualizar objetivo general si cambió
        if (command.GeneralObjective != ppa.GeneralObjective)
        {
            ppa.UpdateGeneralObjective(command.GeneralObjective);

            historyEntries.Add(PpaHistoryEntry.Create(
                ppaId: ppa.Id,
                performedByUserId: currentUserId,
                actionType: PpaHistoryActionType.UpdatedGeneralObjective,
                oldValue: ppa.GeneralObjective,
                newValue: command.GeneralObjective));
        }

        // 10. Actualizar objetivos específicos si cambió
        if (command.SpecificObjectives != ppa.SpecificObjectives)
        {
            ppa.UpdateSpecificObjectives(command.SpecificObjectives);

            historyEntries.Add(PpaHistoryEntry.Create(
                ppaId: ppa.Id,
                performedByUserId: currentUserId,
                actionType: PpaHistoryActionType.UpdatedSpecificObjectives,
                oldValue: ppa.SpecificObjectives,
                newValue: command.SpecificObjectives));
        }

        // 11. Cambiar docente responsable si se especificó y es diferente
        if (command.NewResponsibleTeacherId.HasValue &&
            command.NewResponsibleTeacherId.Value != ppa.ResponsibleTeacherId)
        {
            // Validar que el nuevo docente exista
            var newTeacher = await _userRepository.GetByIdAsync(command.NewResponsibleTeacherId.Value, ct);
            if (newTeacher == null)
                throw new InvalidOperationException(
                    $"El docente con ID '{command.NewResponsibleTeacherId.Value}' no existe.");

            var oldTeacherId = ppa.ResponsibleTeacherId;
            ppa.ChangeResponsibleTeacher(command.NewResponsibleTeacherId.Value);

            historyEntries.Add(PpaHistoryEntry.Create(
                ppaId: ppa.Id,
                performedByUserId: currentUserId,
                actionType: PpaHistoryActionType.ChangedResponsibleTeacher,
                oldValue: oldTeacherId.ToString(),
                newValue: command.NewResponsibleTeacherId.Value.ToString(),
                notes: $"Docente anterior: {oldTeacherId}, Nuevo docente: {newTeacher.Name}"));
        }

        // 12. Cambiar asignaciones si se especificaron
        if (command.NewTeacherAssignmentIds != null)
        {
            var newAssignmentsList = command.NewTeacherAssignmentIds.ToList();

            // Validar que todas las asignaciones existan y pertenezcan al mismo período
            foreach (var assignmentId in newAssignmentsList)
            {
                var assignment = await _teacherAssignmentRepository.GetByIdAsync(assignmentId, ct);
                if (assignment == null)
                    throw new InvalidOperationException(
                        $"La asignación docente-asignatura con ID '{assignmentId}' no existe.");

                if (assignment.AcademicPeriodId != ppa.AcademicPeriodId)
                    throw new InvalidOperationException(
                        $"La asignación docente-asignatura con ID '{assignmentId}' no pertenece al período académico del PPA.");

                if (!assignment.IsActive)
                    throw new InvalidOperationException(
                        $"La asignación docente-asignatura con ID '{assignmentId}' no está activa.");
            }

            // Verificar si realmente cambió la lista
            var currentAssignments = ppa.TeacherAssignmentIds.OrderBy(x => x).ToList();
            var newAssignments = newAssignmentsList.OrderBy(x => x).ToList();

            if (!currentAssignments.SequenceEqual(newAssignments))
            {
                var oldAssignmentsStr = string.Join(", ", currentAssignments);
                var newAssignmentsStr = string.Join(", ", newAssignments);

                ppa.SetTeacherAssignments(newAssignmentsList);

                historyEntries.Add(PpaHistoryEntry.Create(
                    ppaId: ppa.Id,
                    performedByUserId: currentUserId,
                    actionType: PpaHistoryActionType.UpdatedAssignments,
                    oldValue: oldAssignmentsStr,
                    newValue: newAssignmentsStr,
                    notes: $"Total asignaciones: {newAssignmentsList.Count}"));
            }
        }

        // 13. Sincronizar estudiantes si se especificaron
        if (command.NewStudents != null)
        {
            if (command.NewStudents.Any())
            {
                // Guardar estado anterior para historial
                var currentStudents = ppa.Students.Select(s => (s.Id, s.Name)).OrderBy(x => x.Name).ToList();

                // Preparar datos para sincronización
                var studentsToSync = command.NewStudents
                    .Select(s => (Id: s.Id, Name: s.Name))
                    .ToList();

                // Ejecutar sincronización (update/create/delete)
                ppa.SyncStudents(studentsToSync);

                // Comparar estados para registrar en historial
                var newStudents = ppa.Students.Select(s => (s.Id, s.Name)).OrderBy(x => x.Name).ToList();

                // Registrar en historial si hubo cambios
                var studentsChanged =
                    currentStudents.Count != newStudents.Count ||
                    !currentStudents.Select(s => s.Name).SequenceEqual(
                        newStudents.Select(s => s.Name),
                        StringComparer.OrdinalIgnoreCase);

                if (studentsChanged)
                {
                    var oldStudentsStr = currentStudents.Any()
                        ? string.Join(", ", currentStudents.Select(s => s.Name))
                        : "Sin estudiantes";
                    var newStudentsStr = newStudents.Any()
                        ? string.Join(", ", newStudents.Select(s => s.Name))
                        : "Sin estudiantes";

                    var created = command.NewStudents.Count(s => !s.Id.HasValue || s.Id.Value == Guid.Empty);
                    var updated = command.NewStudents.Count(s => s.Id.HasValue && s.Id.Value != Guid.Empty);
                    var deleted = currentStudents.Count - updated;

                    historyEntries.Add(PpaHistoryEntry.Create(
                        ppaId: ppa.Id,
                        performedByUserId: currentUserId,
                        actionType: PpaHistoryActionType.UpdatedStudents,
                        oldValue: oldStudentsStr,
                        newValue: newStudentsStr,
                        notes: $"Total: {newStudents.Count} estudiantes. Creados: {created}, Actualizados: {updated}, Eliminados: {deleted}"));
                }
            }
            else
            {
                // Si se proporciona una lista vacía, eliminar todos los estudiantes existentes
                var currentStudents = ppa.Students.ToList();
                if (currentStudents.Count != 0)
                {
                    var oldStudentsStr = string.Join(", ", currentStudents.Select(s => s.Name));

                    // Sincronizar con lista vacía elimina todos
                    ppa.SyncStudents([]);

                    historyEntries.Add(PpaHistoryEntry.Create(
                        ppaId: ppa.Id,
                        performedByUserId: currentUserId,
                        actionType: PpaHistoryActionType.UpdatedStudents,
                        oldValue: oldStudentsStr,
                        newValue: "Sin estudiantes",
                        notes: $"Todos los estudiantes ({currentStudents.Count}) fueron eliminados"));
                }
            }
        }

        await _ppaRepository.UpdateAsync(
            ppa,
            ct);

        // 15. Guardar todas las entradas de historial
        if (historyEntries.Count != 0)
        {
            await _historyRepository.AddRangeAsync(historyEntries, ct);
        }
    }

    /// <summary>
    /// Cambia el estado de un PPA.
    /// </summary>
    /// <param name="command">Comando con el nuevo estado.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">Si el PPA no existe o el cambio de estado no es válido.</exception>
    public async Task ChangeStatusAsync(ChangePpaStatusCommand command, CancellationToken ct = default)
    {
        var historyEntries = new List<PpaHistoryEntry>();

        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Obtener el PPA
        var ppa = await _ppaRepository.GetByIdAsync(command.Id, ct) ?? throw new InvalidOperationException($"PPA con ID '{command.Id}' no encontrado.");

        var statusOld = PpaStatusTranslations.Spanish[ppa.Status];
        var newSatus = PpaStatusTranslations.Spanish[command.NewStatus];

        if(PpaStatus.Completed == ppa.Status)
            throw new InvalidOperationException("No se cambia el estado de un ppa completado.");

        var currentUser = _currentUserService.GetCurrentUser();
        if (currentUser == null)
            throw new InvalidOperationException("No se pudo obtener el usuario autenticado.");

        if (command.NewStatus == PpaStatus.Completed)
        {
            var documentCount = await _ppaAttachmentRepository.CountByTypeAsync(
                command.Id, PpaAttachmentType.PpaDocument, includeDeleted: false, ct);
            if (documentCount == 0)
                throw new InvalidOperationException(
                    "No se puede marcar el PPA como Completo sin al menos un documento formal (tipo PpaDocument).");
        }

        // Cambiar el estado usando método del dominio
        ppa.ChangeStatus(command.NewStatus);

        // Guardar cambios usando actualización parcial (solo información básica)
        await _ppaRepository.UpdateBasicInfoAsync(
            ppaId: ppa.Id,
            title: ppa.Title,
            generalObjective: ppa.GeneralObjective,
            specificObjectives: ppa.SpecificObjectives,
            description: ppa.Description,
            status: ppa.Status,
            primaryTeacherId: ppa.PrimaryTeacherId,
            continuationOfPpaId: ppa.ContinuationOfPpaId,
            continuedByPpaId: ppa.ContinuedByPpaId,
            ct);

        historyEntries.Add(PpaHistoryEntry.Create(
                 ppaId: ppa.Id,
                 performedByUserId: currentUser.UserId,
                 actionType: PpaHistoryActionType.ChangedStatus,
                 oldValue: statusOld,
                 newValue: newSatus,
                 notes: $"Cambio de estado de {statusOld} - {newSatus} "));

        // Si el PPA cambia a estado Completed, completar recursivamente todos los PPAs que son continuación de este
        if (command.NewStatus == PpaStatus.Completed)
        {
            await CompleteContinuationPpasRecursivelyAsync(ppa.Id, currentUser.UserId, historyEntries, ct);
        }

        if (historyEntries.Any())
        {
            await _historyRepository.AddRangeAsync(historyEntries, ct);
        }

    }

    /// <summary>
    /// Completa recursivamente todos los PPAs que son continuación del PPA especificado.
    /// </summary>
    /// <param name="ppaId">ID del PPA completado.</param>
    /// <param name="performedByUserId">ID del usuario que realizó el cambio.</param>
    /// <param name="historyEntries">Lista de entradas de historial a actualizar.</param>
    /// <param name="ct">Token de cancelación.</param>
    private async Task CompleteContinuationPpasRecursivelyAsync(
        Guid ppaId,
        Guid performedByUserId,
        List<PpaHistoryEntry> historyEntries,
        CancellationToken ct)
    {
        // Obtener todos los PPAs que son continuación de este PPA
        var continuationPpas = await _ppaRepository.GetByContinuationOfAsync(ppaId, ct);

        foreach (var continuationPpa in continuationPpas)
        {
            // Solo completar si no está ya completado o archivado
            if (continuationPpa.Status != PpaStatus.Completed )
            {
                var oldStatus = PpaStatusTranslations.Spanish[continuationPpa.Status];
                var newStatus = PpaStatusTranslations.Spanish[PpaStatus.Completed];

                // Cambiar el estado a Completed
                continuationPpa.ChangeStatus(PpaStatus.Completed);

                // Guardar cambios
                await _ppaRepository.UpdateBasicInfoAsync(
                    ppaId: continuationPpa.Id,
                    title: continuationPpa.Title,
                    generalObjective: continuationPpa.GeneralObjective,
                    specificObjectives: continuationPpa.SpecificObjectives,
                    description: continuationPpa.Description,
                    status: continuationPpa.Status,
                    primaryTeacherId: continuationPpa.PrimaryTeacherId,
                    continuationOfPpaId: continuationPpa.ContinuationOfPpaId,
                    continuedByPpaId: continuationPpa.ContinuedByPpaId,
                    ct);

                // Registrar en historial
                historyEntries.Add(PpaHistoryEntry.Create(
                    ppaId: continuationPpa.Id,
                    performedByUserId: performedByUserId,
                    actionType: PpaHistoryActionType.ChangedStatus,
                    oldValue: oldStatus,
                    newValue: newStatus,
                    notes: $"Completado automáticamente en cascada debido a que el PPA origen (ID: {ppaId}) fue completado."));

                // Completar recursivamente los PPAs que son continuación de este
                await CompleteContinuationPpasRecursivelyAsync(continuationPpa.Id, performedByUserId, historyEntries, ct);
            }
        }
    }

    /// <summary>
    /// Continúa un PPA existente en otro período académico.
    /// Crea un nuevo PPA vinculado al original, permitiendo dar continuidad al proyecto.
    /// </summary>
    /// <param name="command">Comando con los datos para la continuación.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>ID del nuevo PPA creado.</returns>
    /// <exception cref="InvalidOperationException">
    /// Si el PPA origen no existe, el usuario no tiene permisos,
    /// los periodos no son válidos, o el título ya existe en el periodo destino.
    /// </exception>
    public async Task<Guid> ContinuePpaAsync(ContinuePpaCommand command, CancellationToken ct = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // 1. Obtener usuario autenticado
        var currentUser = _currentUserService.GetCurrentUser();
        if (currentUser == null)
            throw new InvalidOperationException("No se pudo obtener el usuario autenticado.");

        var currentUserId = currentUser.UserId;

        // 2. Cargar PPA origen
        var sourcePpa = await _ppaRepository.GetByIdAsync(command.SourcePpaId, ct);
        if (sourcePpa == null)
            throw new InvalidOperationException($"El PPA origen con ID '{command.SourcePpaId}' no existe.");

        // 3. Validar que el PPA origen no haya sido continuado previamente
        if (sourcePpa.ContinuedByPpaId.HasValue)
            throw new InvalidOperationException(
                $"El PPA origen ya ha sido continuado por otro PPA (ID: {sourcePpa.ContinuedByPpaId.Value}). " +
                "Un PPA solo puede ser continuado una vez.");

        // 4. Validar permisos: debe ser ADMIN o docente responsable del PPA origen
        var isAdmin = currentUser.Roles.Contains("ADMIN");
        var isResponsible = sourcePpa.ResponsibleTeacherId == currentUserId;

        if (!isAdmin && !isResponsible)
            throw new InvalidOperationException(
                "No tiene permisos para continuar este PPA. " +
                "Solo el docente responsable o un administrador pueden continuar el PPA.");

        // 5. Cargar y validar periodo origen
        var sourcePeriod = await _periodRepository.GetByIdAsync(sourcePpa.AcademicPeriodId, ct);
        if (sourcePeriod == null)
            throw new InvalidOperationException(
                $"El período académico origen con ID '{sourcePpa.AcademicPeriodId}' no existe.");

        // 6. Cargar y validar periodo destino
        var targetPeriod = await _periodRepository.GetByIdAsync(command.TargetAcademicPeriodId, ct);
        if (targetPeriod == null)
            throw new InvalidOperationException(
                $"El período académico destino con ID '{command.TargetAcademicPeriodId}' no existe.");

        if (!targetPeriod.IsActive)
            throw new InvalidOperationException(
                $"El período académico destino '{targetPeriod.Code}' no está activo. " +
                "Solo se puede continuar un PPA a un periodo activo.");

        // 7. Validar que los periodos sean diferentes
        if (command.TargetAcademicPeriodId == sourcePpa.AcademicPeriodId)
            throw new InvalidOperationException(
                "El período académico destino debe ser diferente al periodo del PPA origen. " +
                "No se puede continuar un PPA dentro del mismo periodo.");

        // 8. Validar que el periodo destino sea cronológicamente posterior al origen
        if (targetPeriod.StartDate <= sourcePeriod.StartDate)
            throw new InvalidOperationException(
                $"El período académico destino '{targetPeriod.Code}' debe ser cronológicamente posterior " +
                $"al periodo origen '{sourcePeriod.Code}'. " +
                "Solo se puede continuar un PPA hacia periodos futuros.");

        // 9. Resolver título para el nuevo PPA
        var newTitle = string.IsNullOrWhiteSpace(command.NewTitle)
            ? sourcePpa.Title
            : command.NewTitle.Trim();

        // 10. Validar unicidad del título en el periodo destino
        var titleExists = await _ppaRepository.ExistsWithTitleAsync(
            newTitle,
            command.TargetAcademicPeriodId,
            excludePpaId: null,
            ct);

        if (titleExists)
            throw new InvalidOperationException(
                $"Ya existe un PPA con el título '{newTitle}' en el período académico '{targetPeriod.Code}'. " +
                "Los títulos deben ser únicos dentro del mismo período.");

        // 11. Resolver docente responsable para el nuevo PPA
        var newResponsibleTeacherId = command.NewResponsibleTeacherId ?? sourcePpa.ResponsibleTeacherId;

        // 12. Validar que el docente responsable exista
        var newResponsibleTeacher = await _userRepository.GetByIdAsync(newResponsibleTeacherId, ct);
        if (newResponsibleTeacher == null)
            throw new InvalidOperationException(
                $"El docente responsable con ID '{newResponsibleTeacherId}' no existe.");

        // 13. Validar que todas las asignaciones docente-asignatura del periodo destino existan y sean válidas
        foreach (var assignmentId in command.TeacherAssignmentIds)
        {
            var assignment = await _teacherAssignmentRepository.GetByIdAsync(assignmentId, ct);
            if (assignment == null)
                throw new InvalidOperationException(
                    $"La asignación docente-asignatura con ID '{assignmentId}' no existe.");

            if (assignment.AcademicPeriodId != command.TargetAcademicPeriodId)
                throw new InvalidOperationException(
                    $"La asignación docente-asignatura con ID '{assignmentId}' no pertenece al período académico destino '{targetPeriod.Code}'.");

            if (!assignment.IsActive)
                throw new InvalidOperationException(
                    $"La asignación docente-asignatura con ID '{assignmentId}' no está activa.");
        }

        // 14. Crear el nuevo PPA (continuación)
        var newPpa = Domain.Ppa.Entities.Ppa.Create(
            title: newTitle,
            academicPeriodId: command.TargetAcademicPeriodId,
            primaryTeacherId: newResponsibleTeacherId,
            generalObjective: sourcePpa.GeneralObjective,
            specificObjectives: sourcePpa.SpecificObjectives,
            description: sourcePpa.Description);

        // 15. Establecer la relación de continuación en el nuevo PPA
        newPpa.SetContinuationOf(sourcePpa.Id);

        // 16. Establecer las asignaciones del nuevo periodo
        newPpa.SetTeacherAssignments(command.TeacherAssignmentIds);

        // 17. Establecer los estudiantes del nuevo periodo
        if (command.StudentNames.Any())
        {
            newPpa.SetStudents(command.StudentNames);
        }

        // 18. Marcar el PPA origen como continuado
        sourcePpa.MarkContinuedBy(newPpa.Id, command.TargetAcademicPeriodId);

        // 19. Guardar ambos PPAs
        await _ppaRepository.AddAsync(newPpa, ct); // Nuevo PPA

        // PPA origen actualizado (solo información básica - se marcó como continuado)
        await _ppaRepository.UpdateBasicInfoAsync(
            ppaId: sourcePpa.Id,
            title: sourcePpa.Title,
            generalObjective: sourcePpa.GeneralObjective,
            specificObjectives: sourcePpa.SpecificObjectives,
            description: sourcePpa.Description,
            status: PpaStatus.InContinuing,
            primaryTeacherId: sourcePpa.PrimaryTeacherId,
            continuationOfPpaId: sourcePpa.ContinuationOfPpaId,
            continuedByPpaId: sourcePpa.ContinuedByPpaId,
            ct);

        // 20. Preparar entradas de historial
        var historyEntries = new List<PpaHistoryEntry>();

        // 21. Historial en PPA origen: ContinuationCreated
        var sourceStudentsStr = string.Join(", ", sourcePpa.Students.Select(s => s.Name));
        var newStudentsStr = command.StudentNames.Count != 0
            ? string.Join(", ", command.StudentNames)
            : "Sin estudiantes";

        historyEntries.Add(PpaHistoryEntry.Create(
            ppaId: sourcePpa.Id,
            performedByUserId: currentUserId,
            actionType: PpaHistoryActionType.ContinuationCreated,
            oldValue: null,
            newValue: $"Continuado en periodo '{targetPeriod.Code}' con PPA ID: {newPpa.Id}",
            notes: $"Título del nuevo PPA: '{newTitle}'. Responsable: {newResponsibleTeacher.Name}"));

        // 22. Historial en PPA nuevo: Created con referencia al origen
        historyEntries.Add(PpaHistoryEntry.Create(
            ppaId: newPpa.Id,
            performedByUserId: currentUserId,
            actionType: PpaHistoryActionType.Created,
            oldValue: null,
            newValue: $"Título: {newTitle}, Período: {targetPeriod.Code}",
            notes: $"PPA creado como continuación del PPA '{sourcePpa.Title}' (ID: {sourcePpa.Id}) del periodo '{sourcePeriod.Code}'. " +
                   $"Asignaciones: {command.TeacherAssignmentIds.Count}, Estudiantes: {command.StudentNames.Count}"));

        historyEntries.Add(PpaHistoryEntry.Create(
         ppaId: sourcePpa.Id,
         performedByUserId: currentUser.UserId,
         actionType: PpaHistoryActionType.ChangedStatus,
         oldValue: PpaStatusTranslations.Spanish[sourcePpa.Status],
         newValue: PpaStatusTranslations.Spanish[PpaStatus.InContinuing],
         notes: $"Cambio de estado de {PpaStatusTranslations.Spanish[sourcePpa.Status]} - {PpaStatusTranslations.Spanish[PpaStatus.InContinuing]} "));

        // 23. Si cambió la lista de estudiantes, registrar historial adicional
        var sourceStudentsList = sourcePpa.Students.Select(s => s.Name).OrderBy(x => x).ToList();
        var newStudentsList = command.StudentNames.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();

        if (!sourceStudentsList.SequenceEqual(newStudentsList, StringComparer.OrdinalIgnoreCase))
        {
            historyEntries.Add(PpaHistoryEntry.Create(
                ppaId: newPpa.Id,
                performedByUserId: currentUserId,
                actionType: PpaHistoryActionType.UpdatedStudents,
                oldValue: sourceStudentsStr,
                newValue: newStudentsStr,
                notes: $"Cambio de estudiantes al continuar PPA desde periodo '{sourcePeriod.Code}'"));
        }

        // 24. Guardar historial
        await _historyRepository.AddRangeAsync(historyEntries, ct);

        return newPpa.Id;
    }

    /// <summary>
    /// Mapea una entidad de dominio Ppa a un DTO básico.
    /// </summary>
    private static PpaDto ToDto(Domain.Ppa.Entities.Ppa ppa)
    {
        return new PpaDto
        {
            Id = ppa.Id,
            Title = ppa.Title,
            Description = ppa.Description,
            GeneralObjective = ppa.GeneralObjective,
            SpecificObjectives = ppa.SpecificObjectives,
            Status = ppa.Status,
            AcademicPeriodId = ppa.AcademicPeriodId,
            PrimaryTeacherId = ppa.PrimaryTeacherId,
            CreatedAt = ppa.CreatedAt,
            UpdatedAt = ppa.UpdatedAt,
            TeacherPrimaryName = ppa.TeacherPrimaryName,
        };
    }

    /// <summary>
    /// Mapea una entidad de dominio Ppa a un DTO detallado con información agregada.
    /// </summary>
    private static PpaDetailDto ToDetailDto(
        Domain.Ppa.Entities.Ppa ppa,
        string? academicPeriodCode = null,
        string? primaryTeacherName = null,
        IReadOnlyCollection<PpaTeacherAssignmentDetailDto>? assignmentDetails = null,
        IReadOnlyCollection<PpaStudentDto>? students = null)
    {
        return new PpaDetailDto
        {
            Id = ppa.Id,
            Title = ppa.Title,
            Description = ppa.Description,
            GeneralObjective = ppa.GeneralObjective,
            SpecificObjectives = ppa.SpecificObjectives,
            Status = ppa.Status,
            AcademicPeriodId = ppa.AcademicPeriodId,
            AcademicPeriodCode = academicPeriodCode,
            PrimaryTeacherId = ppa.PrimaryTeacherId,
            PrimaryTeacherName = primaryTeacherName,
            TeacherAssignmentIds = ppa.TeacherAssignmentIds,
            AssignmentDetails = assignmentDetails,
            Students = students,
            CreatedAt = ppa.CreatedAt,
            UpdatedAt = ppa.UpdatedAt,
            HasContinuation = ppa.ContinuedByPpaId.HasValue,
            IsContinuation = ppa.ContinuationOfPpaId.HasValue,
            ContinuationOfPpaId = ppa.ContinuationOfPpaId,
            ContinuedByPpaId = ppa.ContinuedByPpaId,
        };
    }

    /// <summary>
    /// Obtiene una descripción legible del tipo de acción de historial.
    /// </summary>
    private static string GetActionTypeDescription(PpaHistoryActionType actionType)
    {
        return actionType switch
        {
            PpaHistoryActionType.Created => "Creado",
            PpaHistoryActionType.UpdatedTitle => "Título actualizado",
            PpaHistoryActionType.ChangedStatus => "Estado cambiado",
            PpaHistoryActionType.ChangedResponsibleTeacher => "Responsable cambiado",
            PpaHistoryActionType.UpdatedAssignments => "Asignaciones actualizadas",
            PpaHistoryActionType.UpdatedStudents => "Estudiantes actualizados",
            PpaHistoryActionType.UpdatedContinuationSettings => "Configuración de continuidad actualizada",
            PpaHistoryActionType.AttachmentAdded => "Anexo agregado",
            PpaHistoryActionType.AttachmentRemoved => "Anexo eliminado",
            PpaHistoryActionType.ContinuationCreated => "Continuación creada",
            PpaHistoryActionType.UpdatedGeneralObjective => "Objetivo general actualizado",
            PpaHistoryActionType.UpdatedSpecificObjectives => "Objetivos específicos actualizados",
            PpaHistoryActionType.UpdatedDescription => "Descripción actualizada",
            _ => "Acción desconocida"
        };
    }
}
