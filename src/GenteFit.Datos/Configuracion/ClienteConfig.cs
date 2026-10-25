using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenteFit.Datos;

public class ClienteConfig : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> b)
    {
        b.ToTable("cliente");
        b.Property(c => c.Dni).HasMaxLength(9);
        b.Property(c => c.Nombre).HasMaxLength(60);
        b.Property(c => c.Apellidos).HasMaxLength(100);
        b.Property(c => c.Email).HasMaxLength(150);
        b.Property(c => c.Telefono).HasMaxLength(15);
        b.Property(c => c.FechaNacimiento).HasColumnType("date");
        b.Property(c => c.FechaAlta).HasColumnType("date");
        b.Property(c => c.FechaBaja).HasColumnType("date");

        b.HasIndex(c => c.Dni).IsUnique();     // E-04
        b.HasIndex(c => c.Email).IsUnique();   // E-04
    }
}