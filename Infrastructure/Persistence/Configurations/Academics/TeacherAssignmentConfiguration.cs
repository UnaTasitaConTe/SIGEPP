using Infrastructure.Persistence.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Academics;

/// <summary>
/// Configuración Fluent API para la entidad TeacherAssignmentEntity.
/// </summary>
public class TeacherAssignmentConfiguration : IEntityTypeConfiguration<TeacherAssignmentEntity>
{
    public void Configure(EntityTypeBuilder<TeacherAssignmentEntity> builder)
    {
        // Tabla
        builder.ToTable("TeacherAssignments");

        // Primary Key
        builder.HasKey(ta => ta.Id);

        // Propiedades
        builder.Property(ta => ta.TeacherId)
            .IsRequired();

        builder.Property(ta => ta.SubjectId)
            .IsRequired();

        builder.Property(ta => ta.AcademicPeriodId)
            .IsRequired();

        builder.Property(ta => ta.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ta => ta.CreatedAt)
            .IsRequired();

        builder.Property(ta => ta.UpdatedAt)
            .IsRequired();

        // Índices
        // Índice único compuesto para evitar duplicados: un docente no puede estar asignado dos veces a la misma asignatura en el mismo período
        builder.HasIndex(ta => new { ta.TeacherId, ta.SubjectId, ta.AcademicPeriodId })
            .IsUnique()
            .HasDatabaseName("IX_TeacherAssignments_Unique");

        builder.HasIndex(ta => ta.TeacherId)
            .HasDatabaseName("IX_TeacherAssignments_TeacherId");

        builder.HasIndex(ta => ta.SubjectId)
            .HasDatabaseName("IX_TeacherAssignments_SubjectId");

        builder.HasIndex(ta => ta.AcademicPeriodId)
            .HasDatabaseName("IX_TeacherAssignments_AcademicPeriodId");

        builder.HasIndex(ta => ta.IsActive)
            .HasDatabaseName("IX_TeacherAssignments_IsActive");

        // Relación con UserEntity (Teacher)
        builder.HasOne(ta => ta.Teacher)
            .WithMany() // No necesitamos navegación inversa en UserEntity
            .HasForeignKey(ta => ta.TeacherId)
            .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar un usuario que tiene asignaciones

        // Relación con SubjectEntity
        builder.HasOne(ta => ta.Subject)
            .WithMany(s => s.TeacherAssignments)
            .HasForeignKey(ta => ta.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación con AcademicPeriodEntity
        builder.HasOne(ta => ta.AcademicPeriod)
            .WithMany(ap => ap.TeacherAssignments)
            .HasForeignKey(ta => ta.AcademicPeriodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
