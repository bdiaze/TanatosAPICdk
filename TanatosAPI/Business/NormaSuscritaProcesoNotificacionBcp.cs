using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
	public class NormaSuscritaProcesoNotificacionBcp(IDateTimeProvider dateTimeProvider, INormaSuscritaProcesoNotificacionDao normaSuscritaProcesoNotificacionDao) : INormaSuscritaProcesoNotificacionBcp {
		public bool EstaVigente(NormaSuscritaProcesoNotificacion? item) {
			return item != null && item.Vigencia;
		}

		public List<NormaSuscritaProcesoNotificacion> FiltrarVigentes(List<NormaSuscritaProcesoNotificacion> items) {
			return [.. items.Where(v => EstaVigente(v))];
		}

		public async Task<List<NormaSuscritaProcesoNotificacion>> ObtenerPorNormaSuscrita(long idNormaSuscrita, bool filtrarVigente = true, NpgsqlTransaction? transaction = null) {
			List<NormaSuscritaProcesoNotificacion> items = await normaSuscritaProcesoNotificacionDao.ObtenerPorNormaSuscrita(idNormaSuscrita, transaction);
			if (filtrarVigente) items = FiltrarVigentes(items);
			return items;
		}

		public async Task<NormaSuscritaProcesoNotificacion> Crear(long idNormaSuscrita, long idProcesoAutomatico, NpgsqlTransaction? transaction = null) {
			NormaSuscritaProcesoNotificacion item = new() {
				Id = 0,
				IdNormaSuscrita = idNormaSuscrita,
				IdProcesoAutomatico = idProcesoAutomatico,
				FechaCreacion = dateTimeProvider.UtcNow,
				FechaEliminacion = null,
				Vigencia = true
			};
			item.Id = await normaSuscritaProcesoNotificacionDao.Insertar(item, transaction);
			return item;
		}

		public async Task Eliminar(NormaSuscritaProcesoNotificacion item, NpgsqlTransaction? transaction = null) {
			if (item.Vigencia) {
				item.FechaEliminacion = dateTimeProvider.UtcNow;
				item.Vigencia = false;
				await normaSuscritaProcesoNotificacionDao.Actualizar(item, transaction);
			}
		}
	}
}
