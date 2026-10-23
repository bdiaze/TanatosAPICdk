using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
	public class CategoriaFinancieraBcp(IDateTimeProvider dateTimeProvider, ICategoriaFinancieraDao categoriaFinancieraDao) : ICategoriaFinancieraBcp {
		public bool EstaVigente(CategoriaFinanciera? item) {
			return item != null && item.Vigencia;
		}

		public bool Pertenece(CategoriaFinanciera item, string sub) {
			return item.Sub == sub;
		}

		public bool PerteneceNegocio(CategoriaFinanciera item, long idNegocio) {
			return item.IdNegocio == idNegocio;
		}

		public List<CategoriaFinanciera> FiltrarVigentes(List<CategoriaFinanciera> items) {
			return [.. items.Where(c => EstaVigente(c))];
		}

		public async Task<CategoriaFinanciera?> Obtener(long id, bool validarVigencia = false, string? validarSub = null, long? validarIdNegocio = null, NpgsqlTransaction? transaction = null) {
			CategoriaFinanciera? item = await categoriaFinancieraDao.Obtener(id, transaction);
			if (validarVigencia && !EstaVigente(item)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "La categoría financiera no existe o no está vigente", "La categoría es inválida.");
			if (item != null) {
				if (validarSub != null && !Pertenece(item, validarSub)) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "La categoría financiera no pertenece al usuario", "La categoría es inválida.");
				if (validarIdNegocio != null && !PerteneceNegocio(item, validarIdNegocio.Value)) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "La categoría financiera no pertenece al negocio", "La categoría es inválida.");
			}
			return item;
		}

		public async Task<List<CategoriaFinanciera>> ObtenerPorSub(string sub, long? idNegocio = null, bool filtrarVigentes = false, NpgsqlTransaction? transaction = null) {
			List<CategoriaFinanciera> items = await categoriaFinancieraDao.ObtenerPorSub(sub, idNegocio, transaction);
			if (filtrarVigentes) items = FiltrarVigentes(items);
			return items;
		}

		public async Task<CategoriaFinanciera> Crear(string sub, long idNegocio, string nombre, short tipoMovimiento, NpgsqlTransaction? transaction = null) {
			CategoriaFinanciera nuevo = new() {
				Id = 0,
				Sub = sub,
				IdNegocio = idNegocio,
				Nombre = nombre,
				TipoMovimiento = tipoMovimiento,
				FechaCreacion = dateTimeProvider.UtcNow,
				FechaEliminacion = null,
				Vigencia = true
			};
			nuevo.Id = await categoriaFinancieraDao.Insertar(nuevo, transaction);
			return nuevo;
		}

		public async Task Actualizar(CategoriaFinanciera item, NpgsqlTransaction? transaction = null) {
			await categoriaFinancieraDao.Actualizar(item, transaction);
		}

		public async Task Eliminar(CategoriaFinanciera item, NpgsqlTransaction? transaction = null) {
			if (EstaVigente(item)) {
				item.FechaEliminacion = dateTimeProvider.UtcNow;
				item.Vigencia = false;
				await categoriaFinancieraDao.Actualizar(item, transaction);
			}
		}
	}
}
