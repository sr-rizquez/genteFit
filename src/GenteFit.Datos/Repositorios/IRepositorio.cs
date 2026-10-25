using System.Linq.Expressions;

namespace GenteFit.Datos;

// Operaciones comunes a todas las entidades (diagrama de acceso a datos del P1)
public interface IRepositorio<T> where T : class
{
    T? ObtenerPorId(int id);
    List<T> Listar(Expression<Func<T, bool>>? filtro = null);
    void Anyadir(T entidad);
    void Actualizar(T entidad);
    void Eliminar(T entidad);
}