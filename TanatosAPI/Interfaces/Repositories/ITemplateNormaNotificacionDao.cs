using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface ITemplateNormaNotificacionDao {
		public Task<List<TemplateNormaNotificacion>> ObtenerPorTemplateNorma(long idTemplate, long? idNorma = null, NpgsqlTransaction? transaction = null);
		public Task<List<TemplateNormaNotificacion>> ObtenerPorTipoUnidadTiempoAntelacion(long idTipoUnidadTiempoAntelacion, NpgsqlTransaction? transaction = null);
		public Task Insertar(TemplateNormaNotificacion item, NpgsqlTransaction? transaction = null);
		public Task Eliminar(long idTemplate, long? idNorma, long? idTipoUnidadTiempoAntelacion, int? cantAntelacion, NpgsqlTransaction? transaction = null);
	}
}
