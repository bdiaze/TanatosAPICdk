using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface ICategoriaNormaBcp {
		public Task<CategoriaNorma?> ObtenerPorId(long id);
		public Task<List<CategoriaNorma>> ObtenerVigentes();
		public Task<List<CategoriaNorma>> ObtenerPorVigencia(bool? vigencia);
		public Task<CategoriaNorma> RegistrarCategoria(long id, string nombre, string? nombreCorto, string? descripcion, bool vigencia);
		public Task ActualizarCategoria(CategoriaNorma categoriaNorma);
		public Task EliminarCategoria(long id);
	}
}
