using Application.Academics.Commands;
using Application.Academics.DTOs;
using Application.Common;
using Domain.Academics.Entities;
using Domain.Academics.Repositories;
using Domain.Common;

namespace Application.Academics;

/// <summary>
/// Servicio de aplicación para gestión de asignaturas en SIGEPP.
/// Orquesta los casos de uso relacionados con asignaturas.
/// </summary>
public sealed class SubjectsAppService
{
    private readonly ISubjectRepository _subjectRepository;

    public SubjectsAppService(ISubjectRepository subjectRepository)
    {
        _subjectRepository = subjectRepository ?? throw new ArgumentNullException(nameof(subjectRepository));
    }

    /// <summary>
    /// Obtiene todas las asignaturas del sistema.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de todas las asignaturas.</returns>
    public async Task<IReadOnlyCollection<SubjectDto>> GetAllAsync(CancellationToken ct = default)
    {
        var subjects = await _subjectRepository.GetAllAsync(ct);
        return subjects.Select(ToDto).ToList().AsReadOnly();
    }

    /// <summary>
    /// Obtiene todas las asignaturas activas del sistema.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de asignaturas activas.</returns>
    public async Task<IReadOnlyCollection<SubjectDto>> GetActiveAsync(CancellationToken ct = default)
    {
        var subjects = await _subjectRepository.GetActiveAsync(ct);
        return subjects.Select(ToDto).ToList().AsReadOnly();
    }

    /// <summary>
    /// Obtiene una lista paginada de asignaturas con filtros opcionales.
    /// </summary>
    /// <param name="query">Filtros de paginación y búsqueda.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resultado paginado con asignaturas.</returns>
    public async Task<PagedResult<SubjectDto>> GetPagedAsync(PagedQuery query, CancellationToken ct = default)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        var pagedResult = await _subjectRepository.GetPagedAsync(
            page: query.Page,
            pageSize: query.PageSize,
            search: query.Search,
            isActive: query.IsActive,
            ct: ct);

        // Mapear las entidades de dominio a DTOs
        var dtos = pagedResult.Items.Select(ToDto).ToList().AsReadOnly();

        return new PagedResult<SubjectDto>(
            items: dtos,
            page: pagedResult.Page,
            pageSize: pagedResult.PageSize,
            totalItems: pagedResult.TotalItems);
    }

    /// <summary>
    /// Obtiene una asignatura por su ID.
    /// </summary>
    /// <param name="id">ID de la asignatura.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Asignatura detallada o null si no existe.</returns>
    public async Task<SubjectDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var subject = await _subjectRepository.GetByIdAsync(id, ct);
        return subject == null ? null : ToDetailDto(subject);
    }

    /// <summary>
    /// Crea una nueva asignatura en el sistema.
    /// </summary>
    /// <param name="command">Comando con los datos de la asignatura a crear.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>ID de la asignatura creada.</returns>
    /// <exception cref="InvalidOperationException">Si el código ya está en uso.</exception>
    public async Task<Guid> CreateAsync(CreateSubjectCommand command, CancellationToken ct = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Validar que el código no exista
        var codeExists = await _subjectRepository.CodeExistsAsync(command.Code, ct);
        if (codeExists)
            throw new InvalidOperationException($"El código '{command.Code}' ya está en uso por otra asignatura.");

        // Crear la asignatura usando el método de fábrica del dominio
        var subject = Subject.Create(
            code: command.Code,
            name: command.Name,
            description: command.Description,
            isActive: true);

        // Guardar la asignatura
        await _subjectRepository.AddAsync(subject, ct);

        return subject.Id;
    }

    /// <summary>
    /// Actualiza los datos de una asignatura existente.
    /// </summary>
    /// <param name="command">Comando con los datos a actualizar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">Si la asignatura no existe o el código está en uso.</exception>
    public async Task UpdateAsync(UpdateSubjectCommand command, CancellationToken ct = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Cargar la asignatura
        var subject = await _subjectRepository.GetByIdAsync(command.Id, ct);
        if (subject == null)
            throw new InvalidOperationException($"La asignatura con ID '{command.Id}' no existe.");

        // Validar que el código no esté en uso por otra asignatura
        var codeExists = await _subjectRepository.CodeExistsAsync(command.Code, command.Id, ct);
        if (codeExists)
            throw new InvalidOperationException($"El código '{command.Code}' ya está en uso por otra asignatura.");

        // Actualizar código, nombre y descripción usando métodos de dominio
        subject.ChangeCode(command.Code);
        subject.ChangeName(command.Name);
        subject.ChangeDescription(command.Description);

        // Guardar los cambios
        await _subjectRepository.UpdateAsync(subject, ct);
    }

    /// <summary>
    /// Activa una asignatura previamente desactivada.
    /// </summary>
    /// <param name="id">ID de la asignatura a activar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">Si la asignatura no existe.</exception>
    public async Task ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var subject = await _subjectRepository.GetByIdAsync(id, ct);
        if (subject == null)
            throw new InvalidOperationException($"La asignatura con ID '{id}' no existe.");

        subject.Activate();
        await _subjectRepository.UpdateAsync(subject, ct);
    }

    /// <summary>
    /// Desactiva una asignatura.
    /// </summary>
    /// <param name="id">ID de la asignatura a desactivar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">Si la asignatura no existe.</exception>
    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var subject = await _subjectRepository.GetByIdAsync(id, ct);
        if (subject == null)
            throw new InvalidOperationException($"La asignatura con ID '{id}' no existe.");

        subject.Deactivate();
        await _subjectRepository.UpdateAsync(subject, ct);
    }

    /// <summary>
    /// Mapea una entidad de dominio (Subject) a un DTO básico.
    /// </summary>
    private static SubjectDto ToDto(Subject subject)
    {
        return new SubjectDto
        {
            Id = subject.Id,
            Code = subject.Code,
            Name = subject.Name,
            Description = subject.Description,
            IsActive = subject.IsActive
        };
    }

    /// <summary>
    /// Mapea una entidad de dominio (Subject) a un DTO detallado.
    /// </summary>
    private static SubjectDetailDto ToDetailDto(Subject subject)
    {
        return new SubjectDetailDto
        {
            Id = subject.Id,
            Code = subject.Code,
            Name = subject.Name,
            Description = subject.Description,
            IsActive = subject.IsActive,
            CreatedAt = subject.CreatedAt,
            UpdatedAt = subject.UpdatedAt
        };
    }
}
