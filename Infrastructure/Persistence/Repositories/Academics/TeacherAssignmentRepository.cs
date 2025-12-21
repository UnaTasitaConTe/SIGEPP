using System.Reflection;
using Domain.Academics.Entities;
using Domain.Academics.Repositories;
using Infrastructure.Persistence.Entities.Academics;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Academics;

/// <summary>
/// Implementación del repositorio de asignaciones docentes usando EF Core.
/// Maneja el mapeo entre entidades de dominio (TeacherAssignment) y entidades de persistencia (TeacherAssignmentEntity).
/// </summary>
public class TeacherAssignmentRepository : ITeacherAssignmentRepository
{
    private readonly ApplicationDbContext _context;

    public TeacherAssignmentRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<TeacherAssignment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.TeacherAssignments
            .Include(x => x.Teacher)
            .Include(x => x.AcademicPeriod)
            .Include(x => x.Subject)
            .FirstOrDefaultAsync(ta => ta.Id == id, ct);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<IReadOnlyCollection<TeacherAssignment>> GetByTeacherAsync(
        Guid teacherId,
        CancellationToken ct = default)
    {
        var entities = await _context.TeacherAssignments
            .Include(x => x.Teacher)
            .Include(x => x.AcademicPeriod)
            .Include(x => x.Subject)
            .Where(ta => ta.TeacherId == teacherId)
            .OrderByDescending(ta => ta.CreatedAt)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<TeacherAssignment>> GetByTeacherAndPeriodAsync(
        Guid teacherId,
        Guid academicPeriodId,
        CancellationToken ct = default)
    {
        var entities = await _context.TeacherAssignments
            .Include(x => x.Teacher)
            .Include(x => x.AcademicPeriod)
            .Include(x => x.Subject)
            .Where(ta => ta.TeacherId == teacherId && ta.AcademicPeriodId == academicPeriodId)
            .OrderBy(ta => ta.CreatedAt)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<TeacherAssignment>> GetActiveByTeacherAndPeriodAsync(
        Guid teacherId,
        Guid academicPeriodId,
        CancellationToken ct = default)
    {
        var entities = await _context.TeacherAssignments
            .Include(x => x.Teacher)
            .Include(x => x.AcademicPeriod)
            .Include(x => x.Subject)
            .Where(ta => ta.TeacherId == teacherId
                && ta.AcademicPeriodId == academicPeriodId
                && ta.IsActive)
            .OrderBy(ta => ta.CreatedAt)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<TeacherAssignment>> GetBySubjectAndPeriodAsync(
        Guid subjectId,
        Guid academicPeriodId,
        CancellationToken ct = default)
    {
        var entities = await _context.TeacherAssignments
            .Include(x => x.Teacher)
            .Include(x => x.AcademicPeriod)
            .Include(x => x.Subject)
            .Where(ta => ta.SubjectId == subjectId && ta.AcademicPeriodId == academicPeriodId)
            .OrderBy(ta => ta.CreatedAt)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<TeacherAssignment>> GetByPeriodAsync(
        Guid academicPeriodId,
        CancellationToken ct = default)
    {
        var entities = await _context.TeacherAssignments
            .Include(x => x.Teacher)
            .Include(x => x.AcademicPeriod)
            .Include(x => x.Subject)
            .Where(ta => ta.AcademicPeriodId == academicPeriodId)
            .OrderBy(ta => ta.TeacherId)
            .ThenBy(ta => ta.SubjectId)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<TeacherAssignment>> GetActiveByPeriodAsync(
        Guid academicPeriodId,
        CancellationToken ct = default)
    {
        var entities = await _context.TeacherAssignments
            .Include(x=>x.Teacher)
            .Include(x=>x.AcademicPeriod)
            .Include(x=>x.Subject)
            .Where(ta => ta.AcademicPeriodId == academicPeriodId && ta.IsActive)
            .OrderBy(ta => ta.TeacherId)
            .ThenBy(ta => ta.SubjectId)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<bool> ExistsAsync(
        Guid teacherId,
        Guid subjectId,
        Guid academicPeriodId,
        CancellationToken ct = default)
    {
        return await _context.TeacherAssignments
            .AnyAsync(ta => ta.TeacherId == teacherId
                && ta.SubjectId == subjectId
                && ta.AcademicPeriodId == academicPeriodId,
                ct);
    }

    public async Task<bool> ExistsAsync(
        Guid teacherId,
        Guid subjectId,
        Guid academicPeriodId,
        Guid excludeAssignmentId,
        CancellationToken ct = default)
    {
        return await _context.TeacherAssignments
            .AnyAsync(ta => ta.TeacherId == teacherId
                && ta.SubjectId == subjectId
                && ta.AcademicPeriodId == academicPeriodId
                && ta.Id != excludeAssignmentId,
                ct);
    }

    public async Task AddAsync(TeacherAssignment assignment, CancellationToken ct = default)
    {
        if (assignment == null)
            throw new ArgumentNullException(nameof(assignment));

        var entity = new TeacherAssignmentEntity
        {
            Id = assignment.Id,
            TeacherId = assignment.TeacherId,
            SubjectId = assignment.SubjectId,
            AcademicPeriodId = assignment.AcademicPeriodId,
            IsActive = assignment.IsActive,
            CreatedAt = assignment.CreatedAt,
            UpdatedAt = assignment.UpdatedAt
        };

        await _context.TeacherAssignments.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TeacherAssignment assignment, CancellationToken ct = default)
    {
        if (assignment == null)
            throw new ArgumentNullException(nameof(assignment));

        var entity = await _context.TeacherAssignments
            .FirstOrDefaultAsync(ta => ta.Id == assignment.Id, ct);

        if (entity == null)
            throw new InvalidOperationException($"Asignación docente con ID '{assignment.Id}' no encontrada en la base de datos.");

        // Actualizar propiedades
        entity.TeacherId = assignment.TeacherId;
        entity.SubjectId = assignment.SubjectId;
        entity.AcademicPeriodId = assignment.AcademicPeriodId;
        entity.IsActive = assignment.IsActive;
        entity.UpdatedAt = assignment.UpdatedAt;

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(TeacherAssignment assignment, CancellationToken ct = default)
    {
        if (assignment == null)
            throw new ArgumentNullException(nameof(assignment));

        var entity = await _context.TeacherAssignments
            .FirstOrDefaultAsync(ta => ta.Id == assignment.Id, ct);

        if (entity == null)
            throw new InvalidOperationException($"Asignación docente con ID '{assignment.Id}' no encontrada en la base de datos.");

        _context.TeacherAssignments.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Mapea una entidad de EF Core (TeacherAssignmentEntity) a una entidad de dominio (TeacherAssignment).
    /// </summary>
    private TeacherAssignment MapToDomain(TeacherAssignmentEntity entity)
    {
        // Crear la asignación usando el método de fábrica con ID específico
        var assignment = TeacherAssignment.CreateWithId(
            id: entity.Id,
            teacherId: entity.TeacherId,
            subjectId: entity.SubjectId,
            academicPeriodId: entity.AcademicPeriodId,
            isActive: entity.IsActive,
            teacherName : entity.Teacher?.Name ?? string.Empty,
            subjectCode : entity.Subject?.Code ?? string.Empty,
            subjectName : entity.Subject?.Name ?? string.Empty,
            academicPeriodCode : entity.AcademicPeriod?.Code ?? string.Empty,
            academicPeriodName : entity.AcademicPeriod?.Name ?? string.Empty
            );
        // Restaurar las fechas usando reflection (propiedades con private set)
        var createdAtProperty = typeof(TeacherAssignment).GetProperty(
            nameof(TeacherAssignment.CreatedAt),
            BindingFlags.Public | BindingFlags.Instance);

        var updatedAtProperty = typeof(TeacherAssignment).GetProperty(
            nameof(TeacherAssignment.UpdatedAt),
            BindingFlags.Public | BindingFlags.Instance);

        if (createdAtProperty != null && createdAtProperty.CanWrite)
        {
            createdAtProperty.SetValue(assignment, entity.CreatedAt);
        }

        if (updatedAtProperty != null && updatedAtProperty.CanWrite)
        {
            updatedAtProperty.SetValue(assignment, entity.UpdatedAt);
        }

        return assignment;
    }
}
