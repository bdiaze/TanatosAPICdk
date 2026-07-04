using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class FiscalizadorNormaSuscritaBcp(IDateTimeProvider dateTimeProvider, IFiscalizadorNormaSuscritaDao fiscalizadorNormaSuscritaDao) {
		public async Task ActualizarPorNormaSuscrita(NormaSuscrita normaSuscrita, HashSet<long> idTiposFiscalizadores, NpgsqlTransaction? transaction = null) {
			List<FiscalizadorNormaSuscrita> fiscalizadoresExistentes = await fiscalizadorNormaSuscritaDao.ObtenerPorNormaSuscrita(normaSuscrita.Id, true, transaction);

			// Se eliminan los fiscalizadores existentes que no se incluyen en la entrada...
			foreach (FiscalizadorNormaSuscrita fiscalizadorExistente in fiscalizadoresExistentes) {
				if (!idTiposFiscalizadores.Any(n => n == fiscalizadorExistente.IdTipoFiscalizador)) {
					fiscalizadorExistente.FechaEliminacion = dateTimeProvider.UtcNow;
					fiscalizadorExistente.Vigencia = false;
					await fiscalizadorNormaSuscritaDao.Actualizar(fiscalizadorExistente, transaction);
				}
			}

			// Se agregan los nuevos fiscalizadores...
			foreach (long idTipoFiscalizadorNuevo in idTiposFiscalizadores) {
				if (!fiscalizadoresExistentes.Any(fe => fe.IdTipoFiscalizador == idTipoFiscalizadorNuevo)) {
					await fiscalizadorNormaSuscritaDao.Insertar(new FiscalizadorNormaSuscrita {
						Id = 0,
						IdNormaSuscrita = normaSuscrita.Id,
						IdTipoFiscalizador = idTipoFiscalizadorNuevo,
						FechaCreacion = dateTimeProvider.UtcNow,
						FechaEliminacion = null,
						Vigencia = true
					}, transaction);
				}
			}
		}

		public async Task EliminarPorNormaSuscrita(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			List<FiscalizadorNormaSuscrita> fiscalizadoresVigentes = await fiscalizadorNormaSuscritaDao.ObtenerPorNormaSuscrita(normaSuscrita.Id, true, transaction);
			foreach (FiscalizadorNormaSuscrita fiscalizador in fiscalizadoresVigentes) {
				fiscalizador.FechaEliminacion = dateTimeProvider.UtcNow;
				fiscalizador.Vigencia = false;
				await fiscalizadorNormaSuscritaDao.Actualizar(fiscalizador, transaction);
			}
		}
	}
}
