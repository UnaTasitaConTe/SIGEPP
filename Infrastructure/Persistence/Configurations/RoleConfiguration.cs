using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de EF Core para la entity RoleEntity.
/// Define el mapeo a la tabla Roles y sus restricciones.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.ToTable("Roles");

        // Clave primaria
        builder.HasKey(r => r.Id);

        // Propiedades
        builder.Property(r => r.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.IsSystemRole)
            .IsRequired()
            .HasDefaultValue(false);

        // Índice único en Code
        builder.HasIndex(r => r.Code)
            .IsUnique()
            .HasDatabaseName("IX_Roles_Code");

        // Índice para búsquedas por IsSystemRole
        builder.HasIndex(r => r.IsSystemRole)
            .HasDatabaseName("IX_Roles_IsSystemRole");
    }
}
