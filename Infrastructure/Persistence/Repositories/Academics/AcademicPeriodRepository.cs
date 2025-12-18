using System.Reflection;
using Domain.Academics.Entities;
using Domain.Academics.Repositories;
using Infrastructure.Persistence.Entities.Academics;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Academics;

/// <summary>
/// Implementación del repositorio de períodos académicos usando EF Core.
/// Maneja el mapeo entre entidades de dominio (AcademicPeriod) y entidades de persistencia (AcademicPeriodEntity).
/// </summary>
public class AcademicPeriodRepository : IAcademicPeriodRepository
{
    private readonly ApplicationDbContext _context;

    public AcademicPeriodRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<AcademicPeriod?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.AcademicPeriods
            .FirstOrDefaultAsync(ap => ap.Id == id, ct);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<AcademicPeriod?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        var entity = await _context.AcademicPeriods
            .FirstOrDefaultAsync(ap => ap.Code == normalizedCode, ct);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return await _context.AcademicPeriods
            .AnyAsync(ap => ap.Code == normalizedCode, ct);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid excludePeriodId, CancellationToken ct = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return await _context.AcademicPeriods
            .AnyAsync(ap => ap.Code == normalizedCode && ap.Id != excludePeriodId, ct);
    }

    public async Task<IReadOnlyCollection<AcademicPeriod>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await _context.AcademicPeriods
            .OrderByDescending(ap => ap.StartDate)
            .ThenBy(ap => ap.Code)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<AcademicPeriod>> GetActiveAsync(CancellationToken ct = default)
    {
        var entities = await _context.AcademicPeriods
            .Where(ap => ap.IsActive)
            .OrderByDescending(ap => ap.StartDate)
            .ThenBy(ap => ap.Code)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<AcademicPeriod?> GetCurrentAsync(DateOnly date, CancellationToken ct = default)
    {
        var entity = await _context.AcademicPeriods
            .Where(ap => ap.IsActive
                && ap.StartDate.HasValue
                && ap.EndDate.HasValue
                && ap.StartDate.Value <= date
                && ap.EndDate.Value >= date)
            .FirstOrDefaultAsync(ct);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<AcademicPeriod?> GetCurrentTodayAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await GetCurrentAsync(today, ct);
    }

    public async Task AddAsync(AcademicPeriod period, CancellationToken ct = default)
    {
        if (period == null)
            throw new ArgumentNullException(nameof(period));

        var entity = new AcademicPeriodEntity
        {
            Id = period.Id,
            Code = period.Code,
            Name = period.Name,
            StartDate = period.StartDate,
            EndDate = period.EndDate,
            IsActive = period.IsActive,
            CreatedAt = period.CreatedAt,
            UpdatedAt = period.UpdatedAt
        };

        await _context.AcademicPeriods.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(AcademicPeriod period, CancellationToken ct = default)
    {
        if (period == null)
            throw new ArgumentNullException(nameof(period));

        var entity = await _context.AcademicPeriods
            .FirstOrDefaultAsync(ap => ap.Id == period.Id, ct);

        if (entity == null)
            throw new InvalidOperationException($"Período académico con ID '{period.Id}' no encontrado en la base de datos.");

        // Actualizar propiedades
        entity.Code = period.Code;
        entity.Name = period.Name;
        entity.StartDate = period.StartDate;
        entity.EndDate = period.EndDate;
        entity.IsActive = period.IsActive;
        entity.UpdatedAt = period.UpdatedAt;

        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Mapea una entidad de EF Core (AcademicPeriodEntity) a una entidad de dominio (AcademicPeriod).
    /// </summary>
    private AcademicPeriod MapToDomain(AcademicPeriodEntity entity)
    {
        // Crear el período académico usando el método de fábrica con ID específico
        var period = AcademicPeriod.CreateWithId(
            id: entity.Id,
            code: entity.Code,
            name: entity.Name,
            startDate: entity.StartDate,
            endDate: entity.EndDate,
            isActive: entity.IsActive);

        // Restaurar las fechas usando reflection (propiedades con private set)
        var createdAtProperty = typeof(AcademicPeriod).GetProperty(
            nameof(AcademicPeriod.CreatedAt),
            BindingFlags.Public | BindingFlags.Instance);

        var updatedAtProperty = typeof(AcademicPeriod).GetProperty(
            nameof(AcademicPeriod.UpdatedAt),
            BindingFlags.Public | BindingFlags.Instance);

        if (createdAtProperty != null && createdAtProperty.CanWrite)
        {
            createdAtProperty.SetValue(period, entity.CreatedAt);
        }

        if (updatedAtProperty != null && updatedAtProperty.CanWrite)
        {
            updatedAtProperty.SetValue(period, entity.UpdatedAt);
        }

        return period;
    }
}
