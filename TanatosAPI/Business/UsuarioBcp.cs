using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class UsuarioBcp(ICognitoHelper cognitoHelper, UsuarioDao usuarioDao) {
		public async Task<Usuario> ObtenerInformacionUsuario(string sub, NpgsqlTransaction? transaction = null) {
			Usuario? usuario = await usuarioDao.Obtener(sub, transaction);
			if (usuario == null) {
				// Si el usuario no existe, se inserta según información de Cognito...
				Dictionary<string, string> atributosUsuario = await cognitoHelper.ObtenerUsuario(sub);
				usuario = new() {
					Sub = sub,
					FlowCustomerId = null,
					Nombre = atributosUsuario.TryGetValue("given_name", out string? givenName) ? givenName : null,
					Apellido = atributosUsuario.TryGetValue("family_name", out string? familyName) ? familyName : null,
					CorreoElectronico = atributosUsuario.TryGetValue("email", out string? email) ? email : null
				};
				await usuarioDao.Insertar(usuario, transaction);
			} else if (usuario.Nombre == null || usuario.Apellido == null || usuario.CorreoElectronico == null) {
				// Si el usuario existe, pero no cuenta con toda su información, se actualiza según información de Cognito...
				Dictionary<string, string> atributosUsuario = await cognitoHelper.ObtenerUsuario(sub);
				usuario.Nombre = atributosUsuario.TryGetValue("given_name", out string? givenName) ? givenName : null;
				usuario.Apellido = atributosUsuario.TryGetValue("family_name", out string? familyName) ? familyName : null;
				usuario.CorreoElectronico = atributosUsuario.TryGetValue("email", out string? email) ? email : null;
				await usuarioDao.Actualizar(usuario, transaction);
			}

			return usuario;
		}
	}
}
