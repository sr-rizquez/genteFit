using GenteFit.Datos;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace GenteFit.Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            using var context = new GenteFitContext();

            while (true)
            {
                MostrarMenu();
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
                catch (OperationCanceledException)
                {
                    // Se ha escrito 0 en un campo: se vuelve al menú sin guardar nada
                    Console.WriteLine("Operación cancelada.");
                    context.ChangeTracker.Clear();
                }
                catch (Exception ex)
                {
                    // Cualquier otro error inesperado: tampoco cierra el programa
                    MostrarError($"[ERROR] {ex.Message}");
                    context.ChangeTracker.Clear(); // Descartar cambios pendientes
                }

                Console.Write("\nPulsa Intro para volver al menú...");
                Console.ReadLine();
            }
        }

        // Cada campo se valida al escribirlo: si está mal, se explica el error y se vuelve a pedir.
        // El DNI o el email repetidos no se pueden comprobar aquí: los rechaza la base de datos (E-04).
        static void Alta(GenteFitContext context)
        {
            Console.WriteLine("Los campos marcados con * son obligatorios. Escribe 0 para cancelar.");
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

            Console.WriteLine("Deja un campo vacío para no cambiarlo. Escribe 0 para cancelar.");
            cliente.Dni = Pedir("DNI", true, ValidarDni, cliente.Dni)!.ToUpperInvariant();
            cliente.Nombre = Pedir("Nombre", true, t => ValidarTexto(t, 60), cliente.Nombre)!;
            cliente.Apellidos = Pedir("Apellidos", true, t => ValidarTexto(t, 100), cliente.Apellidos)!;
            cliente.Email = Pedir("Email", true, ValidarEmail, cliente.Email)!;
            cliente.Telefono = Pedir("Teléfono", false, ValidarTelefono, cliente.Telefono);

            repositorio.Actualizar(cliente);
            context.SaveChanges();
            MostrarExito($"Cliente {cliente.Nombre} {cliente.Apellidos} (id {cliente.Id}) modificado.");
        }

        // Exporta todos los clientes a xml/salida/clientes.xml, en UTF-8.
        // XDocument escapa solo los caracteres especiales (&, <, >...), así que acentos y apóstrofes se guardan bien.
        static void ExportarXml(GenteFitContext context)
        {
            var repositorio = new RepositorioEF<Cliente>(context);
            var clientes = repositorio.Listar();

            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("clientes",
                    new XAttribute("exportado", DateTime.Now.ToString("s")),
                    new XAttribute("total", clientes.Count),
                    clientes.Select(c => new XElement("cliente",
                        new XAttribute("dni", c.Dni),
                        new XElement("nombre", c.Nombre),
                        new XElement("apellidos", c.Apellidos),
                        new XElement("email", c.Email),
                        c.Telefono is null ? null : new XElement("telefono", c.Telefono),
                        new XElement("fechaAlta", c.FechaAlta.ToString("yyyy-MM-dd")),
                        c.FechaBaja is null ? null : new XElement("fechaBaja", c.FechaBaja.Value.ToString("yyyy-MM-dd")),
                        new XElement("activo", c.Activo)))));

            string carpeta = Path.Combine(CarpetaXml(), "salida");
            string ruta = Path.Combine(carpeta, "clientes.xml");
            try
            {
                Directory.CreateDirectory(carpeta);
                doc.Save(ruta);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new GenteFitException("X-04", $"No se puede escribir el fichero {ruta}: {ex.Message}");
            }
            MostrarExito($"Exportados {clientes.Count} clientes a {ruta}");
        }

        // Importa clientes de un fichero de xml/entrada.
        // Si el fichero no se puede leer (no existe, está mal formado...), se avisa y no se importa nada.
        // Si un cliente tiene datos incorrectos, se descarta indicando su línea y se importan los demás.
        static void ImportarXml(GenteFitContext context)
        {
            string carpeta = Path.Combine(CarpetaXml(), "entrada");
            Console.WriteLine($"Ficheros en {carpeta}:");
            if (Directory.Exists(carpeta))
                foreach (var f in Directory.GetFiles(carpeta, "*.xml").OrderBy(f => f))
                    Console.WriteLine($"  - {Path.GetFileName(f)}");

            Console.Write("Fichero a importar (Intro = clientes.xml): ");
            string fichero = Console.ReadLine()?.Trim() ?? "";
            if (fichero == "") fichero = "clientes.xml";
            if (!fichero.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)) fichero += ".xml";

            XDocument doc = CargarXml(Path.Combine(carpeta, fichero));
            if (doc.Root is null || doc.Root.Name != "clientes")
                throw new GenteFitException("X-03", $"El fichero no tiene el formato esperado: la raíz es <{doc.Root?.Name}> y debería ser <clientes>.");

            var repositorio = new RepositorioEF<Cliente>(context);
            var descartados = new List<string>();
            var dnisDelFichero = new HashSet<string>();
            int total = 0, importados = 0;

            foreach (var x in doc.Root.Elements("cliente"))
            {
                total++;
                int linea = ((IXmlLineInfo)x).LineNumber;
                string? dni = ((string?)x.Attribute("dni"))?.Trim().ToUpperInvariant();
                string? nombre = ((string?)x.Element("nombre"))?.Trim();
                string? apellidos = ((string?)x.Element("apellidos"))?.Trim();
                string? email = ((string?)x.Element("email"))?.Trim();
                string? telefono = ((string?)x.Element("telefono"))?.Trim();

                // Se usan las mismas validaciones que en el alta
                string? error = Comprobar("dni", dni, ValidarDni)
                    ?? Comprobar("nombre", nombre, t => ValidarTexto(t, 60))
                    ?? Comprobar("apellidos", apellidos, t => ValidarTexto(t, 100))
                    ?? Comprobar("email", email, ValidarEmail)
                    ?? Comprobar("telefono", telefono, ValidarTelefono, obligatorio: false);
                if (error is null && !dnisDelFichero.Add(dni!))
                    error = $"el DNI {dni} está repetido en el fichero.";
                if (error is null && repositorio.Listar(c => c.Dni == dni).Count > 0)
                    error = $"ya existe un cliente con el DNI {dni}.";

                if (error is not null)
                {
                    descartados.Add($"Línea {linea}: {error}");
                    continue;
                }

                try
                {
                    repositorio.Anyadir(new Cliente { Dni = dni!, Nombre = nombre!, Apellidos = apellidos!, Email = email!, Telefono = telefono });
                    context.SaveChanges();
                    importados++;
                }
                catch (GenteFitException ex)
                {
                    // Por ejemplo, un email que ya tiene otro cliente (E-04): se descarta solo este cliente
                    context.ChangeTracker.Clear();
                    descartados.Add($"Línea {linea}: [{ex.Codigo}] {ex.Message}");
                }
            }

            MostrarExito($"Importados {importados} de {total} clientes desde {fichero}.");
            foreach (var d in descartados)
                MostrarError($"  Descartado. {d}");
        }

        // Abre el XML. Los errores que impiden leer el fichero entero se convierten en errores de GenteFit.
        static XDocument CargarXml(string ruta)
        {
            try
            {
                // DtdProcessing.Prohibit: protege contra ataques XXE (entidades externas)
                var ajustes = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit };
                using var lector = XmlReader.Create(ruta, ajustes);
                return XDocument.Load(lector, LoadOptions.SetLineInfo);
            }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            {
                throw new GenteFitException("X-01", $"La fuente XML no está disponible: no existe el fichero {Path.GetFileName(ruta)}.");
            }
            catch (UnauthorizedAccessException)
            {
                throw new GenteFitException("X-01", $"La fuente XML no está disponible: no hay permiso para leer {Path.GetFileName(ruta)}.");
            }
            catch (IOException ex)
            {
                throw new GenteFitException("X-01", $"La fuente XML no está disponible: {ex.Message}");
            }
            catch (XmlException ex)
            {
                throw new GenteFitException("X-02", $"El XML está mal formado (línea {ex.LineNumber}, posición {ex.LinePosition}): {ex.Message}");
            }
        }

        // Comprueba un campo leído del XML; devuelve el error o null si es correcto
        static string? Comprobar(string campo, string? valor, Func<string, string?> validar, bool obligatorio = true)
        {
            if (string.IsNullOrEmpty(valor))
                return obligatorio ? $"falta el campo {campo}." : null;
            string? error = validar(valor);
            return error is null ? null : $"{campo} incorrecto. {error}";
        }

        // Busca la carpeta xml del repositorio subiendo desde la carpeta del programa (bin/Debug/net10.0),
        // para que funcione igual con dotnet run, con Visual Studio o desde cualquier carpeta
        static string CarpetaXml()
        {
            var carpeta = new DirectoryInfo(AppContext.BaseDirectory);
            while (carpeta != null && !Directory.Exists(Path.Combine(carpeta.FullName, "xml")))
                carpeta = carpeta.Parent;
            return Path.Combine(carpeta?.FullName ?? AppContext.BaseDirectory, "xml");
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
                if (valor == "0")
                    throw new OperationCanceledException();

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

        // Dibuja el menú: limpia la pantalla, el título en un recuadro y las opciones agrupadas por secciones
        static void MostrarMenu()
        {
            const int ancho = 42;
            string titulo = "GENTEFIT · Gestión de clientes";
            int izquierda = (ancho - titulo.Length) / 2;

            Console.Clear();
            Escribir("╔" + new string('═', ancho) + "╗", ConsoleColor.Cyan);
            Escribir("║" + new string(' ', izquierda) + titulo.PadRight(ancho - izquierda) + "║", ConsoleColor.Cyan);
            Escribir("╚" + new string('═', ancho) + "╝", ConsoleColor.Cyan);
            Console.WriteLine();
            Escribir(" CLIENTES", ConsoleColor.Yellow);
            Console.WriteLine("   1. Alta de cliente");
            Console.WriteLine("   2. Baja de cliente");
            Console.WriteLine("   3. Consulta de clientes");
            Console.WriteLine("   4. Modificación de cliente");
            Console.WriteLine();
            Escribir(" INTERCAMBIO XML", ConsoleColor.Yellow);
            Console.WriteLine("   5. Exportar clientes a XML");
            Console.WriteLine("   6. Importar clientes desde XML");
            Console.WriteLine();
            Escribir(" PRUEBAS", ConsoleColor.Yellow);
            Console.WriteLine("   7. Probar errores de la base de datos");
            Console.WriteLine();
            Console.WriteLine("   0. Salir");
            Console.WriteLine();
            Console.Write("Elige una opción: ");
        }

        // Escribe una línea en el color indicado y vuelve al color normal
        static void Escribir(string texto, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(texto);
            Console.ResetColor();
        }
    }
}