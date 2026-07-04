using Microsoft.IdentityModel.Logging;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Flow;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class UsuarioBcp(IUsuarioDao usuarioDao, ICognitoHelper cognitoHelper, IFlowHelper flowHelper) : IUsuarioBcp {
		public async Task<Usuario?> ObtenerPorFlowCustomerId(string flowCustomerId, NpgsqlTransaction? transaction = null) {
			return await usuarioDao.ObtenerPorFlowCustomerId(flowCustomerId, transaction);
		}	

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

		public async Task<string> RegistrarUsuarioEnFlow(string sub, NpgsqlTransaction? transaction = null) {
			Usuario usuario = await ObtenerInformacionUsuario(sub, transaction);

			string nombre = usuario.Nombre ?? "";
			string apellido = usuario.Apellido ?? "";
			string correo = usuario.CorreoElectronico ?? throw new InvalidOperationException("No se encuentra registro del correo electrónico del usuario.");

			// Se crea el usuario en flow si no existe...
			if (usuario.FlowCustomerId == null) {
				SalFlowCustomerCreate salFlowCustomerCreate = await flowHelper.CustomerCreate($"{nombre} {apellido}".Trim(), correo, sub);
				usuario.FlowCustomerId = salFlowCustomerCreate.CustomerId;
				await usuarioDao.Actualizar(usuario, transaction);
			}

			return usuario.FlowCustomerId!;
		}

		public async Task<string> RegistrarTarjetaEnFlow(string flowCustomerId) {
			SalFlowUrlToken salFlowUrlToken = await flowHelper.CustomerRegister(flowCustomerId);
			return $"{salFlowUrlToken.Url}?token={salFlowUrlToken.Token}";
		}
	}
}
