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
        public virtual string Nombre { get; set; }
        public virtual string Email { get; set; }
    }

    public class PruebaNHibernate
    {
        public static void Ejecutar()
        {
            var cfg = new Configuration();
            cfg.DataBaseIntegration(db =>
            {
                db.ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Database=GenteFitDB;Trusted_Connection=True;TrustServerCertificate=True;";
                db.Dialect<MsSql2012Dialect>();
                db.Driver<MicrosoftDataSqlClientDriver>();
            });

            var mapper = new ModelMapper();
            mapper.Class<ClienteNh>(map =>
            {
                map.Table("Clientes");
                map.Id(c => c.Id, m => m.Generator(Generators.Identity));
                map.Property(c => c.Nombre);
                map.Property(c => c.Email);
            });
            cfg.AddMapping(mapper.CompileMappingForAllExplicitlyAddedEntities());

            using var sessionFactory = cfg.BuildSessionFactory();
            using var session = sessionFactory.OpenSession();

            var antes = session.QueryOver<ClienteNh>().RowCount();
            Console.WriteLine($"[NHibernate] Antes: {antes} clientes.");

            using var tx = session.BeginTransaction();
            session.Save(new ClienteNh { Nombre = "PruebaOrm", Email = "prueba@orm.com" });
            tx.Commit();

            var despues = session.QueryOver<ClienteNh>().RowCount();
            Console.WriteLine($"[NHibernate] Después: {despues} clientes.");
        }
    }
}