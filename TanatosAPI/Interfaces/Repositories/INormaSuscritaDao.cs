using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface INormaSuscritaDao {
		public Task<List<NormaSuscrita>> ObtenerPorSub(string sub, long? idNegocio = null, bool? vigencia = true, NpgsqlTransaction? transaction = null);
		public Task<NormaSuscrita?> ObtenerPorId(long idNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task<List<NormaSuscrita>> ObtenerPorTemplate(long idTemplate, long? idNorma = null, bool? vigencia = true, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(NormaSuscrita item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(NormaSuscrita item, NpgsqlTransaction? transaction = null);
	}
}
