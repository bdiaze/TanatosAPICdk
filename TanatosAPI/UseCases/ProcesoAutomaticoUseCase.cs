using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;

namespace TanatosAPI.UseCases {
	public class ProcesoAutomaticoUseCase(IProcesoAutomaticoBcp procesoAutomaticoBcp) {
		public async Task<(List<ProcesoAutomatico> items, long? siguienteId)> ObtenerConPaginacion(long? primerId = null, int? cantidad = null, string? nombre = null, bool? vigencia = true) {
			int cantRegistros = cantidad ?? 50;

			if (cantRegistros <= 0 || cantRegistros > 100) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "La cantidad de registros debe estar entre 1 y 100.");
			}
			
			// Se solicita un registro adicional para determinar si hay más registros disponibles...
			List<ProcesoAutomatico> items = await procesoAutomaticoBcp.ObtenerConPaginacion(primerId, cantRegistros + 1, nombre, vigencia);
			
			// Se determina si hay más registros comparando la cantidad de registros retornados...
			bool hayMas = items.Count > cantRegistros;

			// Si hay más registros, se obtiene el siguiente ID y se elimina del retorno...
			long? siguienteId = null;
			if (hayMas) {
				siguienteId = items[^1].Id;
				items.RemoveAt(items.Count - 1);
			}

			return (items, siguienteId);
		}
	}
}
