using Npgsql;
using System.Transactions;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
	public class TemplateNormaNotificacionBcp(ITemplateNormaNotificacionDao templateNormaNotificacionDao) : ITemplateNormaNotificacionBcp {
		public HashSet<(long IdTipoUnidadTiempoAntelacion, int CantAntelacion)> ExtraerAntelaciones(List<TemplateNormaNotificacion> notificaciones) {
			return [.. notificaciones.Select(n => (n.IdTipoUnidadTiempoAntelacion, n.CantAntelacion))];
		}

		public async Task<List<TemplateNormaNotificacion>> ObtenerPorTemplateNorma(long idTemplate, long? idNorma = null, NpgsqlTransaction? transaction = null) {
			return await templateNormaNotificacionDao.ObtenerPorTemplateNorma(idTemplate, idNorma, transaction);
		}

		public async Task Eliminar(long idTemplate, long? idNorma, long? idTipoUnidadTiempoAntelacion, int? cantAntelacion, NpgsqlTransaction? transaction = null) {
			await templateNormaNotificacionDao.Eliminar(idTemplate, idNorma, idTipoUnidadTiempoAntelacion, cantAntelacion, transaction);
		}

		public async Task<TemplateNormaNotificacion> Insertar(long idTemplate, long idNorma, long idTipoUnidadTiempoAntelacion, int cantAntelacion, NpgsqlTransaction? transaction = null) {
			TemplateNormaNotificacion nuevo = new() { 
				IdTemplate = idTemplate,
				IdNorma = idNorma,
				IdTipoUnidadTiempoAntelacion = idTipoUnidadTiempoAntelacion,
				CantAntelacion = cantAntelacion,
			};
			await templateNormaNotificacionDao.Insertar(nuevo, transaction);
			return nuevo;
		}
	}
}
