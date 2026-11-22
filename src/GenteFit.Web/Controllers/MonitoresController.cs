using GenteFit.Datos;
using Monitor = GenteFit.Datos.Monitor;
using GenteFit.Negocio.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GenteFit.Web.Controllers;

[Authorize(Roles = "ADMINISTRADOR,ENCARGADO")]
public class MonitoresController : Controller
{
    private readonly GenteFitContext _context;
    private readonly ServicioActividades _servicio;

    public MonitoresController(
        GenteFitContext context,
        ServicioActividades servicio)
    {
        _context = context;
        _servicio = servicio;
    }

    public async Task<IActionResult> Index()
    {
        var monitores = await _context.Monitores
            .Include(m => m.Actividades)
            .OrderBy(m => m.Apellidos)
            .ThenBy(m => m.Nombre)
            .ToListAsync();

        return View(monitores);
    }

    [HttpGet]
    public IActionResult Crear()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(Monitor monitor)
    {
        if (!ModelState.IsValid)
            return View(monitor);

        try
        {
            _servicio.GuardarMonitor(monitor);
            TempData["Mensaje"] = "Monitor creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(monitor);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var monitor = await _context.Monitores
            .FirstOrDefaultAsync(m => m.Id == id);

        if (monitor == null)
            return NotFound();

        return View(monitor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(int id, Monitor monitor)
    {
        if (id != monitor.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(monitor);

        try
        {
            _servicio.GuardarMonitor(monitor);
            TempData["Mensaje"] = "Monitor modificado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(monitor);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DarDeBaja(int id)
    {
        try
        {
            _servicio.DarDeBajaMonitor(id);
            TempData["Mensaje"] = "Monitor dado de baja correctamente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}


