using GenteFit.Datos;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
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
                Console.WriteLine("7. Probar errores de la base de datos");
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
                        case "7": ProbarErroresBaseDatos(context); break;
                        case "0": return;
                        default: MostrarError("Opción no válida."); break;
                    }
                }
                catch (GenteFitException ex)
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

        // Cada campo se valida al escribirlo: si está mal, se explica el error y se vuelve a pedir.
        // El DNI o el email repetidos no se pueden comprobar aquí: los rechaza la base de datos (E-04).
        static void Alta(GenteFitContext context)
        {
            Console.WriteLine("Los campos marcados con * son obligatorios.");
            var cliente = new Cliente
            {
                Dni = Pedir("DNI", true, ValidarDni)!.ToUpperInvariant(),
                Nombre = Pedir("Nombre", true, t => ValidarTexto(t, 60))!,
                Apellidos = Pedir("Apellidos", true, t => ValidarTexto(t, 100))!,
                Email = Pedir("Email", true, ValidarEmail)!,
                Telefono = Pedir("Teléfono", false, ValidarTelefono)
            };

            var repositorio = new RepositorioEF<Cliente>(context);
            repositorio.Anyadir(cliente);
            context.SaveChanges();
            MostrarExito($"Cliente añadido con id {cliente.Id}.");
        }


        /*
             Aquí he cambiado toda la función de baja. Explico: El cliente no se elimina, se da de baja, por lo tanto se queda en la tabla con el campo de fecha de baja.
             Qué hace?: Pone FechaBaja = hoy y Activo = false, UPDATE (la fila se queda, no se elimina).
             Regla error, la E-03 del doc Producto 1; No deja dar de baja a un cliente con reservas activas.
        */
        static void Baja(GenteFitContext context)
        {
            Console.Write("Id del cliente a dar de baja: ");
            if (!int.TryParse(Console.ReadLine(), out var id)) { MostrarError("El id tiene que ser un número."); return; }

            var repositorio = new RepositorioEF<Cliente>(context);
            var cliente = repositorio.ObtenerPorId(id);
            if (cliente == null) { MostrarError($"No existe ningún cliente con id {id}."); return; }
            if (!cliente.Activo) { MostrarError($"El cliente con id {id} ya está dado de baja."); return; }

            // E-03: No deja dar de baja a un cliente con reservas activas.
            bool tieneReservasActivas = context.Reservas.Any(r => r.IdCliente == id && r.Estado != EstadoReserva.CANCELADA);
            if (tieneReservasActivas)
                throw new ClienteConReservasException($"No se puede dar de baja: el cliente {cliente.Nombre} con id {id} tiene reservas activas.");

            // Baja lógica: el cliente no se borra, se marca con la fecha de baja y activo = false
            cliente.FechaBaja = DateTime.Today;
            cliente.Activo = false;
            repositorio.Actualizar(cliente);
            context.SaveChanges();
            MostrarExito($"Cliente {cliente.Nombre} {cliente.Apellidos} dado de baja el {cliente.FechaBaja:dd/MM/yyyy}.");
        }

        static void Consulta(GenteFitContext context)
        {
            var repositorio = new RepositorioEF<Cliente>(context);
            var clientes = repositorio.Listar();
            if (clientes.Count == 0) { Console.WriteLine("No hay clientes."); return; }

            foreach (var c in clientes)
            {
                string estado = c.Activo ? "activo" : $"de baja desde {c.FechaBaja:dd/MM/yyyy}";
                Console.WriteLine($"{c.Id} - {c.Dni} - {c.Nombre} {c.Apellidos} - {c.Telefono} - {c.Email} - {estado}");
            }

            int activos = repositorio.Listar(c => c.Activo).Count;
            Console.WriteLine($"Clientes activos: {activos} de {clientes.Count}.");
        }

        static void Modificacion(GenteFitContext context)
        {
            Console.Write("Id del cliente a modificar: ");
            if (!int.TryParse(Console.ReadLine(), out var id)) { MostrarError("El id tiene que ser un número."); return; }

            var repositorio = new RepositorioEF<Cliente>(context);
            var cliente = repositorio.ObtenerPorId(id);
            if (cliente == null) { MostrarError($"No existe ningún cliente con id {id}."); return; }

            Console.WriteLine("Deja un campo vacío para no cambiarlo.");
            cliente.Dni = Pedir("DNI", true, ValidarDni, cliente.Dni)!.ToUpperInvariant();
            cliente.Nombre = Pedir("Nombre", true, t => ValidarTexto(t, 60), cliente.Nombre)!;
            cliente.Apellidos = Pedir("Apellidos", true, t => ValidarTexto(t, 100), cliente.Apellidos)!;
            cliente.Email = Pedir("Email", true, ValidarEmail, cliente.Email)!;
            cliente.Telefono = Pedir("Teléfono", false, ValidarTelefono, cliente.Telefono);

            repositorio.Actualizar(cliente);
            context.SaveChanges();
            MostrarExito($"Cliente {cliente.Nombre} {cliente.Apellidos} (id {cliente.Id}) modificado.");
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

        // Guarda a propósito datos incorrectos sin validarlos, para enseñar cómo reacciona la base de datos.
        // Ningún dato se queda guardado: todas las pruebas fallan y se descartan.
        static void ProbarErroresBaseDatos(GenteFitContext context)
        {
            Console.WriteLine("Se intenta guardar cada dato incorrecto directamente en la base de datos:");

            Probar(context, "Cliente sin apellidos (campo obligatorio)", () =>
                context.Clientes.Add(new Cliente { Dni = "00000001R", Nombre = "Prueba", Apellidos = null!, Email = "prueba1@gentefit.test" }));

            Probar(context, "DNI de más de 9 caracteres", () =>
                context.Clientes.Add(new Cliente { Dni = "1234567890123", Nombre = "Prueba", Apellidos = "Larga", Email = "prueba2@gentefit.test" }));

            var existente = context.Clientes.AsNoTracking().FirstOrDefault();
            if (existente != null)
                Probar(context, $"DNI repetido ({existente.Dni})", () =>
                    context.Clientes.Add(new Cliente { Dni = existente.Dni, Nombre = "Prueba", Apellidos = "Repetida", Email = "prueba3@gentefit.test" }));

            Probar(context, "Reserva de una sesión que no existe (id 9999)", () =>
                context.Reservas.Add(new Reserva { IdSesion = 9999, IdCliente = existente?.Id ?? 1, FechaSolicitud = DateTime.Now, Estado = EstadoReserva.CONFIRMADA }));

            Probar(context, "Sala con aforo 0", () =>
                context.Salas.Add(new Sala { Nombre = "Sala de prueba", Aforo = 0 }));

            Probar(context, "Borrar una actividad que tiene sesiones", () =>
                context.Actividades.Remove(context.Actividades.First(a => a.Sesiones.Any())));
        }

        // Ejecuta una prueba: si la base de datos la rechaza, muestra el error traducido
        static void Probar(GenteFitContext context, string descripcion, Action preparar)
        {
            Console.Write($"- {descripcion}: ");
            try
            {
                preparar();
                context.SaveChanges();
                MostrarExito("se ha guardado (no debería).");
            }
            catch (GenteFitException ex)
            {
                MostrarError($"[{ex.Codigo}] {ex.Message}");
            }
            finally
            {
                context.ChangeTracker.Clear();
            }
        }

        // Pide un dato y lo repite hasta que sea válido.
        // Si se pasa el valor actual (al modificar), dejarlo vacío conserva ese valor.
        static string? Pedir(string etiqueta, bool obligatorio, Func<string, string?> validar, string? actual = null)
        {
            string pista = actual is not null ? $" ({actual})" : obligatorio ? " *" : " (opcional)";
            while (true)
            {
                Console.Write($"{etiqueta}{pista}: ");
                string? valor = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(valor))
                {
                    if (actual is not null) return actual;
                    if (!obligatorio) return null;
                    MostrarError($"El campo {etiqueta} es obligatorio.");
                    continue;
                }

                string? error = validar(valor);
                if (error is null) return valor;
                MostrarError(error);
            }
        }

        // Las validaciones devuelven el mensaje de error, o null si el dato es correcto

        static string? ValidarDni(string dni)
        {
            if (!Regex.IsMatch(dni, @"^\d{8}[A-Za-z]$"))
                return "El DNI tiene que tener 8 números y una letra, por ejemplo 12345678Z.";
            char letraCorrecta = "TRWAGMYFPDXBNJZSQVHLCKE"[int.Parse(dni[..8]) % 23];
            if (char.ToUpperInvariant(dni[8]) != letraCorrecta)
                return $"La letra del DNI no es correcta: para {dni[..8]} tiene que ser {letraCorrecta}.";
            return null;
        }

        static string? ValidarTexto(string texto, int maximo)
        {
            if (texto.Length > maximo)
                return $"No puede tener más de {maximo} caracteres.";
            if (!Regex.IsMatch(texto, @"^\p{L}[\p{L} '’\-]*$"))
                return "Introduce solo letras, sin números ni símbolos (se admiten espacios, apóstrofes y guiones).";
            return null;
        }

        static string? ValidarEmail(string email)
        {
            if (email.Length > 150)
                return "El email no puede tener más de 150 caracteres.";
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return "El email tiene que tener el formato nombre@dominio.com.";
            return null;
        }

        static string? ValidarTelefono(string telefono)
        {
            if (telefono.Length > 15 || !Regex.IsMatch(telefono, @"^\+?[0-9 ]{9,}$"))
                return "El teléfono solo puede tener números (y un + delante), entre 9 y 15 caracteres.";
            return null;
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