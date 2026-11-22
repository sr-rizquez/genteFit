using System.Security.Claims;
using GenteFit.Datos;
using GenteFit.Negocio.Modelos;
using GenteFit.Negocio.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GenteFit.Web.Controllers;

[Authorize(Roles = "CLIENTE,RECEPCIONISTA,ADMINISTRADOR,ENCARGADO")]
public class ReservasController : Controller
{
    private readonly ServicioReservas _servicio;
    private readonly GenteFitContext _context;

    public ReservasController(
        ServicioReservas servicio,
        GenteFitContext context)
    {
        _servicio = servicio;
        _context = context;
    }

    private bool EsGestor =>
        User.IsInRole("ADMINISTRADOR") ||
        User.IsInRole("RECEPCIONISTA");

    private bool EsCliente => User.IsInRole("CLIENTE");

    private int? ObtenerIdClienteAutenticado()
    {
        var identificador = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(identificador, out int idUsuario))
            return null;

        return _context.Usuarios
            .Where(u => u.Id == idUsuario && u.Activo)
            .Select(u => u.IdCliente)
            .FirstOrDefault();
    }

    private int? ResolverIdCliente(int idClienteSolicitado)
    {
        if (EsCliente)
            return ObtenerIdClienteAutenticado();

        if (EsGestor && idClienteSolicitado > 0)
        {
            bool existe = _context.Clientes.Any(c =>
                c.Id == idClienteSolicitado && c.Activo);

            return existe ? idClienteSolicitado : null;
        }

        return null;
    }

    public IActionResult Index(int idCliente = 0)
    {
        var id = ResolverIdCliente(idCliente);

        if (!id.HasValue)
            return EsCliente ? Forbid() : NotFound();

        var reservas = _servicio.VerReservasDeCliente(id.Value);
        ViewBag.IdCliente = id.Value;

        return View(reservas);
    }

    public IActionResult Disponibles(
        int idCliente = 0,
        DateTime? desde = null,
        DateTime? hasta = null)
    {
        var id = ResolverIdCliente(idCliente);

        if (!id.HasValue)
            return EsCliente ? Forbid() : NotFound();

        if (!EsGestor && !EsCliente)
            return Forbid();

        var fechaDesde = desde ?? DateTime.Today;
        var fechaHasta = hasta ?? DateTime.Today.AddDays(7);

        if (fechaHasta < fechaDesde)
        {
            TempData["Error"] = "La fecha final no puede ser anterior a la inicial.";
            return RedirectToAction(nameof(Disponibles), new { idCliente = id.Value });
        }

        var sesiones = _servicio.VerActividadesDisponibles(
            fechaDesde,
            fechaHasta,
            new FiltroSesiones());

        ViewBag.IdCliente = id.Value;
        ViewBag.Desde = fechaDesde;
        ViewBag.Hasta = fechaHasta;

        return View(sesiones);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reservar(int idCliente, int idSesion)
    {
        if (!EsGestor && !EsCliente)
            return Forbid();

        var id = ResolverIdCliente(idCliente);

        if (!id.HasValue)
            return EsCliente ? Forbid() : NotFound();

        try
        {
            var reserva = _servicio.Reservar(id.Value, idSesion);

            TempData["Mensaje"] = reserva.Estado == EstadoReserva.CONFIRMADA
                ? "Reserva confirmada correctamente."
                : "Sesión completa: has quedado en lista de espera.";

            return RedirectToAction(nameof(Index), new { idCliente = id.Value });
        }
        catch (Exception ex) when (
            ex is InvalidOperationException ||
            ex is KeyNotFoundException ||
            ex is ArgumentException)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Disponibles), new { idCliente = id.Value });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancelar(int idReserva, int idCliente = 0)
    {
        if (!EsGestor && !EsCliente)
            return Forbid();

        var id = ResolverIdCliente(idCliente);

        if (!id.HasValue)
            return EsCliente ? Forbid() : NotFound();

        var reservaPerteneceAlCliente = _context.Reservas.Any(r =>
            r.Id == idReserva && r.IdCliente == id.Value);

        if (!reservaPerteneceAlCliente)
            return NotFound();

        try
        {
            var reserva = _servicio.CancelarReserva(idReserva);

            if (reserva == null)
                return NotFound();

            TempData["Mensaje"] = "Reserva cancelada correctamente.";
        }
        catch (Exception ex) when (
            ex is InvalidOperationException ||
            ex is KeyNotFoundException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { idCliente = id.Value });
    }

    public IActionResult Estado(int id)
    {
        var estado = _servicio.VerEstadoSesion(id);

        if (estado == null)
            return NotFound();

        return View(estado);
    }
}
