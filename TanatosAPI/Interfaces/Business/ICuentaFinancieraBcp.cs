using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface ICuentaFinancieraBcp {
		public bool EstaVigente(CuentaFinanciera? item);
		public bool Pertenece(CuentaFinanciera item, string sub);
		public bool PerteneceNegocio(CuentaFinanciera item, long idNegocio);
		public List<CuentaFinanciera> FiltrarVigentes(List<CuentaFinanciera> items);
		public Task<CuentaFinanciera?> Obtener(long id, bool validarVigencia = false, string? validarSub = null, long? validarIdNegocio = null, bool forUpdate = false, NpgsqlTransaction? transaction = null);
		public Task<List<CuentaFinanciera>> ObtenerPorSub(string sub, long? idNegocio = null, bool filtrarVigentes = false, NpgsqlTransaction? transaction = null);
		public Task<CuentaFinanciera> Crear(string sub, long idNegocio, string nombre, long idTipoCuentaFinanciera, decimal saldoInicial, DateTime fechaSaldoInicialUtc, NpgsqlTransaction? transaction = null);
		public Task Actualizar(CuentaFinanciera item, NpgsqlTransaction? transaction = null);
		public Task Eliminar(CuentaFinanciera item, NpgsqlTransaction? transaction = null);
	}
}
