using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
	public class TipoUnidadTiempoBcp(ITipoUnidadTiempoDao tipoUnidadTiempoDao) : ITipoUnidadTiempoBcp {
		public async Task<TipoUnidadTiempo?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null) {
			return await tipoUnidadTiempoDao.ObtenerPorId(id, transaction);
		}

		public async Task<List<TipoUnidadTiempo>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null) {
			return await tipoUnidadTiempoDao.ObtenerPorVigencia(vigencia, transaction);
		}

		public async Task<List<TipoUnidadTiempo>> ObtenerVigentes(NpgsqlTransaction? transaction = null) {
			return await ObtenerPorVigencia(true, transaction);
		}
	}
}
