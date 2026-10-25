// Prototipos de los ORM comparados en el Producto 2: Entity Framework Core, Dapper, LINQ to DB y RepoDB.
// Cada uno hace las mismas operaciones con la tabla cliente de una base de datos de pruebas aparte
// (GenteFitPrototipoOrm), así que no toca los datos de GenteFitDB.
// Los resultados se muestran por consola y se guardan en prototipos/orm/resultados.md.
//
// Ejecutar desde la carpeta del repositorio (SQL Server tiene que estar en marcha):
//   dotnet run --project prototipos/orm

using System.Diagnostics;
using System.Text;
using Dapper;
using GenteFit.Datos;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.SqlServer;
using LinqToDB.Mapping;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RepoDb;

Console.OutputEncoding = Encoding.UTF8;

const int Clientes = 1_000;
const int Busquedas = 500;

// Misma conexión que la aplicación, pero con otra base de datos
var cadena = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("GENTEFIT_BD")
    ?? @"Server=(localdb)\MSSQLLocalDB;Trusted_Connection=True;TrustServerCertificate=True;")
{ InitialCatalog = "GenteFitPrototipoOrm" }.ConnectionString;
Ef.Cadena = ConDapper.Cadena = ConLinqToDb.Cadena = ConRepoDb.Cadena = cadena;

DbContextOptions<GenteFitContext> OpcionesEf() =>
    new DbContextOptionsBuilder<GenteFitContext>().UseSqlServer(cadena).Options;

// EF Core crea la base de datos de pruebas con el mismo modelo que GenteFitDB
using (var contexto = new GenteFitContext(OpcionesEf()))
{
    contexto.Database.EnsureDeleted();
    contexto.Database.EnsureCreated();
}
GlobalConfiguration.Setup().UseSqlServer();

var datos = Enumerable.Range(1, Clientes).Select(i => new ClienteFila
{
    Dni = DniValido(10_000_000 + i),
    Nombre = "Núria",
    Apellidos = $"L'Hereu Ibáñez {i}",
    Email = $"cliente{i}@gentefit.test",
    Telefono = "600000000",
    FechaAlta = DateTime.Today,
    Activo = true,
}).ToList();
var dnisBuscados = datos.Take(Busquedas).Select(c => c.Dni).ToList();

var orms = new (string Nombre, Func<List<ClienteFila>, int> Insertar, Func<int> LeerTodos, Func<List<string>, int> Buscar)[]
{
    ("Entity Framework Core", Ef.Insertar, Ef.LeerTodos, Ef.Buscar),
    ("Dapper", ConDapper.Insertar, ConDapper.LeerTodos, ConDapper.Buscar),
    ("LINQ to DB", ConLinqToDb.Insertar, ConLinqToDb.LeerTodos, ConLinqToDb.Buscar),
    ("RepoDB", ConRepoDb.Insertar, ConRepoDb.LeerTodos, ConRepoDb.Buscar),
};

var md = new StringBuilder();
md.AppendLine("# Resultados de los prototipos ORM");
md.AppendLine();
md.AppendLine($"Ejecutado el {DateTime.Now:dd/MM/yyyy HH:mm} con .NET {Environment.Version}, contra SQL Server 2022.");
md.AppendLine();
md.AppendLine($"Cada ORM inserta {Clientes:N0} clientes en una transacción, los lee todos y busca {Busquedas} por su DNI, uno a uno. Antes de medir se hace una vuelta de calentamiento, porque la primera llamada incluye la compilación JIT y la carga del modelo.");
md.AppendLine();
md.AppendLine("| ORM | Insertar (ms) | Leer todos (ms) | Buscar por DNI (ms) | Clientes leídos | Encontrados | Resultado |");
md.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");

