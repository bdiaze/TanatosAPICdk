using Amazon.Lambda.Core;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;

namespace TanatosAPI.Endpoints {
	public static class MensajeEndpoints {
		public static IEndpointRouteBuilder MapMensajeEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/Mensaje");
			group.MapObtener();
			group.MapIngresarEndpoint();

			// Endpoints públicos 
			RouteGroupBuilder publicGroup = routes.MapGroup("/public/Mensaje");
			publicGroup.MapIngresarAnonimoEndpoint();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtener(this IEndpointRouteBuilder routes) {
			routes.MapGet("/{fechaInicial}/{fechaFinal}", async (DateTime fechaInicial, DateTime fechaFinal, IHostEnvironment environment, MensajeDao mensajeDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

			try {
					List<Mensaje> retorno = await mensajeDao.ObtenerPorRangoFechas(fechaInicial, fechaFinal);

					LambdaLogger.Log(
						$"[GET] - [Mensaje] - [Obtener] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los mensajes - Fecha Inicial: {fechaInicial:O} - Fecha Final: {fechaFinal:O}  - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Mensaje] - [Obtener] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los mensajes - Fecha Inicial: {fechaInicial:O} - Fecha Final: {fechaFinal:O}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapIngresarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (EntMensajeIngresar entrada, IHostEnvironment environment, ClaimsPrincipal user, VariableEntornoHelper variableEntorno, GoogleRecaptchaHelper googleRecaptchaHelper, MensajeDao mensajeDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					const string ACTION = "contact_form";
					const float MIN_SCORE = 0.7f;

					(bool valid, string invalidReason, string action, float score) response = await googleRecaptchaHelper.ObtenerAssesment(entrada.RecaptchaToken, ACTION);

					// Se valida que la respuesta sea válida...
					if (!response.valid) {
						LambdaLogger.Log(
							$"[POST] - [Mensaje] - [Ingresar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El token de reCAPTCHA no es válido. Razón: {response.invalidReason}.");
						return Results.BadRequest("El token de reCAPTCHA no es válido.");
					}

					// Se valida que el action concuerde con el esperado...
					if (response.action != ACTION) {
						LambdaLogger.Log(
							$"[POST] - [Mensaje] - [Ingresar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El token de reCAPTCHA no es válido dado que el Action no es correcto - Action: {response.action} - Expected Action: {ACTION}.");
						return Results.BadRequest($"El token de reCAPTCHA no es válido dado que el Action no es correcto - Action: {response.action} - Expected Action: {ACTION}.");
					}

					// Y se valida que el score sea superior al mínimo aceptable...
					if (response.score <= MIN_SCORE) {
						LambdaLogger.Log(
							$"[POST] - [Mensaje] - [Ingresar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El token de reCAPTCHA no es válido dado que el Score es inferior al mínimo aceptado - Score: {response.score} - Score Mínimo: {MIN_SCORE}.");
						return Results.BadRequest($"El token de reCAPTCHA no es válido dado que el Score es inferior al mínimo aceptado - Score: {response.score} - Score Mínimo: {MIN_SCORE}.");
					}

					entrada.Nombre = entrada.Nombre.Trim();
					entrada.Correo = entrada.Correo.Trim();
					entrada.Contenido = entrada.Contenido.Trim();

					// Se valida que el correo sea un correo válido...
					EmailAddressAttribute emailValidator = new();
					if (!emailValidator.IsValid(entrada.Correo)) {
						LambdaLogger.Log(
							$"[POST] - [Mensaje] - [Ingresar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El correo ingresado no es válido.");
						return Results.BadRequest($"El correo ingresado no es válido.");
					}

					long id = await mensajeDao.Insertar(new Mensaje {
						Id = 0,
						Sub = sub,
						Nombre = entrada.Nombre,
						Correo = entrada.Correo,
						Contenido = entrada.Contenido,
						FechaCreacion = DateTime.UtcNow
					});

					LambdaLogger.Log(
						$"[POST] - [Mensaje] - [Ingresar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Se ingresa exitosamente el mensaje ID: {id}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [Mensaje] - [Ingresar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al ingresar el mensaje. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Negocios.Write.Self").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapIngresarAnonimoEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (EntMensajeIngresar entrada, IHostEnvironment environment, GoogleRecaptchaHelper googleRecaptchaHelper, MensajeDao mensajeDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					const string ACTION = "contact_form";
					const float MIN_SCORE = 0.7f;

					(bool valid, string invalidReason, string action, float score) response = await googleRecaptchaHelper.ObtenerAssesment(entrada.RecaptchaToken, ACTION);

					// Se valida que la respuesta sea válida...
					if (!response.valid) {
						LambdaLogger.Log(
							$"[POST] - [Mensaje] - [IngresarAnonimo] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El token de reCAPTCHA no es válido. Razón: {response.invalidReason}.");
						return Results.BadRequest("El token de reCAPTCHA no es válido.");
					}

					// Se valida que el action concuerde con el esperado...
					if (response.action != ACTION) {
						LambdaLogger.Log(
							$"[POST] - [Mensaje] - [IngresarAnonimo] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El token de reCAPTCHA no es válido dado que el Action no es correcto - Action: {response.action} - Expected Action: {ACTION}.");
						return Results.BadRequest($"El token de reCAPTCHA no es válido dado que el Action no es correcto - Action: {response.action} - Expected Action: {ACTION}.");
					}

					// Y se valida que el score sea superior al mínimo aceptable...
					if (response.score <= MIN_SCORE) {
						LambdaLogger.Log(
							$"[POST] - [Mensaje] - [IngresarAnonimo] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El token de reCAPTCHA no es válido dado que el Score es inferior al mínimo aceptado - Score: {response.score} - Score Mínimo: {MIN_SCORE}.");
						return Results.BadRequest($"El token de reCAPTCHA no es válido dado que el Score es inferior al mínimo aceptado - Score: {response.score} - Score Mínimo: {MIN_SCORE}.");
					}

					entrada.Nombre = entrada.Nombre.Trim();
					entrada.Correo = entrada.Correo.Trim();
					entrada.Contenido = entrada.Contenido.Trim();

					// Se valida que el correo sea un correo válido...
					EmailAddressAttribute emailValidator = new();
					if (!emailValidator.IsValid(entrada.Correo)) {
						LambdaLogger.Log(
							$"[POST] - [Mensaje] - [IngresarAnonimo] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El correo ingresado no es válido.");
						return Results.BadRequest($"El correo ingresado no es válido.");
					}

					long id = await mensajeDao.Insertar(new Mensaje {
						Id = 0,
						Sub = null,
						Nombre = entrada.Nombre,
						Correo = entrada.Correo,
						Contenido = entrada.Contenido,
						FechaCreacion = DateTime.UtcNow
					});

					LambdaLogger.Log(
						$"[POST] - [Mensaje] - [IngresarAnonimo] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Se ingresa exitosamente el mensaje ID: {id}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [Mensaje] - [IngresarAnonimo] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al ingresar el mensaje. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).AllowAnonymous().WithOpenApi();

			return routes;
		}
	}
}
