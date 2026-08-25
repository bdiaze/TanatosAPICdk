using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class TipoProcesoAutomaticoBcp(IDateTimeProvider dateTimeProvider, ITipoProcesoAutomaticoDao tipoProcesoAutomaticoDao) : ITipoProcesoAutomaticoBcp {
		public bool EstaVigente(TipoProcesoAutomatico? item) {
			return item != null && item.Vigencia;
		}

		public bool EstaHabilitado(TipoProcesoAutomatico item) {
			return item.Habilitado;
		}

		public List<TipoProcesoAutomatico> FiltrarVigentes(List<TipoProcesoAutomatico> items) {
			return [.. items.Where(i => EstaVigente(i))];
		}

		public List<TipoProcesoAutomatico> FiltrarHabilitados(List<TipoProcesoAutomatico> items) {
			return [.. items.Where(i => EstaHabilitado(i))];
		}

		public async Task<TipoProcesoAutomatico?> Obtener(long id, bool filtrarVigente = false, bool filtrarHabilitado = false, bool validarVigencia = false, bool validarHabilitado = false, NpgsqlTransaction? transaction = null) {
			TipoProcesoAutomatico? item = await tipoProcesoAutomaticoDao.Obtener(id, transaction);

			// Se aplican todas las validaciones...
			if (validarVigencia && !EstaVigente(item)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El tipo de proceso automático no existe o no está vigente", "El tipo de proceso automático es inválido.");
			if (item != null) {
				if (validarHabilitado && !EstaHabilitado(item)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El tipo de proceso automático no está habilitado", "El tipo de proceso automático es inválido.");
			}

			// Se aplican los filtros...
			if (filtrarVigente && !EstaVigente(item)) return null;
			if (item != null) {
				if (filtrarHabilitado && !EstaHabilitado(item)) return null;
			}

			return item;
		}

		public async Task<List<TipoProcesoAutomatico>> ObtenerTodos(bool filtrarVigentes = false, bool filtrarHabilitados = false, NpgsqlTransaction? transaction = null) {
			List<TipoProcesoAutomatico> items = await tipoProcesoAutomaticoDao.ObtenerTodos(transaction);
			if (filtrarVigentes) items = FiltrarVigentes(items);
			if (filtrarHabilitados) items = FiltrarHabilitados(items);
			return items;
		}

		public async Task<TipoProcesoAutomatico> Insertar(string nombre, string? descripcion, bool habilitado, int orden, NpgsqlTransaction? transaction = null) {
			TipoProcesoAutomatico nuevo = new() {
				Id = 0,
				Nombre = nombre,
				Descripcion = descripcion,
				Habilitado = habilitado,
				Orden = orden,
				FechaCreacion = dateTimeProvider.UtcNow,
				FechaEliminacion = null,
				Vigencia = true
			};
			nuevo.Id = await tipoProcesoAutomaticoDao.Insertar(nuevo, transaction);
			return nuevo;
		}

		public async Task Modificar(TipoProcesoAutomatico item, NpgsqlTransaction? transaction = null) {
			await tipoProcesoAutomaticoDao.Actualizar(item, transaction);
		}

		public async Task Eliminar(TipoProcesoAutomatico item, NpgsqlTransaction? transaction = null) {
			if (item.Vigencia) {
				item.FechaEliminacion = dateTimeProvider.UtcNow;
				item.Vigencia = false;
				await tipoProcesoAutomaticoDao.Actualizar(item, transaction);
			}
		}
	}
}
