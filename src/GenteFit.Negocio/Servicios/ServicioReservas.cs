using GenteFit.Datos;
using GenteFit.Negocio.Modelos;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GenteFit.Negocio.Servicios;

public class ServicioReservas
{
    private readonly GenteFitContext _contexto;

    public ServicioReservas(GenteFitContext contexto)
    {
        _contexto = contexto;
    }

    public List<Sesion> VerActividadesDisponibles(
        DateTime desde, DateTime hasta, FiltroSesiones? filtro = null)
    {
        var consulta = _contexto.Sesiones
            .Include(s => s.Actividad)
            .Include(s => s.Sala)
            .Include(s => s.Monitor)
            .Include(s => s.Reservas)
            .Where(s => s.FechaHoraInicio >= desde &&
                        s.FechaHoraInicio <= hasta &&
                        s.Estado == EstadoSesion.PROGRAMADA);

        if (filtro != null)
        {
            if (!string.IsNullOrWhiteSpace(filtro.Texto))
            {
                var texto = filtro.Texto.Trim();
                consulta = consulta.Where(s =>
                    s.Actividad.Nombre.Contains(texto) ||
                    (s.Actividad.Descripcion ?? "").Contains(texto));
            }

            if (filtro.Tipo.HasValue)
                consulta = consulta.Where(s => s.Actividad.Tipo == filtro.Tipo.Value);

            if (filtro.Intensidad.HasValue)
                consulta = consulta.Where(s => s.Actividad.Intensidad == filtro.Intensidad.Value);

            if (filtro.IdSala.HasValue)
                consulta = consulta.Where(s => s.IdSala == filtro.IdSala.Value);
        }

        return consulta.OrderBy(s => s.FechaHoraInicio).ToList();
    }

    public Reserva Reservar(int idCliente, int idSesion)
    {
        using var transaccion = _contexto.Database.BeginTransaction(
            IsolationLevel.Serializable);

        var cliente = _contexto.Clientes.FirstOrDefault(c => c.Id == idCliente);

        if (cliente == null || !cliente.Activo || cliente.FechaBaja != null)
            throw new InvalidOperationException("El cliente no existe o está dado de baja.");

        var sesion = _contexto.Sesiones
            .Include(s => s.Sala)
            .Include(s => s.Reservas)
            .FirstOrDefault(s => s.Id == idSesion);

        if (sesion == null ||
            sesion.Estado != EstadoSesion.PROGRAMADA ||
            sesion.FechaHoraInicio <= DateTime.Now)
            throw new SesionNoDisponibleException(
                "La sesión no existe, está cancelada o ya ha comenzado.");

        if (sesion.Reservas.Any(r =>
            r.IdCliente == idCliente && r.Estado != EstadoReserva.CANCELADA))
            throw new ReservaDuplicadaException(
                "El cliente ya tiene una reserva para esta sesión.");

        int confirmadas = sesion.Reservas.Count(
            r => r.Estado == EstadoReserva.CONFIRMADA);

        int aforo = Math.Min(Sesion.AFORO_MAXIMO, sesion.Sala.Aforo);

        var reserva = new Reserva
        {
            IdCliente = idCliente,
            IdSesion = idSesion,
            FechaSolicitud = DateTime.Now,
            Estado = confirmadas < aforo
                ? EstadoReserva.CONFIRMADA
                : EstadoReserva.EN_ESPERA
        };

        _contexto.Reservas.Add(reserva);
        _contexto.SaveChanges();
        transaccion.Commit();

        return reserva;
    }

    public List<Reserva> VerReservasDeCliente(int idCliente)
    {
        return _contexto.Reservas
            .Include(r => r.Sesion).ThenInclude(s => s.Actividad)
            .Include(r => r.Sesion).ThenInclude(s => s.Sala)
            .Where(r => r.IdCliente == idCliente &&
                        r.Estado != EstadoReserva.CANCELADA)
            .OrderBy(r => r.Sesion.FechaHoraInicio)
            .ToList();
    }

    public Reserva? CancelarReserva(int idReserva)
    {
        using var transaccion = _contexto.Database.BeginTransaction(
            IsolationLevel.Serializable);

        var reserva = _contexto.Reservas
            .Include(r => r.Sesion)
            .ThenInclude(s => s.Reservas)
            .FirstOrDefault(r => r.Id == idReserva);

        if (reserva == null)
            return null;

        if (reserva.Estado == EstadoReserva.CANCELADA)
        {
            transaccion.Commit();
            return reserva;
        }

        bool estabaConfirmada = reserva.Estado == EstadoReserva.CONFIRMADA;

        reserva.Estado = EstadoReserva.CANCELADA;
        reserva.FechaCancelacion = DateTime.Now;

        if (estabaConfirmada)
        {
            var siguiente = reserva.Sesion.Reservas
                .Where(r => r.Estado == EstadoReserva.EN_ESPERA)
                .OrderBy(r => r.FechaSolicitud)
                .ThenBy(r => r.Id)
                .FirstOrDefault();

            if (siguiente != null)
                siguiente.Estado = EstadoReserva.CONFIRMADA;
        }

        _contexto.SaveChanges();
        transaccion.Commit();

        return reserva;
    }

    public EstadoSesionDto VerEstadoSesion(int idSesion)
    {
        var sesion = _contexto.Sesiones
            .Include(s => s.Sala)
            .Include(s => s.Actividad)
            .Include(s => s.Reservas).ThenInclude(r => r.Cliente)
            .FirstOrDefault(s => s.Id == idSesion);

        if (sesion == null)
            throw new SesionNoDisponibleException("No se ha encontrado la sesión.");

        var confirmadas = sesion.Reservas
            .Where(r => r.Estado == EstadoReserva.CONFIRMADA)
            .OrderBy(r => r.FechaSolicitud)
            .ToList();

        var enEspera = sesion.Reservas
            .Where(r => r.Estado == EstadoReserva.EN_ESPERA)
            .OrderBy(r => r.FechaSolicitud)
            .ThenBy(r => r.Id)
            .ToList();

        int aforo = Math.Min(Sesion.AFORO_MAXIMO, sesion.Sala.Aforo);

        return new EstadoSesionDto
        {
            Sesion = sesion,
            Confirmadas = confirmadas,
            EnEspera = enEspera,
            PlazasLibres = Math.Max(0, aforo - confirmadas.Count)
        };
    }
}

