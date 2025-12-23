using System.Reflection;
using Domain.Academics.Entities;
using Domain.Academics.Repositories;
using Domain.Common;
using Infrastructure.Persistence.Entities.Academics;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Academics;

/// <summary>
/// Implementación del repositorio de asignaturas usando EF Core.
/// Maneja el mapeo entre entidades de dominio (Subject) y entidades de persistencia (SubjectEntity).
/// </summary>
public class SubjectRepository : ISubjectRepository
{
    private readonly ApplicationDbContext _context;

    public SubjectRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Subject?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Subjects
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<Subject?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        var entity = await _context.Subjects
            .FirstOrDefaultAsync(s => s.Code == normalizedCode, ct);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return await _context.Subjects
            .AnyAsync(s => s.Code == normalizedCode, ct);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid excludeSubjectId, CancellationToken ct = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return await _context.Subjects
            .AnyAsync(s => s.Code == normalizedCode && s.Id != excludeSubjectId, ct);
    }

    public async Task<IReadOnlyCollection<Subject>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await _context.Subjects
            .OrderBy(s => s.Code)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<Subject>> GetActiveAsync(CancellationToken ct = default)
    {
        var entities = await _context.Subjects
            .Where(s => s.IsActive)
            .OrderBy(s => s.Code)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<Subject>> GetByTeacherAndPeriodAsync(
        Guid teacherId,
        Guid academicPeriodId,
        CancellationToken ct = default)
    {
        var entities = await _context.TeacherAssignments
            .Where(ta => ta.TeacherId == teacherId
                && ta.AcademicPeriodId == academicPeriodId
                && ta.IsActive)
            .Include(ta => ta.Subject)
            .Select(ta => ta.Subject!)
            .OrderBy(s => s.Code)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<PagedResult<Subject>> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        bool? isActive = null,
        CancellationToken ct = default)
    {
        // Query base
        var query = _context.Subjects.AsQueryable();

        // Filtro de búsqueda por código o nombre
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToUpperInvariant();
            query = query.Where(s =>
                s.Code.Contains(searchTerm) ||
                s.Name.ToUpper().Contains(searchTerm));
        }

        // Filtro por estado activo/inactivo
        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        // Contar total de elementos (antes de paginar)
        var totalItems = await query.CountAsync(ct);

        // Aplicar ordenamiento (igual que GetAllAsync)
        query = query.OrderBy(s => s.Code);

        // Aplicar paginación
        var skip = (page - 1) * pageSize;
        var entities = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);

        // Mapear a dominio
        var domainItems = entities.Select(MapToDomain).ToList().AsReadOnly();

        return new PagedResult<Subject>(
            items: domainItems,
            page: page,
            pageSize: pageSize,
            totalItems: totalItems);
    }

    public async Task AddAsync(Subject subject, CancellationToken ct = default)
    {
        if (subject == null)
            throw new ArgumentNullException(nameof(subject));

        var entity = new SubjectEntity
        {
            Id = subject.Id,
            Code = subject.Code,
            Name = subject.Name,
            Description = subject.Description,
            IsActive = subject.IsActive,
            CreatedAt = subject.CreatedAt,
            UpdatedAt = subject.UpdatedAt
        };

        await _context.Subjects.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Subject subject, CancellationToken ct = default)
    {
        if (subject == null)
            throw new ArgumentNullException(nameof(subject));

        var entity = await _context.Subjects
            .FirstOrDefaultAsync(s => s.Id == subject.Id, ct);

        if (entity == null)
            throw new InvalidOperationException($"Asignatura con ID '{subject.Id}' no encontrada en la base de datos.");

        // Actualizar propiedades
        entity.Code = subject.Code;
        entity.Name = subject.Name;
        entity.Description = subject.Description;
        entity.IsActive = subject.IsActive;
        entity.UpdatedAt = subject.UpdatedAt;

        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Mapea una entidad de EF Core (SubjectEntity) a una entidad de dominio (Subject).
    /// </summary>
    private Subject MapToDomain(SubjectEntity entity)
    {
        // Crear la asignatura usando el método de fábrica con ID específico
        var subject = Subject.CreateWithId(
            id: entity.Id,
            code: entity.Code,
            name: entity.Name,
            description: entity.Description,
            isActive: entity.IsActive);

        // Restaurar las fechas usando reflection (propiedades con private set)
        var createdAtProperty = typeof(Subject).GetProperty(
            nameof(Subject.CreatedAt),
            BindingFlags.Public | BindingFlags.Instance);

        var updatedAtProperty = typeof(Subject).GetProperty(
            nameof(Subject.UpdatedAt),
            BindingFlags.Public | BindingFlags.Instance);

        if (createdAtProperty != null && createdAtProperty.CanWrite)
        {
            createdAtProperty.SetValue(subject, entity.CreatedAt);
        }

        if (updatedAtProperty != null && updatedAtProperty.CanWrite)
        {
            updatedAtProperty.SetValue(subject, entity.UpdatedAt);
        }

        return subject;
    }
}
