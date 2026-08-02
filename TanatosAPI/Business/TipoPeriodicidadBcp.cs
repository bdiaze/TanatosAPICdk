using Actions_Compile;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
	public class TipoPeriodicidadBcp(ITipoPeriodicidadDao tipoPeriodicidadDao) : ITipoPeriodicidadBcp {
		public bool EstaVigente(TipoPeriodicidad? periodicidad) {
			return periodicidad != null && periodicidad.Vigencia;
		}

		public void ValidarDeltas(TipoPeriodicidad tipoPeriodicidad) {
            // Si el tipo periodicidad no tiene deltas o tienes múltiples deltas, se lanza excepción...
            int cantDeltas =
                (tipoPeriodicidad.DeltaAnnos != null ? 1 : 0) +
                (tipoPeriodicidad.DeltaMeses != null ? 1 : 0) +
                (tipoPeriodicidad.DeltaDias != null ? 1 : 0);

            if (cantDeltas == 0)
                throw new InvalidOperationException($"No se puede calcular vencimiento futuro para este tipo de periodicidad - ID Tipo Periodicidad: {tipoPeriodicidad.Id}");

            if (cantDeltas > 1)
                throw new InvalidOperationException($"El tipo de periodicidad tiene múltiples deltas definidos - ID Tipo Periodicidad: {tipoPeriodicidad.Id}");

            if (tipoPeriodicidad.DeltaAnnos != null && tipoPeriodicidad.DeltaAnnos <= 0)
                throw new InvalidOperationException($"El tipo de periodicidad tiene un delta de años inválido - ID Tipo Periodicidad: {tipoPeriodicidad.Id}");

            if (tipoPeriodicidad.DeltaMeses != null && tipoPeriodicidad.DeltaMeses <= 0)
                throw new InvalidOperationException($"El tipo de periodicidad tiene un delta de meses inválido - ID Tipo Periodicidad: {tipoPeriodicidad.Id}");

            if (tipoPeriodicidad.DeltaDias != null && tipoPeriodicidad.DeltaDias <= 0)
                throw new InvalidOperationException($"El tipo de periodicidad tiene un delta de dias inválido - ID Tipo Periodicidad: {tipoPeriodicidad.Id}");
        }

        public async Task<TipoPeriodicidad?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null) {
			return await tipoPeriodicidadDao.ObtenerPorId(id, transaction);
		}

        public async Task<TipoPeriodicidad> ObtenerValidandoVigencia(long? id, NpgsqlTransaction? transaction = null) {
			if (id == null) throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "ID del tipo de periodicidad es inválido.");
			TipoPeriodicidad? periodicidad = await ObtenerPorId(id.Value, transaction);
			if (!EstaVigente(periodicidad)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El tipo de periodicidad no está vigente.");
            return periodicidad!;
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
