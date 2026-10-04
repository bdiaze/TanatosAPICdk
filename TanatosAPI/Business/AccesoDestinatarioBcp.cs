using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class AccesoDestinatarioBcp(IDateTimeProvider dateTimeProvider, IAccesoDestinatarioDao accesoDestinatarioDao) : IAccesoDestinatarioBcp {
		public bool EstaVigente(AccesoDestinatario? item) {
			return item != null && item.Vigencia;
		}

		public List<AccesoDestinatario> FiltrarVigentes(List<AccesoDestinatario> items) {
			return [.. items.Where(i => EstaVigente(i))];
		}

		public async Task<List<AccesoDestinatario>> ObtenerPorIdAcceso(long idAcceso, bool filtrarVigentes = false, NpgsqlTransaction? transaction = null) {
			List<AccesoDestinatario> items = await accesoDestinatarioDao.ObtenerPorIdAcceso(idAcceso, transaction);
			if (filtrarVigentes) items = FiltrarVigentes(items);
			return items;
		}

		public async Task<AccesoDestinatario> Crear(long idAcceso, long idDestinatarioNotificacion, NpgsqlTransaction? transaction = null) {
			AccesoDestinatario nuevo = new() {
				Id = 0,
				IdAcceso = idAcceso,
				IdDestinatarioNotificacion = idDestinatarioNotificacion,
				FechaCreacion = dateTimeProvider.UtcNow,
				FechaEliminacion = null,
				Vigencia = true
			};
			nuevo.Id = await accesoDestinatarioDao.Insertar(nuevo, transaction);
			return nuevo;
		}
	}
}
