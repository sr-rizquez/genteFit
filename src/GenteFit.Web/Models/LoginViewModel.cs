using System.ComponentModel.DataAnnotations;

namespace GenteFit.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Introduce tu usuario o email.")]
    [Display(Name = "Usuario o email")]
    public string Identificador { get; set; } = "";

    [Required(ErrorMessage = "Introduce tu contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = "";

    [Display(Name = "Recordarme")]
    public bool Recordarme { get; set; }
}
