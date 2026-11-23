using GenteFit.Datos;
using GenteFit.Negocio.Servicios;
using Microsoft.EntityFrameworkCore;

namespace GenteFit.Negocio.Inicializacion;

/// <summary>
/// Datos de prueba para desarrollo y para el vídeo de demostración. Solo se cargan en el
/// entorno Development: en una instalación real solo existe el administrador que crea
/// InicioBaseDatos (proyecto Web), que además aplica las migraciones.
///
/// Si faltan, crea:
/// 1. Un usuario por rol (encargado, recepcion y laura, cliente) y dos clientes.
/// 2. Los monitores de cada actividad que no tenga ninguno.
/// 3. El horario de las próximas dos semanas, si no hay sesiones futuras.
/// Cada paso solo actúa si falta lo que crea, así que se puede ejecutar en cada arranque.
/// </summary>
public static class InicializadorDatos
{
    /// <summary>
    /// Carga los datos de prueba. Las cuentas usan la misma contraseña que el administrador
    /// inicial (variable GENTEFIT_ADMIN_PASSWORD), así no hay contraseñas en el código.
    /// Si no hay contraseña válida, no se crean cuentas, pero sí el resto de datos.
    /// </summary>
    /// <returns>Los nombres de las cuentas creadas (vacío si no se ha creado ninguna).</returns>
    public static List<string> CargarDatosPrueba(GenteFitContext context, string? contrasena)
    {
        var cuentas = CrearClientesYUsuarios(context, contrasena);
        AsignarMonitores(context);
        ProgramarHorario(context, DateTime.Now);
        return cuentas;
    }

    private static List<string> CrearClientesYUsuarios(GenteFitContext context, string? contrasena)
    {
        // Ya hay cuentas además del administrador, o no hay contraseña con la que crearlas
        if (context.Usuarios.Any(u => u.Rol != Rol.ADMINISTRADOR) ||
            string.IsNullOrWhiteSpace(contrasena) || contrasena.Length < 8)
            return new List<string>();

        var laura = BuscarOCrearCliente(context, "12345678Z", "Laura", "García López",
            "laura@gentefit.local", "600111222");
        BuscarOCrearCliente(context, "87654321X", "Àlex", "D'Amico Pérez",
            "alex@gentefit.local", "600333444");

        // Cada cuenta lleva su propia sal, aunque compartan la contraseña.
        string Hash() => ServicioAutenticacion.CrearHashContrasena(contrasena!);

        var usuarios = new List<Usuario>
        {
            new() { NombreUsuario = "encargado", Email = "encargado@gentefit.local", HashContrasena = Hash(), Rol = Rol.ENCARGADO },
            new() { NombreUsuario = "recepcion", Email = "recepcion@gentefit.local", HashContrasena = Hash(), Rol = Rol.RECEPCIONISTA },
            new() { NombreUsuario = "laura", Email = laura.Email, HashContrasena = Hash(), Rol = Rol.CLIENTE, Cliente = laura }
        };
        context.Usuarios.AddRange(usuarios);
        context.SaveChanges();

        return usuarios.Select(u => u.NombreUsuario).ToList();
    }

    private static Cliente BuscarOCrearCliente(GenteFitContext context, string dni,
        string nombre, string apellidos, string email, string telefono)
    {
        var cliente = context.Clientes.FirstOrDefault(c => c.Dni == dni);
        if (cliente != null)
            return cliente;

        cliente = new Cliente
        {
            Dni = dni,
            Nombre = nombre,
            Apellidos = apellidos,
            Email = email,
            Telefono = telefono,
            FechaAlta = DateTime.Today
        };
        context.Clientes.Add(cliente);
        return cliente;
    }

    private static void AsignarMonitores(GenteFitContext context)
    {
        var actividades = context.Actividades.Include(a => a.Monitores).ToList();
        var monitores = context.Monitores.Where(m => m.Activo).OrderBy(m => m.Id).ToList();

        if (monitores.Count == 0)
            return;

        // Actividades de alta intensidad en bici o combate para el segundo monitor; el resto, para el primero.
        foreach (var actividad in actividades.Where(a => a.Monitores.Count == 0))
        {
            bool segundo = actividad.Tipo is TipoActividad.SPINNING or TipoActividad.BODY_COMBAT;
            var monitor = segundo && monitores.Count > 1 ? monitores[1] : monitores[0];
            actividad.Monitores.Add(monitor);
        }

        context.SaveChanges();
    }

    private static void ProgramarHorario(GenteFitContext context, DateTime ahora)
    {
        bool haySesionesFuturas = context.Sesiones.Any(s =>
            s.Estado == EstadoSesion.PROGRAMADA && s.FechaHoraInicio > ahora);

        if (haySesionesFuturas)
            return;

        var actividades = context.Actividades
            .Include(a => a.Monitores)
            .Where(a => a.Activa)
            .ToList();
        var salas = context.Salas.OrderBy(s => s.Id).ToList();

        if (actividades.Count == 0 || salas.Count == 0)
            return;

        var salaPrincipal = salas[0];
        var salaSpinning = salas.Count > 1 ? salas[1] : salas[0];

        // Horario tipo: (hora, minuto, actividad, sala)
        var horarioLaborable = new (int Hora, int Minuto, TipoActividad Tipo, Sala Sala)[]
        {
            (9, 0, TipoActividad.YOGA, salaPrincipal),
            (18, 0, TipoActividad.BODY_PUMP, salaPrincipal),
            (19, 0, TipoActividad.SPINNING, salaSpinning),
            (19, 30, TipoActividad.CORE, salaPrincipal),
            (20, 15, TipoActividad.BODY_COMBAT, salaPrincipal)
        };
        var horarioSabado = new (int Hora, int Minuto, TipoActividad Tipo, Sala Sala)[]
        {
            (10, 0, TipoActividad.ZUMBA, salaPrincipal),
            (11, 30, TipoActividad.YOGA, salaPrincipal)
        };

        for (int dia = 0; dia < 14; dia++)
        {
            var fecha = ahora.Date.AddDays(dia);
            (int Hora, int Minuto, TipoActividad Tipo, Sala Sala)[] horario = fecha.DayOfWeek switch
            {
                DayOfWeek.Sunday => [],
                DayOfWeek.Saturday => horarioSabado,
                _ => horarioLaborable
            };

            foreach (var (hora, minuto, tipo, sala) in horario)
            {
                var inicio = fecha.AddHours(hora).AddMinutes(minuto);
                var actividad = actividades.FirstOrDefault(a => a.Tipo == tipo);
                var monitor = actividad?.Monitores.FirstOrDefault(m => m.Activo);

                if (inicio <= ahora || actividad == null || monitor == null)
                    continue;

                // El índice único (sala, inicio) no admite dos sesiones a la misma hora en la misma sala.
                bool ocupada = context.Sesiones.Any(s =>
                    s.IdSala == sala.Id && s.FechaHoraInicio == inicio);
                if (ocupada)
                    continue;

                context.Sesiones.Add(new Sesion
                {
                    FechaHoraInicio = inicio,
                    IdActividad = actividad.Id,
                    IdSala = sala.Id,
                    IdMonitor = monitor.Id,
                    Estado = EstadoSesion.PROGRAMADA
                });
            }
        }

        context.SaveChanges();
    }
}
