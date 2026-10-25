using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenteFit.Datos;

public class SalaConfig : IEntityTypeConfiguration<Sala>
{
    public void Configure(EntityTypeBuilder<Sala> b)
    {
        b.ToTable("sala", t => t.HasCheckConstraint("ck_sala_aforo", "[aforo] > 0"));
        b.Property(s => s.Nombre).HasMaxLength(60);
        b.HasIndex(s => s.Nombre).IsUnique();

        b.HasData(
            new Sala { Id = 1, Nombre = "Sala Montjuïc", Aforo = 16 },
            new Sala { Id = 2, Nombre = "Sala d'Spinning", Aforo = 16 });
    }
}