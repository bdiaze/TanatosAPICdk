using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TanatosAPI.Business {
	public class ProcesoAutomaticoBcp(IDateTimeProvider dateTimeProvider, IProcesoAutomaticoDao procesoAutomaticoDao) : IProcesoAutomaticoBcp {
		public bool EstaVigente(ProcesoAutomatico? item) {
			return item != null && item.Vigencia;
		}

		public List<ProcesoAutomatico> FiltrarVigentes(List<ProcesoAutomatico> items) {
			return [.. items.Where(v => EstaVigente(v))];
		}

		public async Task<List<ProcesoAutomatico>> ObtenerVarios(HashSet<long> ids, bool filtrarVigente = false, NpgsqlTransaction? transaction = null) {
			List<ProcesoAutomatico> items = await procesoAutomaticoDao.ObtenerVarios(ids, transaction);
			if (filtrarVigente) items = FiltrarVigentes(items);
			return items;
		}

		public async Task<List<ProcesoAutomatico>> ObtenerPorNombre(string nombre, bool filtrarVigente = false, NpgsqlTransaction? transaction = null) {
			List<ProcesoAutomatico> items = await procesoAutomaticoDao.ObtenerPorNombre(nombre, transaction);
			if (filtrarVigente) items = FiltrarVigentes(items);
			return items;
		}

		public async Task<ProcesoAutomatico> Crear(long idTipoProcesoAutomatico, string idProcesoKairos, string idCalendarizacionKairos, string nombre, string arnRol, string arnProceso, string parametros, string? cron, int? frecuenciaDias, DateTime? inicioEjecucionUtc, NpgsqlTransaction? transaction = null) {
			List<ProcesoAutomatico> existentes = await ObtenerPorNombre(nombre, filtrarVigente: true, transaction: transaction);
			if (existentes.Count > 0) throw new ErrorValidacion(TipoErrorValidacion.YaExiste, "Ya existe un proceso automático con el mismo nombre", "El proceso automático es inválido.");
			
			ProcesoAutomatico item = new() { 
				Id = 0,
				IdTipoProcesoAutomatico = idTipoProcesoAutomatico,
				IdProcesoKairos = idProcesoKairos,
				IdCalendarizacionKairos = idCalendarizacionKairos,
				Nombre = nombre,
				ArnRol = arnRol,
				ArnProceso = arnProceso,
				Parametros = parametros,
				Cron = cron,
				FrecuenciaDias = frecuenciaDias,
				InicioEjecucionUtc = inicioEjecucionUtc,
				FechaCreacion = dateTimeProvider.UtcNow,
				FechaEliminacion = null,
				Vigencia = true,
			};
			item.Id = await procesoAutomaticoDao.Insertar(item, transaction);
			return item;
		}

		public async Task Modificar(ProcesoAutomatico item, NpgsqlTransaction? transaction = null) {
			List<ProcesoAutomatico> existentes = [.. (await ObtenerPorNombre(item.Nombre, filtrarVigente: true, transaction: transaction)).Where(pa => pa.Id != item.Id)];
			if (existentes.Count > 0) throw new ErrorValidacion(TipoErrorValidacion.YaExiste, "Ya existe un proceso automático con el mismo nombre", "El proceso automático es inválido.");

			await procesoAutomaticoDao.Actualizar(item, transaction);
		}

		public async Task Eliminar(ProcesoAutomatico item, NpgsqlTransaction? transaction = null) {
			if (item.Vigencia) {
				item.FechaEliminacion = dateTimeProvider.UtcNow;
				item.Vigencia = false;
				await procesoAutomaticoDao.Actualizar(item, transaction);
			}
		}
	}
}
