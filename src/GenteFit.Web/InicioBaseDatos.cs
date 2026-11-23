using System.Security.Cryptography;
using GenteFit.Datos;
using GenteFit.Negocio.Servicios;
using Microsoft.EntityFrameworkCore;

namespace GenteFit.Web;

// Deja la base de datos lista al arrancar, para que la instalación no necesite pasos manuales:
// aplica las migraciones pendientes y, si no hay ningún usuario, crea el administrador inicial.
public static class InicioBaseDatos
{
    public const string UsuarioAdministrador = "admin";

    public static void Preparar(IServiceProvider servicios, ILogger logger)
    {
        using var scope = servicios.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GenteFitContext>();

        try
        {
            context.Database.Migrate();

            if (context.Usuarios.Any())
                return;

            // La contraseña no está en el código: se toma de GENTEFIT_ADMIN_PASSWORD o se genera una al azar
            string? contrasena = Environment.GetEnvironmentVariable("GENTEFIT_ADMIN_PASSWORD");
            if (contrasena != null && contrasena.Length < 8)
            {
                logger.LogWarning("GENTEFIT_ADMIN_PASSWORD tiene menos de 8 caracteres; se genera una contraseña al azar.");
                contrasena = null;
            }
            bool generada = string.IsNullOrWhiteSpace(contrasena);
            if (generada)
                contrasena = Convert.ToBase64String(RandomNumberGenerator.GetBytes(9));

            context.Usuarios.Add(new Usuario
            {
                NombreUsuario = UsuarioAdministrador,
                Email = "admin@gentefit.local",
                HashContrasena = ServicioAutenticacion.CrearHashContrasena(contrasena!),
                Rol = Rol.ADMINISTRADOR,
                Activo = true
            });
            context.SaveChanges();

            if (generada)
                logger.LogWarning(
                    "Administrador inicial creado. Usuario: {Usuario} · Contraseña: {Contrasena} · Cámbiala al entrar.",
                    UsuarioAdministrador, contrasena);
            else
                logger.LogInformation("Administrador inicial creado con la contraseña de GENTEFIT_ADMIN_PASSWORD.");
        }
        catch (Exception ex)
        {
            // BD-01: la web arranca igualmente y cada pantalla mostrará el error al consultar
            logger.LogError(ex, "[BD-01] No se ha podido preparar la base de datos. Revisa que SQL Server esté en marcha.");
        }
    }
}
