using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad UserRoleEntity (tabla de unión many-to-many).
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRoleEntity>
{
    public void Configure(EntityTypeBuilder<UserRoleEntity> builder)
    {
        // Tabla
        builder.ToTable("UserRoles");

        // Primary Key compuesta
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        // Relación con User
        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con Role
        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Restrict); // No eliminar roles si tienen usuarios asignados

        // Índices adicionales para mejorar consultas
        builder.HasIndex(ur => ur.UserId)
            .HasDatabaseName("IX_UserRoles_UserId");

        builder.HasIndex(ur => ur.RoleId)
            .HasDatabaseName("IX_UserRoles_RoleId");
    }
}
