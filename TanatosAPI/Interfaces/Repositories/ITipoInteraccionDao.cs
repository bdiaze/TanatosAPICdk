using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface ITipoInteraccionDao {
		public Task<TipoInteraccion?> Obtener(long id, NpgsqlTransaction? transaction = null);
		public Task<List<TipoInteraccion>> ObtenerTodos(NpgsqlTransaction? transaction = null);
		public Task Insertar(TipoInteraccion item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(TipoInteraccion item, NpgsqlTransaction? transaction = null);
		public Task Eliminar(long id, NpgsqlTransaction? transaction = null);
	}
}
