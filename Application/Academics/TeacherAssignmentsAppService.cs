using Application.Academics.Commands;
using Application.Academics.DTOs;
using Domain.Academics.Entities;
using Domain.Academics.Repositories;
using Domain.Users;

namespace Application.Academics;

/// <summary>
/// Servicio de aplicación para gestión de asignaciones docentes en SIGEPP.
/// Orquesta los casos de uso relacionados con la asignación de docentes a asignaturas en períodos académicos.
/// </summary>
public sealed class TeacherAssignmentsAppService
{
    private readonly ITeacherAssignmentRepository _assignmentRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly IAcademicPeriodRepository _periodRepository;
    private readonly IUserRepository _userRepository;

    public TeacherAssignmentsAppService(
        ITeacherAssignmentRepository assignmentRepository,
        ISubjectRepository subjectRepository,
        IAcademicPeriodRepository periodRepository,
        IUserRepository userRepository)
    {
        _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
        _subjectRepository = subjectRepository ?? throw new ArgumentNullException(nameof(subjectRepository));
        _periodRepository = periodRepository ?? throw new ArgumentNullException(nameof(periodRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <summary>
    /// Obtiene todas las asignaciones de un docente en un período académico específico.
    /// </summary>
    /// <param name="teacherId">ID del docente.</param>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de asignaciones del docente en el período.</returns>
    public async Task<IReadOnlyCollection<TeacherAssignmentDto>> GetByTeacherAsync(
        Guid teacherId,
        Guid academicPeriodId,
        CancellationToken ct = default)
    {
        var assignments = await _assignmentRepository.GetByTeacherAndPeriodAsync(teacherId, academicPeriodId, ct);
        return assignments.Select(ToDto).ToList().AsReadOnly();
    }

    /// <summary>
    /// Obtiene todas las asignaciones de una asignatura en un período académico específico.
    /// </summary>
    /// <param name="subjectId">ID de la asignatura.</param>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de asignaciones de la asignatura en el período.</returns>
    public async Task<IReadOnlyCollection<TeacherAssignmentDto>> GetBySubjectAsync(
        Guid subjectId,
        Guid academicPeriodId,
        CancellationToken ct = default)
    {
        var assignments = await _assignmentRepository.GetBySubjectAndPeriodAsync(subjectId, academicPeriodId, ct);
        return assignments.Select(ToDto).ToList().AsReadOnly();
    }

    /// <summary>
    /// Obtiene todas las asignaciones de un período académico específico.
    /// </summary>
    /// <param name="academicPeriodId">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de todas las asignaciones del período.</returns>
    public async Task<IReadOnlyCollection<TeacherAssignmentDto>> GetByPeriodAsync(
        Guid academicPeriodId,
        CancellationToken ct = default)
    {
        var assignments = await _assignmentRepository.GetByPeriodAsync(academicPeriodId, ct);
        return assignments.Select(ToDto).ToList().AsReadOnly();
    }

    /// <summary>
    /// Asigna un docente a una asignatura en un período académico.
    /// </summary>
    /// <param name="command">Comando con los datos de la asignación.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>ID de la asignación creada.</returns>
    /// <exception cref="InvalidOperationException">
    /// Si el docente no existe, la asignatura no existe o no está activa,
    /// el período no existe o no está activo, o ya existe una asignación duplicada.
    /// </exception>
    public async Task<Guid> AssignAsync(AssignTeacherCommand command, CancellationToken ct = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Validar que el docente existe
        var teacher = await _userRepository.GetByIdAsync(command.TeacherId, ct);
        if (teacher == null)
            throw new InvalidOperationException($"El usuario con ID '{command.TeacherId}' no existe.");

        if (!teacher.IsActive)
            throw new InvalidOperationException($"El usuario con ID '{command.TeacherId}' está inactivo.");

        // Opcional: Verificar que el usuario tenga rol DOCENTE
        // (Esto depende de si tienes una manera de verificar roles en el dominio)
        // if (!teacher.HasRole("DOCENTE"))
        //     throw new InvalidOperationException("El usuario debe tener rol DOCENTE para ser asignado a una asignatura.");

        // Validar que la asignatura existe y está activa
        var subject = await _subjectRepository.GetByIdAsync(command.SubjectId, ct);
        if (subject == null)
            throw new InvalidOperationException($"La asignatura con ID '{command.SubjectId}' no existe.");

        if (!subject.IsActive)
            throw new InvalidOperationException($"La asignatura '{subject.Name}' está inactiva y no se puede asignar.");

        // Validar que el período académico existe y está activo
        var period = await _periodRepository.GetByIdAsync(command.AcademicPeriodId, ct);
        if (period == null)
            throw new InvalidOperationException($"El período académico con ID '{command.AcademicPeriodId}' no existe.");

        if (!period.IsActive)
            throw new InvalidOperationException($"El período académico '{period.Name}' está inactivo y no se puede asignar.");

        // Validar que no exista ya una asignación duplicada
        var assignmentExists = await _assignmentRepository.ExistsAsync(
            command.TeacherId,
            command.SubjectId,
            command.AcademicPeriodId,
            ct);

        if (assignmentExists)
            throw new InvalidOperationException(
                $"El docente '{teacher.Name}' ya está asignado a la asignatura '{subject.Name}' en el período '{period.Name}'.");

        // Crear la asignación usando el método de fábrica del dominio
        var assignment = TeacherAssignment.Create(
            teacherId: command.TeacherId,
            subjectId: command.SubjectId,
            academicPeriodId: command.AcademicPeriodId,
            isActive: true);

        // Guardar la asignación
        await _assignmentRepository.AddAsync(assignment, ct);

        return assignment.Id;
    }

    /// <summary>
    /// Desactiva una asignación docente.
    /// </summary>
    /// <param name="id">ID de la asignación a desactivar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">Si la asignación no existe.</exception>
    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id, ct);
        if (assignment == null)
            throw new InvalidOperationException($"La asignación con ID '{id}' no existe.");

        assignment.Deactivate();
        await _assignmentRepository.UpdateAsync(assignment, ct);
    }

    /// <summary>
    /// Reactiva una asignación docente previamente desactivada.
    /// </summary>
    /// <param name="id">ID de la asignación a activar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">Si la asignación no existe.</exception>
    public async Task ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id, ct);
        if (assignment == null)
            throw new InvalidOperationException($"La asignación con ID '{id}' no existe.");

