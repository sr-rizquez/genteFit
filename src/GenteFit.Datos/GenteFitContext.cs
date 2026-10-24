using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GenteFit.Datos;

public class GenteFitContext : DbContext
{
    // Windows: LocalDB, sin configurar nada. Mac: la variable de entorno GENTEFIT_BD.
    private const string CadenaLocalDb =
        @"Server=(localdb)\MSSQLLocalDB;Database=GenteFitDB;Trusted_Connection=True;TrustServerCertificate=True;";

    public GenteFitContext() { }
    public GenteFitContext(DbContextOptions<GenteFitContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Actividad> Actividades => Set<Actividad>();
    public DbSet<Monitor> Monitores => Set<Monitor>();
    public DbSet<Sala> Salas => Set<Sala>();
    public DbSet<Sesion> Sesiones => Set<Sesion>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<LoteIntercambio> Lotes => Set<LoteIntercambio>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            string cadena = Environment.GetEnvironmentVariable("GENTEFIT_BD") ?? CadenaLocalDb;
            optionsBuilder.UseSqlServer(cadena);
        }
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GenteFitContext).Assembly);

    // Todos los guardados pasan por aquí: los errores de SQL Server salen traducidos
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        try
        {
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql)
        {
            throw TraductorErroresSql.Traducir(sql, ex);
        }
    }
}