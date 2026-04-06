using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class SuscripcionBcp(SuscripcionDao suscripcionDao) {
		public async Task<bool> TienePlanEmpresa(string sub) {
			// Se obtienen las suscripciones del usuario...
			List<Suscripcion> suscripciones = await suscripcionDao.ObtenerPorSub(sub, true);

			if (suscripciones.Any(s => s.FechaExpiracion != null && s.FechaExpiracion > DateTime.UtcNow)) {
				return true;
			}

			return false;
		}
	}
}
