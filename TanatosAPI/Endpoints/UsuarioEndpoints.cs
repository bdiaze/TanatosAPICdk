using Amazon.Lambda.Core;
using System.Diagnostics;
using System.Security.Claims;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Negocio;
using TanatosAPI.Entities.Others.Usuario;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.UseCases;
using TanatosAPI.UseCases;

namespace TanatosAPI.Endpoints {
	public static class UsuarioEndpoints {
		public static void MapUsuarioEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/Usuario");
			group.MapObtener();
			group.MapActualizarWhatsappEndpoint();
		}

		private static void MapObtener(this IEndpointRouteBuilder routes) {
			routes.MapGet("/", async (IHostEnvironment environment, ClaimsPrincipal user, UsuarioUseCase usuarioUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					(Usuario usuario, WhatsappUsuario? whatsappUsuario, bool tienePlanEmpresa) = await usuarioUseCase.ObtenerConWhatsappYPlanEmpresa(sub);

					SalUsuario retorno = new() {
						Nombre = usuario.Nombre,
						Apellido = usuario.Apellido,
						Email = usuario.CorreoElectronico,
						Whatsapp = whatsappUsuario?.Whatsapp,
						WhatsappValidado = whatsappUsuario?.Validado,
						TienePlanEmpresa = tienePlanEmpresa
					};

					LambdaLogger.Log(
						$"[GET] - [Usuario] - [Obtener] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de la información del usuario.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [Usuario] - [Obtener] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Usuario] - [Obtener] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener la información del usuario. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Perfil.Read.Self");
		}

		private static void MapActualizarWhatsappEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/Whatsapp", async (EntUsuarioActualizarWhatsapp entrada, IHostEnvironment environment, ClaimsPrincipal user, IWhatsappUsuarioUseCase whatsappUsuarioUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					_ = await whatsappUsuarioUseCase.ActualizarWhatsapp(sub, entrada.Whatsapp);

					LambdaLogger.Log(
						$"[PUT] - [Usuario] - [ActualizarWhatsapp] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa del número de WhatsApp del usuario.");
					return Results.Ok();
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[PUT] - [Usuario] - [ActualizarWhatsapp] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [Usuario] - [ActualizarWhatsapp] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al actualizar el número de WhatsApp del usuario. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Perfil.Read.Self", "Perfil.Write.Self");
		}
	}
}
