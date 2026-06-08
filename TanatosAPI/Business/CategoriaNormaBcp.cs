using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class CategoriaNormaBcp(CategoriaNormaDao categoriaNormaDao) {
		public async Task<CategoriaNorma?> ObtenerPorId(long id) {
			return await categoriaNormaDao.ObtenerPorId(id);
		}

		public async Task<List<CategoriaNorma>> ObtenerVigentes() {
			return await categoriaNormaDao.ObtenerPorVigencia(true);
		}

		public async Task<List<CategoriaNorma>> ObtenerPorVigencia(bool? vigencia) {
			return await categoriaNormaDao.ObtenerPorVigencia(vigencia);
		}

		public async Task<CategoriaNorma> RegistrarCategoria(long id, string nombre, string? nombreCorto, string? descripcion, bool vigencia) {
			CategoriaNorma nuevo = new() { 
				Id = id,
				Nombre = nombre,
				NombreCorto = nombreCorto,
				Descripcion = descripcion,
				Vigencia = vigencia
			};
			await categoriaNormaDao.Insertar(nuevo);
			return nuevo;
		}

		public async Task ActualizarCategoria(CategoriaNorma categoriaNorma) {
			await categoriaNormaDao.Actualizar(categoriaNorma);
		}

		public async Task EliminarCategoria(long id) {
			await categoriaNormaDao.Eliminar(id);
		}
	}
}
