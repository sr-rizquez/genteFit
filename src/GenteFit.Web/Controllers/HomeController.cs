using GenteFit.Datos;
using GenteFit.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GenteFit.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly GenteFitContext _context;

    public HomeController(GenteFitContext context)
    {
        _context = context;
    }

    // Cada rol entra en su pantalla principal, como en el mapa de navegación del Producto 1:
    // cliente → C1 horario, recepcionista → R1 clientes, encargado → E1 actividades,
    // administrador → A1 panel.
    public IActionResult Index()
    {
        if (User.IsInRole(nameof(Rol.CLIENTE)))
            return RedirectToAction("Disponibles", "Reservas");
        if (User.IsInRole(nameof(Rol.RECEPCIONISTA)))
            return RedirectToAction("Index", "Clientes");
        if (User.IsInRole(nameof(Rol.ENCARGADO)))
            return RedirectToAction("Index", "Actividades");

        return View(CrearPanel(DateTime.Now));
    }

    private PanelViewModel CrearPanel(DateTime ahora)
    {
        var finSemana = ahora.Date.AddDays(7);

        var proximas = _context.Sesiones
            .AsNoTracking()
            .Include(s => s.Actividad)
            .Include(s => s.Sala)
            .Include(s => s.Reservas)
            .Where(s => s.Estado == EstadoSesion.PROGRAMADA &&
                        s.FechaHoraInicio > ahora &&
                        s.FechaHoraInicio < finSemana)
            .OrderBy(s => s.FechaHoraInicio)
            .ToList();

        var filas = proximas.Select(s =>
        {
            int aforo = Math.Min(Sesion.AFORO_MAXIMO, s.Sala.Aforo);
            int confirmadas = s.Reservas.Count(r => r.Estado == EstadoReserva.CONFIRMADA);
            int espera = s.Reservas.Count(r => r.Estado == EstadoReserva.EN_ESPERA);
            return new PanelSesion(s.Id, s.Actividad.Nombre, s.Sala.Nombre,
                s.FechaHoraInicio, confirmadas, espera, aforo);
        }).ToList();

        int plazasTotales = filas.Sum(f => f.Aforo);

        return new PanelViewModel
        {
            ClientesActivos = _context.Clientes.Count(c => c.Activo),
            SesionesSemana = filas.Count,
            ReservasConfirmadas = filas.Sum(f => f.Confirmadas),
            PersonasEnEspera = filas.Sum(f => f.EnEspera),
            OcupacionMedia = plazasTotales == 0
                ? 0
                : (int)Math.Round(100.0 * filas.Sum(f => f.Confirmadas) / plazasTotales),
            ActividadMasReservada = filas
                .GroupBy(f => f.Actividad)
                .Select(g => new { g.Key, Total = g.Sum(f => f.Confirmadas + f.EnEspera) })
                .Where(g => g.Total > 0)
                .OrderByDescending(g => g.Total)
                .Select(g => g.Key)
                .FirstOrDefault(),
            Proximas = filas.Take(8).ToList()
        };
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [AllowAnonymous]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
