using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace GenteFit.Datos;

/// <summary>
/// Decide la cadena de conexión que usa toda la aplicación (web y demo):
/// 1. La variable de entorno GENTEFIT_BD, si existe.
/// 2. La cadena configurada (appsettings.json en la web).
/// 3. LocalDB por defecto.
///
/// En Windows ARM los programas .NET no pueden conectar con "(localdb)\Instancia"
/// (error 56, SQLUserInstance.dll). Por eso, en ese caso, se arranca LocalDB con
/// sqllocaldb y se usa la dirección de su canalización con nombre (np:\\.\pipe\LOCALDB#...).
/// Esa dirección cambia cada vez que LocalDB se reinicia, así que se guarda en memoria
/// y se vuelve a calcular cuando falla la conexión (ver <see cref="Invalidar"/>).
/// </summary>
public static class CadenaConexion
{
    public const string LocalDbPorDefecto =
        @"Server=(localdb)\MSSQLLocalDB;Database=GenteFitDB;Trusted_Connection=True;TrustServerCertificate=True;";

    private const string PrefijoLocalDb = @"(localdb)\";
    private static readonly Regex Canalizacion =
        new(@"np:\\\\\.\\pipe\\LOCALDB#[^\s]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly object Bloqueo = new();
    private static readonly Dictionary<string, string> Resueltas = new();

    public static string Obtener(string? configurada = null)
    {
        string cadena = Environment.GetEnvironmentVariable("GENTEFIT_BD")
                        ?? configurada
                        ?? LocalDbPorDefecto;

        lock (Bloqueo)
        {
            if (!Resueltas.TryGetValue(cadena, out var resuelta))
            {
                resuelta = ResolverLocalDb(cadena);
                Resueltas[cadena] = resuelta;
            }
            return resuelta;
        }
    }

    /// <summary>Olvida las direcciones calculadas; la próxima conexión las vuelve a buscar.</summary>
    public static void Invalidar()
    {
        lock (Bloqueo)
            Resueltas.Clear();
    }

    private static string ResolverLocalDb(string cadena)
    {
        if (!OperatingSystem.IsWindows())
            return cadena;

        SqlConnectionStringBuilder datos;
        try
        {
            datos = new SqlConnectionStringBuilder(cadena);
        }
        catch (ArgumentException)
        {
            return cadena;
        }

        string servidor = datos.DataSource ?? "";
        string instancia;

        if (servidor.StartsWith(PrefijoLocalDb, StringComparison.OrdinalIgnoreCase))
        {
            // En x64 la forma normal funciona: no hace falta tocar nada.
            if (RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
                return cadena;
            instancia = servidor[PrefijoLocalDb.Length..];
        }
        else if (servidor.Contains(@"\pipe\LOCALDB#", StringComparison.OrdinalIgnoreCase))
        {
            // Una dirección de canalización guardada a mano puede haber caducado: se recalcula.
            instancia = "MSSQLLocalDB";
        }
        else
        {
            return cadena;
        }

        string? canalizacion = BuscarCanalizacion(instancia);
        if (canalizacion == null)
            return cadena;

        datos.DataSource = canalizacion;
        return datos.ConnectionString;
    }

    private static string? BuscarCanalizacion(string instancia)
    {
        try
        {
            EjecutarSqlLocalDb("start", instancia);
            var coincidencia = Canalizacion.Match(EjecutarSqlLocalDb("info", instancia));
            return coincidencia.Success ? coincidencia.Value : null;
        }
        catch (Exception)
        {
            // sqllocaldb no está instalado o no responde: se deja la cadena como estaba
            // y el error de conexión se mostrará en la pantalla de base de datos no disponible.
            return null;
        }
    }

    private static string EjecutarSqlLocalDb(string orden, string instancia)
    {
        var inicio = new ProcessStartInfo("sqllocaldb", $"{orden} \"{instancia}\"")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proceso = Process.Start(inicio)
            ?? throw new InvalidOperationException("No se pudo ejecutar sqllocaldb.");
        string salida = proceso.StandardOutput.ReadToEnd();
        proceso.WaitForExit(15000);
        return salida;
    }
}
