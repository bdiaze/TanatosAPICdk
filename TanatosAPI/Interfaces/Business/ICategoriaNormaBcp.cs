using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface ICategoriaNormaBcp {
		public bool EstaVigente(CategoriaNorma? categoria);
        public Task<CategoriaNorma?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null);
		public Task<CategoriaNorma> ObtenerValidandoVigencia(long? id, NpgsqlTransaction? transaction = null);
        public Task<List<CategoriaNorma>> ObtenerVigentes(NpgsqlTransaction? transaction = null);
		public Task<List<CategoriaNorma>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null);
		public Task<CategoriaNorma> Crear(long id, string nombre, string? nombreCorto, string? descripcion, bool vigencia, NpgsqlTransaction? transaction = null);
		public Task Actualizar(CategoriaNorma categoriaNorma, NpgsqlTransaction? transaction = null);
		public Task Eliminar(long id, NpgsqlTransaction? transaction = null);
	}
}
