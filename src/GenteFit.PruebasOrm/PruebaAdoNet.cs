using Microsoft.Data.SqlClient;

namespace GenteFit.PruebasOrm
{
    public class PruebaAdoNet
    {
        public static void Ejecutar()
        {
            var cadena = Conexion.Cadena;
            using var connection = new SqlConnection(cadena);
            connection.Open();

            using var cmdAntes = new SqlCommand("SELECT COUNT(*) FROM cliente", connection);
            var antes = (int)cmdAntes.ExecuteScalar();
            Console.WriteLine($"[ADO.NET] Antes: {antes} clientes.");

            using var cmdInsert = new SqlCommand("INSERT INTO cliente (dni, nombre, apellidos, email, fecha_alta, activo) VALUES (@Dni, @Nombre, @Apellidos, @Email, @FechaAlta, 1)", connection);
            cmdInsert.Parameters.AddWithValue("@Dni", "99000003E");
            cmdInsert.Parameters.AddWithValue("@Nombre", "PruebaOrm");
            cmdInsert.Parameters.AddWithValue("@Apellidos", "ADO.NET");
            cmdInsert.Parameters.AddWithValue("@Email", "adonet@pruebaorm.test");
            cmdInsert.Parameters.AddWithValue("@FechaAlta", DateTime.Today);
            cmdInsert.ExecuteNonQuery();

            using var cmdDespues = new SqlCommand("SELECT COUNT(*) FROM cliente", connection);
            var despues = (int)cmdDespues.ExecuteScalar();
            Console.WriteLine($"[ADO.NET] Después: {despues} clientes.");
        }
    }
}