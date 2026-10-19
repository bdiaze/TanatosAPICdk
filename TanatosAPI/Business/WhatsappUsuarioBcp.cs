using Microsoft.AspNetCore.Components.RenderTree;
using Npgsql;
using Scriban.Runtime;
using System.Net;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Hermes;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
	public class WhatsappUsuarioBcp(IDateTimeProvider dateTimeProvider, IVariableEntornoHelper variableEntorno, IWhatsappUsuarioDao whatsappUsuarioDao, IHermesHelper hermesHelper) : IWhatsappUsuarioBcp {
		public bool EstaVigente(WhatsappUsuario? item) {
			return item != null && item.Vigencia;
		}

		public bool FueValidado(WhatsappUsuario item) {
			return item.Validado;
		}

		public List<WhatsappUsuario> FiltrarVigentes(List<WhatsappUsuario> items) {
			return [.. items.Where(c => EstaVigente(c))];
		}

		public List<WhatsappUsuario> FiltrarValidados(List<WhatsappUsuario> items) {
			return [.. items.Where(c => FueValidado(c))];
		}

		public async Task<WhatsappUsuario?> Obtener(long id, bool validarVigencia = false, bool validarValidacion = false, NpgsqlTransaction? transaction = null) {
			WhatsappUsuario? item = await whatsappUsuarioDao.Obtener(id, transaction);
			if (validarVigencia && !EstaVigente(item)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El número de Whatsapp no existe o no está vigente", "El número de Whatsapp es inválido.");
			if (item != null) {
				if (validarValidacion && !FueValidado(item)) throw new ErrorValidacion(TipoErrorValidacion.EstadoNoValido, "El número de Whatsapp no está validado", "El número de Whatsapp es inválido.");
			}
			return item;
		}

		public async Task<List<WhatsappUsuario>> ObtenerPorSub(string sub, bool filtrarVigentes = false, bool filtrarValidados = false, NpgsqlTransaction? transaction = null) {
			List<WhatsappUsuario> items = await whatsappUsuarioDao.ObtenerPorSub(sub, transaction);
			if (filtrarVigentes) items = FiltrarVigentes(items);
			if (filtrarValidados) items = FiltrarValidados(items);
			return items;
		}

		public async Task<WhatsappUsuario> Crear(string sub, string whatsapp, NpgsqlTransaction? transaction = null) {
			WhatsappUsuario nuevo = new() {
				Id = 0,
				Sub = sub,
				Whatsapp = whatsapp,
				IdAccesoValidacion = null,
				FechaValidacion = null,
				Validado = false,
				HermesIdMensaje = null,
				FechaCreacion = dateTimeProvider.UtcNow,
				FechaEliminacion = null,
				Vigencia = true,
			};

			nuevo.Id = await whatsappUsuarioDao.Insertar(nuevo, transaction);
			return nuevo;
		}

		public async Task Actualizar(WhatsappUsuario item, NpgsqlTransaction? transaction = null) {
			await whatsappUsuarioDao.Actualizar(item, transaction);
		}

		public async Task Validar(WhatsappUsuario item, NpgsqlTransaction? transaction = null) {
			if (!item.Validado) {
				item.FechaValidacion = dateTimeProvider.UtcNow;
				item.Validado = true;
				await whatsappUsuarioDao.Actualizar(item, transaction);
			}
		}

		public async Task Eliminar(WhatsappUsuario item, NpgsqlTransaction? transaction = null) {
			if (item.Vigencia) {
				item.FechaEliminacion = dateTimeProvider.UtcNow;
				item.Vigencia = false;
				await whatsappUsuarioDao.Actualizar(item, transaction);
			}
		}

		public async Task<string> EnviarWhatsappValidacion(string codigoAcceso, string numeroWhatsapp, string nombreUsuario, long idWhatsappUsuario) {
			SalHermesEnviar retorno = await hermesHelper.EnviarWhatsapp(new EntHermesWhatsappEnviar() {
				De = variableEntorno.Obtener("HERMES_DE_WHATSAPP"),
				Para = numeroWhatsapp,
				NombreTemplate = "validar_whatsapp_usuario",
				ParametrosCuerpo = [
					nombreUsuario
				],
				ParametrosBoton = [
					$"{idWhatsappUsuario}?codigo={Uri.EscapeDataString(codigoAcceso)}"
				]
			});

			return retorno.IdMensaje;
		}
	}
}
