using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class DestinatarioNotificacionBcp(DestinatarioNotificacionDao destinatarioNotificacionDao, NormaSuscritaDao normaSuscritaDao, HistorialNormaSuscritaBcp historialNormaSuscritaBcp) {
		public async Task Validar(DestinatarioNotificacion destinatarioNotificacion, NpgsqlTransaction? transaction = null) {
			if (!destinatarioNotificacion.Validado) {
				destinatarioNotificacion.Validado = true;
				destinatarioNotificacion.FechaValidacion = DateTime.UtcNow;
				await destinatarioNotificacionDao.Actualizar(destinatarioNotificacion, transaction);

				List<NormaSuscrita> normasSuscritas = [.. (await normaSuscritaDao.ObtenerPorSub(destinatarioNotificacion.Sub, destinatarioNotificacion.IdNegocio, true, transaction)).Where(ns => ns.Activado)];
				foreach (NormaSuscrita normaSuscrita in normasSuscritas) {
					await historialNormaSuscritaBcp.ActualizarHistorialNotificacionPorNormaSuscrita(normaSuscrita, transaction);
				}
			}
		}

		public async Task Eliminar(DestinatarioNotificacion destinatarioNotificacion, NpgsqlTransaction? transaction = null) {
			if (destinatarioNotificacion.Vigencia) {
				destinatarioNotificacion.FechaEliminacion = DateTime.UtcNow;
				destinatarioNotificacion.Vigencia = false;
				await destinatarioNotificacionDao.Actualizar(destinatarioNotificacion, transaction);

				List<NormaSuscrita> normasSuscritas = [.. (await normaSuscritaDao.ObtenerPorSub(destinatarioNotificacion.Sub, destinatarioNotificacion.IdNegocio, true, transaction)).Where(ns => ns.Activado)];
				foreach (NormaSuscrita normaSuscrita in normasSuscritas) {
					await historialNormaSuscritaBcp.ActualizarHistorialNotificacionPorNormaSuscrita(normaSuscrita, transaction);
				}
			}
		}
	}
}
