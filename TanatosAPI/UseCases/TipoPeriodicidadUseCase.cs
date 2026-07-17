using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;

namespace TanatosAPI.UseCases {
	public class TipoPeriodicidadUseCase(ITipoPeriodicidadBcp tipoPeriodicidadBcp) {
		public async Task<List<TipoPeriodicidad>> ObtenerVigentes() {
			return await tipoPeriodicidadBcp.ObtenerVigentes();
		}

		public async Task<List<TipoPeriodicidad>> ObtenerPorVigencia(bool? vigencia) {
			return await tipoPeriodicidadBcp.ObtenerPorVigencia(vigencia);
		}

		public async Task<TipoPeriodicidad> Crear(long id, string nombre, string? descripcion, string? cron, int? frecuenciaDias, int? deltaDias, int? deltaMeses, int? deltaAnnos, bool vigencia) {
			TipoPeriodicidad? existente = await tipoPeriodicidadBcp.ObtenerPorId(id);
			if (existente != null) {
				throw new ErrorValidacion(TipoErrorValidacion.YaExiste, $"Ya existe una periodicidad con ID {id}.");
			}

			return await tipoPeriodicidadBcp.Crear(id, nombre, descripcion, cron, frecuenciaDias, deltaDias, deltaMeses, deltaAnnos, vigencia);
		}

		public async Task<TipoPeriodicidad> Modificar(long id, string nombre, string? descripcion, string? cron, int? frecuenciaDias, int? deltaDias, int? deltaMeses, int? deltaAnnos, bool vigencia) {
			TipoPeriodicidad? existente = await tipoPeriodicidadBcp.ObtenerPorId(id) ?? throw new ErrorValidacion(TipoErrorValidacion.NoVigente, $"No existe una periodicidad con ID {id}.");
			
			if (existente.Nombre != nombre || existente.Descripcion != descripcion || existente.Cron != cron || existente.FrecuenciaDias != frecuenciaDias ||
				existente.DeltaDias != deltaDias || existente.DeltaMeses != deltaMeses || existente.DeltaAnnos != deltaAnnos || existente.Vigencia != vigencia) {

				existente.Nombre = nombre;
				existente.Descripcion = descripcion;
				existente.Cron = cron;
				existente.FrecuenciaDias = frecuenciaDias;
				existente.DeltaDias = deltaDias;
				existente.DeltaMeses = deltaMeses;
				existente.DeltaAnnos = deltaAnnos;
				existente.Vigencia = vigencia;

				existente = await tipoPeriodicidadBcp.Modificar(existente);
			}

			return existente;
		}

		public async Task Eliminar(long id) {
			TipoPeriodicidad? existente = await tipoPeriodicidadBcp.ObtenerPorId(id);
			if (existente != null) {
				await tipoPeriodicidadBcp.Eliminar(existente);
			}
		}
	}
}
