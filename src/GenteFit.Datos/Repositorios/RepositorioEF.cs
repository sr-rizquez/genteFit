using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GenteFit.Datos;

public class RepositorioEF<T>(GenteFitContext contexto) : IRepositorio<T> where T : class
{
    protected readonly GenteFitContext Contexto = contexto;
    protected DbSet<T> Tabla => Contexto.Set<T>();

    public T? ObtenerPorId(int id) => Ejecutar(() => Tabla.Find(id));

    public List<T> Listar(Expression<Func<T, bool>>? filtro = null) =>
        Ejecutar(() => filtro is null ? Tabla.ToList() : Tabla.Where(filtro).ToList());

    // Estas tres solo apuntan el cambio; se guarda al llamar a Contexto.SaveChanges()
    public void Anyadir(T entidad) => Tabla.Add(entidad);
    public void Actualizar(T entidad) => Tabla.Update(entidad);
    public void Eliminar(T entidad) => Tabla.Remove(entidad);

    // Si SQL Server está apagado, el error salta al consultar: también lo traducimos
    protected static TResultado Ejecutar<TResultado>(Func<TResultado> accion)
    {
        try
        {
            return accion();
        }
        catch (SqlException ex)
        {
            throw TraductorErroresSql.Traducir(ex, ex);
        }
    }
}