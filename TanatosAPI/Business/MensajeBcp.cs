using System.Net;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class MensajeBcp(MensajeDao mensajeDao, IHostEnvironment environment, HermesHelper hermesHelper, VariableEntornoHelper variableEntorno) {
		public async Task<Mensaje> Ingresar(string nombre, string correo, string contenido, string? sub = null) {

			Mensaje nuevo = new() { 
				Id = 0,
				Sub = sub,
				Nombre = nombre,
				Correo = correo, 
				Contenido = contenido,
				FechaCreacion = DateTime.UtcNow
			};

			nuevo.Id = await mensajeDao.Insertar(nuevo);

			string strTemplateCorreo;
			if (environment.IsProduction()) {
				strTemplateCorreo = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "TemplatesCorreos", "MensajeRecibido.html"));
			} else {
				strTemplateCorreo = await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "TemplatesCorreos", "MensajeRecibido.html"));
			}

			await hermesHelper.EnviarCorreo(new EntHermesCorreoEnviar() {
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
				Para = [.. variableEntorno.Obtener("DESTINATARIOS_NUEVO_MENSAJE").Split(',').Select(destinatario => new DireccionCorreo() { 
					Correo = destinatario 
				})],
				Asunto = "¡Hemos recibido un mensaje!",
				Cuerpo = strTemplateCorreo
							.Replace("[NOMBRE_USUARIO]", WebUtility.HtmlEncode(nuevo.Nombre))
							.Replace("[CORREO_USUARIO]", WebUtility.HtmlEncode(nuevo.Correo))
							.Replace("[CONTENIDO]", WebUtility.HtmlEncode(nuevo.Contenido)),
			});

			return nuevo;
		}
	}
}