foreach (var orm in orms)
{
    string fila;
    try
    {
        // Calentamiento con 10 clientes
        Vaciar();
        orm.Insertar(datos.Take(10).ToList());
        orm.LeerTodos();
        orm.Buscar(dnisBuscados.Take(10).ToList());
        Vaciar();

        var reloj = Stopwatch.StartNew();
        orm.Insertar(datos);
        long insertar = reloj.ElapsedMilliseconds;

        reloj.Restart();
        int leidos = orm.LeerTodos();
        long leer = reloj.ElapsedMilliseconds;

        reloj.Restart();
        int encontrados = orm.Buscar(dnisBuscados);
        long buscar = reloj.ElapsedMilliseconds;

        bool correcto = leidos == Clientes && encontrados == Busquedas;
        fila = $"| {orm.Nombre} | {insertar} | {leer} | {buscar} | {leidos} | {encontrados} | {(correcto ? "Correcto" : "Resultado incompleto")} |";
    }
    catch (Exception ex)
    {
        fila = $"| {orm.Nombre} | - | - | - | - | - | Error: {ex.GetType().Name}: {ex.Message.ReplaceLineEndings(" ").Split(". ")[0]} |";
    }
    Console.WriteLine(fila);
    md.AppendLine(fila);
}

md.AppendLine();
md.AppendLine("NHibernate no se ha probado: necesita definir el mapeo de cada tabla en ficheros XML o con FluentNHibernate, y no aporta nada que no haga ya Entity Framework Core.");

Vaciar();
using (var contexto = new GenteFitContext(OpcionesEf()))
    contexto.Database.EnsureDeleted();

string carpeta = Path.GetDirectoryName(BuscarHaciaArriba("prototipos/orm/PrototiposOrm.csproj"))!;
File.WriteAllText(Path.Combine(carpeta, "resultados.md"), md.ToString());
Console.WriteLine();
Console.WriteLine($"Resultados guardados en {Path.Combine(carpeta, "resultados.md")}");

void Vaciar()
{
    using var conexion = new SqlConnection(cadena);
    conexion.Execute("DELETE FROM cliente; DBCC CHECKIDENT ('cliente', RESEED, 0);");
}

static string BuscarHaciaArriba(string relativa)
{
    var carpeta = new DirectoryInfo(AppContext.BaseDirectory);
    while (carpeta != null && !File.Exists(Path.Combine(carpeta.FullName, relativa)))
        carpeta = carpeta.Parent;
    return Path.Combine(carpeta?.FullName ?? AppContext.BaseDirectory, relativa);
}

static string DniValido(int numero) => $"{numero:D8}{"TRWAGMYFPDXBNJZSQVHLCKE"[numero % 23]}";

// Fila de la tabla cliente para los ORM que no usan las entidades de GenteFit.Datos.
// LINQ to DB lee los nombres de columna de [Column]; RepoDB, de [Map] cuando no coinciden con la propiedad.
[Table("cliente")]
[RepoDb.Attributes.Map("cliente")]
public class ClienteFila
{
    [Column("id"), LinqToDB.Mapping.PrimaryKey, Identity] public int Id { get; set; }
    [Column("dni")] public string Dni { get; set; } = "";
    [Column("nombre")] public string Nombre { get; set; } = "";
    [Column("apellidos")] public string Apellidos { get; set; } = "";
    [Column("email")] public string Email { get; set; } = "";
    [Column("telefono")] public string? Telefono { get; set; }
    [Column("fecha_alta"), RepoDb.Attributes.Map("fecha_alta")] public DateTime FechaAlta { get; set; }
    [Column("activo")] public bool Activo { get; set; }
}

static class Ef
{
    public static string Cadena = "";
    static GenteFitContext Contexto() => new(new DbContextOptionsBuilder<GenteFitContext>().UseSqlServer(Cadena).Options);

