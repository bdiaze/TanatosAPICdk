using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface ITipoUnidadTiempoBcp {
		public bool EstaVigente(TipoUnidadTiempo? tipoUnidadTiempo);
		public Task<List<TipoUnidadTiempo>> ValidarTodosVigentes(HashSet<long> ids, NpgsqlTransaction? transaction = null);
        public Task<TipoUnidadTiempo?> Obtener(long id, NpgsqlTransaction? transaction = null);
		public Task<List<TipoUnidadTiempo>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null);
		public Task<List<TipoUnidadTiempo>> ObtenerVigentes(NpgsqlTransaction? transaction = null);
		public Task<TipoUnidadTiempo> Insertar(long id, string nombre, string? nombrePlural, long cantSegundos, long? cantMinutos, long? cantHoras, long? cantDias, bool vigencia, NpgsqlTransaction? transaction = null);
		public Task<TipoUnidadTiempo> Actualizar(long id, string nombre, string? nombrePlural, long cantSegundos, long? cantMinutos, long? cantHoras, long? cantDias, bool vigencia, NpgsqlTransaction? transaction = null);
		public Task Eliminar(long idTipoUnidadTiempo, NpgsqlTransaction? transaction = null);
    }
}
