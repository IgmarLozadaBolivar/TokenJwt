using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TokenJwt.Entities;
namespace TokenJwt.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(e => e.Id);

        builder.Property(f => f.Id)
        .IsRequired()
        .HasMaxLength(3);

        builder.Property(f => f.Username)
        .IsRequired()
        .HasColumnName("Username")
        .HasComment("Nombre del usuario")
        .HasColumnType("varchar(255)")
        .HasMaxLength(255);

        builder.Property(f => f.Password)
        .IsRequired()
        .HasColumnName("Password")
        .HasComment("Contraseña del usuario")
        .HasColumnType("varchar(255)")
        .HasMaxLength(255);

        builder.HasMany(p => p.RefreshTokens)
        .WithOne(p => p.User)
        .HasForeignKey(p => p.IdUser);
    }
}