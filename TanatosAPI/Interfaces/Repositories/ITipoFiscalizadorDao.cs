using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface ITipoFiscalizadorDao {
		public Task<TipoFiscalizador?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null);
		public Task<List<TipoFiscalizador>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null);
		public Task Insertar(TipoFiscalizador item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(TipoFiscalizador item, NpgsqlTransaction? transaction = null);
		public Task Eliminar(long id, NpgsqlTransaction? transaction = null);
	}
}
