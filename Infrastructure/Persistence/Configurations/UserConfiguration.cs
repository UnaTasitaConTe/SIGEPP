using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad UserEntity.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        // Tabla
        builder.ToTable("Users");

        // Primary Key
        builder.HasKey(u => u.Id);

        // Propiedades
        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.IsActive)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .IsRequired();

        // Índices
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        builder.HasIndex(u => u.IsActive)
            .HasDatabaseName("IX_Users_IsActive");

        // Relación many-to-many con Roles a través de UserRoles
        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
