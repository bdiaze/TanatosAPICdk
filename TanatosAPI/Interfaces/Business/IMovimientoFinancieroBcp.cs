using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IMovimientoFinancieroBcp {
		public bool EstaVigente(MovimientoFinanciero? item);
		public bool EstaConfirmado(MovimientoFinanciero item);
		public bool PerteneceCuenta(MovimientoFinanciero item, long idCuentaFinanciera);
		public List<MovimientoFinanciero> FiltrarVigentes(List<MovimientoFinanciero> items);
		public List<MovimientoFinanciero> FiltrarConfirmados(List<MovimientoFinanciero> items);
		public Task<MovimientoFinanciero?> Obtener(long id, bool filtrarVigente = false, bool filtrarConfirmado = false, bool validarVigencia = false, long? validarIdCuentaFinanciera = null, NpgsqlTransaction? transaction = null);
		public Task<List<MovimientoFinanciero>> ObtenerConPaginacion(long idCuentaFinanciera, DateTime? fechaDesde = null, DateTime? fechaHasta = null, short estado = 1, bool? vigencia = true, DateTime? primeraFecha = null, long? primerId = null, int cantidad = 50, NpgsqlTransaction? transaction = null);
		public Task<List<MovimientoFinanciero>> ObtenerPrevios(long idCuentaFinanciera, DateTime fechaReferencia, long? idReferencia = null, short estado = 1, bool? vigencia = true, int cantidad = 1, NpgsqlTransaction? transaction = null);
		public Task<List<MovimientoFinanciero>> ObtenerPosteriores(long idCuentaFinanciera, DateTime fechaReferencia, long idReferencia, short estado = 1, bool? vigencia = true, bool incluirReferencia = false, NpgsqlTransaction? transaction = null);
		public Task<MovimientoFinanciero> Crear(long idCuentaFinanciera, long? idCategoriaFinanciera, short tipoMovimiento, decimal monto, decimal saldo, DateTime fechaMovimientoUtc, string? descripcion, short estado = 1, NpgsqlTransaction? transaction = null);
		public Task Actualizar(MovimientoFinanciero item, NpgsqlTransaction? transaction = null);
		public Task Eliminar(MovimientoFinanciero item, NpgsqlTransaction? transaction = null);
	}
}
