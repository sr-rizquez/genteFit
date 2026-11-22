using GenteFit.Datos;
using Monitor = GenteFit.Datos.Monitor;
using Microsoft.EntityFrameworkCore;

namespace GenteFit.Negocio.Servicios;

public class ServicioActividades
{
    private readonly GenteFitContext _context;

    public ServicioActividades(GenteFitContext context)
    {
        _context = context;
    }

    public Actividad CrearActividad(Actividad actividad, IEnumerable<int> idsMonitores)
    {
        ValidarActividad(actividad);

        var monitores = ObtenerMonitoresValidos(idsMonitores);

        var nueva = new Actividad
        {
            Nombre = actividad.Nombre.Trim(),
            Tipo = actividad.Tipo,
            Descripcion = actividad.Descripcion?.Trim(),
            Intensidad = actividad.Intensidad,
            DuracionMinutos = actividad.DuracionMinutos,
            Activa = true
        };

        foreach (var monitor in monitores)
            nueva.Monitores.Add(monitor);

        _context.Actividades.Add(nueva);
        _context.SaveChanges();

        return nueva;
    }

    public void ModificarActividad(Actividad actividad, IEnumerable<int> idsMonitores)
    {
        if (actividad == null)
            throw new ArgumentNullException(nameof(actividad));

        var existente = _context.Actividades
            .Include(a => a.Monitores)
            .FirstOrDefault(a => a.Id == actividad.Id);

        if (existente == null)
            throw new KeyNotFoundException(
                $"No existe la actividad {actividad.Id}.");

        ValidarActividad(actividad);
        var monitores = ObtenerMonitoresValidos(idsMonitores);

        existente.Nombre = actividad.Nombre.Trim();
        existente.Tipo = actividad.Tipo;
        existente.Descripcion = actividad.Descripcion?.Trim();
        existente.Intensidad = actividad.Intensidad;
        existente.DuracionMinutos = actividad.DuracionMinutos;

        existente.Monitores.Clear();

        foreach (var monitor in monitores)
            existente.Monitores.Add(monitor);

        _context.SaveChanges();
    }

    public void EliminarActividad(int idActividad)
    {
        var actividad = _context.Actividades
            .Include(a => a.Sesiones)
            .FirstOrDefault(a => a.Id == idActividad);

        if (actividad == null)
            throw new KeyNotFoundException(
                $"No existe la actividad {idActividad}.");

        var ahora = DateTime.Now;

        bool tieneSesionesFuturas = actividad.Sesiones.Any(s =>
            s.FechaHoraInicio > ahora &&
            s.Estado == EstadoSesion.PROGRAMADA);

        if (tieneSesionesFuturas)
        {
            throw new InvalidOperationException(
                "No se puede eliminar una actividad que tiene sesiones futuras programadas.");
        }

        if (actividad.Sesiones.Any())
        {
            // Conservamos el histórico y realizamos una baja lógica.
            actividad.Activa = false;
        }
        else
        {
            _context.Actividades.Remove(actividad);
        }

        _context.SaveChanges();
    }

