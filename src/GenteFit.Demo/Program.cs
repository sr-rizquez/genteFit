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

                switch (opcion)
                {
                    case "1": Alta(context); break;
                    case "2": Baja(context); break;
                    case "3": Consulta(context); break;
                    case "4": Modificacion(context); break;
                    case "5": ExportarXml(context); break;
                    case "6": ImportarXml(context); break;
                    case "0": return;
                    default: Console.WriteLine("Opción no válida."); break;
                }
            }
        }

        static void Alta(GenteFitContext context)
        {
            Console.Write("Nombre: ");
            var nombre = Console.ReadLine();
            Console.Write("Email: ");
            var email = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("Error: nombre y email son obligatorios.");
                return;
            }

            context.Clientes.Add(new Cliente { Nombre = nombre, Email = email });
            context.SaveChanges();
            Console.WriteLine("Cliente añadido.");
        }

        static void Baja(GenteFitContext context)
        {
            Console.Write("Id del cliente a eliminar: ");
            if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Id no válido."); return; }
            var cliente = context.Clientes.Find(id);
            if (cliente == null) { Console.WriteLine("No existe."); return; }
            context.Clientes.Remove(cliente);
            context.SaveChanges();
            Console.WriteLine("Cliente eliminado.");
        }

        static void Consulta(GenteFitContext context)
        {
            var clientes = context.Clientes.ToList();
            if (clientes.Count == 0) { Console.WriteLine("No hay clientes."); return; }
            foreach (var c in clientes)
                Console.WriteLine($"{c.Id} - {c.Nombre} - {c.Email}");
        }

        static void Modificacion(GenteFitContext context)
        {
            Console.Write("Id del cliente a modificar: ");
            if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Id no válido."); return; }
            var cliente = context.Clientes.Find(id);
            if (cliente == null) { Console.WriteLine("No existe."); return; }
            Console.Write($"Nuevo nombre ({cliente.Nombre}): ");
            var nombre = Console.ReadLine();
            Console.Write($"Nuevo email ({cliente.Email}): ");
            var email = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(nombre)) cliente.Nombre = nombre;
            if (!string.IsNullOrWhiteSpace(email)) cliente.Email = email;
            context.SaveChanges();
            Console.WriteLine("Cliente modificado.");
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
            Console.WriteLine("Exportado a clientes.xml");
        }

        static void ImportarXml(GenteFitContext context)
        {
            if (!File.Exists("clientes.xml")) { Console.WriteLine("No existe clientes.xml"); return; }

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
                Console.WriteLine("Importado desde clientes.xml");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al importar el XML: {ex.Message}");
            }
        }
    }
}