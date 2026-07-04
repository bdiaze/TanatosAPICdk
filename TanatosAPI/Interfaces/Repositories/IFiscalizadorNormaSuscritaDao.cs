using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IFiscalizadorNormaSuscritaDao {
		public Task<List<FiscalizadorNormaSuscrita>> ObtenerPorNormaSuscrita(long idNormaSuscrita, bool? vigencia = true, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(FiscalizadorNormaSuscrita item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(FiscalizadorNormaSuscrita item, NpgsqlTransaction? transaction = null);
	}
}
