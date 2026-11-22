using System.Security.Cryptography;
using System.Text;
using GenteFit.Datos;

namespace GenteFit.Negocio.Servicios;

public class ServicioAutenticacion
{
    private const int Iteraciones = 210000;
    private const int LongitudHash = 32;

    private readonly GenteFitContext _context;

    public ServicioAutenticacion(GenteFitContext context)
    {
        _context = context;
    }

    public Usuario? IniciarSesion(string identificador, string contrasena)
    {
        if (string.IsNullOrWhiteSpace(identificador) ||
            string.IsNullOrEmpty(contrasena))
            return null;

        string dato = identificador.Trim().ToLowerInvariant();

        var usuario = _context.Usuarios.FirstOrDefault(u =>
            u.NombreUsuario.ToLower() == dato ||
            u.Email.ToLower() == dato);

        if (usuario == null || !usuario.Activo)
            return null;

        return VerificarContrasena(contrasena, usuario.HashContrasena)
            ? usuario
            : null;
    }

    public static string CrearHashContrasena(string contrasena)
    {
        if (string.IsNullOrWhiteSpace(contrasena) || contrasena.Length < 8)
            throw new ArgumentException("La contraseña debe tener al menos 8 caracteres.");

        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(contrasena),
            salt,
            Iteraciones,
            HashAlgorithmName.SHA256,
            LongitudHash);

        return $"v1${Iteraciones}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    private static bool VerificarContrasena(string contrasena, string hashGuardado)
    {
        try
        {
            string[] partes = hashGuardado.Split('$');

            if (partes.Length != 4 || partes[0] != "v1")
                return false;

            int iteraciones = int.Parse(partes[1]);
            if (iteraciones < 100000 || iteraciones > 1000000)
                return false;

            byte[] salt = Convert.FromBase64String(partes[2]);
            byte[] hashOriginal = Convert.FromBase64String(partes[3]);

            byte[] hashCalculado = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(contrasena),
                salt,
                iteraciones,
                HashAlgorithmName.SHA256,
                hashOriginal.Length);

            return CryptographicOperations.FixedTimeEquals(
                hashOriginal, hashCalculado);
        }
        catch
        {
            return false;
        }
    }
}
