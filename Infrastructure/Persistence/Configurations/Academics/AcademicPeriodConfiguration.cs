using Infrastructure.Persistence.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Academics;

/// <summary>
/// Configuración Fluent API para la entidad AcademicPeriodEntity.
/// </summary>
public class AcademicPeriodConfiguration : IEntityTypeConfiguration<AcademicPeriodEntity>
{
    public void Configure(EntityTypeBuilder<AcademicPeriodEntity> builder)
    {
        // Tabla
        builder.ToTable("AcademicPeriods");

        // Primary Key
        builder.HasKey(ap => ap.Id);

        // Propiedades
        builder.Property(ap => ap.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(ap => ap.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ap => ap.StartDate)
            .IsRequired(false); // Opcional

        builder.Property(ap => ap.EndDate)
            .IsRequired(false); // Opcional

        builder.Property(ap => ap.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ap => ap.CreatedAt)
            .IsRequired();

        builder.Property(ap => ap.UpdatedAt)
            .IsRequired();

        // Índices
        builder.HasIndex(ap => ap.Code)
            .IsUnique()
            .HasDatabaseName("IX_AcademicPeriods_Code");

        builder.HasIndex(ap => ap.IsActive)
            .HasDatabaseName("IX_AcademicPeriods_IsActive");

        // Si se requiere búsqueda por fechas
        builder.HasIndex(ap => new { ap.StartDate, ap.EndDate })
            .HasDatabaseName("IX_AcademicPeriods_Dates");

        // Relación con TeacherAssignments (one-to-many)
        builder.HasMany(ap => ap.TeacherAssignments)
            .WithOne(ta => ta.AcademicPeriod)
            .HasForeignKey(ta => ta.AcademicPeriodId)
            .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar un período si tiene asignaciones
    }
}
