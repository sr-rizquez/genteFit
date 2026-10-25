using Microsoft.Data.SqlClient;

namespace GenteFit.PruebasOrm
{
    public class PruebaAdoNet
    {
        public static void Ejecutar()
        {
            var cadena = @"Server=(localdb)\MSSQLLocalDB;Database=GenteFitDB;Trusted_Connection=True;TrustServerCertificate=True;";
            using var connection = new SqlConnection(cadena);
            connection.Open();

            using var cmdAntes = new SqlCommand("SELECT COUNT(*) FROM Clientes", connection);
            var antes = (int)cmdAntes.ExecuteScalar();
            Console.WriteLine($"[ADO.NET] Antes: {antes} clientes.");

            using var cmdInsert = new SqlCommand("INSERT INTO Clientes (Nombre, Email) VALUES (@Nombre, @Email)", connection);
            cmdInsert.Parameters.AddWithValue("@Nombre", "PruebaOrm");
            cmdInsert.Parameters.AddWithValue("@Email", "prueba@orm.com");
            cmdInsert.ExecuteNonQuery();

            using var cmdDespues = new SqlCommand("SELECT COUNT(*) FROM Clientes", connection);
            var despues = (int)cmdDespues.ExecuteScalar();
            Console.WriteLine($"[ADO.NET] Después: {despues} clientes.");
        }
    }
}