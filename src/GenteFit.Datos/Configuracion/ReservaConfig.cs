using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenteFit.Datos;

public class ReservaConfig : IEntityTypeConfiguration<Reserva>
{
    public void Configure(EntityTypeBuilder<Reserva> b)
    {
        b.ToTable("reserva");
        b.Property(r => r.Estado).HasConversion<string>().HasMaxLength(12);

        // E-01: un cliente no puede tener dos reservas vivas en la misma sesión
        b.HasIndex(r => new { r.IdSesion, r.IdCliente })
         .IsUnique()
         .HasFilter("[estado] <> 'CANCELADA'");

        b.HasOne(r => r.Sesion).WithMany(s => s.Reservas)
         .HasForeignKey(r => r.IdSesion).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(r => r.Cliente).WithMany(c => c.Reservas)
         .HasForeignKey(r => r.IdCliente).OnDelete(DeleteBehavior.Restrict);
    }
}