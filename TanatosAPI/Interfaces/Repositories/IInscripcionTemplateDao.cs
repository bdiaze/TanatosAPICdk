using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IInscripcionTemplateDao {
		public Task<List<InscripcionTemplate>> ObtenerPorSub(string sub, long idNegocio, bool? vigencia = true, NpgsqlTransaction? transaction = null);
		public Task Insertar(InscripcionTemplate item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(InscripcionTemplate item, NpgsqlTransaction? transaction = null);
	}
}
