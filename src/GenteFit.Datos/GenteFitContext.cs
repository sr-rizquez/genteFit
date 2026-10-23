using Microsoft.EntityFrameworkCore;

namespace GenteFit.Datos
{
    public class GenteFitContext : DbContext
    {
        public DbSet<Cliente> Clientes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Mac: usa la variable GENTEFIT_BD (Docker). Windows: si no existe, usa LocalDB.
            var cadena = Environment.GetEnvironmentVariable("GENTEFIT_BD")
                         ?? @"Server=(localdb)\MSSQLLocalDB;Database=GenteFitDB;Trusted_Connection=True;TrustServerCertificate=True;";
            optionsBuilder.UseSqlServer(cadena);
        }
    }
}