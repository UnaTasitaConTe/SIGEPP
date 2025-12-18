using Infrastructure.Persistence.Entities.Ppa;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Ppa;

/// <summary>
/// Configuración Fluent API para la entidad PpaAttachmentEntity.
/// </summary>
public class PpaAttachmentConfiguration : IEntityTypeConfiguration<PpaAttachmentEntity>
{
    public void Configure(EntityTypeBuilder<PpaAttachmentEntity> builder)
    {
        // Tabla
        builder.ToTable("PpaAttachments");

        // Primary Key
        builder.HasKey(a => a.Id);

        // Propiedades
        builder.Property(a => a.PpaId)
            .IsRequired();

        builder.Property(a => a.Type)
            .IsRequired();

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(a => a.FileKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.ContentType)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(a => a.UploadedByUserId)
            .IsRequired();

        builder.Property(a => a.UploadedAt)
            .IsRequired();

        builder.Property(a => a.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.DeletedAt)
            .IsRequired(false);

        // Índices
        builder.HasIndex(a => a.PpaId)
            .HasDatabaseName("IX_PpaAttachments_PpaId");

        builder.HasIndex(a => a.Type)
            .HasDatabaseName("IX_PpaAttachments_Type");

        builder.HasIndex(a => a.UploadedByUserId)
            .HasDatabaseName("IX_PpaAttachments_UploadedByUserId");

        builder.HasIndex(a => a.IsDeleted)
            .HasDatabaseName("IX_PpaAttachments_IsDeleted");

        // Índice compuesto para búsquedas por PPA y tipo
        builder.HasIndex(a => new { a.PpaId, a.Type })
            .HasDatabaseName("IX_PpaAttachments_Ppa_Type");

        // Índice único para FileKey (para prevenir duplicados)
        builder.HasIndex(a => a.FileKey)
            .IsUnique()
            .HasDatabaseName("IX_PpaAttachments_FileKey");

        // Relaciones
        builder.HasOne(a => a.Ppa)
            .WithMany(p => p.Attachments)
            .HasForeignKey(a => a.PpaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.UploadedBy)
            .WithMany()
            .HasForeignKey(a => a.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
