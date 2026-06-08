using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces {
	public interface ICategoriaNormaDao {
		public Task<CategoriaNorma?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null);

		public Task<List<CategoriaNorma>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null);

		public Task Insertar(CategoriaNorma item, NpgsqlTransaction? transaction = null);

		public Task Actualizar(CategoriaNorma item, NpgsqlTransaction? transaction = null);

		public Task Eliminar(long id, NpgsqlTransaction? transaction = null);
	}
}
