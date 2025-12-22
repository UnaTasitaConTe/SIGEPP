using System.Reflection;
using Domain.Ppa;
using Domain.Ppa.Repositories;
using Domain.Ppa.ValueObjects;
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
            .Include(p => p.PrimaryTeacher)
            .Include(p => p.PpaTeacherAssignments)
                .ThenInclude(pta => pta.TeacherAssignment)
                    .ThenInclude(ta => ta!.Teacher)
            .Include(p => p.PpaTeacherAssignments)
                .ThenInclude(pta => pta.TeacherAssignment)
                    .ThenInclude(ta => ta!.Subject)
            .Include(p => p.Students)
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
            .Include(p => p.Students)
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
                .Include(p => p.PrimaryTeacher) // ✅ navegación directa

            .Include(p => p.PpaTeacherAssignments)
            .ThenInclude(pta => pta.TeacherAssignment)
                        .ThenInclude(ta => ta.Teacher)

            .Include(p => p.Students)

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
            .Include(p => p.Students)
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
            .Include(p => p.Students)
            .Where(p => p.PpaTeacherAssignments.Any(pta => pta.TeacherAssignmentId == teacherAssignmentId))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<Domain.Ppa.Entities.Ppa>> GetByContinuationOfAsync(
        Guid continuationOfPpaId,
        CancellationToken ct = default)
    {
        var entities = await _context.Ppas
            .Include(p => p.PpaTeacherAssignments)
            .Include(p => p.Students)
            .Where(p => p.ContinuationOfPpaId == continuationOfPpaId)
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

    public async Task<bool> ExistsWithTitleAsync(
        string title,
        Guid academicPeriodId,
        Guid? excludePpaId = null,
        CancellationToken ct = default)
    {
        // Delega a TitleExistsInPeriodAsync
        return await TitleExistsInPeriodAsync(title, academicPeriodId, excludePpaId, ct);
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
            UpdatedAt = ppa.UpdatedAt,
            ContinuationOfPpaId = ppa.ContinuationOfPpaId,
            ContinuedByPpaId = ppa.ContinuedByPpaId
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

        // Agregar estudiantes
        foreach (var student in ppa.Students)
        {
            entity.Students.Add(new PpaStudentEntity
            {
                Id = student.Id,
                PpaId = ppa.Id,
                Name = student.Name
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
            .Include(p => p.Students)
            .FirstOrDefaultAsync(p => p.Id == ppa.Id, ct);

        if (entity == null)
            throw new InvalidOperationException($"PPA con ID '{ppa.Id}' no encontrado en la base de datos.");

        // Aplicar actualizaciones siguiendo el Principio de Responsabilidad Única
        UpdateBasicInfo(entity, ppa);
        UpdateTeacherAssignments(entity, ppa);
        UpdateStudents(entity, ppa);

        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Actualiza la información básica del PPA (título, descripción, objetivos, estado, etc.).
    /// Responsabilidad: Actualizar solo propiedades escalares del PPA.
    /// </summary>
    /// <param name="entity">Entidad de persistencia a actualizar.</param>
    /// <param name="ppa">Entidad de dominio con los nuevos valores.</param>
    private static void UpdateBasicInfo(PpaEntity entity, Domain.Ppa.Entities.Ppa ppa)
    {
        entity.Title = ppa.Title;
        entity.GeneralObjective = ppa.GeneralObjective;
        entity.SpecificObjectives = ppa.SpecificObjectives;
        entity.Description = ppa.Description;
        entity.Status = (int)ppa.Status;
        entity.AcademicPeriodId = ppa.AcademicPeriodId;
        entity.PrimaryTeacherId = ppa.PrimaryTeacherId;
        entity.UpdatedAt = ppa.UpdatedAt;
        entity.ContinuationOfPpaId = ppa.ContinuationOfPpaId;
        entity.ContinuedByPpaId = ppa.ContinuedByPpaId;
    }

    /// <summary>
    /// Actualiza las asignaciones docente-asignatura asociadas al PPA.
    /// Responsabilidad: Sincronizar la colección de asignaciones (agregar nuevas, eliminar obsoletas).
    /// </summary>
    /// <param name="entity">Entidad de persistencia a actualizar.</param>
    /// <param name="ppa">Entidad de dominio con las nuevas asignaciones.</param>
    private  void UpdateTeacherAssignments(PpaEntity entity, Domain.Ppa.Entities.Ppa ppa)
    {
        var currentAssignmentIds = ppa.TeacherAssignmentIds.ToList();

        // Eliminar asignaciones que ya no existen en el dominio
        var assignmentsToRemove = entity.PpaTeacherAssignments
            .Where(pta => !currentAssignmentIds.Contains(pta.TeacherAssignmentId))
            .ToList();

        foreach (var assignmentToRemove in assignmentsToRemove)
        {
            _context.PpaTeacherAssignments.RemoveRange(assignmentToRemove);
        }

        // Agregar nuevas asignaciones que no existían
        var existingAssignmentIds = entity.PpaTeacherAssignments
            .Select(pta => pta.TeacherAssignmentId)
            .ToList();

        List<PpaTeacherAssignmentEntity> ppaTeacherAssignmentsToInsert = [];

        foreach (var assignmentId in currentAssignmentIds)
        {
            if (!existingAssignmentIds.Contains(assignmentId))
            {
                ppaTeacherAssignmentsToInsert.Add(new PpaTeacherAssignmentEntity
                {
                    PpaId = ppa.Id,
                    TeacherAssignmentId = assignmentId
                });
            }
        }
        
        _context.AddRange(ppaTeacherAssignmentsToInsert);

        
    }

    /// <summary>
    /// Actualiza los estudiantes asociados al PPA.
    /// Responsabilidad: Sincronizar la colección de estudiantes.
    /// Estrategia: Eliminar todos y recrear para evitar problemas de tracking/concurrencia de EF Core.
    /// </summary>
    /// <param name="entity">Entidad de persistencia a actualizar.</param>
    /// <param name="ppa">Entidad de dominio con los nuevos estudiantes.</param>
    private void UpdateStudents(PpaEntity entity, Domain.Ppa.Entities.Ppa ppa)
    {
        // Eliminar todos los estudiantes existentes
        // Mapa de estudiantes actuales en BD (trackeados)
        var currentById = entity.Students.ToDictionary(s => s.Id);

        // IDs objetivo (los que llegan desde dominio)
        var targetIds = new HashSet<Guid>();

        var listInsert = new List<PpaStudentEntity>();

        foreach (var s in ppa.Students)
        {
            // Si tu dominio permite estudiantes nuevos con Guid.Empty, genéralo aquí
            var id = s.Id == Guid.Empty ? Guid.NewGuid() : s.Id;

            targetIds.Add(id);

            if (currentById.TryGetValue(id, out var existing))
                existing.Name = s.Name;
            else
            {
                listInsert.Add(new PpaStudentEntity
                {
                    Id = id,
                    PpaId = entity.Id,
                    Name = s.Name
                });
            }
        }

        _context.PpaStudents.AddRange(listInsert);

        var toDelete = entity.Students.Where(x => !targetIds.Contains(x.Id)).ToList();
        if (toDelete.Count > 0)
            _context.PpaStudents.RemoveRange(toDelete);

        // DELETE: los que están en BD pero ya no vienen en el dominio

    }

    public async Task UpdateBasicInfoAsync(
        Guid ppaId,
        string title,
        string? generalObjective,
        string? specificObjectives,
        string? description,
        PpaStatus status,
        Guid primaryTeacherId,
        Guid? continuationOfPpaId,
        Guid? continuedByPpaId,
        CancellationToken ct = default)
    {
        // Lectura independiente de la entidad básica del PPA
        var entity = await _context.Ppas
            .FirstOrDefaultAsync(p => p.Id == ppaId, ct);

        if (entity == null)
            throw new InvalidOperationException($"PPA con ID '{ppaId}' no encontrado en la base de datos.");

        // Actualizar solo propiedades básicas
        entity.Title = title;
        entity.GeneralObjective = generalObjective;
        entity.SpecificObjectives = specificObjectives;
        entity.Description = description;
        entity.Status = (int)status;
        entity.PrimaryTeacherId = primaryTeacherId;
        entity.ContinuationOfPpaId = continuationOfPpaId;
        entity.ContinuedByPpaId = continuedByPpaId;
        entity.UpdatedAt = DateTime.UtcNow;

        // SaveChanges independiente solo para datos básicos
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateTeacherAssignmentsAsync(
        Guid ppaId,
        IEnumerable<Guid> teacherAssignmentIds,
        CancellationToken ct = default)
    {
        // Lectura independiente solo de las asignaciones
        var entity = await _context.Ppas
            .Include(p => p.PpaTeacherAssignments)
            .FirstOrDefaultAsync(p => p.Id == ppaId, ct);

        if (entity == null)
            throw new InvalidOperationException($"PPA con ID '{ppaId}' no encontrado en la base de datos.");

        var currentAssignmentIds = teacherAssignmentIds.ToList();

        // Eliminar asignaciones que ya no existen
        var assignmentsToRemove = entity.PpaTeacherAssignments
            .Where(pta => !currentAssignmentIds.Contains(pta.TeacherAssignmentId))
            .ToList();

        foreach (var assignmentToRemove in assignmentsToRemove)
        {
            entity.PpaTeacherAssignments.Remove(assignmentToRemove);
        }

        // Agregar nuevas asignaciones
        var existingAssignmentIds = entity.PpaTeacherAssignments
            .Select(pta => pta.TeacherAssignmentId)
            .ToList();

        foreach (var assignmentId in currentAssignmentIds)
        {
            if (!existingAssignmentIds.Contains(assignmentId))
            {
                entity.PpaTeacherAssignments.Add(new PpaTeacherAssignmentEntity
                {
                    PpaId = ppaId,
                    TeacherAssignmentId = assignmentId
                });
            }
        }

        entity.UpdatedAt = DateTime.UtcNow;

        // SaveChanges independiente solo para asignaciones
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateStudentsAsync(
        Guid ppaId,
        IEnumerable<(Guid? Id, string Name)> students,
        CancellationToken ct = default)
    {
        // Lectura independiente solo de los estudiantes
        var entity = await _context.PpaStudents
            .Where(p => p.Id == ppaId)
            .ToListAsync(ct);

        if (entity == null)
        {
            var listAdd = students.Select(s => new PpaStudentEntity
            {
                Id = Guid.NewGuid(),
                PpaId = ppaId,
                Name = s.Name
            }).ToList();

            _context.PpaStudents.AddRange(listAdd);
            await _context.SaveChangesAsync(ct);

            return;
        }

        var studentsList = students.ToList();

        // Mapa de estudiantes actuales en BD (trackeados)
        var currentById = entity.ToDictionary(s => s.Id);

        // IDs objetivo (los que llegan desde el request)
        var targetIds = new HashSet<Guid>();

        foreach (var s in studentsList)
        {
            // Si el ID es null o Guid.Empty, generar uno nuevo
            var id = (!s.Id.HasValue || s.Id.Value == Guid.Empty) ? Guid.NewGuid() : s.Id.Value;
            targetIds.Add(id);

            if (currentById.TryGetValue(id, out var existing))
            {
                // UPDATE: actualizar nombre si cambió
                existing.Name = s.Name;
                existing.PpaId = ppaId;
            }
            else
            {
                // INSERT: agregar nuevo estudiante
                entity.Add(new PpaStudentEntity
                {
                    Id = id,
                    PpaId = ppaId,
                    Name = s.Name
                });
            }
        }

        // DELETE: eliminar estudiantes que ya no vienen en la lista
        var studentsToRemove = entity
            .Where(s => !targetIds.Contains(s.Id))
            .ToList();

        foreach (var studentToRemove in studentsToRemove)
        {
            entity.Remove(studentToRemove);
        }

        // SaveChanges independiente solo para estudiantes
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Mapea una entidad de EF Core (PpaEntity) a una entidad de dominio (Ppa).
    /// </summary>
    private Domain.Ppa.Entities.Ppa MapToDomain(PpaEntity entity)
    {

        string? teacherNamePrimary = entity.PpaTeacherAssignments
            .Where(x => x.TeacherAssignment?.Teacher?.Id == entity.PrimaryTeacherId)
            .Select(x => x.TeacherAssignment?.Teacher?.Name).FirstOrDefault();

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
            teacherPrimaryName: teacherNamePrimary,
            updatedAt: entity.UpdatedAt,
            continuationOfPpaId : entity.ContinuationOfPpaId,
            continuedByPpaId : entity.ContinuedByPpaId
            );

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

        // Restaurar estudiantes usando reflection
        var students = entity.Students
            .Select(s => PpaStudent.CreateWithId(s.Id, s.Name))
            .ToList();

        if (students.Count != 0)
        {
            var restoreStudentsMethod = typeof(Domain.Ppa.Entities.Ppa).GetMethod(
                "RestoreStudents",
                BindingFlags.NonPublic | BindingFlags.Instance);

            restoreStudentsMethod?.Invoke(ppa, new object[] { students });
        }

        return ppa;
    }
}
