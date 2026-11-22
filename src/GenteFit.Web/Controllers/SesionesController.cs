using GenteFit.Datos;
using GenteFit.Negocio.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GenteFit.Web.Controllers;

[Authorize(Roles = "ADMINISTRADOR,ENCARGADO,RECEPCIONISTA")]
public class SesionesController : Controller
{
    private readonly GenteFitContext _context;
    private readonly ServicioActividades _servicio;

    public SesionesController(
        GenteFitContext context,
        ServicioActividades servicio)
    {
        _context = context;
        _servicio = servicio;
    }

    public async Task<IActionResult> Index(
        int? idActividad,
        DateTime? desde,
        DateTime? hasta)
    {
        var consulta = _context.Sesiones
            .Include(s => s.Actividad)
            .Include(s => s.Sala)
            .Include(s => s.Monitor)
            .Include(s => s.Reservas)
            .AsQueryable();

        if (idActividad.HasValue)
            consulta = consulta.Where(s => s.IdActividad == idActividad.Value);

        if (desde.HasValue)
            consulta = consulta.Where(s => s.FechaHoraInicio >= desde.Value);

        if (hasta.HasValue)
            consulta = consulta.Where(s => s.FechaHoraInicio < hasta.Value.Date.AddDays(1));

        var sesiones = await consulta
            .OrderBy(s => s.FechaHoraInicio)
            .ToListAsync();

        ViewBag.Actividades = await _context.Actividades
            .OrderBy(a => a.Nombre)
            .ToListAsync();

        ViewBag.IdActividad = idActividad;
        ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
        ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");

        return View(sesiones);
    }

    [Authorize(Roles = "ADMINISTRADOR,ENCARGADO")]

    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        await CargarListas();
        return View();
    }

    [Authorize(Roles = "ADMINISTRADOR,ENCARGADO")]

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(
        int idActividad,
        int idSala,
        int idMonitor,
        DateTime inicio,
        DateTime? repetirHasta)
    {
        try
        {
            if (inicio == default)
                throw new InvalidOperationException(
                    "Debes indicar la fecha y hora de inicio.");

            var sesiones = _servicio.ProgramarSesion(
                idActividad,
                idSala,
                idMonitor,
                inicio,
                repetirHasta);

            TempData["Mensaje"] = sesiones.Count == 1
                ? "Sesión programada correctamente."
                : $"Se han programado {sesiones.Count} sesiones.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (
            ex is InvalidOperationException ||
            ex is KeyNotFoundException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await CargarListas();
            return View();
        }
    }

    [Authorize(Roles = "ADMINISTRADOR,ENCARGADO")]

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancelar(int id)
    {
        try
        {
            _servicio.CancelarSesion(id);
            TempData["Mensaje"] = "Sesión cancelada correctamente.";
        }
        catch (Exception ex) when (
            ex is InvalidOperationException ||
            ex is KeyNotFoundException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarListas()
    {
        ViewBag.Actividades = await _context.Actividades
            .Where(a => a.Activa)
            .OrderBy(a => a.Nombre)
            .ToListAsync();

        ViewBag.Salas = await _context.Salas
            .OrderBy(s => s.Nombre)
            .ToListAsync();

        ViewBag.Monitores = await _context.Monitores
            .Where(m => m.Activo)
            .OrderBy(m => m.Apellidos)
            .ThenBy(m => m.Nombre)
            .ToListAsync();
    }
}

