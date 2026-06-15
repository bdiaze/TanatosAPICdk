using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Components.RenderTree;
using Scriban.Runtime;
using System.Diagnostics;
using System.Net;
using TanatosAPI.Entities.Others;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;
using TanatosAPI.UseCases;

namespace TanatosAPI.Endpoints {
	public static class ProfileEndpoints {
		public static void MapProfileEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/Profile");
			group.MapEnviarCodigoVerificacion();
		}

		private static void MapEnviarCodigoVerificacion(this IEndpointRouteBuilder routes) {
			routes.MapPost("/EnviarCodigoVerificacion", async (EntProfileEnviarCodigoVerificacion entrada, IHostEnvironment environment, IVariableEntornoHelper variableEntorno, IKMSHelper kmsHelper, HtmlRenderer htmlRenderer, HermesHelper hermesHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					SalHermesEnviar retorno = await hermesHelper.EnviarCorreo(new EntHermesCorreoEnviar() {
						De = new DireccionCorreo() {
							Nombre = variableEntorno.Obtener("HERMES_DE_NOMBRE"),
							Correo = variableEntorno.Obtener("HERMES_DE_CORREO"),
						},
						Para = [
							new DireccionCorreo() {
								Correo = entrada.CorreoElectronico
							}
						],
						Asunto = entrada.Nombre != null ? $"¡Hola {entrada.Nombre}, aquí tu código de verificación!" : "¡Ha llegado tu código de verificación!",
						Cuerpo = await htmlRenderer.GenerarHtml("CodigoVerificacion.html", new ScriptObject() {
							["NOMBRE"] = WebUtility.HtmlEncode(entrada.Nombre),
							["CODIGO"] = await kmsHelper.Desencriptar(entrada.CodigoEncriptado),
						})
					});

					LambdaLogger.Log(
						$"[POST] - [Profile] - [EnviarCodigoVerificacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Envío exitoso del código de verificación - Hermes ID Mensaje: {retorno.IdMensaje}.");

					return Results.Ok();
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[POST] - [Profile] - [EnviarCodigoVerificacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [Profile] - [EnviarCodigoVerificacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrio un error al enviar el código de verificación. " +
						$"{ex}");
					return Results.Problem($"Ocurrio un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization(
				"Profile.Read.All",
				"Profile.Write.All"
			);
		}
	}
}
