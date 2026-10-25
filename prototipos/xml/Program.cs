// Prototipos de los cinco componentes XML comparados en el Producto 2:
// XmlDocument, LINQ to XML, XmlReader/XmlWriter, XmlSerializer y DataSet.
// Cada componente lee los mismos ficheros de xml/entrada y un fichero grande que se genera con XmlWriter.
// Los resultados se muestran por consola y se guardan en prototipos/xml/resultados.md.
//
// Ejecutar desde la carpeta del repositorio:
//   dotnet run --project prototipos\xml\PrototiposXml.csproj

using System.Data;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

Console.OutputEncoding = Encoding.UTF8;

var lectores = new (string Nombre, Func<string, List<ClienteXml>> Leer)[]
{
    ("XmlDocument", Lectores.ConXmlDocument),
    ("LINQ to XML", Lectores.ConLinqToXml),
    ("XmlReader", Lectores.ConXmlReader),
    ("XmlSerializer", Lectores.ConXmlSerializer),
    ("DataSet", Lectores.ConDataSet),
};

string carpetaXml = Lectores.CarpetaXml();
string entrada = Path.Combine(carpetaXml, "entrada");
string grande = Path.Combine(Path.GetTempPath(), "gentefit_clientes_grande.xml");
Lectores.GenerarFicheroGrande(grande, 50_000);

var pruebas = new (string Titulo, string Ruta)[]
{
    ("Fichero correcto con acentos y apóstrofes", Path.Combine(entrada, "clientes.xml")),
    ("Estructura cambiada", Path.Combine(entrada, "clientes_estructura_cambiada.xml")),
    ("Datos incorrectos (solo lectura, sin validar)", Path.Combine(entrada, "clientes_datos_erroneos.xml")),
    ("XML mal formado", Path.Combine(entrada, "clientes_mal_formado.xml")),
    ("Codificación incorrecta", Path.Combine(entrada, "clientes_codificacion.xml")),
    ("Fichero inexistente", Path.Combine(entrada, "no_existe.xml")),
    ("Fichero grande (50.000 clientes, generado con XmlWriter)", grande),
};

// Calentamiento: la primera llamada a cada componente incluye la compilación JIT y falsearía los tiempos
foreach (var (_, leer) in lectores)
    leer(Path.Combine(entrada, "clientes.xml"));

var md = new StringBuilder();
md.AppendLine("# Resultados de los prototipos XML");
md.AppendLine();
md.AppendLine($"Ejecutado el {DateTime.Now:dd/MM/yyyy HH:mm} con .NET {Environment.Version}.");

foreach (var (titulo, ruta) in pruebas)
{
    Console.WriteLine();
    Console.WriteLine($"== {titulo} ({Path.GetFileName(ruta)})");
    md.AppendLine();
    md.AppendLine($"## {titulo}");
    md.AppendLine();
    md.AppendLine($"Fichero: `{Path.GetFileName(ruta)}`");
    md.AppendLine();
    md.AppendLine("| Componente | Clientes leídos | Sin DNI o email | Primer cliente | Tiempo (ms) | Memoria asignada (MB) | Resultado |");
    md.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");

    foreach (var (nombre, leer) in lectores)
    {
        GC.Collect();
        long memoriaAntes = GC.GetAllocatedBytesForCurrentThread();
        var reloj = Stopwatch.StartNew();
        string fila;
        try
        {
            var clientes = leer(ruta);
            reloj.Stop();
            double mb = (GC.GetAllocatedBytesForCurrentThread() - memoriaAntes) / 1024.0 / 1024.0;
            int incompletos = clientes.Count(c => string.IsNullOrWhiteSpace(c.Dni) || string.IsNullOrWhiteSpace(c.Email));
            string primero = clientes.Count > 0 ? $"{clientes[0].Nombre} {clientes[0].Apellidos}".Trim() : "-";
            fila = $"| {nombre} | {clientes.Count} | {incompletos} | {primero} | {reloj.ElapsedMilliseconds} | {mb:0.0} | Correcto |";
        }
        catch (Exception ex)
        {
            reloj.Stop();
            fila = $"| {nombre} | - | - | - | {reloj.ElapsedMilliseconds} | - | Error: {Lectores.DescribirError(ex)} |";
        }
        Console.WriteLine(fila);
        md.AppendLine(fila);
    }
}

