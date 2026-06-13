using Npgsql;
using Scriban.Runtime;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class DestinatarioNotificacionBcp(IVariableEntornoHelper variableEntorno, IDateTimeProvider dateTimeProvider, CryptoHelper cryptoHelper, HermesHelper hermesHelper, HtmlRenderer renderer, DestinatarioNotificacionDao destinatarioNotificacionDao) {
		public const short HORAS_CADUCIDAD_CODIGO_VALIDACION = 24;

        public bool EstaVigente(DestinatarioNotificacion? destinatarioNotificacion) {
            return destinatarioNotificacion != null && destinatarioNotificacion.Vigencia;
        }

		public bool EstaValidado(DestinatarioNotificacion destinatarioNotificacion) {
			return destinatarioNotificacion.Validado;
		}

        public bool CodigoValidacionVigente(DestinatarioNotificacion destinatarioNotificacion) {
            return destinatarioNotificacion.FechaCaducidadCodigoValidacion >= dateTimeProvider.UtcNow;
        }

        public async Task<string> GenerarCodigoValidacion(NpgsqlTransaction? transaction = null) {
            string codigoValidacion = cryptoHelper.GenerarToken();
            DestinatarioNotificacion? mismoCodigo = await ObtenerPorCodigoValidacion(codigoValidacion, transaction);
            while (mismoCodigo != null) {
                codigoValidacion = cryptoHelper.GenerarToken();
                mismoCodigo = await ObtenerPorCodigoValidacion(codigoValidacion, transaction);
            }
			return codigoValidacion;
        }

		public async Task<DestinatarioNotificacion?> ObtenerPorCodigoValidacion(string codigoValidacion, NpgsqlTransaction? transaction = null) {
			return await destinatarioNotificacionDao.ObtenerPorCodigoValidacion(cryptoHelper.HashSHA256(codigoValidacion), transaction);
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
                CodigoValidacion = cryptoHelper.HashSHA256(codigoValidacion),
                FechaCaducidadCodigoValidacion = nowUtc.AddHours(HORAS_CADUCIDAD_CODIGO_VALIDACION),
                Validado = yaValidado,
                FechaValidacion = yaValidado ? nowUtc : null,
                FechaCreacion = nowUtc,
                Vigencia = true
            };
            nuevoDestinatario.Id = await destinatarioNotificacionDao.Insertar(nuevoDestinatario, transaction);
			return (nuevoDestinatario, codigoValidacion);
        }

		public async Task RegistrarHermesIdMensaje(DestinatarioNotificacion destinatarioNotificacion, string hermesIdMensaje, NpgsqlTransaction? transaction = null) {
            destinatarioNotificacion.HermesIdMensaje = hermesIdMensaje;
            await destinatarioNotificacionDao.Actualizar(destinatarioNotificacion, transaction);
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
                    ["NOMBRE_USUARIO"] = nombreUsuario,
                    ["NOMBRE_NEGOCIO"] = nombreNegocio,
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
        				
		public async Task Validar(DestinatarioNotificacion destinatarioNotificacion, NpgsqlTransaction? transaction = null) {
			if (!destinatarioNotificacion.Validado) {
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
