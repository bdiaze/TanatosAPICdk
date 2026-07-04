using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface ITemplateNormaDao {
		public Task<List<TemplateNorma>> ObtenerPorTemplate(long idTemplate, NpgsqlTransaction? transaction = null);
		public Task Insertar(TemplateNorma item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(TemplateNorma item, NpgsqlTransaction? transaction = null);
		public Task Eliminar(long idTemplate, long? idNorma, NpgsqlTransaction? transaction = null);
	}
}