string rutaResultados = Path.Combine(Directory.GetParent(carpetaXml)!.FullName, "prototipos", "xml", "resultados.md");
File.WriteAllText(rutaResultados, md.ToString(), new UTF8Encoding(false));
Console.WriteLine();
Console.WriteLine($"Resultados guardados en {rutaResultados}");

// Datos de un cliente tal como los lee cada prototipo (sin validar)
public class ClienteXml
{
    public string? Dni { get; set; }
    public string? Nombre { get; set; }
    public string? Apellidos { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
}

// Clases para XmlSerializer: la estructura del XML se describe con atributos
[XmlRoot("clientes")]
public class ClientesSer
{
    [XmlElement("cliente")]
    public List<ClienteSer> Clientes { get; set; } = new();
}

public class ClienteSer
{
    [XmlAttribute("dni")] public string? Dni { get; set; }
    [XmlElement("nombre")] public string? Nombre { get; set; }
    [XmlElement("apellidos")] public string? Apellidos { get; set; }
    [XmlElement("email")] public string? Email { get; set; }
    [XmlElement("telefono")] public string? Telefono { get; set; }
}

public static class Lectores
{
    // 1. XmlDocument: carga el árbol DOM completo y lo recorre con XPath
    public static List<ClienteXml> ConXmlDocument(string ruta)
    {
        var doc = new XmlDocument();
        doc.Load(ruta);
        var lista = new List<ClienteXml>();
        foreach (XmlElement n in doc.SelectNodes("/clientes/cliente")!)
        {
            lista.Add(new ClienteXml
            {
                Dni = n.HasAttribute("dni") ? n.GetAttribute("dni") : null,
                Nombre = n["nombre"]?.InnerText,
                Apellidos = n["apellidos"]?.InnerText,
                Email = n["email"]?.InnerText,
                Telefono = n["telefono"]?.InnerText,
            });
        }
        return lista;
    }

    // 2. LINQ to XML: carga el árbol en memoria y se consulta con LINQ
    public static List<ClienteXml> ConLinqToXml(string ruta)
    {
        var doc = XDocument.Load(ruta);
        return doc.Root!.Elements("cliente").Select(x => new ClienteXml
        {
            Dni = (string?)x.Attribute("dni"),
            Nombre = (string?)x.Element("nombre"),
            Apellidos = (string?)x.Element("apellidos"),
            Email = (string?)x.Element("email"),
            Telefono = (string?)x.Element("telefono"),
        }).ToList();
    }

    // 3. XmlReader: lectura secuencial, nodo a nodo, sin cargar el fichero en memoria
    public static List<ClienteXml> ConXmlReader(string ruta)
    {
        var ajustes = new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true, DtdProcessing = DtdProcessing.Prohibit };
        var lista = new List<ClienteXml>();
        using var r = XmlReader.Create(ruta, ajustes);
        while (r.Read())
        {
            if (r.NodeType != XmlNodeType.Element || r.Name != "cliente" || r.Depth != 1)
                continue;

            var c = new ClienteXml { Dni = r.GetAttribute("dni") };
            using var sub = r.ReadSubtree();
            sub.Read(); // <cliente>
            sub.Read(); // primer hijo
            while (!sub.EOF)
            {
                if (sub.NodeType == XmlNodeType.Element && sub.Depth == 1)
                {
                    // ReadElementContentAsString ya avanza al siguiente nodo; Skip salta lo que no conocemos
                    switch (sub.Name)
                    {
                        case "nombre": c.Nombre = sub.ReadElementContentAsString(); break;
                        case "apellidos": c.Apellidos = sub.ReadElementContentAsString(); break;
                        case "email": c.Email = sub.ReadElementContentAsString(); break;
                        case "telefono": c.Telefono = sub.ReadElementContentAsString(); break;
                        default: sub.Skip(); break;
                    }
                }
                else
                {
                    sub.Read();
                }
            }
            lista.Add(c);
        }
        return lista;
    }

