using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenteFit.Datos;

public class MonitorConfig : IEntityTypeConfiguration<Monitor>
{
    public void Configure(EntityTypeBuilder<Monitor> b)
    {
        b.ToTable("monitor");
        b.Property(m => m.Nombre).HasMaxLength(60);
        b.Property(m => m.Apellidos).HasMaxLength(100);
        b.Property(m => m.Email).HasMaxLength(150);
        b.HasIndex(m => m.Email).IsUnique();

        // Muchos a muchos: un monitor puede impartir varias actividades y al revés
        b.HasMany(m => m.Actividades).WithMany(a => a.Monitores)
         .UsingEntity<Dictionary<string, object>>(
             "monitor_actividad",
             r => r.HasOne<Actividad>().WithMany().HasForeignKey("id_actividad"),
             l => l.HasOne<Monitor>().WithMany().HasForeignKey("id_monitor"));

        b.HasData(
            new Monitor { Id = 1, Nombre = "María José", Apellidos = "Núñez O'Connor", Email = "mariajose@gentefit.test" },
            new Monitor { Id = 2, Nombre = "Jordi", Apellidos = "Puig i Font", Email = "jordi@gentefit.test" });
    }
}