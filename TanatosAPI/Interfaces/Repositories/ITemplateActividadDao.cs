using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface ITemplateActividadDao {
		public Task<List<TemplateActividad>> ObtenerPorTemplate(long idTemplate, NpgsqlTransaction? transaction = null);
		public Task<List<TemplateActividad>> ObtenerPorActividad(long idTipoActividad, NpgsqlTransaction? transaction = null);
		public Task Insertar(TemplateActividad item, NpgsqlTransaction? transaction = null);
		public Task Eliminar(long idTemplate, long? idTipoActividad, NpgsqlTransaction? transaction = null);
	}
}
