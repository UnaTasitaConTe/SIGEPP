using Infrastructure.Persistence.Entities.Ppa;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Ppa;

/// <summary>
/// Configuración Fluent API para la entidad PpaEntity.
/// </summary>
public class PpaConfiguration : IEntityTypeConfiguration<PpaEntity>
{
    public void Configure(EntityTypeBuilder<PpaEntity> builder)
    {
        // Tabla
        builder.ToTable("Ppas");

        // Primary Key
        builder.HasKey(p => p.Id);

        // Propiedades
        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(p => p.GeneralObjective)
            .IsRequired(false)
            .HasMaxLength(1000);

        builder.Property(p => p.SpecificObjectives)
            .IsRequired(false)
            .HasMaxLength(2000);

        builder.Property(p => p.Description)
            .IsRequired(false)
            .HasMaxLength(3000);

        builder.Property(p => p.Status)
            .IsRequired();

        builder.Property(p => p.AcademicPeriodId)
            .IsRequired();

        builder.Property(p => p.PrimaryTeacherId)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired(false);

        // Índices
        builder.HasIndex(p => p.AcademicPeriodId)
            .HasDatabaseName("IX_Ppas_AcademicPeriodId");

        builder.HasIndex(p => p.PrimaryTeacherId)
            .HasDatabaseName("IX_Ppas_PrimaryTeacherId");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Ppas_Status");

        // Índice compuesto para búsquedas frecuentes
        builder.HasIndex(p => new { p.PrimaryTeacherId, p.AcademicPeriodId })
            .HasDatabaseName("IX_Ppas_Teacher_Period");

        // Relaciones
        builder.HasOne(p => p.AcademicPeriod)
            .WithMany()
            .HasForeignKey(p => p.AcademicPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.PrimaryTeacher)
            .WithMany()
            .HasForeignKey(p => p.PrimaryTeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Attachments)
            .WithOne(a => a.Ppa)
            .HasForeignKey(a => a.PpaId)
            .OnDelete(DeleteBehavior.Cascade); // Si se elimina el PPA, eliminar sus anexos

        // Configuración de continuidad
        builder.Property(p => p.ContinuationOfPpaId)
            .IsRequired(false);

        builder.Property(p => p.ContinuedByPpaId)
            .IsRequired(false);

        builder.HasIndex(p => p.ContinuationOfPpaId)
            .HasDatabaseName("IX_Ppas_ContinuationOfPpaId");

        builder.HasIndex(p => p.ContinuedByPpaId)
            .HasDatabaseName("IX_Ppas_ContinuedByPpaId");
    }
}
