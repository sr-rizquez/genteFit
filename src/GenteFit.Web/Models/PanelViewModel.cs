namespace GenteFit.Web.Models;

/// <summary>Datos del panel del administrador (pantalla A1).</summary>
public class PanelViewModel
{
    public int ClientesActivos { get; set; }
    public int SesionesSemana { get; set; }
    public int ReservasConfirmadas { get; set; }
    public int PersonasEnEspera { get; set; }
    public int OcupacionMedia { get; set; }
    public string? ActividadMasReservada { get; set; }
    public List<PanelSesion> Proximas { get; set; } = new();
}

public record PanelSesion(int Id, string Actividad, string Sala, DateTime Inicio,
    int Confirmadas, int EnEspera, int Aforo)
{
    public int PlazasLibres => Math.Max(0, Aforo - Confirmadas);
}
