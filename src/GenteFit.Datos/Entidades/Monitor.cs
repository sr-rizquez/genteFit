namespace GenteFit.Datos;

public class Monitor
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool Activo { get; set; } = true;

    public ICollection<Actividad> Actividades { get; } = new List<Actividad>();
}