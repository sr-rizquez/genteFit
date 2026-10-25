namespace GenteFit.Datos;

public class Sesion
{
    public const int AFORO_MAXIMO = 16;

    public int Id { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public EstadoSesion Estado { get; set; } = EstadoSesion.PROGRAMADA;

    public int IdActividad { get; set; }
    public Actividad Actividad { get; set; } = null!;
    public int IdSala { get; set; }
    public Sala Sala { get; set; } = null!;
    public int IdMonitor { get; set; }
    public Monitor Monitor { get; set; } = null!;

    public ICollection<Reserva> Reservas { get; } = new List<Reserva>();
}