    public List<Sesion> ProgramarSesion(
        int idActividad,
        int idSala,
        int idMonitor,
        DateTime inicio,
        DateTime? repetirHasta = null)
    {
        var actividad = _context.Actividades
            .Include(a => a.Monitores)
            .FirstOrDefault(a => a.Id == idActividad);

        if (actividad == null || !actividad.Activa)
            throw new InvalidOperationException(
                "La actividad no existe o está inactiva.");

        var sala = _context.Salas
            .FirstOrDefault(s => s.Id == idSala);

        if (sala == null)
            throw new KeyNotFoundException(
                $"No existe la sala {idSala}.");

        if (sala.Aforo <= 0)
            throw new InvalidOperationException(
                "El aforo de la sala debe ser superior a cero.");

        var monitor = _context.Monitores
            .FirstOrDefault(m => m.Id == idMonitor);

        if (monitor == null || !monitor.Activo)
            throw new InvalidOperationException(
                "El monitor no existe o está inactivo.");

        if (!actividad.Monitores.Any(m => m.Id == idMonitor))
            throw new InvalidOperationException(
                "El monitor seleccionado no está habilitado para impartir esta actividad.");

        if (inicio <= DateTime.Now)
            throw new InvalidOperationException(
                "No se pueden programar sesiones en el pasado.");

        if (repetirHasta.HasValue && repetirHasta.Value.Date < inicio.Date)
            throw new ArgumentException(
                "La fecha final de repetición no puede ser anterior al inicio.");

        if (actividad.DuracionMinutos <= 0)
            throw new InvalidOperationException(
                "La duración de la actividad debe ser superior a cero.");

        // Si se indica una fecha final, se genera una sesión cada siete días.
        var fechas = new List<DateTime> { inicio };

        if (repetirHasta.HasValue)
        {
            var siguiente = inicio.AddDays(7);

            while (siguiente.Date <= repetirHasta.Value.Date)
            {
                fechas.Add(siguiente);
                siguiente = siguiente.AddDays(7);
            }
        }

        var sesionesExistentes = _context.Sesiones
            .Include(s => s.Actividad)
            .Where(s => s.Estado == EstadoSesion.PROGRAMADA)
            .AsNoTracking()
            .ToList();

        var nuevasSesiones = new List<Sesion>();

        foreach (var fecha in fechas)
        {
            var fin = fecha.AddMinutes(actividad.DuracionMinutos);

            bool solapamientoExistente = sesionesExistentes.Any(s =>
            {
                var finExistente = s.FechaHoraInicio.AddMinutes(
                    s.Actividad.DuracionMinutos);

                bool seSolapa = s.FechaHoraInicio < fin &&
                                finExistente > fecha;

                bool mismaSala = s.IdSala == idSala;
                bool mismoMonitor = s.IdMonitor == idMonitor;

                return seSolapa && (mismaSala || mismoMonitor);
            });

            bool solapamientoNuevo = nuevasSesiones.Any(s =>
            {
                var finNueva = s.FechaHoraInicio.AddMinutes(
                    actividad.DuracionMinutos);

                bool seSolapa = s.FechaHoraInicio < fin &&
                                finNueva > fecha;

                return seSolapa &&
                    (s.IdSala == idSala || s.IdMonitor == idMonitor);
            });

            if (solapamientoExistente || solapamientoNuevo)
            {
                throw new InvalidOperationException(
                    $"La sesión del {fecha:dd/MM/yyyy HH:mm} se solapa con otra sesión de la sala o del monitor.");
            }

            nuevasSesiones.Add(new Sesion
            {
                FechaHoraInicio = fecha,
                Estado = EstadoSesion.PROGRAMADA,
                IdActividad = idActividad,
                IdSala = idSala,
                IdMonitor = idMonitor
            });
        }

        _context.Sesiones.AddRange(nuevasSesiones);
        _context.SaveChanges();

        return nuevasSesiones;
    }

    public void CancelarSesion(int idSesion)
    {
        var sesion = _context.Sesiones
            .Include(s => s.Reservas)
            .FirstOrDefault(s => s.Id == idSesion);

        if (sesion == null)
            throw new KeyNotFoundException(
                $"No existe la sesión {idSesion}.");

        if (sesion.Estado == EstadoSesion.CANCELADA)
            return;

        if (sesion.Estado == EstadoSesion.REALIZADA)
            throw new InvalidOperationException(
                "No se puede cancelar una sesión que ya se ha realizado.");

        sesion.Estado = EstadoSesion.CANCELADA;

        var ahora = DateTime.Now;

        foreach (var reserva in sesion.Reservas.Where(r =>
            r.Estado == EstadoReserva.CONFIRMADA ||
            r.Estado == EstadoReserva.EN_ESPERA))
        {
            reserva.Estado = EstadoReserva.CANCELADA;
            reserva.FechaCancelacion = ahora;
        }

        _context.SaveChanges();
    }

    public Monitor GuardarMonitor(Monitor monitor)
    {
        if (monitor == null)
            throw new ArgumentNullException(nameof(monitor));

        monitor.Nombre = monitor.Nombre?.Trim() ?? "";
        monitor.Apellidos = monitor.Apellidos?.Trim() ?? "";
        monitor.Email = monitor.Email?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(monitor.Nombre) ||
            string.IsNullOrWhiteSpace(monitor.Apellidos))
            throw new ArgumentException(
                "El nombre y los apellidos del monitor son obligatorios.");

