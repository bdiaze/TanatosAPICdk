using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface ITipoProcesoAutomaticoBcp {
		public bool EstaVigente(TipoProcesoAutomatico? item);
		public bool EstaHabilitado(TipoProcesoAutomatico item);
		public List<TipoProcesoAutomatico> FiltrarVigentes(List<TipoProcesoAutomatico> items);
		public List<TipoProcesoAutomatico> FiltrarHabilitados(List<TipoProcesoAutomatico> items);
		public Task<TipoProcesoAutomatico?> Obtener(long id, bool filtrarVigente = false, bool filtrarHabilitado = false, bool validarVigencia = false, bool validarHabilitado = false, NpgsqlTransaction? transaction = null);
		public Task<List<TipoProcesoAutomatico>> ObtenerTodos(bool filtrarVigentes = false, bool filtrarHabilitados = false, NpgsqlTransaction? transaction = null);
		public Task<TipoProcesoAutomatico> Insertar(long id, string nombre, string? descripcion, bool habilitado, int orden, NpgsqlTransaction? transaction = null);
		public Task Modificar(TipoProcesoAutomatico item, NpgsqlTransaction? transaction = null);
		public Task Eliminar(TipoProcesoAutomatico item, NpgsqlTransaction? transaction = null);
	}
}
