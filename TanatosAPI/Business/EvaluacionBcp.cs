using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
	public class EvaluacionBcp(IDateTimeProvider dateTimeProvider, IEvaluacionDao evaluacionDao) : IEvaluacionBcp {
		public async Task<List<Evaluacion>> Obtener(DateTime? fechaDesde = null, DateTime? fechasHasta = null, NpgsqlTransaction? transaction = null) {
			return await evaluacionDao.Obtener(fechaDesde, fechasHasta, transaction);
		}

		public async Task<Evaluacion> Insertar(string sub, short puntaje, string? comentario = null, NpgsqlTransaction? transaction = null) {
			Evaluacion nuevo = new() { 
				Id = 0,
				Sub = sub,
				Puntaje = puntaje,
				Comentario = comentario,
				FechaCreacion = dateTimeProvider.UtcNow
			};
			nuevo.Id = await evaluacionDao.Insertar(nuevo, transaction);
			return nuevo;
		}
	}
}
