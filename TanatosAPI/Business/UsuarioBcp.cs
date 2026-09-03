using Microsoft.AspNetCore.SignalR;
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
	public class UsuarioBcp(IDateTimeProvider dateTimeProvider, IUsuarioDao usuarioDao, ICognitoHelper cognitoHelper, IFlowHelper flowHelper) : IUsuarioBcp {
		public async Task<Usuario?> ObtenerPorFlowCustomerId(string flowCustomerId, NpgsqlTransaction? transaction = null) {
			return await usuarioDao.ObtenerPorFlowCustomerId(flowCustomerId, transaction);
		}	

		public async Task<Usuario> Crear(string sub, string userName, string? flowCustomerId, string? nombre, string? apellido, string? correoElectronico, NpgsqlTransaction? transaction = null) {
			Usuario nuevo = new() { 
				Sub = sub,
				UserName = userName,
				FlowCustomerId = flowCustomerId,
				Nombre = nombre,
				Apellido = apellido,
				CorreoElectronico = correoElectronico,
				FechaCreacion = dateTimeProvider.UtcNow
			};
			await usuarioDao.Insertar(nuevo, transaction);
			return nuevo;
		}

		public async Task<Usuario> CargarDesdeCognitoSiNoExiste(string userName, NpgsqlTransaction? transaction = null) {
			Usuario? usuario = await usuarioDao.ObtenerPorUserName(userName, transaction);
			if (usuario == null) {
				Dictionary<string, string> atributosUsuario = await cognitoHelper.ObtenerUsuario(userName);
				usuario = await Crear(
					atributosUsuario.GetValueOrDefault("sub") ?? throw new InvalidOperationException("No se encuentra el sub en Cognito."),
					userName,
					null,
					atributosUsuario.GetValueOrDefault("given_name"),
					atributosUsuario.GetValueOrDefault("family_name"),
					atributosUsuario.GetValueOrDefault("email"),
					transaction
				);
			}
			return usuario;
		}

		public async Task<Usuario?> Obtener(string sub, NpgsqlTransaction? transaction = null) {
			return await usuarioDao.Obtener(sub, transaction);
		}

		public async Task<string> RegistrarUsuarioEnFlow(string sub, NpgsqlTransaction? transaction = null) {
			Usuario usuario = await Obtener(sub, transaction) ?? throw new InvalidOperationException("No se encuentra registro del usuario.");

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
