using GenteFit.Datos;
using GenteFit.Negocio.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenteFit.Web.Controllers;

[Authorize(Roles = "ADMINISTRADOR,RECEPCIONISTA")]
public class ClientesController : Controller
{
    private readonly ServicioClientes _servicio;

    public ClientesController(ServicioClientes servicio)
    {
        _servicio = servicio;
    }

    public IActionResult Index(string? texto, bool soloActivos = true)
    {
        ViewBag.Texto = texto;
        ViewBag.SoloActivos = soloActivos;
        return View(_servicio.Buscar(texto ?? "", soloActivos));
    }

    public IActionResult Details(int id)
    {
        try { return View(_servicio.Obtener(id)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    public IActionResult Create() => View(new Cliente());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Cliente cliente)
    {
        try
        {
            _servicio.CrearCliente(cliente);
            TempData["Mensaje"] = "Cliente creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
        {
            ModelState.AddModelError("", ex.Message);
            return View(cliente);
        }
    }

    public IActionResult Edit(int id)
    {
        try { return View(_servicio.Obtener(id)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Cliente cliente)
    {
        if (id != cliente.Id) return NotFound();

        try
        {
            _servicio.ModificarCliente(cliente);
            TempData["Mensaje"] = "Datos del cliente actualizados.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException || ex is KeyNotFoundException)
        {
            ModelState.AddModelError("", ex.Message);
            return View(cliente);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DarDeBaja(int id, bool cancelarReservas = false)
    {
        try
        {
            _servicio.DarDeBaja(id, cancelarReservas);
            TempData["Mensaje"] = "Cliente dado de baja correctamente.";
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is KeyNotFoundException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
