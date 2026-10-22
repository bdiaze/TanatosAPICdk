using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
	public class TipoCuentaFinancieraBcp(ITipoCuentaFinancieraDao tipoCuentaFinancieraDao) : ITipoCuentaFinancieraBcp {
		public bool EstaVigente(TipoCuentaFinanciera? item) {
			return item != null && item.Vigencia;
		}

		public List<TipoCuentaFinanciera> FiltrarVigentes(List<TipoCuentaFinanciera> items) {
			return [.. items.Where(c => EstaVigente(c))];
		}

		public async Task<TipoCuentaFinanciera?> Obtener(long id, bool validarVigencia = false, NpgsqlTransaction? transaction = null) {
			TipoCuentaFinanciera? item = await tipoCuentaFinancieraDao.Obtener(id, transaction);
			if (validarVigencia && !EstaVigente(item)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El tipo de cuenta financiera no existe o no está vigente", "El tipo de cuenta es inválido.");
			return item;
		}

		public async Task<List<TipoCuentaFinanciera>> ObtenerTodos(bool filtrarVigentes = false, NpgsqlTransaction? transaction = null) {
			List<TipoCuentaFinanciera> items = await tipoCuentaFinancieraDao.ObtenerTodos(transaction);
			if (filtrarVigentes) items = FiltrarVigentes(items);
			return items;
		}

		public async Task<TipoCuentaFinanciera> Crear(long id, string nombre, bool vigencia, NpgsqlTransaction? transaction = null) {
			TipoCuentaFinanciera nuevo = new() {
				Id = id,
				Nombre = nombre,
				Vigencia = vigencia
			};
			await tipoCuentaFinancieraDao.Insertar(nuevo, transaction);
			return nuevo;
		}

		public async Task Actualizar(TipoCuentaFinanciera item, NpgsqlTransaction? transaction = null) {
			await tipoCuentaFinancieraDao.Actualizar(item, transaction);
		}

		public async Task Eliminar(long id, NpgsqlTransaction? transaction = null) {
			await tipoCuentaFinancieraDao.Eliminar(id, transaction);
		}
	}
}
