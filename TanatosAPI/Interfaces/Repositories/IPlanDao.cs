using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IPlanDao {
		public Task<Plan?> Obtener(long id, NpgsqlTransaction? transaction = null);
		public Task<List<Plan>> ObtenerPorVigencia(bool? vigencia = true, NpgsqlTransaction? transaction = null);
		public Task Insertar(Plan item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(Plan item, NpgsqlTransaction? transaction = null);
		public Task Eliminar(long id, NpgsqlTransaction? transaction = null);
	}
}
