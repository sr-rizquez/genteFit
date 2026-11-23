using GenteFit.Datos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.SqlClient;

namespace GenteFit.Web.Filtros;

/// <summary>
/// Si una acción falla porque SQL Server no responde, muestra la pantalla
/// "Base de datos no disponible" (503) dentro del diseño de la aplicación,
/// en lugar de la página técnica de error. Los demás errores siguen su curso.
/// </summary>
public class BaseDatosNoDisponibleFilter : IExceptionFilter
{
    // Números de error de SQL Server / SqlClient que indican que no hay conexión:
    // -2 tiempo agotado, 2/53/40 servidor no encontrado, 233/10054 conexión cortada,
    // 4060 base de datos inexistente, 1225/10061 conexión rechazada, 11001 host desconocido.
    private static readonly HashSet<int> ErroresDeConexion =
        new() { -2, -1, 2, 40, 53, 233, 1225, 4060, 10053, 10054, 10060, 10061, 11001 };

    private readonly IModelMetadataProvider _metadatos;
    private readonly ILogger<BaseDatosNoDisponibleFilter> _registro;

    public BaseDatosNoDisponibleFilter(
        IModelMetadataProvider metadatos,
        ILogger<BaseDatosNoDisponibleFilter> registro)
    {
        _metadatos = metadatos;
        _registro = registro;
    }

    public void OnException(ExceptionContext context)
    {
        if (!EsFaltaDeConexion(context.Exception))
            return;

        _registro.LogError(context.Exception, "Base de datos no disponible.");

        // La dirección de LocalDB puede haber cambiado: la siguiente petición la recalcula.
        CadenaConexion.Invalidar();

        var peticion = context.HttpContext.Request;
        var vista = new ViewResult
        {
            ViewName = "SinConexion",
            StatusCode = StatusCodes.Status503ServiceUnavailable,
            ViewData = new ViewDataDictionary(_metadatos, context.ModelState)
            {
                // Reintentar repite la misma página si era una consulta; si era un envío
                // de formulario, vuelve al inicio para no reenviar datos.
                ["Reintentar"] = HttpMethods.IsGet(peticion.Method)
                    ? peticion.Path + peticion.QueryString
                    : "/"
            }
        };

        context.Result = vista;
        context.ExceptionHandled = true;
    }

    public static bool EsFaltaDeConexion(Exception? ex)
    {
        for (var actual = ex; actual != null; actual = actual.InnerException)
        {
            if (actual is BaseDatosException { Codigo: "BD-01" })
                return true;
            if (actual is SqlException sql &&
                (ErroresDeConexion.Contains(sql.Number) || sql.Class >= 20))
                return true;
        }
        return false;
    }
}
