using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IModeloCanvasDao {
		public Task<ModeloCanvas?> Obtener(long id, NpgsqlTransaction? transaction = null);
		public Task<List<ModeloCanvas>> ObtenerPorSub(string sub, long? idNegocio = null, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(ModeloCanvas item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(ModeloCanvas item, NpgsqlTransaction? transaction = null);
	}
}
