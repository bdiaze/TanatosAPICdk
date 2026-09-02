using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class TipoProcesoAutomaticoBcp(IDateTimeProvider dateTimeProvider, ITipoProcesoAutomaticoDao tipoProcesoAutomaticoDao) : ITipoProcesoAutomaticoBcp {
		
		public bool EstaHabilitado(TipoProcesoAutomatico item) {
			return item.Habilitado;
		}

		public List<TipoProcesoAutomatico> FiltrarHabilitados(List<TipoProcesoAutomatico> items) {
			return [.. items.Where(i => EstaHabilitado(i))];
		}

		public async Task<TipoProcesoAutomatico?> Obtener(long id, bool filtrarHabilitado = false, bool validarHabilitado = false, NpgsqlTransaction? transaction = null) {
			TipoProcesoAutomatico? item = await tipoProcesoAutomaticoDao.Obtener(id, transaction);

			// Se aplican todas las validaciones...
			if (item != null) {
				if (validarHabilitado && !EstaHabilitado(item)) throw new ErrorValidacion(TipoErrorValidacion.EstadoNoValido, "El tipo de proceso automático no está habilitado", "El tipo de proceso automático es inválido.");
			}

			// Se aplican los filtros...
			if (item != null) {
				if (filtrarHabilitado && !EstaHabilitado(item)) return null;
			}

			return item;
		}

		public async Task<List<TipoProcesoAutomatico>> ObtenerTodos(bool filtrarHabilitados = false, NpgsqlTransaction? transaction = null) {
			List<TipoProcesoAutomatico> items = await tipoProcesoAutomaticoDao.ObtenerTodos(transaction);
			if (filtrarHabilitados) items = FiltrarHabilitados(items);
			return items;
		}

		public async Task<TipoProcesoAutomatico> Insertar(long id, string nombre, string? descripcion, bool habilitado, int orden, NpgsqlTransaction? transaction = null) {
			TipoProcesoAutomatico nuevo = new() {
				Id = id,
				Nombre = nombre,
				Descripcion = descripcion,
				Habilitado = habilitado,
				Orden = orden,
				FechaCreacion = dateTimeProvider.UtcNow,
			};
			await tipoProcesoAutomaticoDao.Insertar(nuevo, transaction);
			return nuevo;
		}

		public async Task Modificar(TipoProcesoAutomatico item, NpgsqlTransaction? transaction = null) {
			await tipoProcesoAutomaticoDao.Actualizar(item, transaction);
		}

		public async Task Eliminar(TipoProcesoAutomatico item, NpgsqlTransaction? transaction = null) {
			await tipoProcesoAutomaticoDao.Eliminar(item.Id, transaction);
		}
	}
}
