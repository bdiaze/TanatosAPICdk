using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
	public class EvaluacionBcp(IDateTimeProvider dateTimeProvider, IEvaluacionDao evaluacionDao) : IEvaluacionBcp {
		private const short MIN_PUNTAJE = 1;
		private const short MAX_PUNTAJE = 5;

		public async Task<List<Evaluacion>> Obtener(DateTime? fechaDesde = null, DateTime? fechasHasta = null, NpgsqlTransaction? transaction = null) {
			return await evaluacionDao.Obtener(fechaDesde, fechasHasta, transaction);
		}

		public async Task<Evaluacion> Insertar(string sub, short puntaje, string? comentario = null, NpgsqlTransaction? transaction = null) {
			comentario = comentario?.Trim();
			comentario = string.IsNullOrWhiteSpace(comentario) ? null : comentario;

			if (puntaje < MIN_PUNTAJE) throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, $"El puntaje no puede ser menor a {MIN_PUNTAJE}.");
			if (puntaje > MAX_PUNTAJE) throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, $"El puntaje no puede ser mayor a {MAX_PUNTAJE}.");

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
