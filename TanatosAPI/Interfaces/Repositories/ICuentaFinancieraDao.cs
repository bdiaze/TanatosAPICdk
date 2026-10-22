using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface ICuentaFinancieraDao {
		public Task<CuentaFinanciera?> Obtener(long id, NpgsqlTransaction? transaction = null);
		public Task<List<CuentaFinanciera>> ObtenerPorSub(string sub, long? idNegocio = null, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(CuentaFinanciera item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(CuentaFinanciera item, NpgsqlTransaction? transaction = null);
	}
}
