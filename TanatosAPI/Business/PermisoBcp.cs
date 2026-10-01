using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class PermisoBcp(IDateTimeProvider dateTimeProvider, IPermisoDao permisoDao) : IPermisoBcp {
		public bool EstaVigente(Permiso? item) {
			return item != null && item.Vigencia;
		}

		public List<Permiso> FiltrarVigentes(List<Permiso> items) {
			return [.. items.Where(i => EstaVigente(i))];
		}

		public async Task<List<Permiso>> ObtenerPorIdAcceso(long idAcceso, bool filtrarVigentes = false, NpgsqlTransaction? transaction = null) {
			List<Permiso> permisos = await permisoDao.ObtenerPorIdAcceso(idAcceso, transaction);
			if (filtrarVigentes) permisos = FiltrarVigentes(permisos);
			return permisos;
		}

		public async Task<Permiso> Crear(long idAcceso, long idRecurso, string accion, NpgsqlTransaction? transaction = null) {
			Permiso nuevo = new() {
				Id = 0,
				IdAcceso = idAcceso,
				IdRecurso = idRecurso,
				Accion = accion,
				FechaCreacion = dateTimeProvider.UtcNow,
				FechaEliminacion = null,
				Vigencia = true
			};
			nuevo.Id = await permisoDao.Insertar(nuevo, transaction);
			return nuevo;
		}

		public async Task Eliminar(Permiso item, NpgsqlTransaction? transaction = null) {
			if (item.Vigencia) {
				item.FechaEliminacion = dateTimeProvider.UtcNow;
				item.Vigencia = false;
				await permisoDao.Actualizar(item, transaction);
			}
		}
	}
}
