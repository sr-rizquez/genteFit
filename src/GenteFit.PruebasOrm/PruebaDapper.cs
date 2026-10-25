using Dapper;
using Microsoft.Data.SqlClient;

namespace GenteFit.PruebasOrm
{
    public class PruebaDapper
    {
        public static void Ejecutar()
        {
            var cadena = Conexion.Cadena;
            using var connection = new SqlConnection(cadena);

            var antes = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM cliente");
            Console.WriteLine($"[Dapper] Antes: {antes} clientes.");

            connection.Execute("INSERT INTO cliente (dni, nombre, apellidos, email, fecha_alta, activo) VALUES (@Dni, @Nombre, @Apellidos, @Email, @FechaAlta, 1)",
                new { Dni = "99000002K", Nombre = "PruebaOrm", Apellidos = "Dapper", Email = "dapper@pruebaorm.test", FechaAlta = DateTime.Today });

            var despues = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM cliente");
            Console.WriteLine($"[Dapper] Después: {despues} clientes.");
        }
    }
}