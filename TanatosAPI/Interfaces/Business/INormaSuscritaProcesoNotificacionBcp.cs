using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface INormaSuscritaProcesoNotificacionBcp {
		public bool EstaVigente(NormaSuscritaProcesoNotificacion? item);
		public List<NormaSuscritaProcesoNotificacion> FiltrarVigentes(List<NormaSuscritaProcesoNotificacion> items);
		public Task<List<NormaSuscritaProcesoNotificacion>> ObtenerPorNormaSuscrita(long idNormaSuscrita, bool filtrarVigente = true, NpgsqlTransaction? transaction = null);
		public Task<NormaSuscritaProcesoNotificacion> Crear(long idNormaSuscrita, long idProcesoAutomatico, NpgsqlTransaction? transaction = null);
		public Task Eliminar(NormaSuscritaProcesoNotificacion item, NpgsqlTransaction? transaction = null);
	}
}
