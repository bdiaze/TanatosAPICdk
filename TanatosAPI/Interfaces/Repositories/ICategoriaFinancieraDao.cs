using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface ICategoriaFinancieraDao {
		public Task<CategoriaFinanciera?> Obtener(long id, NpgsqlTransaction? transaction = null);
		public Task<List<CategoriaFinanciera>> ObtenerPorSub(string sub, long? idNegocio = null, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(CategoriaFinanciera item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(CategoriaFinanciera item, NpgsqlTransaction? transaction = null);
	}
}
