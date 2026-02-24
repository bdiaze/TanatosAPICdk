using Npgsql;
using System.Net;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class DestinatarioNotificacionBcp(IHostEnvironment environment, VariableEntornoHelper variableEntorno, CryptoHelper cryptoHelper, CognitoHelper cognitoHelper, HermesHelper hermesHelper, DestinatarioNotificacionDao destinatarioNotificacionDao, NormaSuscritaDao normaSuscritaDao, NegocioDao negocioDao, HistorialNormaSuscritaBcp historialNormaSuscritaBcp) {
		private const short HORAS_CADUCIDAD_CODIGO_VALIDACION = 24;

		public async Task<DestinatarioNotificacion> Crear(string sub, long idNegocio, long idTipoReceptor, string destino) {
			// Se crea un código de validación...
			string codigoValidacion = cryptoHelper.GenerarToken(12);
			DestinatarioNotificacion? mismoCodigo = await destinatarioNotificacionDao.ObtenerPorCodigoValidacion(cryptoHelper.HashSHA256(codigoValidacion));
			while (mismoCodigo != null) {
				codigoValidacion = cryptoHelper.GenerarToken(12);
				mismoCodigo = await destinatarioNotificacionDao.ObtenerPorCodigoValidacion(cryptoHelper.HashSHA256(codigoValidacion));
			}

			DestinatarioNotificacion nuevoDestinatario = new() {
				Id = 0,
				Sub = sub,
				IdNegocio = idNegocio,
				IdTipoReceptor = idTipoReceptor,
				Destino = destino,
				CodigoValidacion = cryptoHelper.HashSHA256(codigoValidacion),
				FechaCaducidadCodigoValidacion = DateTime.UtcNow.AddHours(HORAS_CADUCIDAD_CODIGO_VALIDACION),
				Validado = false,
				FechaCreacion = DateTime.UtcNow,
				Vigencia = true
			};
			nuevoDestinatario.Id = await destinatarioNotificacionDao.Insertar(nuevoDestinatario);

			// Se envía mensaje con el código de validación...
			if (nuevoDestinatario.IdTipoReceptor == 1 /* Correo electrónico */) {
				Negocio negocio = (await negocioDao.ObtenerPorSub(sub)).FirstOrDefault(n => n.Id == idNegocio) ?? throw new Exception("ID de negocio no válido");
				Dictionary<string, string> atributosUsuario = await cognitoHelper.ObtenerUsuario(sub);

				string strTemplateCorreo;
				if (environment.IsProduction()) {
					strTemplateCorreo = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "TemplatesCorreos", "ValidacionDestinatario.html"));
				} else {
					strTemplateCorreo = await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "TemplatesCorreos", "ValidacionDestinatario.html"));
				}

				await hermesHelper.EnviarCorreo(new EntHermesCorreoEnviar() {
					De = new DireccionCorreo() {
						Nombre = variableEntorno.Obtener("HERMES_DE_NOMBRE"),
						Correo = variableEntorno.Obtener("HERMES_DE_CORREO"),
					},
					Para = [
						new DireccionCorreo() {
								Correo = nuevoDestinatario.Destino
							}
					],
					Asunto = "¡[NOMBRE_USUARIO] te añadió como destinatario de notificaciones de [NOMBRE_NEGOCIO]!"
								.Replace("[NOMBRE_USUARIO]", atributosUsuario["given_name"])
								.Replace("[NOMBRE_NEGOCIO]", negocio.Nombre),
					Cuerpo = strTemplateCorreo
								.Replace("[NOMBRE_USUARIO]", WebUtility.HtmlEncode(atributosUsuario["given_name"]))
								.Replace("[NOMBRE_NEGOCIO]", WebUtility.HtmlEncode(negocio.Nombre))
								.Replace("[CODIGO_VALIDACION]", WebUtility.UrlEncode(codigoValidacion)),
				});
			} else if (nuevoDestinatario.IdTipoReceptor == 2 /* Whatsapp */) {
				Negocio negocio = (await negocioDao.ObtenerPorSub(sub)).FirstOrDefault(n => n.Id == idNegocio) ?? throw new Exception("ID de negocio no válido");
				Dictionary<string, string> atributosUsuario = await cognitoHelper.ObtenerUsuario(sub);

				await hermesHelper.EnviarWhatsapp(new EntHermesWhatsappEnviar() {
					De = variableEntorno.Obtener("HERMES_DE_WHATSAPP"),
					Para = nuevoDestinatario.Destino,
					NombreTemplate = "validacion_destinatario",
					ParametrosCuerpo = [
						atributosUsuario["given_name"],
						negocio.Nombre
					],
				});
			}

			return nuevoDestinatario;
		}
		
		public async Task Validar(DestinatarioNotificacion destinatarioNotificacion, NpgsqlTransaction? transaction = null) {
			if (!destinatarioNotificacion.Validado) {
				destinatarioNotificacion.Validado = true;
				destinatarioNotificacion.FechaValidacion = DateTime.UtcNow;
				await destinatarioNotificacionDao.Actualizar(destinatarioNotificacion, transaction);

				List<NormaSuscrita> normasSuscritas = [.. (await normaSuscritaDao.ObtenerPorSub(destinatarioNotificacion.Sub, destinatarioNotificacion.IdNegocio, true, transaction)).Where(ns => ns.Activado)];
				foreach (NormaSuscrita normaSuscrita in normasSuscritas) {
					await historialNormaSuscritaBcp.ActualizarHistorialNotificacionPorNormaSuscrita(normaSuscrita, transaction);
				}
			}
		}

		public async Task Eliminar(DestinatarioNotificacion destinatarioNotificacion, NpgsqlTransaction? transaction = null) {
			if (destinatarioNotificacion.Vigencia) {
				destinatarioNotificacion.FechaEliminacion = DateTime.UtcNow;
				destinatarioNotificacion.Vigencia = false;
				await destinatarioNotificacionDao.Actualizar(destinatarioNotificacion, transaction);

				List<NormaSuscrita> normasSuscritas = [.. (await normaSuscritaDao.ObtenerPorSub(destinatarioNotificacion.Sub, destinatarioNotificacion.IdNegocio, true, transaction)).Where(ns => ns.Activado)];
				foreach (NormaSuscrita normaSuscrita in normasSuscritas) {
					await historialNormaSuscritaBcp.ActualizarHistorialNotificacionPorNormaSuscrita(normaSuscrita, transaction);
				}
			}
		}
	}
}
