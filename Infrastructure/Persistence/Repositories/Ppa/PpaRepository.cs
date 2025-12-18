using System.Reflection;
using Domain.Ppa;
using Domain.Ppa.Repositories;
using Infrastructure.Persistence.Entities.Ppa;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Ppa;

/// <summary>
/// Implementación del repositorio de PPAs usando EF Core.
/// Maneja el mapeo entre entidades de dominio (Ppa) y entidades de persistencia (PpaEntity).
/// </summary>
public sealed class PpaRepository : IPpaRepository
{
    private readonly ApplicationDbContext _context;

    public PpaRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Domain.Ppa.Entities.Ppa?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Ppas
            .Include(p => p.PpaTeacherAssignments)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<IReadOnlyCollection<Domain.Ppa.Entities.Ppa>> GetByTeacherAsync(
        Guid teacherId,
        Guid academicPeriodId,
        CancellationToken ct = default)
    {
        var entities = await _context.Ppas
            .Include(p => p.PpaTeacherAssignments)
            .Where(p => p.PrimaryTeacherId == teacherId && p.AcademicPeriodId == academicPeriodId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<Domain.Ppa.Entities.Ppa>> GetByAcademicPeriodAsync(
        Guid academicPeriodId,
        CancellationToken ct = default)
    {
        var entities = await _context.Ppas
            .Include(p => p.PpaTeacherAssignments)
            .Where(p => p.AcademicPeriodId == academicPeriodId)
            .OrderBy(p => p.Title)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<Domain.Ppa.Entities.Ppa>> GetByStatusAsync(
        PpaStatus status,
        CancellationToken ct = default)
    {
        var statusInt = (int)status;

        var entities = await _context.Ppas
            .Include(p => p.PpaTeacherAssignments)
            .Where(p => p.Status == statusInt)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<Domain.Ppa.Entities.Ppa>> GetByTeacherAssignmentAsync(
        Guid teacherAssignmentId,
        CancellationToken ct = default)
    {
        var entities = await _context.Ppas
            .Include(p => p.PpaTeacherAssignments)
            .Where(p => p.PpaTeacherAssignments.Any(pta => pta.TeacherAssignmentId == teacherAssignmentId))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<bool> ExistsActiveForAssignmentsAsync(
        IEnumerable<Guid> teacherAssignmentIds,
        CancellationToken ct = default)
    {
        var assignmentIdsList = teacherAssignmentIds.ToList();

        if (!assignmentIdsList.Any())
            return false;

        // Buscar PPAs activos (no Archived) que contengan exactamente las mismas asignaciones
        var activePpas = await _context.Ppas
            .Include(p => p.PpaTeacherAssignments)
            .Where(p => p.Status != (int)PpaStatus.Archived)
            .ToListAsync(ct);

        foreach (var ppa in activePpas)
        {
            var ppaAssignmentIds = ppa.PpaTeacherAssignments
                .Select(pta => pta.TeacherAssignmentId)
                .ToList();

            // Verificar si los conjuntos son iguales (mismos IDs sin importar orden)
            if (ppaAssignmentIds.Count == assignmentIdsList.Count &&
                !ppaAssignmentIds.Except(assignmentIdsList).Any())
            {
                return true;
            }
        }

        return false;
    }

    public async Task<bool> TitleExistsInPeriodAsync(
        string title,
        Guid academicPeriodId,
        Guid? excludePpaId = null,
        CancellationToken ct = default)
    {
        var normalizedTitle = title.Trim().ToUpperInvariant();

        var query = _context.Ppas
            .Where(p => p.AcademicPeriodId == academicPeriodId);

        if (excludePpaId.HasValue)
        {
            query = query.Where(p => p.Id != excludePpaId.Value);
        }

        return await query.AnyAsync(p => p.Title.ToUpper() == normalizedTitle, ct);
    }

    public async Task AddAsync(Domain.Ppa.Entities.Ppa ppa, CancellationToken ct = default)
    {
        if (ppa == null)
            throw new ArgumentNullException(nameof(ppa));

        var entity = new PpaEntity
        {
            Id = ppa.Id,
            Title = ppa.Title,
            GeneralObjective = ppa.GeneralObjective,
            SpecificObjectives = ppa.SpecificObjectives,
            Description = ppa.Description,
            Status = (int)ppa.Status,
            AcademicPeriodId = ppa.AcademicPeriodId,
            PrimaryTeacherId = ppa.PrimaryTeacherId,
            CreatedAt = ppa.CreatedAt,
            UpdatedAt = ppa.UpdatedAt
        };

        // Agregar relaciones con TeacherAssignments
        foreach (var assignmentId in ppa.TeacherAssignmentIds)
        {
            entity.PpaTeacherAssignments.Add(new PpaTeacherAssignmentEntity
            {
                PpaId = ppa.Id,
                TeacherAssignmentId = assignmentId
            });
        }

        await _context.Ppas.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Domain.Ppa.Entities.Ppa ppa, CancellationToken ct = default)
    {
        if (ppa == null)
            throw new ArgumentNullException(nameof(ppa));

        var entity = await _context.Ppas
            .Include(p => p.PpaTeacherAssignments)
            .FirstOrDefaultAsync(p => p.Id == ppa.Id, ct);

        if (entity == null)
            throw new InvalidOperationException($"PPA con ID '{ppa.Id}' no encontrado en la base de datos.");

        // Actualizar propiedades
        entity.Title = ppa.Title;
        entity.GeneralObjective = ppa.GeneralObjective;
        entity.SpecificObjectives = ppa.SpecificObjectives;
        entity.Description = ppa.Description;
        entity.Status = (int)ppa.Status;
        entity.AcademicPeriodId = ppa.AcademicPeriodId;
        entity.PrimaryTeacherId = ppa.PrimaryTeacherId;
        entity.UpdatedAt = ppa.UpdatedAt;

        // Actualizar relaciones con TeacherAssignments
        // Remover las que ya no existen
        var currentAssignmentIds = ppa.TeacherAssignmentIds.ToList();
        var assignmentsToRemove = entity.PpaTeacherAssignments
            .Where(pta => !currentAssignmentIds.Contains(pta.TeacherAssignmentId))
            .ToList();

        foreach (var assignmentToRemove in assignmentsToRemove)
        {
            entity.PpaTeacherAssignments.Remove(assignmentToRemove);
        }

        // Agregar las nuevas
        var existingAssignmentIds = entity.PpaTeacherAssignments
            .Select(pta => pta.TeacherAssignmentId)
            .ToList();

        foreach (var assignmentId in currentAssignmentIds)
        {
            if (!existingAssignmentIds.Contains(assignmentId))
            {
                entity.PpaTeacherAssignments.Add(new PpaTeacherAssignmentEntity
                {
                    PpaId = ppa.Id,
                    TeacherAssignmentId = assignmentId
                });
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Mapea una entidad de EF Core (PpaEntity) a una entidad de dominio (Ppa).
    /// </summary>
    private Domain.Ppa.Entities.Ppa MapToDomain(PpaEntity entity)
    {
        // Crear el PPA usando el método de fábrica con ID específico
        var ppa = Domain.Ppa.Entities.Ppa.CreateWithId(
            id: entity.Id,
            title: entity.Title,
            academicPeriodId: entity.AcademicPeriodId,
            primaryTeacherId: entity.PrimaryTeacherId,
            status: (PpaStatus)entity.Status,
            createdAt: entity.CreatedAt,
            generalObjective: entity.GeneralObjective,
            specificObjectives: entity.SpecificObjectives,
            description: entity.Description,
            updatedAt: entity.UpdatedAt);

        // Restaurar las asignaciones de docentes usando reflection
        var teacherAssignmentIds = entity.PpaTeacherAssignments
            .Select(pta => pta.TeacherAssignmentId)
            .ToList();

        if (teacherAssignmentIds.Count != 0)
        {
            var restoreMethod = typeof(Domain.Ppa.Entities.Ppa).GetMethod(
                "RestoreTeacherAssignmentIds",
                BindingFlags.NonPublic | BindingFlags.Instance);

            restoreMethod?.Invoke(ppa, new object[] { teacherAssignmentIds });
        }

        return ppa;
    }
}
