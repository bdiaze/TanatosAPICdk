using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class PreguntaFrecuenteBcp(IDateTimeProvider dateTimeProvider, IPreguntaFrecuenteDao preguntaFrecuenteDao) : IPreguntaFrecuenteBcp {
		public bool EstaVigente(PreguntaFrecuente? preguntaFrecuente) {
			return preguntaFrecuente != null && preguntaFrecuente.Vigencia;
		}

		public async Task<List<PreguntaFrecuente>> ObtenerVigentes(NpgsqlTransaction? transaction = null) {
			return await preguntaFrecuenteDao.ObtenerPorVigencia(true, transaction);
		}

		public async Task<PreguntaFrecuente> Insertar(string pregunta, string respuesta, bool habilitado, int orden, NpgsqlTransaction? transaction = null) {
			PreguntaFrecuente nuevo = new() {
				Id = 0,
				Pregunta = pregunta,
				Respuesta = respuesta,
				Habilitado = habilitado,
				Orden = orden,
				FechaCreacion = dateTimeProvider.UtcNow,
				FechaEliminacion = null,
				Vigencia = true
			};
			nuevo.Id = await preguntaFrecuenteDao.Insertar(nuevo, transaction);
			return nuevo;
		}

		public async Task Modificar(PreguntaFrecuente preguntaFrecuente, NpgsqlTransaction? transaction = null) {
			await preguntaFrecuenteDao.Actualizar(preguntaFrecuente, transaction);
		}

		public async Task Eliminar(PreguntaFrecuente preguntaFrecuente, NpgsqlTransaction? transaction = null) {
			if (preguntaFrecuente.Vigencia) {
				preguntaFrecuente.FechaEliminacion = dateTimeProvider.UtcNow;
				preguntaFrecuente.Vigencia = false;
				await preguntaFrecuenteDao.Actualizar(preguntaFrecuente, transaction);
			}
		}
	}
}
