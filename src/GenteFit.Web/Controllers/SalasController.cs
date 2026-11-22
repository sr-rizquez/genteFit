using GenteFit.Datos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GenteFit.Web.Controllers;

[Authorize(Roles = "ADMINISTRADOR,ENCARGADO")]
public class SalasController : Controller
{
    private readonly GenteFitContext _context;

    public SalasController(GenteFitContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var salas = await _context.Salas
            .OrderBy(s => s.Nombre)
            .ToListAsync();

        return View(salas);
    }

    [HttpGet]
    public IActionResult Crear()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Sala sala)
    {
        if (!ModelState.IsValid)
            return View(sala);

        sala.Nombre = sala.Nombre.Trim();

        if (await _context.Salas.AnyAsync(s => s.Nombre == sala.Nombre))
        {
            ModelState.AddModelError(
                nameof(sala.Nombre),
                "Ya existe una sala con ese nombre.");

            return View(sala);
        }

        if (sala.Aforo <= 0)
        {
            ModelState.AddModelError(
                nameof(sala.Aforo),
                "El aforo debe ser superior a cero.");

            return View(sala);
        }

        _context.Salas.Add(sala);
        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "Sala creada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var sala = await _context.Salas.FindAsync(id);

        if (sala == null)
            return NotFound();

        return View(sala);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, Sala sala)
    {
        if (id != sala.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(sala);

        sala.Nombre = sala.Nombre.Trim();

        if (await _context.Salas.AnyAsync(
            s => s.Nombre == sala.Nombre && s.Id != sala.Id))
        {
            ModelState.AddModelError(
                nameof(sala.Nombre),
                "Ya existe otra sala con ese nombre.");

            return View(sala);
        }

        if (sala.Aforo <= 0)
        {
            ModelState.AddModelError(
                nameof(sala.Aforo),
                "El aforo debe ser superior a cero.");

            return View(sala);
        }

        var salaExistente = await _context.Salas.FindAsync(id);

        if (salaExistente == null)
            return NotFound();

        salaExistente.Nombre = sala.Nombre;
        salaExistente.Aforo = sala.Aforo;

        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "Sala modificada correctamente.";
        return RedirectToAction(nameof(Index));
    }
}

