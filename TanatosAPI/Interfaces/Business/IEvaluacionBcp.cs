using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IEvaluacionBcp {
		public Task<List<Evaluacion>> Obtener(DateTime? fechaDesde = null, DateTime? fechasHasta = null, NpgsqlTransaction? transaction = null);
		public Task<Evaluacion> Insertar(string sub, short puntaje, string? comentario = null, NpgsqlTransaction? transaction = null);
	}
}
