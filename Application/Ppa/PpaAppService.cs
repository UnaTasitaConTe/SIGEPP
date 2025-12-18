using Application.Ppa.Commands;
using Application.Ppa.DTOs;
using Domain.Academics.Repositories;
using Domain.Ppa;
using Domain.Ppa.Repositories;
using Domain.Users;

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

    public PpaAppService(
        IPpaRepository ppaRepository,
        IAcademicPeriodRepository periodRepository,
        ITeacherAssignmentRepository teacherAssignmentRepository,
        IUserRepository userRepository)
    {
        _ppaRepository = ppaRepository ?? throw new ArgumentNullException(nameof(ppaRepository));
        _periodRepository = periodRepository ?? throw new ArgumentNullException(nameof(periodRepository));
        _teacherAssignmentRepository = teacherAssignmentRepository ?? throw new ArgumentNullException(nameof(teacherAssignmentRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
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

        // Obtener información agregada (opcional, puede ser costoso)
        var period = await _periodRepository.GetByIdAsync(ppa.AcademicPeriodId, ct);
        var teacher = await _userRepository.GetByIdAsync(ppa.PrimaryTeacherId, ct);

        return ToDetailDto(ppa, period?.Code, teacher?.Name);
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
    /// Crea un nuevo PPA en el sistema.
    /// </summary>
    /// <param name="command">Comando con los datos del PPA a crear.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>ID del PPA creado.</returns>
    /// <exception cref="InvalidOperationException">
    /// Si el período no existe/no está activo, el docente no existe,
    /// las asignaciones no existen/no pertenecen al período,
    /// o ya existe un PPA activo para las mismas asignaciones.
    /// </exception>
    public async Task<Guid> CreateAsync(CreatePpaCommand command, CancellationToken ct = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // 1. Validar que el período académico exista y esté activo
        var period = await _periodRepository.GetByIdAsync(command.AcademicPeriodId, ct);
        if (period == null)
            throw new InvalidOperationException($"El período académico con ID '{command.AcademicPeriodId}' no existe.");

        if (!period.IsActive)
            throw new InvalidOperationException($"El período académico '{period.Code}' no está activo.");

        // 2. Validar que el docente principal exista
        var teacher = await _userRepository.GetByIdAsync(command.PrimaryTeacherId, ct);
        if (teacher == null)
            throw new InvalidOperationException($"El docente con ID '{command.PrimaryTeacherId}' no existe.");

        // 3. Validar que todas las asignaciones docente-asignatura existan y pertenezcan al mismo período
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

        // 4. Validar que no exista otro PPA activo (no archivado) para el mismo conjunto de asignaciones
        var existsActivePpa = await _ppaRepository.ExistsActiveForAssignmentsAsync(command.TeacherAssignmentIds, ct);
        if (existsActivePpa)
            throw new InvalidOperationException(
                "Ya existe un PPA activo para el conjunto de asignaciones docente-asignatura especificado. " +
                "No se pueden tener múltiples PPAs activos para las mismas asignaciones en el mismo período.");

        // 5. Crear el PPA usando el método de fábrica del dominio
        var ppa = Domain.Ppa.Entities.Ppa.Create(
            title: command.Title,
            academicPeriodId: command.AcademicPeriodId,
            primaryTeacherId: command.PrimaryTeacherId,
            generalObjective: command.GeneralObjective,
            specificObjectives: command.SpecificObjectives,
            description: command.Description);

        // 6. Agregar las asignaciones docente-asignatura al PPA
        foreach (var assignmentId in command.TeacherAssignmentIds)
        {
            ppa.AttachTeacherAssignment(assignmentId);
        }

        // 7. Guardar el PPA
        await _ppaRepository.AddAsync(ppa, ct);

        return ppa.Id;
    }

    /// <summary>
    /// Actualiza los datos de un PPA existente.
    /// </summary>
    /// <param name="command">Comando con los datos a actualizar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">Si el PPA no existe.</exception>
    public async Task UpdateAsync(UpdatePpaCommand command, CancellationToken ct = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Obtener el PPA
        var ppa = await _ppaRepository.GetByIdAsync(command.Id, ct);
        if (ppa == null)
            throw new InvalidOperationException($"PPA con ID '{command.Id}' no encontrado.");

        // Actualizar propiedades usando métodos del dominio
        ppa.UpdateTitle(command.Title);
        ppa.UpdateGeneralObjective(command.GeneralObjective);
        ppa.UpdateSpecificObjectives(command.SpecificObjectives);
        ppa.UpdateDescription(command.Description);

        // Guardar cambios
        await _ppaRepository.UpdateAsync(ppa, ct);
    }

    /// <summary>
    /// Cambia el estado de un PPA.
    /// </summary>
    /// <param name="command">Comando con el nuevo estado.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">Si el PPA no existe o el cambio de estado no es válido.</exception>
    public async Task ChangeStatusAsync(ChangePpaStatusCommand command, CancellationToken ct = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Obtener el PPA
        var ppa = await _ppaRepository.GetByIdAsync(command.Id, ct);
        if (ppa == null)
            throw new InvalidOperationException($"PPA con ID '{command.Id}' no encontrado.");

        // TODO: Validación futura - Cuando NewStatus == PpaStatus.Completed, validar que exista
        // al menos un anexo de tipo PpaAttachmentType.PpaDocument.
        // Esto se implementará cuando se integre PpaAttachmentsAppService.
        // if (command.NewStatus == PpaStatus.Completed)
        // {
        //     var documentCount = await _ppaAttachmentRepository.CountByTypeAsync(
        //         command.Id, PpaAttachmentType.PpaDocument, includeDeleted: false, ct);
        //     if (documentCount == 0)
        //         throw new InvalidOperationException(
        //             "No se puede marcar el PPA como Completed sin al menos un documento formal (tipo PpaDocument).");
        // }

        // Cambiar el estado usando método del dominio
        ppa.ChangeStatus(command.NewStatus);

        // Guardar cambios
        await _ppaRepository.UpdateAsync(ppa, ct);
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
            UpdatedAt = ppa.UpdatedAt
        };
    }

    /// <summary>
    /// Mapea una entidad de dominio Ppa a un DTO detallado con información agregada.
    /// </summary>
    private static PpaDetailDto ToDetailDto(
        Domain.Ppa.Entities.Ppa ppa,
        string? academicPeriodCode = null,
        string? primaryTeacherName = null)
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
            CreatedAt = ppa.CreatedAt,
            UpdatedAt = ppa.UpdatedAt
        };
    }
}
