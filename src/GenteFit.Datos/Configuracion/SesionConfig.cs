using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenteFit.Datos;

public class SesionConfig : IEntityTypeConfiguration<Sesion>
{
    public void Configure(EntityTypeBuilder<Sesion> b)
    {
        b.ToTable("sesion");
        b.Property(s => s.Estado).HasConversion<string>().HasMaxLength(12);

        // E-05: no puede haber dos sesiones en la misma sala a la misma hora
        b.HasIndex(s => new { s.IdSala, s.FechaHoraInicio }).IsUnique();

        // Restrict: no se puede borrar una actividad, sala o monitor con sesiones (E-06)
        b.HasOne(s => s.Actividad).WithMany(a => a.Sesiones)
         .HasForeignKey(s => s.IdActividad).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(s => s.Sala).WithMany()
         .HasForeignKey(s => s.IdSala).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(s => s.Monitor).WithMany()
         .HasForeignKey(s => s.IdMonitor).OnDelete(DeleteBehavior.Restrict);

        b.HasData(
            new Sesion { Id = 1, IdActividad = 4, IdSala = 1, IdMonitor = 1, FechaHoraInicio = new DateTime(2026, 11, 2, 18, 30, 0) },
            new Sesion { Id = 2, IdActividad = 3, IdSala = 1, IdMonitor = 1, FechaHoraInicio = new DateTime(2026, 11, 3, 19, 0, 0) },
            new Sesion { Id = 3, IdActividad = 5, IdSala = 2, IdMonitor = 2, FechaHoraInicio = new DateTime(2026, 11, 4, 20, 0, 0) });
    }
}