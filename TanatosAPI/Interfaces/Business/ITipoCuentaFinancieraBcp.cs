using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface ITipoCuentaFinancieraBcp {
		public Task<TipoCuentaFinanciera?> Obtener(long id, bool validarVigencia = false, NpgsqlTransaction? transaction = null);
		public Task<List<TipoCuentaFinanciera>> ObtenerTodos(bool filtrarVigentes = false, NpgsqlTransaction? transaction = null);
		public Task<TipoCuentaFinanciera> Crear(long id, string nombre, bool vigencia, NpgsqlTransaction? transaction = null);
		public Task Actualizar(TipoCuentaFinanciera item, NpgsqlTransaction? transaction = null);
		public Task Eliminar(long id, NpgsqlTransaction? transaction = null);
	}
}
