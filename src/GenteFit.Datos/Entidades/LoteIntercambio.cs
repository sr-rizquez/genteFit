namespace GenteFit.Datos;

public class LoteIntercambio
{
    public int Id { get; set; }
    public string Codigo { get; set; } = null!;
    public TipoLote Tipo { get; set; }
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }
    public DateTime FechaGeneracion { get; set; }
    public string RutaFichero { get; set; } = null!;
    public int NumRegistros { get; set; }

    public int IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;
}