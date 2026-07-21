using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface ICategoriaNormaBcp {
		public Task<CategoriaNorma?> ObtenerPorId(long id);
		public Task<List<CategoriaNorma>> ObtenerVigentes();
		public Task<List<CategoriaNorma>> ObtenerPorVigencia(bool? vigencia);
		public Task<CategoriaNorma> Crear(long id, string nombre, string? nombreCorto, string? descripcion, bool vigencia);
		public Task Actualizar(CategoriaNorma categoriaNorma);
		public Task Eliminar(long id);
	}
}
