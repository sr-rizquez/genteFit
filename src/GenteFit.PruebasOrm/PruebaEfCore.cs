using GenteFit.Datos;

namespace GenteFit.PruebasOrm
{
    public class PruebaEfCore
    {
        public static void Ejecutar()
        {
            using var context = new GenteFitContext();

            Console.WriteLine($"[EF Core] Antes: {context.Clientes.Count()} clientes.");

            context.Clientes.Add(new Cliente { Dni = "99000001C", Nombre = "PruebaOrm", Apellidos = "EF Core", Email = "efcore@pruebaorm.test" });
            context.SaveChanges();

            Console.WriteLine($"[EF Core] Después: {context.Clientes.Count()} clientes.");
        }
    }
}