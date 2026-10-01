using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class RecursoBcp(IDateTimeProvider dateTimeProvider, IRecursoDao recursoDao) : IRecursoBcp {
		public bool EstaVigente(Recurso? item) {
			return item != null && item.Vigencia;
		}

		public List<Recurso> FiltrarVigentes(List<Recurso> items) {
			return [.. items.Where(i => EstaVigente(i))];
		}

		public async Task<List<Recurso>> ObtenerVarios(HashSet<long> ids, bool filtrarVigentes = false, NpgsqlTransaction? transaction = null) {
			List<Recurso> items = await recursoDao.ObtenerVarios(ids, transaction);
			if (filtrarVigentes) items = FiltrarVigentes(items);
			return items;
		}

		public async Task<Recurso> Crear(string tipo, string idInterno, NpgsqlTransaction? transaction = null) {
			Recurso nuevo = new() {
				Id = 0,
				Tipo = tipo,
				IdInterno = idInterno,
				FechaCreacion = dateTimeProvider.UtcNow,
				FechaEliminacion = null,
				Vigencia = true
			};
			nuevo.Id = await recursoDao.Insertar(nuevo, transaction);
			return nuevo;
		}

		public async Task Eliminar(Recurso item, NpgsqlTransaction? transaction = null) {
			if (item.Vigencia) {
				item.FechaEliminacion = dateTimeProvider.UtcNow;
				item.Vigencia = false;
				await recursoDao.Actualizar(item, transaction);
			}
		}
	}
}
