namespace GenteFit.Datos;

public class Reserva
{
    public int Id { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public EstadoReserva Estado { get; set; }
    public DateTime? FechaCancelacion { get; set; }

    public int IdSesion { get; set; }
    public Sesion Sesion { get; set; } = null!;
    public int IdCliente { get; set; }
    public Cliente Cliente { get; set; } = null!;
}