using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenteFit.Datos;

public class UsuarioConfig : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("usuario");
        b.Property(u => u.NombreUsuario).HasMaxLength(50);
        b.Property(u => u.Email).HasMaxLength(150);
        b.Property(u => u.HashContrasena).HasMaxLength(255);
        b.Property(u => u.Rol).HasConversion<string>().HasMaxLength(20);

        b.HasIndex(u => u.NombreUsuario).IsUnique();
        b.HasIndex(u => u.Email).IsUnique();

        // Cuenta de acceso 0..1 — 0..1 cliente: la clave foránea está en usuario
        b.HasOne(u => u.Cliente).WithOne(c => c.Usuario)
         .HasForeignKey<Usuario>(u => u.IdCliente)
         .OnDelete(DeleteBehavior.SetNull);
    }
}