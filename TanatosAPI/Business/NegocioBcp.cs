using Amazon.CognitoIdentityProvider.Model.Internal.MarshallTransformations;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class NegocioBcp(INegocioDao negocioDao) : INegocioBcp {
		public bool EstaVigente(Negocio? negocio) {
			return negocio != null && negocio.Vigencia;
		}

		public bool Pertenece(Negocio negocio, string sub) {
			return negocio.Sub == sub;
		}
		
		public List<Negocio> FiltrarVigentes(List<Negocio> negocios) {
			return [.. negocios.Where(n => EstaVigente(n))];
		}

		public async Task<Negocio?> Obtener(long idNegocio, bool filtrarVigente = false, bool validarVigencia = false, string? validarSub = null, NpgsqlTransaction? transaction = null) {
			Negocio? negocio = await negocioDao.Obtener(idNegocio, transaction);
			// Se aplican todas las validaciones...
			if (validarVigencia && !EstaVigente(negocio)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El negocio no existe o no está vigente", "El negocio es inválido.");
			if (negocio != null) {
				if (validarSub != null && !Pertenece(negocio, validarSub)) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El negocio no pertenece al usuario", "El negocio es inválido.");
			}

			// Se aplican los filtros...
			if (filtrarVigente && !EstaVigente(negocio)) return null;

			return negocio;
		}

		public async Task<List<Negocio>> ObtenerPorSub(string sub, bool filtrarVigentes = false, NpgsqlTransaction? transaction = null) {
			List<Negocio> negocios = await negocioDao.ObtenerPorSub(sub, null, transaction);
			if (filtrarVigentes) negocios = FiltrarVigentes(negocios);
			return negocios;
		}

		public async Task<Negocio?> ObtenerPrimerNegocio(string sub, NpgsqlTransaction? transaction = null) {
			List<Negocio> negocios = await ObtenerPorSub(sub, filtrarVigentes: true, transaction: transaction);
			return negocios.OrderBy(n => n.FechaCreacion).FirstOrDefault();
		}

		public async Task Actualizar(Negocio negocio, NpgsqlTransaction? transaction = null) {
			await negocioDao.Actualizar(negocio, transaction);
		}
	}
}
