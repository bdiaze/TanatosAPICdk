using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class NormaSuscritaBcp(NormaSuscritaDao normaSuscritaDao, ProcesoNotificacionBcp procesoNotificacionBcp, FiscalizadorNormaSuscritaBcp fiscalizadorNormaSuscritaBcp, NotificacionNormaSuscritaBcp notificacionNormaSuscritaBcp, HistorialNormaSuscritaBcp historialNormaSuscritaBcp) {		
		public async Task EliminarNormaSuscrita(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			if (normaSuscrita.Vigencia) {
				DateTime utcNow = DateTime.UtcNow;
				if (normaSuscrita.Activado) {
					normaSuscrita.FechaDesactivacion = utcNow;
					normaSuscrita.Activado = false;
				}

				normaSuscrita.FechaEliminacion = utcNow;
				normaSuscrita.Vigencia = false;
				await normaSuscritaDao.Actualizar(normaSuscrita, transaction);
				await procesoNotificacionBcp.ActualizarProgramacionProcesosNormaSuscrita(normaSuscrita.Id, transaction);


				await fiscalizadorNormaSuscritaBcp.EliminarPorNormaSuscrita(normaSuscrita, transaction);
				await notificacionNormaSuscritaBcp.EliminarPorNormaSuscrita(normaSuscrita, transaction);
				await historialNormaSuscritaBcp.EliminarPorNormaSuscrita(normaSuscrita, false, transaction);
			}
		}
	}
}
