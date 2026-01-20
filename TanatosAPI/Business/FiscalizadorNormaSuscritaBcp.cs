using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class FiscalizadorNormaSuscritaBcp(FiscalizadorNormaSuscritaDao fiscalizadorNormaSuscritaDao) {
		public async Task ActualizarPorNormaSuscrita(NormaSuscrita normaSuscrita, List<FiscalizadorNormaSuscrita> fiscalizadoresNormaSuscrita, NpgsqlTransaction? transaction = null) {
			List<FiscalizadorNormaSuscrita> fiscalizadoresExistentes = await fiscalizadorNormaSuscritaDao.ObtenerPorNormaSuscrita(normaSuscrita.Id, true, transaction);

			// Se eliminan los fiscalizadores existentes que no se incluyen en la entrada...
			foreach (FiscalizadorNormaSuscrita fiscalizadorExistente in fiscalizadoresExistentes) {
				if (!fiscalizadoresNormaSuscrita.Any(n => n.IdTipoFiscalizador == fiscalizadorExistente.IdTipoFiscalizador)) {
					fiscalizadorExistente.FechaEliminacion = DateTime.UtcNow;
					fiscalizadorExistente.Vigencia = false;
					await fiscalizadorNormaSuscritaDao.Actualizar(fiscalizadorExistente, transaction);
				}
			}

			// Se agregan los nuevos fiscalizadores...
			foreach (FiscalizadorNormaSuscrita fiscalizadorNuevo in fiscalizadoresNormaSuscrita) {
				if (!fiscalizadoresExistentes.Any(fe => fe.IdTipoFiscalizador == fiscalizadorNuevo.IdTipoFiscalizador)) {
					fiscalizadorNuevo.IdNormaSuscrita = normaSuscrita.Id;
					fiscalizadorNuevo.FechaCreacion = DateTime.UtcNow;
					fiscalizadorNuevo.FechaEliminacion = null;
					fiscalizadorNuevo.Vigencia = true;

					fiscalizadorNuevo.Id = await fiscalizadorNormaSuscritaDao.Insertar(fiscalizadorNuevo, transaction);
				}
			}
		}

		public async Task EliminarPorNormaSuscrita(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			List<FiscalizadorNormaSuscrita> fiscalizadoresVigentes = await fiscalizadorNormaSuscritaDao.ObtenerPorNormaSuscrita(normaSuscrita.Id, true, transaction);
			foreach (FiscalizadorNormaSuscrita fiscalizador in fiscalizadoresVigentes) {
				fiscalizador.FechaEliminacion = DateTime.UtcNow;
				fiscalizador.Vigencia = false;
				await fiscalizadorNormaSuscritaDao.Actualizar(fiscalizador, transaction);
			}
		}
	}
}
