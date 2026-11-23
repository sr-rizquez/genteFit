using GenteFit.Datos;

namespace GenteFit.Web.Helpers;

/// <summary>Clase CSS del punto de color de cada actividad (componente ActividadChip de Figma).</summary>
public static class EstiloActividad
{
    public static string Clase(Actividad? actividad) =>
        actividad == null
            ? "gf-act"
            : "gf-act gf-act--" + actividad.Tipo.ToString().ToLowerInvariant().Replace('_', '-');
}
