using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IMovimientoFinancieroDao {
		public Task<MovimientoFinanciero?> Obtener(long id, NpgsqlTransaction? transaction = null);
		public Task<List<MovimientoFinanciero>> ObtenerConPaginacion(long idCuentaFinanciera, DateTime? fechaDesde = null, DateTime? fechaHasta = null, bool? vigencia = true, DateTime? primeraFecha = null, long? primerId = null, int cantidad = 50, NpgsqlTransaction? transaction = null);
		public Task<List<MovimientoFinanciero>> ObtenerPrevios(long idCuentaFinanciera, DateTime fechaReferencia, long? idReferencia = null, bool? vigencia = true, int cantidad = 1, NpgsqlTransaction? transaction = null);
		public Task<List<MovimientoFinanciero>> ObtenerPosteriores(long idCuentaFinanciera, DateTime fechaReferencia, long idReferencia, bool? vigencia = true, bool incluirReferencia = false, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(MovimientoFinanciero item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(MovimientoFinanciero item, NpgsqlTransaction? transaction = null);
	}
}
