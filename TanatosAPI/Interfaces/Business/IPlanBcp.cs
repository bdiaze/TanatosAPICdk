using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IPlanBcp {
		public bool EstaVigente(Plan? plan);
		public Task<Plan?> ObtenerPorId(long idPlan, NpgsqlTransaction? transaction = null);
		public Task<Plan> ObtenerPorIdValidandoVigencia(long idPlan, NpgsqlTransaction? transaction = null);
		public Task<List<Plan>> ObtenerTodos();
		public Task<List<Plan>> ObtenerVigentes(NpgsqlTransaction? transaction = null);
		public Task<List<Plan>> ObtenerPlanesGratuitos(NpgsqlTransaction? transaction = null);
	}
}
