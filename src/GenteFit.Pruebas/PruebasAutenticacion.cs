using GenteFit.Datos;
using GenteFit.Negocio.Servicios;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GenteFit.Pruebas;

public class PruebasAutenticacion
{
    private static GenteFitContext CrearContexto()
    {
        var opciones = new DbContextOptionsBuilder<GenteFitContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new GenteFitContext(opciones);
    }

    [Fact]
    public void CrearHashContrasena_GeneraHashValido()
    {
        var hash = ServicioAutenticacion.CrearHashContrasena("GenteFit123");

        var partes = hash.Split('$');

        Assert.Equal(4, partes.Length);
        Assert.Equal("v1", partes[0]);
        Assert.Equal("210000", partes[1]);
        Assert.NotEmpty(Convert.FromBase64String(partes[2]));
        Assert.Equal(32, Convert.FromBase64String(partes[3]).Length);
    }

    [Fact]
    public void CrearHashContrasena_GeneraHashesDiferentesParaMismaContrasena()
    {
        var hash1 = ServicioAutenticacion.CrearHashContrasena("GenteFit123");
        var hash2 = ServicioAutenticacion.CrearHashContrasena("GenteFit123");

        Assert.NotEqual(hash1, hash2);
    }

    [Theory]
    [InlineData("")]
    [InlineData("     ")]
    [InlineData("corta")]
    public void CrearHashContrasena_RechazaContrasenasInvalidas(string contrasena)
    {
        Assert.Throws<ArgumentException>(
            () => ServicioAutenticacion.CrearHashContrasena(contrasena));
    }

    [Fact]
    public void IniciarSesion_AceptaUsuarioYContrasenaCorrectos()
    {
        using var context = CrearContexto();

        context.Usuarios.Add(new Usuario
        {
            NombreUsuario = "robert",
            Email = "robert@gentefit.test",
            HashContrasena = ServicioAutenticacion.CrearHashContrasena("GenteFit123"),
            Rol = Rol.ADMINISTRADOR,
            Activo = true
        });

        context.SaveChanges();

        var servicio = new ServicioAutenticacion(context);

        var resultado = servicio.IniciarSesion("robert", "GenteFit123");

        Assert.NotNull(resultado);
        Assert.Equal("robert", resultado.NombreUsuario);
    }

    [Fact]
    public void IniciarSesion_AceptaEmailComoIdentificador()
    {
        using var context = CrearContexto();

        context.Usuarios.Add(new Usuario
        {
            NombreUsuario = "robert",
            Email = "robert@gentefit.test",
            HashContrasena = ServicioAutenticacion.CrearHashContrasena("GenteFit123"),
            Rol = Rol.ADMINISTRADOR,
            Activo = true
        });

        context.SaveChanges();

        var servicio = new ServicioAutenticacion(context);

        var resultado = servicio.IniciarSesion("ROBERT@GENTEFIT.TEST", "GenteFit123");

        Assert.NotNull(resultado);
    }

    [Fact]
    public void IniciarSesion_RechazaContrasenaIncorrecta()
    {
        using var context = CrearContexto();

        context.Usuarios.Add(new Usuario
        {
            NombreUsuario = "robert",
            Email = "robert@gentefit.test",
            HashContrasena = ServicioAutenticacion.CrearHashContrasena("GenteFit123"),
            Rol = Rol.ADMINISTRADOR,
            Activo = true
        });

        context.SaveChanges();

        var servicio = new ServicioAutenticacion(context);

        Assert.Null(servicio.IniciarSesion("robert", "Incorrecta123"));
    }

    [Fact]
    public void IniciarSesion_RechazaUsuarioDesactivado()
    {
        using var context = CrearContexto();

        context.Usuarios.Add(new Usuario
        {
            NombreUsuario = "robert",
            Email = "robert@gentefit.test",
            HashContrasena = ServicioAutenticacion.CrearHashContrasena("GenteFit123"),
            Rol = Rol.ADMINISTRADOR,
            Activo = false
        });

        context.SaveChanges();

        var servicio = new ServicioAutenticacion(context);

        Assert.Null(servicio.IniciarSesion("robert", "GenteFit123"));
    }
}
