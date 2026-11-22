using GenteFit.Datos;

namespace GenteFit.Negocio.Modelos;

public class FiltroSesiones
{
    public string? Texto { get; set; }
    public TipoActividad? Tipo { get; set; }
    public NivelIntensidad? Intensidad { get; set; }
    public int? IdSala { get; set; }
}
