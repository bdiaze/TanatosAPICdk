using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface ITemplateNormaNotificacionBcp {
		public HashSet<(long IdTipoUnidadTiempoAntelacion, int CantAntelacion)> ExtraerAntelaciones(List<TemplateNormaNotificacion> notificaciones);
		public Task<List<TemplateNormaNotificacion>> ObtenerPorTemplateNorma(long idTemplate, long? idNorma = null, NpgsqlTransaction? transaction = null);
		public Task Eliminar(long idTemplate, long? idNorma, long? idTipoUnidadTiempoAntelacion, int? cantAntelacion, NpgsqlTransaction? transaction = null);
		public Task<TemplateNormaNotificacion> Insertar(long idTemplate, long idNorma, long idTipoUnidadTiempoAntelacion, int cantAntelacion, NpgsqlTransaction? transaction = null);
	}
}
