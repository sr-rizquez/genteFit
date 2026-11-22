using System.ComponentModel.DataAnnotations;

namespace GenteFit.Web.Models;

public class CambiarContrasenaViewModel
{
    public int IdUsuario { get; set; }

    [Required, MinLength(8)]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string NuevaContrasena { get; set; } = "";
}
