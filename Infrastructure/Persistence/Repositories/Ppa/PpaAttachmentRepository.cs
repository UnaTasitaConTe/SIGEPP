using Domain.Ppa;
using Domain.Ppa.Entities;
using Domain.Ppa.Repositories;
using Infrastructure.Persistence.Entities.Ppa;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Ppa;

/// <summary>
/// Implementación del repositorio de anexos de PPA usando EF Core.
/// Maneja el mapeo entre entidades de dominio (PpaAttachment) y entidades de persistencia (PpaAttachmentEntity).
/// </summary>
public sealed class PpaAttachmentRepository : IPpaAttachmentRepository
{
    private readonly ApplicationDbContext _context;

    public PpaAttachmentRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<PpaAttachment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.PpaAttachments
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<IReadOnlyCollection<PpaAttachment>> GetByPpaAsync(
        Guid ppaId,
        bool includeDeleted = false,
        CancellationToken ct = default)
    {
        var query = _context.PpaAttachments
            .Where(a => a.PpaId == ppaId);

        if (!includeDeleted)
        {
            query = query.Where(a => !a.IsDeleted);
        }

        var entities = await query
            .OrderBy(a => a.Type)
            .ThenBy(a => a.UploadedAt)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<PpaAttachment>> GetByPpaAndTypeAsync(
        Guid ppaId,
        PpaAttachmentType type,
        bool includeDeleted = false,
        CancellationToken ct = default)
    {
        var typeInt = (int)type;

        var query = _context.PpaAttachments
            .Where(a => a.PpaId == ppaId && a.Type == typeInt);

        if (!includeDeleted)
        {
            query = query.Where(a => !a.IsDeleted);
        }

        var entities = await query
            .OrderBy(a => a.UploadedAt)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<PpaAttachment>> GetByUserAsync(
        Guid userId,
        bool includeDeleted = false,
        CancellationToken ct = default)
    {
        var query = _context.PpaAttachments
            .Where(a => a.UploadedByUserId == userId);

        if (!includeDeleted)
        {
            query = query.Where(a => !a.IsDeleted);
        }

        var entities = await query
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<int> CountByTypeAsync(
        Guid ppaId,
        PpaAttachmentType type,
        bool includeDeleted = false,
        CancellationToken ct = default)
    {
        var typeInt = (int)type;

        var query = _context.PpaAttachments
            .Where(a => a.PpaId == ppaId && a.Type == typeInt);

        if (!includeDeleted)
        {
            query = query.Where(a => !a.IsDeleted);
        }

        return await query.CountAsync(ct);
    }

    public async Task<bool> FileKeyExistsAsync(string fileKey, CancellationToken ct = default)
    {
        var normalizedFileKey = fileKey.Trim();
        return await _context.PpaAttachments
            .AnyAsync(a => a.FileKey == normalizedFileKey, ct);
    }

    public async Task AddAsync(PpaAttachment attachment, CancellationToken ct = default)
    {
        if (attachment == null)
            throw new ArgumentNullException(nameof(attachment));

        var entity = new PpaAttachmentEntity
        {
            Id = attachment.Id,
            PpaId = attachment.PpaId,
            Type = (int)attachment.Type,
            Name = attachment.Name,
            FileKey = attachment.FileKey,
            ContentType = attachment.ContentType,
            UploadedByUserId = attachment.UploadedByUserId,
            UploadedAt = attachment.UploadedAt,
            IsDeleted = attachment.IsDeleted,
            DeletedAt = attachment.DeletedAt
        };

        await _context.PpaAttachments.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(PpaAttachment attachment, CancellationToken ct = default)
    {
        if (attachment == null)
            throw new ArgumentNullException(nameof(attachment));

        var entity = await _context.PpaAttachments
            .FirstOrDefaultAsync(a => a.Id == attachment.Id, ct);

        if (entity == null)
            throw new InvalidOperationException($"Anexo con ID '{attachment.Id}' no encontrado en la base de datos.");

        // Actualizar propiedades (solo las que pueden cambiar)
        entity.IsDeleted = attachment.IsDeleted;
        entity.DeletedAt = attachment.DeletedAt;

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(PpaAttachment attachment, CancellationToken ct = default)
    {
        if (attachment == null)
            throw new ArgumentNullException(nameof(attachment));

        var entity = await _context.PpaAttachments
            .FirstOrDefaultAsync(a => a.Id == attachment.Id, ct);

        if (entity == null)
            throw new InvalidOperationException($"Anexo con ID '{attachment.Id}' no encontrado en la base de datos.");

        _context.PpaAttachments.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Mapea una entidad de EF Core (PpaAttachmentEntity) a una entidad de dominio (PpaAttachment).
    /// </summary>
    private PpaAttachment MapToDomain(PpaAttachmentEntity entity)
    {
        return PpaAttachment.CreateWithId(
            id: entity.Id,
            ppaId: entity.PpaId,
            type: (PpaAttachmentType)entity.Type,
            name: entity.Name,
            fileKey: entity.FileKey,
            uploadedByUserId: entity.UploadedByUserId,
            uploadedAt: entity.UploadedAt,
            contentType: entity.ContentType,
            isDeleted: entity.IsDeleted,
            deletedAt: entity.DeletedAt);
    }
}
