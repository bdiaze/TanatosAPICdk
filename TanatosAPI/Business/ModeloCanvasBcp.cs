using Npgsql;
using Org.BouncyCastle.Crypto.Digests;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class ModeloCanvasBcp(IDateTimeProvider dateTimeProvider, IModeloCanvasDao modeloCanvasDao) : IModeloCanvasBcp {
		public bool EstaVigente(ModeloCanvas? item) {
			return item != null && item.Vigencia;
		}

		public bool Pertenece(ModeloCanvas item, string sub) {
			return item.Sub == sub;
		}

		public bool PerteneceNegocio(ModeloCanvas item, long idNegocio) {
			return item.IdNegocio == idNegocio;
		}

		public List<ModeloCanvas> FiltrarVigentes(List<ModeloCanvas> items) {
			return [.. items.Where(i => EstaVigente(i))];
		}

		public async Task<ModeloCanvas?> Obtener(long idModeloCanvas, bool filtrarVigente = false, bool validarVigencia = false, string? validarSub = null, long? validarIdNegocio = null, NpgsqlTransaction? transaction = null) {
			ModeloCanvas? item = await modeloCanvasDao.Obtener(idModeloCanvas, transaction);
			// Se aplican todas las validaciones...
			if (validarVigencia && !EstaVigente(item)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El modelo Canvas no existe o no está vigente", "El modelo Canvas es inválido.");
			if (item != null) {
				if (validarSub != null && !Pertenece(item, validarSub)) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El modelo Canvas no pertenece al usuario", "El modelo Canvas es inválido.");
				if (validarIdNegocio != null && !PerteneceNegocio(item, validarIdNegocio.Value)) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El modelo Canvas no pertenece al negocio", "El modelo Canvas es inválido.");
			}

			// Se aplican los filtros...
			if (filtrarVigente && !EstaVigente(item)) return null;

			return item;
		}

		public async Task<List<ModeloCanvas>> ObtenerPorSubYNegocio(string sub, long idNegocio, bool filtrarVigentes = false, NpgsqlTransaction? transaction = null) {
			List<ModeloCanvas> items = await modeloCanvasDao.ObtenerPorSub(sub, idNegocio, transaction);
			if (filtrarVigentes) items = FiltrarVigentes(items);
			return items;
		}

		public async Task<ModeloCanvas> Insertar(string sub, long idNegocio, string? sociosClave, string? actividadesClave, string? recursosClave, string? propuestaValor, string? relacionesClientes, string? canales, string? segmentosClientes, string? estructuraCostos, string? fuentesIngresos, NpgsqlTransaction? transaction = null) {
			ModeloCanvas nuevo = new() { 
				Id = 0,
				Sub = sub,
				IdNegocio = idNegocio,
				SociosClave = sociosClave,
				ActividadesClave = actividadesClave,
				RecursosClave = recursosClave,
				PropuestaValor = propuestaValor,
				RelacionesClientes = relacionesClientes,
				Canales = canales,
				SegmentosClientes = segmentosClientes,
				EstructuraCostos = estructuraCostos,
				FuentesIngresos = fuentesIngresos,
				FechaCreacion = dateTimeProvider.UtcNow,
				FechaEliminacion = null,
				Vigencia = true
			};
			nuevo.Id = await modeloCanvasDao.Insertar(nuevo, transaction);
			return nuevo;
		}

		public async Task Modificar(ModeloCanvas modeloCanvas, NpgsqlTransaction? transaction = null) {
			await modeloCanvasDao.Actualizar(modeloCanvas, transaction);
		}

		public async Task Eliminar(ModeloCanvas modeloCanvas, NpgsqlTransaction? transaction = null) {
			if (modeloCanvas.Vigencia) {
				modeloCanvas.FechaEliminacion = dateTimeProvider.UtcNow;
				modeloCanvas.Vigencia = false;
				await modeloCanvasDao.Actualizar(modeloCanvas, transaction);
			}
		}
	}
}
