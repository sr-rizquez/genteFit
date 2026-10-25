namespace GenteFit.Datos;

public class Actividad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public TipoActividad Tipo { get; set; }
    public string? Descripcion { get; set; }
    public NivelIntensidad Intensidad { get; set; }
    public int DuracionMinutos { get; set; }
    public bool Activa { get; set; } = true;

    public ICollection<Monitor> Monitores { get; } = new List<Monitor>();
    public ICollection<Sesion> Sesiones { get; } = new List<Sesion>();
}