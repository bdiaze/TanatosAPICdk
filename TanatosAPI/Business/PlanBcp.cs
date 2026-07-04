using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
	public class PlanBcp(IPlanDao planDao) : IPlanBcp {
		public bool EstaVigente(Plan? plan) {
			return plan != null && plan.Vigencia;
		}

		public async Task<Plan?> ObtenerPorId(long idPlan, NpgsqlTransaction? transaction = null) {
			return await planDao.Obtener(idPlan, transaction);
		}

		public async Task<Plan> ObtenerPorIdValidandoVigencia(long idPlan, NpgsqlTransaction? transaction = null) {
			Plan? plan = await ObtenerPorId(idPlan, transaction);
			if (!EstaVigente(plan)) {
				throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El plan no existe o no está vigente", "El plan es inválido.");
			}

			return plan!;
		}

		public async Task<List<Plan>> ObtenerTodos() {
			return await planDao.ObtenerPorVigencia(null);
		}

		public async Task<List<Plan>> ObtenerVigentes(NpgsqlTransaction? transaction = null) {
			return await planDao.ObtenerPorVigencia(true);
		}

		public async Task<List<Plan>> ObtenerPlanesGratuitos(NpgsqlTransaction? transaction = null) {
			return [.. (await ObtenerVigentes(transaction)).Where(p => p.Precio == 0)];
		}
	}
}
