using GenteFit.Datos;
using GenteFit.Negocio.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GenteFit.Web.Controllers;

[Authorize(Roles = "ADMINISTRADOR,ENCARGADO")]
public class ActividadesController : Controller
{
    private readonly GenteFitContext _context;
    private readonly ServicioActividades _servicio;

    public ActividadesController(
        GenteFitContext context,
        ServicioActividades servicio)
    {
        _context = context;
        _servicio = servicio;
    }

    public async Task<IActionResult> Index()
    {
        var actividades = await _context.Actividades
            .Include(a => a.Monitores)
            .OrderBy(a => a.Nombre)
            .ToListAsync();

        return View(actividades);
    }

    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        ViewBag.Monitores = await _context.Monitores
            .Where(m => m.Activo)
            .OrderBy(m => m.Apellidos)
            .ToListAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(
        Actividad actividad,
        int[] idsMonitores)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Monitores = await _context.Monitores
                .Where(m => m.Activo)
                .OrderBy(m => m.Apellidos)
                .ToListAsync();

            return View(actividad);
        }

        try
        {
            _servicio.CrearActividad(actividad, idsMonitores);
            TempData["Mensaje"] = "Actividad creada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Monitores = await _context.Monitores
                .Where(m => m.Activo)
                .OrderBy(m => m.Apellidos)
                .ToListAsync();

            return View(actividad);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var actividad = await _context.Actividades
            .Include(a => a.Monitores)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (actividad == null)
            return NotFound();

        ViewBag.Monitores = await _context.Monitores
            .Where(m => m.Activo)
            .OrderBy(m => m.Apellidos)
            .ToListAsync();

        ViewBag.MonitoresSeleccionados =
            actividad.Monitores.Select(m => m.Id).ToArray();

        return View(actividad);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(
        int id,
        Actividad actividad,
        int[] idsMonitores)
    {
        if (id != actividad.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            ViewBag.Monitores = await _context.Monitores
                .Where(m => m.Activo)
                .OrderBy(m => m.Apellidos)
                .ToListAsync();

            ViewBag.MonitoresSeleccionados = idsMonitores;
            return View(actividad);
        }

        try
        {
            _servicio.ModificarActividad(actividad, idsMonitores);
            TempData["Mensaje"] = "Actividad modificada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Monitores = await _context.Monitores
                .Where(m => m.Activo)
                .OrderBy(m => m.Apellidos)
                .ToListAsync();

            ViewBag.MonitoresSeleccionados = idsMonitores;
            return View(actividad);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id)
    {
        try
        {
            _servicio.EliminarActividad(id);
            TempData["Mensaje"] = "Operación realizada correctamente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}

