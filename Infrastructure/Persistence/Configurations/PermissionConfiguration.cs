using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de EF Core para la entity PermissionEntity.
/// Define el mapeo a la tabla Permissions y sus restricciones.
/// </summary>
public class PermissionConfiguration : IEntityTypeConfiguration<PermissionEntity>
{
    public void Configure(EntityTypeBuilder<PermissionEntity> builder)
    {
        builder.ToTable("Permissions");

        // Clave primaria
        builder.HasKey(p => p.Id);

        // Propiedades
        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Module)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        // Índices únicos
        builder.HasIndex(p => p.Code)
            .IsUnique()
            .HasDatabaseName("IX_Permissions_Code");

        // Índice compuesto para búsquedas por módulo
        builder.HasIndex(p => new { p.Module, p.Action })
            .HasDatabaseName("IX_Permissions_Module_Action");
    }
}
