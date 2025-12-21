using Domain.Ppa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Ppa;

/// <summary>
/// Configuración de EF Core para la entidad PpaHistoryEntry.
/// </summary>
public class PpaHistoryEntryConfiguration : IEntityTypeConfiguration<PpaHistoryEntry>
{
    public void Configure(EntityTypeBuilder<PpaHistoryEntry> builder)
    {
        builder.ToTable("PpaHistoryEntries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .IsRequired();

        builder.Property(e => e.PpaId)
            .IsRequired();

        builder.Property(e => e.PerformedByUserId)
            .IsRequired();

        builder.Property(e => e.PerformedAt)
            .IsRequired();

        builder.Property(e => e.ActionType)
            .IsRequired()
            .HasConversion<int>(); // Enum a int

        builder.Property(e => e.OldValue)
            .HasMaxLength(2000);

        builder.Property(e => e.NewValue)
            .HasMaxLength(2000);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        // Índices para mejorar el rendimiento de consultas
        builder.HasIndex(e => e.PpaId)
            .HasDatabaseName("IX_PpaHistoryEntries_PpaId");

        builder.HasIndex(e => e.PerformedByUserId)
            .HasDatabaseName("IX_PpaHistoryEntries_PerformedByUserId");

        builder.HasIndex(e => e.PerformedAt)
            .HasDatabaseName("IX_PpaHistoryEntries_PerformedAt");

        builder.HasIndex(e => new { e.PpaId, e.ActionType })
            .HasDatabaseName("IX_PpaHistoryEntries_PpaId_ActionType");
    }
}