    // Usa las entidades y la configuración reales de GenteFit.Datos
    public static int Insertar(List<ClienteFila> filas)
    {
        using var contexto = Contexto();
        contexto.Clientes.AddRange(filas.Select(f => new Cliente
        {
            Dni = f.Dni, Nombre = f.Nombre, Apellidos = f.Apellidos, Email = f.Email,
            Telefono = f.Telefono, FechaAlta = f.FechaAlta, Activo = f.Activo,
        }));
        return contexto.SaveChanges();
    }

    public static int LeerTodos()
    {
        using var contexto = Contexto();
        return contexto.Clientes.AsNoTracking().ToList().Count;
    }

    public static int Buscar(List<string> dnis)
    {
        using var contexto = Contexto();
        return dnis.Count(dni => contexto.Clientes.AsNoTracking().FirstOrDefault(c => c.Dni == dni) != null);
    }
}

static class ConDapper
{
    public static string Cadena = "";
    const string Columnas = "id AS Id, dni AS Dni, nombre AS Nombre, apellidos AS Apellidos, email AS Email, telefono AS Telefono, fecha_alta AS FechaAlta, activo AS Activo";

    // El SQL se escribe a mano; Dapper solo pasa los parámetros y convierte el resultado en objetos
    public static int Insertar(List<ClienteFila> filas)
    {
        using var conexion = new SqlConnection(Cadena);
        conexion.Open();
        using var transaccion = conexion.BeginTransaction();
        int n = conexion.Execute(
            "INSERT INTO cliente (dni, nombre, apellidos, email, telefono, fecha_alta, activo) " +
            "VALUES (@Dni, @Nombre, @Apellidos, @Email, @Telefono, @FechaAlta, @Activo)", filas, transaccion);
        transaccion.Commit();
        return n;
    }

    public static int LeerTodos()
    {
        using var conexion = new SqlConnection(Cadena);
        return conexion.Query<ClienteFila>($"SELECT {Columnas} FROM cliente").Count();
    }

    public static int Buscar(List<string> dnis)
    {
        using var conexion = new SqlConnection(Cadena);
        return dnis.Count(dni => conexion.QueryFirstOrDefault<ClienteFila>($"SELECT {Columnas} FROM cliente WHERE dni = @dni", new { dni }) != null);
    }
}

static class ConLinqToDb
{
    public static string Cadena = "";
    static DataConnection Conexion() =>
        new(new DataOptions().UseSqlServer(Cadena, SqlServerVersion.v2022, SqlServerProvider.MicrosoftDataSqlClient));

    // Consultas con LINQ que se traducen casi literalmente a SQL, sin seguimiento de cambios
    public static int Insertar(List<ClienteFila> filas)
    {
        using var db = Conexion();
        using var transaccion = db.BeginTransaction();
        int n = filas.Sum(f => db.Insert(f));
        transaccion.Commit();
        return n;
    }

    public static int LeerTodos()
    {
        using var db = Conexion();
        return db.GetTable<ClienteFila>().ToList().Count;
    }

    public static int Buscar(List<string> dnis)
    {
        using var db = Conexion();
        return dnis.Count(dni => db.GetTable<ClienteFila>().FirstOrDefault(c => c.Dni == dni) != null);
    }
}

static class ConRepoDb
{
    public static string Cadena = "";

    // Métodos propios (InsertAll, QueryAll, Query) que generan el SQL a partir de la clase
    public static int Insertar(List<ClienteFila> filas)
    {
        using var conexion = new SqlConnection(Cadena);
        conexion.Open();
        using var transaccion = conexion.BeginTransaction();
        int n = conexion.InsertAll(filas, transaction: transaccion);
        transaccion.Commit();
        return n;
    }

    public static int LeerTodos()
    {
        using var conexion = new SqlConnection(Cadena);
        return conexion.QueryAll<ClienteFila>().Count();
    }

    public static int Buscar(List<string> dnis)
    {
        using var conexion = new SqlConnection(Cadena);
        return dnis.Count(dni => conexion.Query<ClienteFila>(c => c.Dni == dni).FirstOrDefault() != null);
    }
}
