using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IProcesoAutomaticoBcp {
		public bool EstaVigente(ProcesoAutomatico? item);
		public List<ProcesoAutomatico> FiltrarVigentes(List<ProcesoAutomatico> items);
		public Task<List<ProcesoAutomatico>> ObtenerVarios(HashSet<long> ids, bool filtrarVigente = false, NpgsqlTransaction? transaction = null);
		public Task<List<ProcesoAutomatico>> ObtenerPorNombre(string nombre, bool filtrarVigente = false, NpgsqlTransaction? transaction = null);
		public Task<ProcesoAutomatico> Crear(long idTipoProcesoAutomatico, string idProcesoKairos, string idCalendarizacionKairos, string nombre, string arnRol, string arnProceso, string parametros, string? cron, int? frecuenciaDias, DateTime? inicioEjecucionUtc, NpgsqlTransaction? transaction = null);
		public Task Modificar(ProcesoAutomatico item, NpgsqlTransaction? transaction = null);
		public Task Eliminar(ProcesoAutomatico item, NpgsqlTransaction? transaction = null);
	}
}
