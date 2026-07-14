using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.UseCases {
	public class EvaluacionUseCase(IEvaluacionBcp evaluacionBcp) {
		public async Task<List<Evaluacion>> Obtener(DateTime? fechaDesde = null, DateTime? fechasHasta = null) {
			return await evaluacionBcp.Obtener(fechaDesde, fechasHasta);
		}

		public async Task<Evaluacion> Insertar(string sub, short puntaje, string? comentario = null) {
			return await evaluacionBcp.Insertar(sub, puntaje, comentario);
		}
	}
}
