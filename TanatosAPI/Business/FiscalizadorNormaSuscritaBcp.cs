using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class FiscalizadorNormaSuscritaBcp(IDateTimeProvider dateTimeProvider, IFiscalizadorNormaSuscritaDao fiscalizadorNormaSuscritaDao) : IFiscalizadorNormaSuscritaBcp {
		public async Task<List<FiscalizadorNormaSuscrita>> ObtenerVigentesPorNormaSuscrita(long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
			return await fiscalizadorNormaSuscritaDao.ObtenerPorNormaSuscrita(idNormaSuscrita, true, transaction);
		}
		
		public async Task Eliminar(FiscalizadorNormaSuscrita fiscalizadorNormaSuscrita, NpgsqlTransaction? transaction = null) {
			if (fiscalizadorNormaSuscrita.Vigencia) {
				fiscalizadorNormaSuscrita.FechaEliminacion = dateTimeProvider.UtcNow;
				fiscalizadorNormaSuscrita.Vigencia = false;
				await fiscalizadorNormaSuscritaDao.Actualizar(fiscalizadorNormaSuscrita, transaction);
			}
		}
		
		public async Task EliminarPorNormaSuscrita(long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
			List<FiscalizadorNormaSuscrita> fiscalizadoresVigentes = await fiscalizadorNormaSuscritaDao.ObtenerPorNormaSuscrita(idNormaSuscrita, true, transaction);
			foreach (FiscalizadorNormaSuscrita fiscalizador in fiscalizadoresVigentes) {
				await Eliminar(fiscalizador, transaction);
			}
		}

		public async Task<FiscalizadorNormaSuscrita> Insertar(long idNormaSuscrita, long idTipoFiscalizador, NpgsqlTransaction? transaction = null) {
			FiscalizadorNormaSuscrita nuevo = new() {
				Id = 0,
				IdNormaSuscrita = idNormaSuscrita,
				IdTipoFiscalizador = idTipoFiscalizador,
				FechaCreacion = dateTimeProvider.UtcNow,
				FechaEliminacion = null,
				Vigencia = true
			};
			nuevo.Id = await fiscalizadorNormaSuscritaDao.Insertar(nuevo, transaction);
			return nuevo;
		}

		public async Task ActualizarPorNormaSuscrita(long idNormaSuscrita, HashSet<long> idTiposFiscalizadores, NpgsqlTransaction? transaction = null) {
			List<FiscalizadorNormaSuscrita> fiscalizadoresExistentes = await fiscalizadorNormaSuscritaDao.ObtenerPorNormaSuscrita(idNormaSuscrita, true, transaction);
			HashSet<long> existentes = [.. fiscalizadoresExistentes.Select(n => n.IdTipoFiscalizador)];

			// Se eliminan los fiscalizadores existentes que no se incluyen en la entrada...
			foreach (FiscalizadorNormaSuscrita fiscalizadorExistente in fiscalizadoresExistentes) {
				if (!idTiposFiscalizadores.Contains(fiscalizadorExistente.IdTipoFiscalizador)) {
					await Eliminar(fiscalizadorExistente, transaction);
				}
			}

			// Se agregan los nuevos fiscalizadores...
			foreach (long idTipoFiscalizadorNuevo in idTiposFiscalizadores) {
				if (!existentes.Contains(idTipoFiscalizadorNuevo)) {
					await Insertar(idNormaSuscrita, idTipoFiscalizadorNuevo, transaction);
				}
			}
		}
	}
}
