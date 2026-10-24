using GenteFit.Datos;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace GenteFit.Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var context = new GenteFitContext();

            while (true)
            {
                Console.WriteLine("\n--- GENTEFIT ---");
                Console.WriteLine("1. Alta de cliente");
                Console.WriteLine("2. Baja de cliente");
                Console.WriteLine("3. Consulta de clientes");
                Console.WriteLine("4. Modificación de cliente");
                Console.WriteLine("5. Exportar a XML");
                Console.WriteLine("6. Importar desde XML");
                Console.WriteLine("0. Salir");
                Console.Write("Opción: ");
                var opcion = Console.ReadLine();

                // He agregado un bloque try-catch para capturar errores de GenteFit y otros errores inesperados, para que no se cierre el programa
                try
                {
                    switch (opcion)
                    {
                        case "1": Alta(context); break;
                        case "2": Baja(context); break;
                        case "3": Consulta(context); break;
                        case "4": Modificacion(context); break;
                        case "5": ExportarXml(context); break;
                        case "6": ImportarXml(context); break;
                        case "0": return;
                        default: MostrarError("Opción no válida."); break;
                    }
                }
                catch ( GenteFitException ex)
                {
                    // Error de GenteFit: se muestra con su código y el programa
                    MostrarError($"[{ex.Codigo}] {ex.Message}");
                    context.ChangeTracker.Clear(); // Descartar cambios pendientes
                }
                catch (Exception ex)
                {
                    // Cualquier otro error inesperado: tamppoco cierra el programa
                    MostrarError($"[ERROR] {ex.Message}");
                    context.ChangeTracker.Clear(); // Descartar cambios pendientes
                }
            }
        }

        static void Alta(GenteFitContext context)
        {
            var cliente = new Cliente
            {
                Dni = Leer("DNI")!,
                Nombre = Leer("Nombre")!,
                Apellidos = Leer("Apellidos")!,
                Email = Leer("Email")!,
                Telefono = Leer("Teléfono"),
            };

            var repositorio = new RepositorioEF<Cliente>(context);
            repositorio.Anyadir(cliente);
            context.SaveChanges();
            MostrarExito($"Cliente añadido con id {cliente.Id}.");
        }

        static void Baja(GenteFitContext context)
        {
            Console.Write("Id del cliente a eliminar: ");
            if (!int.TryParse(Console.ReadLine(), out var id)) { MostrarError("El id tiene que ser un número."); return; }
            var cliente = context.Clientes.Find(id);
            if (cliente == null) { MostrarError($"No existe ningún cliente con id {id}."); return; }
            context.Clientes.Remove(cliente);
            context.SaveChanges();
            Console.WriteLine("Cliente eliminado.");
        }

        static void Consulta(GenteFitContext context)
        {
            var clientes = context.Clientes.ToList();
            if (clientes.Count == 0) { MostrarError("No hay clientes."); return; }
            foreach (var c in clientes)
                Console.WriteLine($"{c.Id} - {c.Nombre} - {c.Email}");
        }

        static void Modificacion(GenteFitContext context)
        {
            Console.Write("Id del cliente a modificar: ");
            if (!int.TryParse(Console.ReadLine(), out var id)) { MostrarError("El id tiene que ser un número."); return; }
            var cliente = context.Clientes.Find(id);
            if (cliente == null) { MostrarError($"No existe ningún cliente con id {id}."); return; }
            Console.Write($"Nuevo nombre ({cliente.Nombre}): ");
            var nombre = Console.ReadLine();
            Console.Write($"Nuevo email ({cliente.Email}): ");
            var email = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(nombre)) cliente.Nombre = nombre;
            if (!string.IsNullOrWhiteSpace(email)) cliente.Email = email;
            context.SaveChanges();
            MostrarExito("Cliente {cliente.Nombre} con {cliente.Id} modificado.");
        }

        static void ExportarXml(GenteFitContext context)
        {
            var clientes = context.Clientes.ToList();
            var doc = new XDocument(
                new XElement("Clientes",
                    clientes.Select(c =>
                        new XElement("Cliente",
                            new XElement("Id", c.Id),
                            new XElement("Nombre", c.Nombre),
                            new XElement("Email", c.Email)))));
            doc.Save("clientes.xml");
            MostrarExito("Exportado a clientes.xml");
        }

        static void ImportarXml(GenteFitContext context)
        {
            if (!File.Exists("clientes.xml")) { MostrarError("No se encuentra el archivo clientes.xml"); return; }

            try
            {
                var doc = XDocument.Load("clientes.xml");
                foreach (var el in doc.Root.Elements("Cliente"))
                {
                    var nombre = el.Element("Nombre")?.Value;
                    var email = el.Element("Email")?.Value;
                    if (nombre != null && email != null)
                        context.Clientes.Add(new Cliente { Nombre = nombre, Email = email });
                }
                context.SaveChanges();
                MostrarExito("Importado desde clientes.xml");
            }
            catch (Exception ex)
            {
                MostrarError($"Error al importar el XML: {ex.Message}");
            }
        }

        static string? Leer(string etiqueta)
        {
            Console.Write($"{etiqueta}: ");
            var texto = Console.ReadLine()?.Trim();
            return string.IsNullOrEmpty(texto) ? null : texto;
        }

        // Nueva funcion para mostrar errores en rojo, para que se vea mejor en la consola
        static void MostrarError(string mensaje)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(mensaje);
            Console.ResetColor();
        }

        // Nueva funcion para mostrar mensajes en verde, para que se vea mejor en la consola
        static void MostrarExito(String mensaje)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(mensaje);
            Console.ResetColor();
        }
    }
}