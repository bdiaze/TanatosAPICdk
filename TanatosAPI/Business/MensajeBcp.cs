using Scriban.Runtime;
using System.Net;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class MensajeBcp(IDateTimeProvider dateTimeProvider, MensajeDao mensajeDao, HermesHelper hermesHelper, IVariableEntornoHelper variableEntorno, HtmlRenderer renderer) {
		public async Task<Mensaje> Ingresar(string nombre, string correo, string contenido, string? sub = null) {

			Mensaje nuevo = new() { 
				Id = 0,
				Sub = sub,
				Nombre = nombre,
				Correo = correo, 
				Contenido = contenido,
				FechaCreacion = dateTimeProvider.UtcNow
			};

			nuevo.Id = await mensajeDao.Insertar(nuevo);

			string cuerpoCorreo = await renderer.GenerarHtml("MensajeRecibido.html", new ScriptObject() {
                ["NOMBRE_USUARIO"] = WebUtility.HtmlEncode(nuevo.Nombre),
                ["CORREO_USUARIO"] = WebUtility.HtmlEncode(nuevo.Correo),
                ["CONTENIDO"] = WebUtility.HtmlEncode(nuevo.Contenido)
            });

			List<string> idsMensajes = [];
			foreach (string destinatario in variableEntorno.Obtener("DESTINATARIOS_NUEVO_MENSAJE").Split(',')) {
				SalHermesEnviar retorno = await hermesHelper.EnviarCorreo(new EntHermesCorreoEnviar() {
					De = new DireccionCorreo() {
						Nombre = variableEntorno.Obtener("HERMES_DE_NOMBRE"),
						Correo = variableEntorno.Obtener("HERMES_DE_CORREO"),
					},
					ResponderA = [
						new DireccionCorreo() {
							Nombre = nuevo.Nombre,
							Correo = nuevo.Correo,
						}
					],
					Para = [ 
						new DireccionCorreo() {
							Correo = destinatario
						}
					],
					Asunto = "¡Hemos recibido un mensaje!",
					Cuerpo = cuerpoCorreo,
				});
				idsMensajes.Add(retorno.IdMensaje);
			}

			nuevo.HermesIdMensaje = string.Join('|', idsMensajes);
			await mensajeDao.Actualizar(nuevo);

			return nuevo;
		}
	}
}
