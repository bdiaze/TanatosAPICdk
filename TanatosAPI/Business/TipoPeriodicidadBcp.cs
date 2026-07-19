using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
	public class TipoPeriodicidadBcp(ITipoPeriodicidadDao tipoPeriodicidadDao) : ITipoPeriodicidadBcp {
		public async Task<TipoPeriodicidad?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null) {
			return await tipoPeriodicidadDao.ObtenerPorId(id, transaction);
		}

		public async Task<List<TipoPeriodicidad>> ObtenerVigentes(NpgsqlTransaction? transaction = null) {
			return await tipoPeriodicidadDao.ObtenerPorVigencia(true, transaction);
		}

		public async Task<List<TipoPeriodicidad>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null) {
			return await tipoPeriodicidadDao.ObtenerPorVigencia(vigencia, transaction);
		}

		public async Task<TipoPeriodicidad> Crear(long id, string nombre, string? descripcion, string? cron, int? frecuenciaDias, int? deltaDias, int? deltaMeses, int? deltaAnnos, int orden, bool vigencia, NpgsqlTransaction? transaction = null) {
			if (cron == null && frecuenciaDias == null) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "Se debe definir una configuración de cron o frecuencia en días.");
			}
			
			if (cron != null && frecuenciaDias != null) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "Solo se puede definir una configuración de cron o de frecuencia en días, no ambas.");
			}

			TipoPeriodicidad nuevo = new() {
				Id = id,
				Nombre = nombre,
				Descripcion = descripcion,
				Cron = cron,
				FrecuenciaDias = frecuenciaDias,
				DeltaDias = deltaDias,
				DeltaMeses = deltaMeses,
				DeltaAnnos = deltaAnnos,
				Orden = orden,
				Vigencia = vigencia
			};
			await tipoPeriodicidadDao.Insertar(nuevo, transaction);
			return nuevo;
		}

		public async Task<TipoPeriodicidad> Modificar(TipoPeriodicidad tipoPeriodicidad, NpgsqlTransaction? transaction = null) {
			if (tipoPeriodicidad.Cron == null && tipoPeriodicidad.FrecuenciaDias == null) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "Se debe definir una configuración de cron o frecuencia en días.");
			}

			if (tipoPeriodicidad.Cron != null && tipoPeriodicidad.FrecuenciaDias != null) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "Solo se puede definir una configuración de cron o de frecuencia en días, no ambas.");
			}

			await tipoPeriodicidadDao.Actualizar(tipoPeriodicidad, transaction);
			return tipoPeriodicidad;
		}

		public async Task Eliminar(TipoPeriodicidad tipoPeriodicidad, NpgsqlTransaction? transaction = null) {
			await tipoPeriodicidadDao.Eliminar(tipoPeriodicidad.Id, transaction);
		}
	}
}