        assignment.Activate();
        await _assignmentRepository.UpdateAsync(assignment, ct);
    }

    /// <summary>
    /// Elimina permanentemente una asignación docente.
    /// </summary>
    /// <param name="id">ID de la asignación a eliminar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">Si la asignación no existe.</exception>
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id, ct);
        if (assignment == null)
            throw new InvalidOperationException($"La asignación con ID '{id}' no existe.");

        await _assignmentRepository.DeleteAsync(assignment, ct);
    }

    /// <summary>
    /// Mapea una entidad de dominio (TeacherAssignment) a un DTO.
    /// </summary>
    private static TeacherAssignmentDto ToDto(TeacherAssignment assignment)
    {
        return new TeacherAssignmentDto
        {
            Id = assignment.Id,
            TeacherId = assignment.TeacherId,
            SubjectId = assignment.SubjectId,
            AcademicPeriodId = assignment.AcademicPeriodId,
            IsActive = assignment.IsActive,
            // Los campos adicionales (nombres) se pueden poblar desde los repositorios si es necesario
            // Por ahora los dejamos null, pero en el futuro se pueden agregar joins o consultas adicionales
            TeacherName = null,
            SubjectCode = null,
            SubjectName = null,
            AcademicPeriodCode = null,
            AcademicPeriodName = null
        };
    }
}
