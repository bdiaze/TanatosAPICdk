using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface ITipoUnidadTiempoBcp {
		public Task<TipoUnidadTiempo?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null);
		public Task<List<TipoUnidadTiempo>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null);
		public Task<List<TipoUnidadTiempo>> ObtenerVigentes(NpgsqlTransaction? transaction = null);
		public Task<TipoUnidadTiempo> Insertar(long id, string nombre, string? nombrePlural, long cantSegundos, long? cantMinutos, long? cantHoras, long? cantDias, bool vigencia, NpgsqlTransaction? transaction = null);
		public Task<TipoUnidadTiempo> Actualizar(long id, string nombre, string? nombrePlural, long cantSegundos, long? cantMinutos, long? cantHoras, long? cantDias, bool vigencia, NpgsqlTransaction? transaction = null);
		public Task Eliminar(long idTipoUnidadTiempo, NpgsqlTransaction? transaction = null);
    }
}