    // 4. XmlSerializer: convierte el XML directamente en objetos a partir de las clases anotadas
    public static List<ClienteXml> ConXmlSerializer(string ruta)
    {
        var serializador = new XmlSerializer(typeof(ClientesSer));
        using var lector = XmlReader.Create(ruta, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
        var datos = (ClientesSer)serializador.Deserialize(lector)!;
        return datos.Clientes.Select(c => new ClienteXml
        {
            Dni = c.Dni, Nombre = c.Nombre, Apellidos = c.Apellidos, Email = c.Email, Telefono = c.Telefono,
        }).ToList();
    }

    // 5. DataSet (ADO.NET): carga el XML como tablas, infiriendo su estructura
    public static List<ClienteXml> ConDataSet(string ruta)
    {
        var ds = new DataSet();
        ds.ReadXml(ruta);
        var tabla = ds.Tables["cliente"];
        if (tabla is null)
            return new List<ClienteXml>();

        string? Valor(DataRow fila, string columna) =>
            tabla.Columns.Contains(columna) && fila[columna] is string s ? s : null;

        return tabla.Rows.Cast<DataRow>().Select(f => new ClienteXml
        {
            Dni = Valor(f, "dni"),
            Nombre = Valor(f, "nombre"),
            Apellidos = Valor(f, "apellidos"),
            Email = Valor(f, "email"),
            Telefono = Valor(f, "telefono"),
        }).ToList();
    }

    // Genera un fichero grande con XmlWriter (escritura secuencial) para comparar rendimiento y memoria
    public static void GenerarFicheroGrande(string ruta, int numClientes)
    {
        var ajustes = new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) };
        using var w = XmlWriter.Create(ruta, ajustes);
        w.WriteStartDocument();
        w.WriteStartElement("clientes");
        for (int i = 0; i < numClientes; i++)
        {
            int numero = 10_000_000 + i;
            char letra = "TRWAGMYFPDXBNJZSQVHLCKE"[numero % 23];
            w.WriteStartElement("cliente");
            w.WriteAttributeString("dni", $"{numero:D8}{letra}");
            w.WriteElementString("nombre", "Núria");
            w.WriteElementString("apellidos", "L'Hereu Ibáñez");
            w.WriteElementString("email", $"cliente{i}@gentefit.test");
            w.WriteElementString("telefono", "600 000 000");
            w.WriteEndElement();
        }
        w.WriteEndElement();
        w.WriteEndDocument();
    }

    // Resume un error en una línea: tipo de excepción (y la interna si la hay) y mensaje
    public static string DescribirError(Exception ex)
    {
        Exception e = ex;
        while (e is not XmlException && e.InnerException is not null)
            e = e.InnerException;
        string mensaje = e.Message.Replace("|", "/").Replace("\r", " ").Replace("\n", " ");
        if (mensaje.Length > 110)
            mensaje = mensaje[..110] + "...";
        return ReferenceEquals(ex, e) ? $"{e.GetType().Name}: {mensaje}" : $"{ex.GetType().Name} → {e.GetType().Name}: {mensaje}";
    }

    // Busca la carpeta xml del repositorio (la que tiene xml/entrada) subiendo desde la carpeta del programa
    public static string CarpetaXml()
    {
        var carpeta = new DirectoryInfo(AppContext.BaseDirectory);
        while (carpeta != null && !Directory.Exists(Path.Combine(carpeta.FullName, "xml", "entrada")))
            carpeta = carpeta.Parent;
        return Path.Combine(carpeta?.FullName ?? AppContext.BaseDirectory, "xml");
    }
}
