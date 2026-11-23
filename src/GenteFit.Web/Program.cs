using GenteFit.Datos;
using GenteFit.Negocio.Servicios;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using GenteFit.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Igual que en el Producto 2: primero la variable de entorno GENTEFIT_BD (Docker, macOS, Linux)
// y, si no existe, la cadena de appsettings.json (LocalDB en Windows)
string cadenaConexion = Environment.GetEnvironmentVariable("GENTEFIT_BD")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddDbContext<GenteFitContext>(options =>
    options.UseSqlServer(cadenaConexion));

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

InicioBaseDatos.Preparar(app.Services, app.Logger);

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


