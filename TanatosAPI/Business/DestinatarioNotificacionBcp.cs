using Npgsql;
using System.Net;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class DestinatarioNotificacionBcp(IHostEnvironment environment, VariableEntornoHelper variableEntorno, CryptoHelper cryptoHelper, HermesHelper hermesHelper, UsuarioBcp usuarioBcp, DestinatarioNotificacionDao destinatarioNotificacionDao, NegocioDao negocioDao) {
		public const short HORAS_CADUCIDAD_CODIGO_VALIDACION = 24;

		public async Task<DestinatarioNotificacion> Crear(string sub, long idNegocio, long? idEmpleado, long idTipoReceptor, string? alias, string destino, bool yaValidado = false, NpgsqlTransaction? transaction = null) {
			// Se crea un código de validación...
			string codigoValidacion = cryptoHelper.GenerarToken();
			DestinatarioNotificacion? mismoCodigo = await destinatarioNotificacionDao.ObtenerPorCodigoValidacion(cryptoHelper.HashSHA256(codigoValidacion), transaction);
			while (mismoCodigo != null) {
				codigoValidacion = cryptoHelper.GenerarToken();
				mismoCodigo = await destinatarioNotificacionDao.ObtenerPorCodigoValidacion(cryptoHelper.HashSHA256(codigoValidacion), transaction);
			}

			DestinatarioNotificacion nuevoDestinatario = new() {
				Id = 0,
				Sub = sub,
				IdNegocio = idNegocio,
				IdEmpleado = idEmpleado,
				IdTipoReceptor = idTipoReceptor,
				Alias = alias,
				Destino = destino,
				CodigoValidacion = cryptoHelper.HashSHA256(codigoValidacion),
				FechaCaducidadCodigoValidacion = DateTime.UtcNow.AddHours(HORAS_CADUCIDAD_CODIGO_VALIDACION),
				Validado = yaValidado,
				FechaValidacion = yaValidado ? DateTime.UtcNow : null,
				FechaCreacion = DateTime.UtcNow,
				Vigencia = true
			};
			nuevoDestinatario.Id = await destinatarioNotificacionDao.Insertar(nuevoDestinatario, transaction);

			if (yaValidado) return nuevoDestinatario;

			// Se envía mensaje con el código de validación...
			if (nuevoDestinatario.IdTipoReceptor == 1 /* Correo electrónico */) {
				Negocio negocio = (await negocioDao.ObtenerPorSub(sub, true, transaction)).FirstOrDefault(n => n.Id == idNegocio) ?? throw new Exception("ID de negocio no válido");
				Usuario usuario = await usuarioBcp.ObtenerInformacionUsuario(sub, transaction);

				string strTemplateCorreo;
				if (environment.IsProduction()) {
					strTemplateCorreo = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "TemplatesCorreos", "ValidacionDestinatario.html"));
				} else {
					strTemplateCorreo = await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "TemplatesCorreos", "ValidacionDestinatario.html"));
				}

				SalHermesEnviar retorno = await hermesHelper.EnviarCorreo(new EntHermesCorreoEnviar() {
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
								.Replace("[NOMBRE_USUARIO]", usuario.Nombre ?? "")
								.Replace("[NOMBRE_NEGOCIO]", negocio.Nombre),
					Cuerpo = strTemplateCorreo
								.Replace("[NOMBRE_USUARIO]", WebUtility.HtmlEncode(usuario.Nombre ?? ""))
								.Replace("[NOMBRE_NEGOCIO]", WebUtility.HtmlEncode(negocio.Nombre))
								.Replace("[CODIGO_VALIDACION]", Uri.EscapeDataString(codigoValidacion)),
				});

				nuevoDestinatario.HermesIdMensaje = retorno.IdMensaje;
				await destinatarioNotificacionDao.Actualizar(nuevoDestinatario, transaction);

			} else if (nuevoDestinatario.IdTipoReceptor == 2 /* Whatsapp */) {
				Negocio negocio = (await negocioDao.ObtenerPorSub(sub, true, transaction)).FirstOrDefault(n => n.Id == idNegocio) ?? throw new Exception("ID de negocio no válido");
				Usuario usuario = await usuarioBcp.ObtenerInformacionUsuario(sub, transaction);

				SalHermesEnviar retorno = await hermesHelper.EnviarWhatsapp(new EntHermesWhatsappEnviar() {
					De = variableEntorno.Obtener("HERMES_DE_WHATSAPP"),
					Para = nuevoDestinatario.Destino,
					NombreTemplate = "validacion_destinatario",
					ParametrosCuerpo = [
						usuario.Nombre ?? "",
						negocio.Nombre
					],
					ParametrosBoton = [
						Uri.EscapeDataString(codigoValidacion)
					]
				});

				nuevoDestinatario.HermesIdMensaje = retorno.IdMensaje;
				await destinatarioNotificacionDao.Actualizar(nuevoDestinatario, transaction);
			}

			return nuevoDestinatario;
		}
		
		public async Task Validar(DestinatarioNotificacion destinatarioNotificacion, NpgsqlTransaction? transaction = null) {
			if (!destinatarioNotificacion.Validado) {
				destinatarioNotificacion.Validado = true;
				destinatarioNotificacion.FechaValidacion = DateTime.UtcNow;
				await destinatarioNotificacionDao.Actualizar(destinatarioNotificacion, transaction);
			}
		}

		public async Task Eliminar(DestinatarioNotificacion destinatarioNotificacion, NpgsqlTransaction? transaction = null) {
			if (destinatarioNotificacion.Vigencia) {
				destinatarioNotificacion.FechaEliminacion = DateTime.UtcNow;
				destinatarioNotificacion.Vigencia = false;
				await destinatarioNotificacionDao.Actualizar(destinatarioNotificacion, transaction);
			}
		}
	}
}
