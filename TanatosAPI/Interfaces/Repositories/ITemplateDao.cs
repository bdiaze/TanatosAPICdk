using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface ITemplateDao {
		public Task<Template?> ObtenerPorId(long idTemplate, NpgsqlTransaction? transaction = null);
		public Task<List<Template>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null);
		public Task Insertar(Template item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(Template item, NpgsqlTransaction? transaction = null);
		public Task Eliminar(long id, NpgsqlTransaction? transaction = null);
	}
}
