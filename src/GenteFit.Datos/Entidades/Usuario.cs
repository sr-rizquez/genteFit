namespace GenteFit.Datos;

public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string HashContrasena { get; set; } = null!;
    public Rol Rol { get; set; }
    public bool Activo { get; set; } = true;

    public int? IdCliente { get; set; }
    public Cliente? Cliente { get; set; }
}