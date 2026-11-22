using GenteFit.Datos;

namespace GenteFit.Negocio.Modelos;

public class EstadoSesionDto
{
    public Sesion Sesion { get; set; } = null!;
    public List<Reserva> Confirmadas { get; set; } = new();
    public List<Reserva> EnEspera { get; set; } = new();
    public int PlazasLibres { get; set; }
}
