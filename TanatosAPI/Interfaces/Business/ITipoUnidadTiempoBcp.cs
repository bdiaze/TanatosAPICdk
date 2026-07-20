using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface ITipoUnidadTiempoBcp {
		public Task<TipoUnidadTiempo?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null);
		public Task<List<TipoUnidadTiempo>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null);
		public Task<List<TipoUnidadTiempo>> ObtenerVigentes(NpgsqlTransaction? transaction = null);
	}
}
