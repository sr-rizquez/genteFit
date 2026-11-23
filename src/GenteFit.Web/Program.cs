using GenteFit.Datos;
using GenteFit.Negocio.Inicializacion;
using GenteFit.Negocio.Servicios;
using GenteFit.Web.Filtros;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using GenteFit.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(opciones =>
    opciones.Filters.Add<BaseDatosNoDisponibleFilter>());

// Igual que en el Producto 2: primero la variable de entorno GENTEFIT_BD (Docker, macOS, Linux)
// y, si no existe, la cadena de appsettings.json (LocalDB en Windows).
// CadenaConexion aplica esa regla y, en Windows ARM, busca sola la dirección actual de LocalDB.
// Se calcula en cada contexto y queda en memoria: si LocalDB se reinicia y la conexión falla,
// BaseDatosNoDisponibleFilter la invalida y la siguiente petición usa la nueva.
string? cadenaConfigurada = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<GenteFitContext>(options =>
    options.UseSqlServer(CadenaConexion.Obtener(cadenaConfigurada)));

builder.Services.AddScoped<ServicioReservas>();
builder.Services.AddScoped<ServicioClientes>();
builder.Services.AddScoped<ServicioActividades>();
builder.Services.AddScoped<ServicioAutenticacion>();
builder.Services.AddScoped<ServicioUsuarios>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";
        options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
        options.Cookie.Name = "GenteFit.Sesion";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Migraciones y administrador inicial (también en una instalación real)
InicioBaseDatos.Preparar(app.Services, app.Logger);

// Datos de prueba (resto de roles, clientes, monitores y horario): solo en desarrollo
if (app.Environment.IsDevelopment())
{
    using var ambito = app.Services.CreateScope();
    try
    {
        var cuentas = InicializadorDatos.CargarDatosPrueba(
            ambito.ServiceProvider.GetRequiredService<GenteFitContext>(),
            Environment.GetEnvironmentVariable("GENTEFIT_ADMIN_PASSWORD"));

        if (cuentas.Count > 0)
            app.Logger.LogInformation(
                "Cuentas de prueba creadas con la contraseña de GENTEFIT_ADMIN_PASSWORD: {Cuentas}",
                string.Join(", ", cuentas));
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "[BD-01] No se han podido cargar los datos de prueba.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();


