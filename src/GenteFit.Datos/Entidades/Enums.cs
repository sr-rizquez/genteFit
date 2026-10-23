namespace GenteFit.Datos;

public enum Rol
{
    ADMINISTRADOR,
    ENCARGADO,
    RECEPCIONISTA,
    CLIENTE
}

public enum TipoActividad
{
    BODY_PUMP,
    BODY_COMBAT,
    CORE,
    YOGA,
    SPINNING,
    ZUMBA
}

public enum NivelIntensidad
{
    BAJA,
    MEDIA,
    ALTA
}

public enum EstadoSesion
{
    PROGRAMADA,
    CANCELADA,
    REALIZADA
}

public enum EstadoReserva
{
    CONFIRMADA,
    EN_ESPERA,
    CANCELADA
}

public enum TipoLote
{
    CLIENTES,
    RESERVAS,
    ACTIVIDADES
}