        if (string.IsNullOrWhiteSpace(monitor.Email) ||
            !monitor.Email.Contains('@'))
            throw new ArgumentException(
                "El correo electrónico del monitor no es válido.");

        var email = monitor.Email.ToLower();

        bool duplicado = _context.Monitores.Any(m =>
            m.Email.ToLower() == email &&
            m.Id != monitor.Id);

        if (duplicado)
            throw new InvalidOperationException(
                "Ya existe un monitor con ese correo electrónico.");

        if (monitor.Id == 0)
        {
            monitor.Activo = true;
            _context.Monitores.Add(monitor);
        }
        else
        {
            var existente = _context.Monitores
                .FirstOrDefault(m => m.Id == monitor.Id);

            if (existente == null)
                throw new KeyNotFoundException(
                    $"No existe el monitor {monitor.Id}.");

            existente.Nombre = monitor.Nombre;
            existente.Apellidos = monitor.Apellidos;
            existente.Email = monitor.Email;
            existente.Activo = monitor.Activo;

            monitor = existente;
        }

        _context.SaveChanges();
        return monitor;
    }

    public void DarDeBajaMonitor(int idMonitor)
    {
        var monitor = _context.Monitores
            .FirstOrDefault(m => m.Id == idMonitor);

        if (monitor == null)
            throw new KeyNotFoundException(
                $"No existe el monitor {idMonitor}.");

        bool tieneSesionesFuturas = _context.Sesiones.Any(s =>
            s.IdMonitor == idMonitor &&
            s.Estado == EstadoSesion.PROGRAMADA &&
            s.FechaHoraInicio > DateTime.Now);

        if (tieneSesionesFuturas)
        {
            throw new InvalidOperationException(
                "No se puede dar de baja al monitor mientras tenga sesiones futuras programadas.");
        }

        monitor.Activo = false;
        _context.SaveChanges();
    }

    public Sala GuardarSala(Sala sala)
    {
        if (sala == null)
            throw new ArgumentNullException(nameof(sala));

        sala.Nombre = sala.Nombre?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(sala.Nombre))
            throw new ArgumentException(
                "El nombre de la sala es obligatorio.");

        if (sala.Aforo <= 0)
            throw new ArgumentException(
                "El aforo de la sala debe ser superior a cero.");

        bool nombreDuplicado = _context.Salas.Any(s =>
            s.Nombre.ToLower() == sala.Nombre.ToLower() &&
            s.Id != sala.Id);

        if (nombreDuplicado)
            throw new InvalidOperationException(
                "Ya existe una sala con ese nombre.");

        if (sala.Id == 0)
        {
            _context.Salas.Add(sala);
        }
        else
        {
            var existente = _context.Salas
                .FirstOrDefault(s => s.Id == sala.Id);

            if (existente == null)
                throw new KeyNotFoundException(
                    $"No existe la sala {sala.Id}.");

            existente.Nombre = sala.Nombre;
            existente.Aforo = sala.Aforo;
            sala = existente;
        }

        _context.SaveChanges();
        return sala;
    }

    private List<Monitor> ObtenerMonitoresValidos(IEnumerable<int> idsMonitores)
    {
        var ids = idsMonitores?.Distinct().ToList()
            ?? throw new ArgumentNullException(nameof(idsMonitores));

        if (ids.Count == 0)
            throw new InvalidOperationException(
                "La actividad debe tener al menos un monitor asignado.");

        var monitores = _context.Monitores
            .Where(m => ids.Contains(m.Id) && m.Activo)
            .ToList();

        if (monitores.Count != ids.Count)
            throw new InvalidOperationException(
                "Uno o varios monitores no existen o están inactivos.");

        return monitores;
    }

    private static void ValidarActividad(Actividad actividad)
    {
        if (actividad == null)
            throw new ArgumentNullException(nameof(actividad));

        if (string.IsNullOrWhiteSpace(actividad.Nombre))
            throw new ArgumentException(
                "El nombre de la actividad es obligatorio.");

        if (actividad.DuracionMinutos <= 0)
            throw new ArgumentException(
                "La duración debe ser superior a cero.");
    }
}

