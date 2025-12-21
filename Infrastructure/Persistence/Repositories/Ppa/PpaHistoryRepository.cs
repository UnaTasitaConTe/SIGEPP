using Domain.Ppa;
using Domain.Ppa.Entities;
using Domain.Ppa.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Ppa;

/// <summary>
/// Implementación de repositorio para el historial de PPAs usando EF Core.
/// </summary>
public sealed class PpaHistoryRepository : IPpaHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public PpaHistoryRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(PpaHistoryEntry entry, CancellationToken ct = default)
    {
        await _context.PpaHistoryEntries.AddAsync(entry, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<PpaHistoryEntry> entries, CancellationToken ct = default)
    {
        await _context.PpaHistoryEntries.AddRangeAsync(entries, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyCollection<PpaHistoryEntry>> GetByPpaAsync(
        Guid ppaId,
        CancellationToken ct = default)
    {
        return await _context.PpaHistoryEntries
            .Where(h => h.PpaId == ppaId)
            .OrderByDescending(h => h.PerformedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<PpaHistoryEntry>> GetByPpaAndActionTypeAsync(
        Guid ppaId,
        PpaHistoryActionType actionType,
        CancellationToken ct = default)
    {
        return await _context.PpaHistoryEntries
            .Where(h => h.PpaId == ppaId && h.ActionType == actionType)
            .OrderByDescending(h => h.PerformedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<PpaHistoryEntry>> GetByUserAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        return await _context.PpaHistoryEntries
            .Where(h => h.PerformedByUserId == userId)
            .OrderByDescending(h => h.PerformedAt)
            .ToListAsync(ct);
    }
}
