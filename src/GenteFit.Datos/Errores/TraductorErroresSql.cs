using Microsoft.Data.SqlClient;

namespace GenteFit.Datos;

// Convierte los errores técnicos de SQL Server en errores de GenteFit con mensaje claro
public static class TraductorErroresSql
{
    public static GenteFitException Traducir(SqlException sql, Exception original)
    {
        string m = sql.Message;
        bool Contiene(string texto) => m.Contains(texto, StringComparison.OrdinalIgnoreCase);

        return sql.Number switch
        {
            2601 or 2627 when Contiene("ix_sesion")
                => new SolapamientoSesionException("La sala ya tiene otra sesión a esa hora.", original),
            2601 or 2627 when Contiene("ix_reserva")
                => new ReservaDuplicadaException("El cliente ya tiene una reserva en esa sesión.", original),
            2601 or 2627
                => new DatosDuplicadosException("Ya existe un registro con ese DNI, email o nombre.", original),
            547 when Contiene("DELETE") && Contiene("actividad")
                => new ActividadConSesionesException("No se puede eliminar una actividad con sesiones.", original),
            547 when Contiene("DELETE")
                => new BaseDatosException("BD-02", "No se puede borrar: otros registros dependen de él.", original),
            547 when Contiene("CHECK")
                => new BaseDatosException("BD-03", "Algún valor no cumple las reglas (por ejemplo, un aforo de 0).", original),
            547
                => new BaseDatosException("BD-04", "El registro hace referencia a otro que no existe.", original),
            _
                => new BaseDatosException("BD-01", $"La base de datos no está disponible (error SQL {sql.Number}).", original)
        };
    }
}