using Application.Academics.Commands;
using Application.Academics.DTOs;
using Domain.Academics.Entities;
using Domain.Academics.Repositories;

namespace Application.Academics;

/// <summary>
/// Servicio de aplicación para gestión de períodos académicos en SIGEPP.
/// Orquesta los casos de uso relacionados con períodos académicos.
/// </summary>
public sealed class AcademicPeriodsAppService
{
    private readonly IAcademicPeriodRepository _periodRepository;

    public AcademicPeriodsAppService(IAcademicPeriodRepository periodRepository)
    {
        _periodRepository = periodRepository ?? throw new ArgumentNullException(nameof(periodRepository));
    }

    /// <summary>
    /// Obtiene todos los períodos académicos del sistema.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de todos los períodos académicos.</returns>
    public async Task<IReadOnlyCollection<AcademicPeriodDto>> GetAllAsync(CancellationToken ct = default)
    {
        var periods = await _periodRepository.GetAllAsync(ct);
        return periods.Select(ToDto).ToList().AsReadOnly();
    }

    /// <summary>
    /// Obtiene todos los períodos académicos activos del sistema.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de períodos académicos activos.</returns>
    public async Task<IReadOnlyCollection<AcademicPeriodDto>> GetActiveAsync(CancellationToken ct = default)
    {
        var periods = await _periodRepository.GetActiveAsync(ct);
        return periods.Select(ToDto).ToList().AsReadOnly();
    }

    /// <summary>
    /// Obtiene un período académico por su ID.
    /// </summary>
    /// <param name="id">ID del período académico.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Período académico detallado o null si no existe.</returns>
    public async Task<AcademicPeriodDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var period = await _periodRepository.GetByIdAsync(id, ct);
        return period == null ? null : ToDetailDto(period);
    }

    /// <summary>
    /// Crea un nuevo período académico en el sistema.
    /// </summary>
    /// <param name="command">Comando con los datos del período a crear.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>ID del período académico creado.</returns>
    /// <exception cref="InvalidOperationException">Si el código ya está en uso.</exception>
    public async Task<Guid> CreateAsync(CreateAcademicPeriodCommand command, CancellationToken ct = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Validar que el código no exista
        var codeExists = await _periodRepository.CodeExistsAsync(command.Code, ct);
        if (codeExists)
            throw new InvalidOperationException($"El código '{command.Code}' ya está en uso por otro período académico.");

        // Validar fechas si están presentes
        if (command.StartDate.HasValue && command.EndDate.HasValue && command.StartDate.Value > command.EndDate.Value)
            throw new InvalidOperationException("La fecha de inicio no puede ser posterior a la fecha de fin.");

        // Crear el período académico usando el método de fábrica del dominio
        var period = AcademicPeriod.Create(
            code: command.Code,
            name: command.Name,
            startDate: command.StartDate,
            endDate: command.EndDate,
            isActive: true);

        // Guardar el período académico
        await _periodRepository.AddAsync(period, ct);

        return period.Id;
    }

    /// <summary>
    /// Actualiza los datos de un período académico existente.
    /// </summary>
    /// <param name="command">Comando con los datos a actualizar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">Si el período no existe o el código está en uso.</exception>
    public async Task UpdateAsync(UpdateAcademicPeriodCommand command, CancellationToken ct = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Cargar el período académico
        var period = await _periodRepository.GetByIdAsync(command.Id, ct);
        if (period == null)
            throw new InvalidOperationException($"El período académico con ID '{command.Id}' no existe.");

        // Validar que el código no esté en uso por otro período
        var codeExists = await _periodRepository.CodeExistsAsync(command.Code, command.Id, ct);
        if (codeExists)
            throw new InvalidOperationException($"El código '{command.Code}' ya está en uso por otro período académico.");

        // Validar fechas si están presentes
        if (command.StartDate.HasValue && command.EndDate.HasValue && command.StartDate.Value > command.EndDate.Value)
            throw new InvalidOperationException("La fecha de inicio no puede ser posterior a la fecha de fin.");

        // Actualizar código y nombre usando métodos de dominio
        period.ChangeCode(command.Code);
        period.ChangeName(command.Name);
        period.SetDates(command.StartDate, command.EndDate);

        // Guardar los cambios
        await _periodRepository.UpdateAsync(period, ct);
    }

    /// <summary>
    /// Activa un período académico previamente desactivado.
    /// </summary>
    /// <param name="id">ID del período académico a activar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">Si el período no existe.</exception>
    public async Task ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var period = await _periodRepository.GetByIdAsync(id, ct);
        if (period == null)
            throw new InvalidOperationException($"El período académico con ID '{id}' no existe.");

        period.Activate();
        await _periodRepository.UpdateAsync(period, ct);
    }

    /// <summary>
    /// Desactiva un período académico.
    /// </summary>
    /// <param name="id">ID del período académico a desactivar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="InvalidOperationException">Si el período no existe.</exception>
    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var period = await _periodRepository.GetByIdAsync(id, ct);
        if (period == null)
            throw new InvalidOperationException($"El período académico con ID '{id}' no existe.");

        period.Deactivate();
        await _periodRepository.UpdateAsync(period, ct);
    }

    /// <summary>
    /// Mapea una entidad de dominio (AcademicPeriod) a un DTO básico.
    /// </summary>
    private static AcademicPeriodDto ToDto(AcademicPeriod period)
    {
        return new AcademicPeriodDto
        {
            Id = period.Id,
            Code = period.Code,
            Name = period.Name,
            StartDate = period.StartDate,
            EndDate = period.EndDate,
            IsActive = period.IsActive
        };
    }

    /// <summary>
    /// Mapea una entidad de dominio (AcademicPeriod) a un DTO detallado.
    /// </summary>
    private static AcademicPeriodDetailDto ToDetailDto(AcademicPeriod period)
    {
        return new AcademicPeriodDetailDto
        {
            Id = period.Id,
            Code = period.Code,
            Name = period.Name,
            StartDate = period.StartDate,
            EndDate = period.EndDate,
            IsActive = period.IsActive,
            CreatedAt = period.CreatedAt,
            UpdatedAt = period.UpdatedAt,
            IsCurrent = period.IsCurrentToday()
        };
    }
}
