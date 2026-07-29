using Microsoft.AspNetCore.SignalR;
using Npgsql;
using Scriban.Runtime;
using System.Net;
using System.Transactions;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Hermes;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class DestinatarioNotificacionBcp(IVariableEntornoHelper variableEntorno, IDateTimeProvider dateTimeProvider, IHermesHelper hermesHelper, IHtmlRenderer renderer, IDestinatarioNotificacionDao destinatarioNotificacionDao) : IDestinatarioNotificacionBcp {
		public const short HORAS_CADUCIDAD_CODIGO_VALIDACION = 24;

        public bool EstaVigente(DestinatarioNotificacion? destinatarioNotificacion) {
            return destinatarioNotificacion != null && destinatarioNotificacion.Vigencia;
        }

		public bool EstaValidado(DestinatarioNotificacion destinatarioNotificacion) {
			return destinatarioNotificacion.Validado;
		}

		public bool Pertenece(DestinatarioNotificacion destinatarioNotificacion, string sub) {
			return destinatarioNotificacion.Sub == sub;
		}

		public bool PerteneceNegocio(DestinatarioNotificacion destinatarioNotificacion, long idNegocio) {
			return destinatarioNotificacion.IdNegocio == idNegocio;
		}

        public bool CodigoValidacionVigente(DestinatarioNotificacion destinatarioNotificacion) {
            return destinatarioNotificacion.FechaCaducidadCodigoValidacion >= dateTimeProvider.UtcNow;
        }

		public List<DestinatarioNotificacion> FiltrarVigentes(List<DestinatarioNotificacion> destinatarios) {
			return [.. destinatarios.Where(d => EstaVigente(d))];
		}

		public List<DestinatarioNotificacion> FiltrarValidados(List<DestinatarioNotificacion> destinatarios) {
			return [.. destinatarios.Where(d => EstaValidado(d))];
		}

		public async Task<string> GenerarCodigoValidacion(NpgsqlTransaction? transaction = null) {
            string codigoValidacion = CryptoHelper.GenerarToken();
            DestinatarioNotificacion? mismoCodigo = await ObtenerPorCodigoValidacion(codigoValidacion, transaction: transaction);
            while (mismoCodigo != null) {
                codigoValidacion = CryptoHelper.GenerarToken();
                mismoCodigo = await ObtenerPorCodigoValidacion(codigoValidacion, transaction: transaction);
            }
			return codigoValidacion;
        }

		public async Task<DestinatarioNotificacion?> Obtener(long idDestinatarioNotificacion, bool filtrarVigente = false, bool filtrarValidado = false, string? filtrarSub = null, long? filtrarIdNegocio = null, NpgsqlTransaction? transaction = null) {
			DestinatarioNotificacion? destinatario = await destinatarioNotificacionDao.ObtenerPorId(idDestinatarioNotificacion, transaction);
			if (filtrarVigente && !EstaVigente(destinatario)) return null;
			if (destinatario != null) {
				if (filtrarValidado && !EstaValidado(destinatario)) return null;
				if (filtrarSub != null && !Pertenece(destinatario, filtrarSub)) return null;
				if (filtrarIdNegocio != null && !PerteneceNegocio(destinatario, filtrarIdNegocio.Value)) return null;
			}
			return destinatario;
		}

		public async Task<DestinatarioNotificacion?> ObtenerPorCodigoValidacion(string codigoValidacion, bool validarVigencia = false, bool validarCodigoValidacionVigente = false, NpgsqlTransaction? transaction = null) {
			DestinatarioNotificacion? destinatario = await destinatarioNotificacionDao.ObtenerPorCodigoValidacion(CryptoHelper.HashSHA256(codigoValidacion), transaction);
			// Se aplican todas las validaciones...
			if (validarVigencia && !EstaVigente(destinatario)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El destinatario no existe o no está vigente", "Código ingresado no es válido.");
			if (destinatario != null) {
				if (validarCodigoValidacionVigente && !CodigoValidacionVigente(destinatario)) throw new ErrorValidacion(TipoErrorValidacion.AccesoCaducado, "El código de validación no está vigente", "Código ingresado no es válido");
			}
			return destinatario;
		}

        public async Task<List<DestinatarioNotificacion>> ObtenerPorSubYNegocio(string sub, long idNegocio, bool filtrarVigente = false, bool filtrarValidado = false, NpgsqlTransaction? transaction = null) {
			List<DestinatarioNotificacion> destinatarios = await destinatarioNotificacionDao.ObtenerPorSub(sub, idNegocio, null, transaction);
            if (filtrarVigente) destinatarios = FiltrarVigentes(destinatarios);
			if (filtrarValidado) destinatarios = FiltrarValidados(destinatarios);
			return destinatarios;
		}

		public async Task<(DestinatarioNotificacion nuevoDestinatario, string codigoValidacion)> Insertar(string sub, long idNegocio, long? idEmpleado, long idTipoReceptor, string? alias, string destino, bool yaValidado = false, NpgsqlTransaction? transaction = null) {
			string codigoValidacion = await GenerarCodigoValidacion(transaction);
			DateTime nowUtc = dateTimeProvider.UtcNow;

            DestinatarioNotificacion nuevoDestinatario = new() {
                Id = 0,
                Sub = sub,
                IdNegocio = idNegocio,
                IdEmpleado = idEmpleado,
                IdTipoReceptor = idTipoReceptor,
                Alias = alias,
                Destino = destino,
                CodigoValidacion = CryptoHelper.HashSHA256(codigoValidacion),
                FechaCaducidadCodigoValidacion = nowUtc.AddHours(HORAS_CADUCIDAD_CODIGO_VALIDACION),
                Validado = yaValidado,
                HermesIdMensaje = null,
                FechaValidacion = yaValidado ? nowUtc : null,
                FechaCreacion = nowUtc,
                Vigencia = true
            };
            nuevoDestinatario.Id = await destinatarioNotificacionDao.Insertar(nuevoDestinatario, transaction);
			return (nuevoDestinatario, codigoValidacion);
        }

		public async Task<string> EnviarCorreoValidacionDestinatario(string correoDestino, string nombreUsuario, string nombreNegocio, string codigoValidacion) {
            SalHermesEnviar retorno = await hermesHelper.EnviarCorreo(new EntHermesCorreoEnviar() {
                De = new DireccionCorreo() {
                    Nombre = variableEntorno.Obtener("HERMES_DE_NOMBRE"),
                    Correo = variableEntorno.Obtener("HERMES_DE_CORREO"),
                },
                Para = [
                    new DireccionCorreo() {
                        Correo = correoDestino
                    }
                ],
                Asunto = $"¡{nombreUsuario} te añadió como destinatario de notificaciones de {nombreNegocio}!",
                Cuerpo = await renderer.GenerarHtml("ValidacionDestinatario.html", new ScriptObject() {
                    ["NOMBRE_USUARIO"] = WebUtility.HtmlEncode(nombreUsuario),
                    ["NOMBRE_NEGOCIO"] = WebUtility.HtmlEncode(nombreNegocio),
                    ["CODIGO_VALIDACION"] = Uri.EscapeDataString(codigoValidacion)
                }),
            });

			return retorno.IdMensaje;
        }

		public async Task<string> EnviarWhatsappValidacionDestinatario(string whatsappDestino, string nombreUsuario, string nombreNegocio, string codigoValidacion) {
			SalHermesEnviar retorno = await hermesHelper.EnviarWhatsapp(new EntHermesWhatsappEnviar() {
				De = variableEntorno.Obtener("HERMES_DE_WHATSAPP"),
				Para = whatsappDestino,
				NombreTemplate = "validacion_destinatario",
				ParametrosCuerpo = [
					nombreUsuario ?? "",
					nombreNegocio
				],
				ParametrosBoton = [
					Uri.EscapeDataString(codigoValidacion)
				]
			});

			return retorno.IdMensaje;
        }
		
		public async Task RegistrarHermesIdMensaje(DestinatarioNotificacion destinatarioNotificacion, string hermesIdMensaje, NpgsqlTransaction? transaction = null) {
			if (destinatarioNotificacion.HermesIdMensaje == null) {
				destinatarioNotificacion.HermesIdMensaje = hermesIdMensaje;
				await destinatarioNotificacionDao.Actualizar(destinatarioNotificacion, transaction);
			} else if (destinatarioNotificacion.HermesIdMensaje != hermesIdMensaje) throw new InvalidOperationException("El destinatario ya tiene un ID de mensaje Hermes asignado");
		}

		public async Task Validar(DestinatarioNotificacion destinatarioNotificacion, NpgsqlTransaction? transaction = null) {
			if (!EstaValidado(destinatarioNotificacion)) {
				destinatarioNotificacion.Validado = true;
				destinatarioNotificacion.FechaValidacion = dateTimeProvider.UtcNow;
				await destinatarioNotificacionDao.Actualizar(destinatarioNotificacion, transaction);
			}
		}

		public async Task Eliminar(DestinatarioNotificacion destinatarioNotificacion, NpgsqlTransaction? transaction = null) {
			if (destinatarioNotificacion.Vigencia) {
				destinatarioNotificacion.FechaEliminacion = dateTimeProvider.UtcNow;
				destinatarioNotificacion.Vigencia = false;
				await destinatarioNotificacionDao.Actualizar(destinatarioNotificacion, transaction);
			}
		}
	}
}
