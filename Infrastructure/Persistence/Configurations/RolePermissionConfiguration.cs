using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de EF Core para la entity RolePermissionEntity.
/// Define el mapeo a la tabla RolePermissions (relación many-to-many).
/// </summary>
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermissionEntity>
{
    public void Configure(EntityTypeBuilder<RolePermissionEntity> builder)
    {
        builder.ToTable("RolePermissions");

        // Clave primaria compuesta
        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

        // Relación con Role
        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con Permission
        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices para optimizar búsquedas
        builder.HasIndex(rp => rp.RoleId)
            .HasDatabaseName("IX_RolePermissions_RoleId");

        builder.HasIndex(rp => rp.PermissionId)
            .HasDatabaseName("IX_RolePermissions_PermissionId");
    }
}
