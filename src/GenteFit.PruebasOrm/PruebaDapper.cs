using Dapper;
using Microsoft.Data.SqlClient;

namespace GenteFit.PruebasOrm
{
    public class PruebaDapper
    {
        public static void Ejecutar()
        {
            var cadena = @"Server=(localdb)\MSSQLLocalDB;Database=GenteFitDB;Trusted_Connection=True;TrustServerCertificate=True;";
            using var connection = new SqlConnection(cadena);

            var antes = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Clientes");
            Console.WriteLine($"[Dapper] Antes: {antes} clientes.");

            connection.Execute("INSERT INTO Clientes (Nombre, Email) VALUES (@Nombre, @Email)",
                new { Nombre = "PruebaOrm", Email = "prueba@orm.com" });

            var despues = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Clientes");
            Console.WriteLine($"[Dapper] Después: {despues} clientes.");
        }
    }
}