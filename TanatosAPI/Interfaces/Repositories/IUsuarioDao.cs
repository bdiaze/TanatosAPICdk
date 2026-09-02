using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IUsuarioDao {
		public Task<Usuario?> Obtener(string sub, NpgsqlTransaction? transaction = null);
		public Task<Usuario?> ObtenerPorUserName(string userName, NpgsqlTransaction? transaction = null);
		public Task<Usuario?> ObtenerPorFlowCustomerId(string flowCustomerId, NpgsqlTransaction? transaction = null);
		public Task Insertar(Usuario item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(Usuario item, NpgsqlTransaction? transaction = null);

	}
}
