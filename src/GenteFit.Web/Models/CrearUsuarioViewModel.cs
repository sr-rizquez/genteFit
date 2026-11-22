using System.ComponentModel.DataAnnotations;
using GenteFit.Datos;

namespace GenteFit.Web.Models;

public class CrearUsuarioViewModel
{
    [Required, StringLength(50, MinimumLength = 3)]
    [Display(Name = "Nombre de usuario")]
    public string NombreUsuario { get; set; } = "";

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = "";

    [Required, MinLength(8)]
    [DataType(DataType.Password)]
    public string Contrasena { get; set; } = "";

    [Required]
    public Rol Rol { get; set; } = Rol.CLIENTE;

    [Display(Name = "Cliente vinculado")]
    public int? IdCliente { get; set; }
}
