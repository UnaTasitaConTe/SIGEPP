using Infrastructure.Persistence.Entities.Ppa;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Ppa;

/// <summary>
/// Configuración Fluent API para la entidad PpaStudentEntity.
/// </summary>
public class PpaStudentConfiguration : IEntityTypeConfiguration<PpaStudentEntity>
{
    public void Configure(EntityTypeBuilder<PpaStudentEntity> builder)
    {
        // Tabla
        builder.ToTable("PpaStudents");

        // Primary Key
        builder.HasKey(s => s.Id);

        // Propiedades
        builder.Property(s => s.PpaId)
            .IsRequired();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Índices
        builder.HasIndex(s => s.PpaId)
            .HasDatabaseName("IX_PpaStudents_PpaId");

        // Relaciones
        builder.HasOne(s => s.Ppa)
            .WithMany(p => p.Students)
            .HasForeignKey(s => s.PpaId)
            .OnDelete(DeleteBehavior.Cascade); // Si se elimina el PPA, eliminar sus estudiantes
    }
}
