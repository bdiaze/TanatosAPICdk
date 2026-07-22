using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
	public class CategoriaNormaBcp(ICategoriaNormaDao categoriaNormaDao) : ICategoriaNormaBcp {
		public bool EstaVigente(CategoriaNorma? categoria) {
			return categoria != null && categoria.Vigencia;
		}

        public async Task<CategoriaNorma?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null) {
			return await categoriaNormaDao.ObtenerPorId(id, transaction);
		}

        public async Task<CategoriaNorma> ObtenerValidandoVigencia(long? id, NpgsqlTransaction? transaction = null) {
            if (id == null) throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "ID de la categoría es inválido.");
            CategoriaNorma? categoria = await ObtenerPorId(id.Value, transaction);
            if (!EstaVigente(categoria)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "La categoría no está vigente.");
            return categoria!;
        }

        public async Task<List<CategoriaNorma>> ObtenerVigentes(NpgsqlTransaction? transaction = null) {
			return await categoriaNormaDao.ObtenerPorVigencia(true, transaction);
		}

		public async Task<List<CategoriaNorma>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null) {
			return await categoriaNormaDao.ObtenerPorVigencia(vigencia, transaction);
		}
		
        public async Task<CategoriaNorma> Crear(long id, string nombre, string? nombreCorto, string? descripcion, bool vigencia, NpgsqlTransaction? transaction = null) {
			CategoriaNorma nuevo = new() { 
				Id = id,
				Nombre = nombre,
				NombreCorto = nombreCorto,
				Descripcion = descripcion,
				Vigencia = vigencia
			};
			await categoriaNormaDao.Insertar(nuevo, transaction);
			return nuevo;
		}

		public async Task Actualizar(CategoriaNorma categoriaNorma, NpgsqlTransaction? transaction = null) {
			await categoriaNormaDao.Actualizar(categoriaNorma, transaction);
		}

		public async Task Eliminar(long id, NpgsqlTransaction? transaction = null) {
			await categoriaNormaDao.Eliminar(id, transaction);
		}
	}
}
