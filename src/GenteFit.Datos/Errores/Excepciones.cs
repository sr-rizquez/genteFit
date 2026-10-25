namespace GenteFit.Datos;

// Excepción base: todas las de GenteFit llevan un código de error del Producto 1
public class GenteFitException(string codigo, string mensaje, Exception? interna = null)
    : Exception(mensaje, interna)
{
    public string Codigo { get; } = codigo;
}

public class ReservaDuplicadaException(string mensaje, Exception? interna = null)
    : GenteFitException("E-01", mensaje, interna);

public class SesionNoDisponibleException(string mensaje, Exception? interna = null)
    : GenteFitException("E-02", mensaje, interna);

public class ClienteConReservasException(string mensaje, Exception? interna = null)
    : GenteFitException("E-03", mensaje, interna);

public class DatosDuplicadosException(string mensaje, Exception? interna = null)
    : GenteFitException("E-04", mensaje, interna);

public class SolapamientoSesionException(string mensaje, Exception? interna = null)
    : GenteFitException("E-05", mensaje, interna);

public class ActividadConSesionesException(string mensaje, Exception? interna = null)
    : GenteFitException("E-06", mensaje, interna);

// Errores técnicos de la base de datos que no son reglas de negocio
public class BaseDatosException(string codigo, string mensaje, Exception? interna = null)
    : GenteFitException(codigo, mensaje, interna);