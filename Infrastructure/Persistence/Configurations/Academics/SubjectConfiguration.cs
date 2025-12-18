using Infrastructure.Persistence.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Academics;

/// <summary>
/// Configuración Fluent API para la entidad SubjectEntity.
/// </summary>
public class SubjectConfiguration : IEntityTypeConfiguration<SubjectEntity>
{
    public void Configure(EntityTypeBuilder<SubjectEntity> builder)
    {
        // Tabla
        builder.ToTable("Subjects");

        // Primary Key
        builder.HasKey(s => s.Id);

        // Propiedades
        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .IsRequired(false) // Opcional
            .HasMaxLength(1000);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .IsRequired();

        // Índices
        builder.HasIndex(s => s.Code)
            .IsUnique()
            .HasDatabaseName("IX_Subjects_Code");

        builder.HasIndex(s => s.IsActive)
            .HasDatabaseName("IX_Subjects_IsActive");

        builder.HasIndex(s => s.Name)
            .HasDatabaseName("IX_Subjects_Name");

        // Relación con TeacherAssignments (one-to-many)
        builder.HasMany(s => s.TeacherAssignments)
            .WithOne(ta => ta.Subject)
            .HasForeignKey(ta => ta.SubjectId)
            .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar una asignatura si tiene asignaciones
    }
}
