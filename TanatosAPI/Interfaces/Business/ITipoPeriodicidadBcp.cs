using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface ITipoPeriodicidadBcp {
		public bool EstaVigente(TipoPeriodicidad? periodicidad);
		public Task<TipoPeriodicidad?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null);
		public Task<List<TipoPeriodicidad>> ObtenerVigentes(NpgsqlTransaction? transaction = null);
		public Task<List<TipoPeriodicidad>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null);
		public Task<TipoPeriodicidad> Crear(long id, string nombre, string? descripcion, string? cron, int? frecuenciaDias, int? deltaDias, int? deltaMeses, int? deltaAnnos, int orden, bool vigencia, NpgsqlTransaction? transaction = null);
		public Task<TipoPeriodicidad> Modificar(TipoPeriodicidad tipoPeriodicidad, NpgsqlTransaction? transaction = null);
		public Task Eliminar(TipoPeriodicidad tipoPeriodicidad, NpgsqlTransaction? transaction = null);
	}
}
