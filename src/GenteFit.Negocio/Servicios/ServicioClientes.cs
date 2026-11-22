using GenteFit.Datos;
using Microsoft.EntityFrameworkCore;

namespace GenteFit.Negocio.Servicios;

public class ServicioClientes
{
    private readonly GenteFitContext _context;

    public ServicioClientes(GenteFitContext context)
    {
        _context = context;
    }

    public Cliente CrearCliente(Cliente cliente, bool crearCuenta = false)
    {
        if (cliente == null)
            throw new ArgumentNullException(nameof(cliente));

        Normalizar(cliente);
        ValidarDatosObligatorios(cliente);
        ComprobarDuplicados(cliente);

        cliente.FechaAlta = cliente.FechaAlta == default
            ? DateTime.Today
            : cliente.FechaAlta;

        cliente.FechaBaja = null;
        cliente.Activo = true;

        // La creación de credenciales se realizará desde ServicioUsuarios
        // cuando se integre el inicio de sesión.
        _ = crearCuenta;

        _context.Clientes.Add(cliente);
        _context.SaveChanges();

        return cliente;
    }

    public void ModificarCliente(Cliente cliente)
    {
        if (cliente == null)
            throw new ArgumentNullException(nameof(cliente));

        var existente = _context.Clientes
            .FirstOrDefault(c => c.Id == cliente.Id);

        if (existente == null)
            throw new KeyNotFoundException(
                $"No existe el cliente con identificador {cliente.Id}.");

        Normalizar(cliente);
        ValidarDatosObligatorios(cliente);
        ComprobarDuplicados(cliente, cliente.Id);

        existente.Dni = cliente.Dni;
        existente.Nombre = cliente.Nombre;
        existente.Apellidos = cliente.Apellidos;
        existente.Email = cliente.Email;
        existente.Telefono = cliente.Telefono;
        existente.FechaNacimiento = cliente.FechaNacimiento;

        _context.SaveChanges();
    }

    public void DarDeBaja(int idCliente, bool cancelarReservas)
    {
        using var transaccion = _context.Database.BeginTransaction(
            System.Data.IsolationLevel.Serializable);

        var cliente = _context.Clientes
            .FirstOrDefault(c => c.Id == idCliente);

        if (cliente == null)
            throw new KeyNotFoundException(
                $"No existe el cliente con identificador {idCliente}.");

        if (!cliente.Activo)
        {
            transaccion.Commit();
            return;
        }

        var reservasActivas = _context.Reservas
            .Where(r =>
                r.IdCliente == idCliente &&
                (r.Estado == EstadoReserva.CONFIRMADA ||
                 r.Estado == EstadoReserva.EN_ESPERA))
            .ToList();

        if (reservasActivas.Count > 0 && !cancelarReservas)
        {
            throw new InvalidOperationException(
                "No se puede dar de baja al cliente porque tiene reservas activas o en lista de espera.");
        }

        // Contabilizamos las plazas confirmadas que se van a liberar.
        var plazasLiberadas = reservasActivas
            .Where(r => r.Estado == EstadoReserva.CONFIRMADA)
            .GroupBy(r => r.IdSesion)
            .ToDictionary(g => g.Key, g => g.Count());

        var ahora = DateTime.Now;

        foreach (var reserva in reservasActivas)
        {
            reserva.Estado = EstadoReserva.CANCELADA;
            reserva.FechaCancelacion = ahora;
        }

        cliente.Activo = false;
        cliente.FechaBaja = ahora;

        // Guardamos las cancelaciones antes de consultar las listas de espera.
        _context.SaveChanges();

        // Promocionamos por orden de solicitud tantas reservas como plazas
        // confirmadas se hayan liberado en cada sesión.
        foreach (var grupo in plazasLiberadas)
        {
            var siguientes = _context.Reservas
                .Where(r =>
                    r.IdSesion == grupo.Key &&
                    r.Estado == EstadoReserva.EN_ESPERA)
                .OrderBy(r => r.FechaSolicitud)
                .ThenBy(r => r.Id)
                .Take(grupo.Value)
                .ToList();

            foreach (var reserva in siguientes)
                reserva.Estado = EstadoReserva.CONFIRMADA;
        }

        _context.SaveChanges();
        transaccion.Commit();
    }

    public List<Cliente> Buscar(string texto, bool soloActivos)
    {
        var consulta = _context.Clientes.AsQueryable();

        if (soloActivos)
            consulta = consulta.Where(c => c.Activo);

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var termino = texto.Trim().ToLower();

            consulta = consulta.Where(c =>
                c.Nombre.ToLower().Contains(termino) ||
                c.Apellidos.ToLower().Contains(termino) ||
                c.Dni.ToLower().Contains(termino) ||
                c.Email.ToLower().Contains(termino));
        }

        return consulta
            .OrderBy(c => c.Apellidos)
            .ThenBy(c => c.Nombre)
            .ToList();
    }

    public Cliente Obtener(int idCliente)
    {
        return _context.Clientes
            .FirstOrDefault(c => c.Id == idCliente)
            ?? throw new KeyNotFoundException(
                $"No existe el cliente con identificador {idCliente}.");
    }

    private void ComprobarDuplicados(Cliente cliente, int? idExcluir = null)
    {
        var dniDuplicado = _context.Clientes.Any(c =>
            c.Dni == cliente.Dni &&
            (!idExcluir.HasValue || c.Id != idExcluir.Value));

        if (dniDuplicado)
            throw new InvalidOperationException(
                $"Ya existe un cliente con el DNI {cliente.Dni}.");

        var emailNormalizado = cliente.Email.ToLower();

        var emailDuplicado = _context.Clientes.Any(c =>
            c.Email.ToLower() == emailNormalizado &&
            (!idExcluir.HasValue || c.Id != idExcluir.Value));

        if (emailDuplicado)
            throw new InvalidOperationException(
                $"Ya existe un cliente con el correo {cliente.Email}.");
    }

    private static void Normalizar(Cliente cliente)
    {
        cliente.Dni = cliente.Dni?.Trim().ToUpperInvariant() ?? "";
        cliente.Nombre = cliente.Nombre?.Trim() ?? "";
        cliente.Apellidos = cliente.Apellidos?.Trim() ?? "";
        cliente.Email = cliente.Email?.Trim() ?? "";
        cliente.Telefono = string.IsNullOrWhiteSpace(cliente.Telefono)
            ? null
            : cliente.Telefono.Trim();
    }

    private static void ValidarDatosObligatorios(Cliente cliente)
    {
        if (string.IsNullOrWhiteSpace(cliente.Dni))
            throw new ArgumentException("El DNI es obligatorio.");

        if (string.IsNullOrWhiteSpace(cliente.Nombre))
            throw new ArgumentException("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(cliente.Apellidos))
            throw new ArgumentException("Los apellidos son obligatorios.");

        if (string.IsNullOrWhiteSpace(cliente.Email) ||
            !cliente.Email.Contains('@'))
            throw new ArgumentException("El correo electrónico no es válido.");
    }
}

