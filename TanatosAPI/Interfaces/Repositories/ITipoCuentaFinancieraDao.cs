using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface ITipoCuentaFinancieraDao {
		public Task<TipoCuentaFinanciera?> Obtener(long id, NpgsqlTransaction? transaction = null);
		public Task<List<TipoCuentaFinanciera>> ObtenerTodos(NpgsqlTransaction? transaction = null);
		public Task Insertar(TipoCuentaFinanciera item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(TipoCuentaFinanciera item, NpgsqlTransaction? transaction = null);
		public Task Eliminar(long id, NpgsqlTransaction? transaction = null);
	}
}
