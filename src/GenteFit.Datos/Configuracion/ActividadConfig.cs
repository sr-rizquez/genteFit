using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenteFit.Datos;

public class ActividadConfig : IEntityTypeConfiguration<Actividad>
{
    public void Configure(EntityTypeBuilder<Actividad> b)
    {
        b.ToTable("actividad", t => t.HasCheckConstraint("ck_actividad_duracion", "[duracion_minutos] > 0"));
        b.Property(a => a.Nombre).HasMaxLength(80);
        b.Property(a => a.Tipo).HasConversion<string>().HasMaxLength(20);
        b.Property(a => a.Descripcion).HasMaxLength(500);
        b.Property(a => a.Intensidad).HasConversion<string>().HasMaxLength(10);

        b.HasData(
            new Actividad { Id = 1, Nombre = "Body Pump", Tipo = TipoActividad.BODY_PUMP, Intensidad = NivelIntensidad.ALTA, DuracionMinutos = 55 },
            new Actividad { Id = 2, Nombre = "Body Combat", Tipo = TipoActividad.BODY_COMBAT, Intensidad = NivelIntensidad.ALTA, DuracionMinutos = 55 },
            new Actividad { Id = 3, Nombre = "Core", Tipo = TipoActividad.CORE, Intensidad = NivelIntensidad.MEDIA, DuracionMinutos = 30 },
            new Actividad { Id = 4, Nombre = "Yoga", Tipo = TipoActividad.YOGA, Intensidad = NivelIntensidad.BAJA, DuracionMinutos = 60,
                            Descripcion = "Estiramientos y respiración para todos los niveles" },
            new Actividad { Id = 5, Nombre = "Spinning", Tipo = TipoActividad.SPINNING, Intensidad = NivelIntensidad.ALTA, DuracionMinutos = 45 },
            new Actividad { Id = 6, Nombre = "Zumba", Tipo = TipoActividad.ZUMBA, Intensidad = NivelIntensidad.MEDIA, DuracionMinutos = 50 });
    }
}