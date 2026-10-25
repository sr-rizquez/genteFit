namespace GenteFit.Datos;

public class Cliente
{
    public int Id { get; set; }
    public string Dni { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Telefono { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public DateTime FechaAlta { get; set; } = DateTime.Today;
    public DateTime? FechaBaja { get; set; }
    public bool Activo { get; set; } = true;

    public Usuario? Usuario { get; set; }
    public ICollection<Reserva> Reservas { get; } = new List<Reserva>();
}