using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenteFit.Datos;

public class LoteIntercambioConfig : IEntityTypeConfiguration<LoteIntercambio>
{
    public void Configure(EntityTypeBuilder<LoteIntercambio> b)
    {
        b.ToTable("lote_intercambio");
        b.Property(l => l.Codigo).HasMaxLength(20);
        b.HasIndex(l => l.Codigo).IsUnique();
        b.Property(l => l.Tipo).HasConversion<string>().HasMaxLength(12);
        b.Property(l => l.FechaDesde).HasColumnType("date");
        b.Property(l => l.FechaHasta).HasColumnType("date");
        b.Property(l => l.RutaFichero).HasMaxLength(260);

        b.HasOne(l => l.Usuario).WithMany()
         .HasForeignKey(l => l.IdUsuario).OnDelete(DeleteBehavior.Restrict);
    }
}