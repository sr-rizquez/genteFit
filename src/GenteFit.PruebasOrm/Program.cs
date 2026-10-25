using GenteFit.PruebasOrm;
using Microsoft.Data.SqlClient;

namespace GenteFit.PruebasOrm
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Pruebas de ORM ===\n");

            PruebaEfCore.Ejecutar();
            PruebaDapper.Ejecutar();
            PruebaAdoNet.Ejecutar();
            PruebaNHibernate.Ejecutar();

            Console.WriteLine("\n=== Limpieza ===");
            Limpiar();

            Console.WriteLine("\n=== Fin ===");
            Console.ReadLine();
        }

        static void Limpiar()
        {
            var cadena = @"Server=(localdb)\MSSQLLocalDB;Database=GenteFitDB;Trusted_Connection=True;TrustServerCertificate=True;";
            using var connection = new SqlConnection(cadena);
            connection.Open();
            using var cmd = new SqlCommand("DELETE FROM Clientes WHERE Nombre = 'PruebaOrm'", connection);
            var borrados = cmd.ExecuteNonQuery();
            Console.WriteLine($"Clientes de prueba borrados: {borrados}");
        }
    }
}