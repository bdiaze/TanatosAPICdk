using Amazon.Lambda.Core;
using System.Diagnostics;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;
using TanatosAPI.Repositories;

namespace TanatosAPI.Endpoints {
	public static class WhatsappEndpoints {
		public static IEndpointRouteBuilder MapWhatsappEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/Whatsapp");
			group.MapObtenerConversaciones();
			group.MapObtenerMensajes();
			group.MapObtenerMedia();
			group.MapEnviarMensaje();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerConversaciones(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Conversaciones", async (DateTime? desde, DateTime? hasta, IHostEnvironment environment, IVariableEntornoHelper variableEntorno, HermesHelper hermesHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<SalHermesWhatsappConversacion> retorno = await hermesHelper.ObtenerConversaciones(variableEntorno.Obtener("HERMES_DE_WHATSAPP"), desde, hasta);

					LambdaLogger.Log(
						$"[GET] - [Whatsapp] - [Conversaciones] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Se obtienen exitosamente las conversaciones de whatsapp - Cant. Registros: {retorno.Count}");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Whatsapp] - [Conversaciones] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener conversaciones de whatsapp. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerMensajes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Mensajes", async (string numeroTelefono, DateTime? desde, DateTime? hasta, IHostEnvironment environment, IVariableEntornoHelper variableEntorno, HermesHelper hermesHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<SalHermesWhatsappMensaje> retorno = await hermesHelper.ObtenerMensajes(variableEntorno.Obtener("HERMES_DE_WHATSAPP"), numeroTelefono, desde, hasta);

					LambdaLogger.Log(
						$"[GET] - [Whatsapp] - [Mensajes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Se obtienen exitosamente los mensajes de la conversación de whatsapp - Cant. Registros: {retorno.Count}");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Whatsapp] - [Mensajes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los mensajes de la conversación de whatsapp. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerMedia(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Media", async (string whatsappMessageId, IHostEnvironment environment, HermesHelper hermesHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					SalHermesWhatsappMedia retorno = await hermesHelper.ObtenerMedia(whatsappMessageId);

					LambdaLogger.Log(
						$"[GET] - [Whatsapp] - [Media] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Se obtiene exitosamente la URL de descarga para media de whatsapp.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Whatsapp] - [Media] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener la URL de descarga para media de whatsapp. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");

			return routes;
		}

		private static IEndpointRouteBuilder MapEnviarMensaje(this IEndpointRouteBuilder routes) {
			routes.MapPost("/Enviar", async (EntWhatsappEnviar entrada, IHostEnvironment environment, IVariableEntornoHelper variableEntorno, HermesHelper hermesHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					entrada.Para = entrada.Para.Trim().Replace(" ", "");
					if (!entrada.Para.StartsWith('+')) entrada.Para = "+" + entrada.Para;
					entrada.Cuerpo = entrada.Cuerpo.Trim();

					SalHermesEnviar retorno = await hermesHelper.EnviarWhatsapp(new EntHermesWhatsappEnviar {
						De = variableEntorno.Obtener("HERMES_DE_WHATSAPP"),
						Para = entrada.Para,
						Cuerpo = entrada.Cuerpo,
					});

					LambdaLogger.Log(
						$"[GET] - [Whatsapp] - [Media] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Se envía exitosamente el mensaje de whatsapp.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Whatsapp] - [Media] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al enviar el mensaje de whatsapp. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");

			return routes;
		}
	}
}
