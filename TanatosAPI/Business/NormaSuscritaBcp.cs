using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class NormaSuscritaBcp(NormaSuscritaDao normaSuscritaDao, FiscalizadorNormaSuscritaBcp fiscalizadorNormaSuscritaBcp, NotificacionNormaSuscritaBcp notificacionNormaSuscritaBcp, HistorialNormaSuscritaBcp historialNormaSuscritaBcp) {
		public async Task ActualizarNormaSuscrita(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {

		}
		
		public async Task EliminarNormaSuscrita(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			if (normaSuscrita.Activado) {
				normaSuscrita.FechaDesactivacion = DateTime.UtcNow;
				normaSuscrita.Activado = false;
			}

			normaSuscrita.FechaEliminacion = DateTime.UtcNow;
			normaSuscrita.Vigencia = false;

			await normaSuscritaDao.Actualizar(normaSuscrita, transaction);
			await fiscalizadorNormaSuscritaBcp.EliminarPorNormaSuscrita(normaSuscrita, transaction);
			await notificacionNormaSuscritaBcp.EliminarPorNormaSuscrita(normaSuscrita, transaction);
			await historialNormaSuscritaBcp.EliminarPorNormaSuscrita(normaSuscrita, transaction);
		}
	}
}
