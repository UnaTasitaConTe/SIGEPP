using Infrastructure.Persistence.Entities.Ppa;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Ppa;

/// <summary>
/// Configuración Fluent API para la tabla intermedia PpaTeacherAssignmentEntity.
/// </summary>
public class PpaTeacherAssignmentConfiguration : IEntityTypeConfiguration<PpaTeacherAssignmentEntity>
{
    public void Configure(EntityTypeBuilder<PpaTeacherAssignmentEntity> builder)
    {
        // Tabla
        builder.ToTable("PpaTeacherAssignments");

        // Clave compuesta
        builder.HasKey(pta => new { pta.PpaId, pta.TeacherAssignmentId });

        // Propiedades
        builder.Property(pta => pta.PpaId)
            .IsRequired();

        builder.Property(pta => pta.TeacherAssignmentId)
            .IsRequired();

        // Índices
        builder.HasIndex(pta => pta.PpaId)
            .HasDatabaseName("IX_PpaTeacherAssignments_PpaId");

        builder.HasIndex(pta => pta.TeacherAssignmentId)
            .HasDatabaseName("IX_PpaTeacherAssignments_TeacherAssignmentId");

        // Relaciones
        builder.HasOne(pta => pta.Ppa)
            .WithMany(p => p.PpaTeacherAssignments)
            .HasForeignKey(pta => pta.PpaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pta => pta.TeacherAssignment)
            .WithMany()
            .HasForeignKey(pta => pta.TeacherAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
