using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class AccesoBcp(IDateTimeProvider dateTimeProvider, IAccesoDao accesoDao) : IAccesoBcp {

		public bool EstaVigente(Acceso? item) {
			return item != null && item.Vigencia;
		}

		public bool EstaExpirado(Acceso item) {
			return item.FechaExpiracion != null && item.FechaExpiracion <= dateTimeProvider.UtcNow;
		}

		public async Task<Acceso?> ObtenerPorCodigo(string codigo, bool validarVigencia = false, bool validarExpiracion = false, NpgsqlTransaction? transaction = null) {
			Acceso? item = await accesoDao.ObtenerPorHashCodigo(CryptoHelper.HashSHA256(codigo), transaction);
			if (validarVigencia && !EstaVigente(item)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El acceso no existe o no está vigente", "El código de acceso es inválido.");
			if (item != null) {
				if (validarExpiracion && EstaExpirado(item)) throw new ErrorValidacion(TipoErrorValidacion.AccesoCaducado, "El acceso está caducado", "El código de acceso es inválido.");
			}

			return item;
		}

		public async Task<(Acceso, string Codigo)> Crear(TimeSpan? duracion = null, NpgsqlTransaction? transaction = null) {
			string codigo;
			Acceso? existente;
			do {
				codigo = CryptoHelper.GenerarToken();
				existente = await ObtenerPorCodigo(codigo, transaction: transaction);
			} while (existente != null);

			DateTime now = dateTimeProvider.UtcNow;
			Acceso nuevo = new() {
				Id = 0,
				HashCodigo = CryptoHelper.HashSHA256(codigo),
				FechaExpiracion = duracion != null ? now.Add(duracion.Value) : null,
				FechaCreacion = now,
				FechaEliminacion = null,
				Vigencia = true
			};
			nuevo.Id = await accesoDao.Insertar(nuevo, transaction);
			return (nuevo, codigo);
		}

		public async Task Eliminar(Acceso item, NpgsqlTransaction? transaction = null) {
			if (item.Vigencia) {
				item.FechaEliminacion = dateTimeProvider.UtcNow;
				item.Vigencia = false;
				await accesoDao.Actualizar(item, transaction);
			}
		}

	}
}
