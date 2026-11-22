using GenteFit.Datos;
using Microsoft.EntityFrameworkCore;

namespace GenteFit.Negocio.Servicios;

public class ServicioUsuarios
{
    private readonly GenteFitContext _context;

    public ServicioUsuarios(GenteFitContext context)
    {
        _context = context;
    }

    public List<Usuario> Listar()
    {
        return _context.Usuarios
            .Include(u => u.Cliente)
            .OrderBy(u => u.NombreUsuario)
            .ToList();
    }

    public Usuario CrearUsuario(
        string nombreUsuario,
        string email,
        string contrasena,
        Rol rol,
        int? idCliente = null)
    {
        nombreUsuario = nombreUsuario.Trim();
        email = email.Trim().ToLowerInvariant();

        if (nombreUsuario.Length < 3)
            throw new InvalidOperationException(
                "El nombre de usuario debe tener al menos 3 caracteres.");

        if (!email.Contains('@'))
            throw new InvalidOperationException("El email no es válido.");

        if (!Enum.IsDefined(rol))
            throw new InvalidOperationException("El rol seleccionado no es válido.");

        if (_context.Usuarios.Any(u =>
            u.NombreUsuario.ToLower() == nombreUsuario.ToLower()))
            throw new InvalidOperationException(
                "Ya existe ese nombre de usuario.");

        if (_context.Usuarios.Any(u => u.Email.ToLower() == email))
            throw new InvalidOperationException(
                "Ya existe una cuenta con ese email.");

        if (idCliente.HasValue)
        {
            bool clienteExiste = _context.Clientes.Any(c =>
                c.Id == idCliente.Value && c.Activo);

            if (!clienteExiste)
                throw new InvalidOperationException(
                    "El cliente seleccionado no existe o está inactivo.");

            if (_context.Usuarios.Any(u => u.IdCliente == idCliente))
                throw new InvalidOperationException(
                    "Ese cliente ya tiene una cuenta de acceso.");

            if (rol != Rol.CLIENTE)
                throw new InvalidOperationException(
                    "Solo las cuentas de cliente pueden vincularse a un cliente.");
        }

        var usuario = new Usuario
        {
            NombreUsuario = nombreUsuario,
            Email = email,
            HashContrasena =
                ServicioAutenticacion.CrearHashContrasena(contrasena),
            Rol = rol,
            Activo = true,
            IdCliente = idCliente
        };

        _context.Usuarios.Add(usuario);
        _context.SaveChanges();

        return usuario;
    }

    public void CambiarRol(int idUsuario, Rol nuevoRol)
    {
        if (!Enum.IsDefined(nuevoRol))
            throw new InvalidOperationException("El rol seleccionado no es válido.");

        var usuario = _context.Usuarios.Find(idUsuario)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        if (usuario.IdCliente.HasValue && nuevoRol != Rol.CLIENTE)
            throw new InvalidOperationException(
                "Una cuenta vinculada a un cliente debe conservar el rol CLIENTE.");

        usuario.Rol = nuevoRol;
        _context.SaveChanges();
    }

    public void CambiarContrasena(
        int idUsuario, string nuevaContrasena)
    {
        var usuario = _context.Usuarios.Find(idUsuario)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        usuario.HashContrasena =
            ServicioAutenticacion.CrearHashContrasena(nuevaContrasena);

        _context.SaveChanges();
    }

    public void CambiarEstado(int idUsuario, bool activo)
    {
        var usuario = _context.Usuarios.Find(idUsuario)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        if (!activo && usuario.Rol == Rol.ADMINISTRADOR &&
            _context.Usuarios.Count(u =>
                u.Activo && u.Rol == Rol.ADMINISTRADOR) <= 1)
            throw new InvalidOperationException(
                "No se puede desactivar el último administrador activo.");

        usuario.Activo = activo;
        _context.SaveChanges();
    }
}
