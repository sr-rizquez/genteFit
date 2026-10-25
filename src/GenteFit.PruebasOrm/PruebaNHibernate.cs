using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;

namespace GenteFit.PruebasOrm
{
    public class ClienteNh
    {
        public virtual int Id { get; set; }
        public virtual string Dni { get; set; } = "";
        public virtual string Nombre { get; set; } = "";
        public virtual string Apellidos { get; set; } = "";
        public virtual string Email { get; set; } = "";
        public virtual DateTime FechaAlta { get; set; } = DateTime.Today;
        public virtual bool Activo { get; set; } = true;
    }

    public class PruebaNHibernate
    {
        public static void Ejecutar()
        {
            var cfg = new Configuration();
            cfg.DataBaseIntegration(db =>
            {
                db.ConnectionString = Conexion.Cadena;
                db.Dialect<MsSql2012Dialect>();
                db.Driver<MicrosoftDataSqlClientDriver>();
            });

            var mapper = new ModelMapper();
            mapper.Class<ClienteNh>(map =>
            {
                map.Table("cliente");
                map.Id(c => c.Id, m => { m.Column("id"); m.Generator(Generators.Identity); });
                map.Property(c => c.Dni, m => m.Column("dni"));
                map.Property(c => c.Nombre, m => m.Column("nombre"));
                map.Property(c => c.Apellidos, m => m.Column("apellidos"));
                map.Property(c => c.Email, m => m.Column("email"));
                map.Property(c => c.FechaAlta, m => m.Column("fecha_alta"));
                map.Property(c => c.Activo, m => m.Column("activo"));
            });
            cfg.AddMapping(mapper.CompileMappingForAllExplicitlyAddedEntities());

            using var sessionFactory = cfg.BuildSessionFactory();
            using var session = sessionFactory.OpenSession();

            var antes = session.QueryOver<ClienteNh>().RowCount();
            Console.WriteLine($"[NHibernate] Antes: {antes} clientes.");

            using var tx = session.BeginTransaction();
            session.Save(new ClienteNh { Dni = "99000004T", Nombre = "PruebaOrm", Apellidos = "NHibernate", Email = "nhibernate@pruebaorm.test" });
            tx.Commit();

            var despues = session.QueryOver<ClienteNh>().RowCount();
            Console.WriteLine($"[NHibernate] Después: {despues} clientes.");
        }
    }
}