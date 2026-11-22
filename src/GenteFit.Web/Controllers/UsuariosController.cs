using GenteFit.Datos;
using GenteFit.Negocio.Servicios;
using GenteFit.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GenteFit.Web.Controllers;

[Authorize(Roles = "ADMINISTRADOR")]
public class UsuariosController : Controller
{
    private readonly ServicioUsuarios _servicio;
    private readonly GenteFitContext _context;

    public UsuariosController(
        ServicioUsuarios servicio,
        GenteFitContext context)
    {
        _servicio = servicio;
        _context = context;
    }

    public IActionResult Index()
    {
        return View(_servicio.Listar());
    }

    [HttpGet]
    public IActionResult Crear()
    {
        CargarClientes();
        return View(new CrearUsuarioViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(CrearUsuarioViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            CargarClientes();
            return View(modelo);
        }

        try
        {
            _servicio.CrearUsuario(
                modelo.NombreUsuario,
                modelo.Email,
                modelo.Contrasena,
                modelo.Rol,
                modelo.IdCliente);

            TempData["Mensaje"] = "Usuario creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (
            ex is InvalidOperationException ||
            ex is KeyNotFoundException)
        {
            ModelState.AddModelError("", ex.Message);
            CargarClientes();
            return View(modelo);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CambiarEstado(int id, bool activo)
    {
        try
        {
            if (User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value == id.ToString()
                && !activo)
            {
                TempData["Error"] = "No puedes desactivar tu propia cuenta.";
                return RedirectToAction(nameof(Index));
            }

            _servicio.CambiarEstado(id, activo);
            TempData["Mensaje"] = "Estado del usuario actualizado.";
        }
        catch (Exception ex) when (
            ex is InvalidOperationException ||
            ex is KeyNotFoundException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CambiarRol(int id, Rol rol)
    {
        try
        {
            _servicio.CambiarRol(id, rol);
            TempData["Mensaje"] = "Rol actualizado correctamente.";
        }
        catch (Exception ex) when (
            ex is InvalidOperationException ||
            ex is KeyNotFoundException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult CambiarContrasena(int id)
    {
        if (!_context.Usuarios.Any(u => u.Id == id))
            return NotFound();

        return View(new CambiarContrasenaViewModel { IdUsuario = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CambiarContrasena(
        CambiarContrasenaViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            _servicio.CambiarContrasena(
                modelo.IdUsuario, modelo.NuevaContrasena);

            TempData["Mensaje"] = "Contraseña actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    private void CargarClientes()
    {
        ViewBag.Clientes = _context.Clientes
            .Where(c => c.Activo)
            .OrderBy(c => c.Apellidos)
            .ThenBy(c => c.Nombre)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Apellidos + ", " + c.Nombre + " (" + c.Dni + ")"
            })
            .ToList();
    }
}
