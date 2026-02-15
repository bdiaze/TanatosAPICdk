using Amazon.Lambda.Core;
using Google.Api.Gax.ResourceNames;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.RecaptchaEnterprise.V1;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;

namespace TanatosAPI.Endpoints {
	public static class MensajeEndpoints {
		public static IEndpointRouteBuilder MapMensajeEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/Mensaje");
			group.MapObtener();

			// Endpoints públicos 
			RouteGroupBuilder publicGroup = routes.MapGroup("/public/Mensaje");
			publicGroup.MapIngresarEndpoint();

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
			routes.MapPost("/", async (EntMensajeIngresar entrada, IHostEnvironment environment, ClaimsPrincipal user, VariableEntornoHelper variableEntorno, MensajeDao mensajeDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string googleAwsExternalAccountJson = variableEntorno.Obtener("GOOGLE_AWS_EXTERNAL_ACCOUNT_JSON");
					string googleRecaptchaProjectId = variableEntorno.Obtener("GOOGLE_RECAPTCHA_PROJECT_ID");
					string googleRecaptchaSiteKey = variableEntorno.Obtener("GOOGLE_RECAPTCHA_SITE_KEY");
					string expectedAction = "contact_form";
					float minimumAcceptableScore = 0.7f;

					// Se obtiene información del token de reCaptcha...
					GoogleCredential credential = CredentialFactory.FromJson<AwsExternalAccountCredential>(googleAwsExternalAccountJson).ToGoogleCredential();
					credential.CreateScoped(["https://www.googleapis.com/auth/recaptchaenterprise"]);
					RecaptchaEnterpriseServiceClient client = new RecaptchaEnterpriseServiceClientBuilder {
						Credential = credential
					}.Build();
					ProjectName projectName = new(googleRecaptchaProjectId);
					CreateAssessmentRequest request = new() { 
						ParentAsProjectName = projectName,
						Assessment = new Assessment {
							Event = new Event {
								SiteKey = googleRecaptchaSiteKey,
								ExpectedAction = expectedAction,
								Token = entrada.RecaptchaToken,
							}
						}
					};
					Assessment response = await client.CreateAssessmentAsync(request);

					// Se valida que la respuesta sea válida...
					if (!response.TokenProperties.Valid) {
						LambdaLogger.Log(
							$"[POST] - [Mensaje] - [Ingresar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El token de reCAPTCHA no es válido. Razón: {response.TokenProperties.InvalidReason}.");
						return Results.BadRequest("El token de reCAPTCHA no es válido.");
					}

					// Se valida que el action concuerde con el esperado...
					if (response.TokenProperties.Action != expectedAction) {
						LambdaLogger.Log(
							$"[POST] - [Mensaje] - [Ingresar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El token de reCAPTCHA no es válido dado que el Action no es correcto.");
						return Results.BadRequest("El token de reCAPTCHA no es válido dado que el Action no es correcto.");
					}

					// Y se valida que el score sea superior al mínimo aceptable...
					if (response.RiskAnalysis.Score <= minimumAcceptableScore) {
						LambdaLogger.Log(
							$"[POST] - [Mensaje] - [Ingresar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El token de reCAPTCHA no es válido dado que el Score es inferior al mínimo aceptado.");
						return Results.BadRequest("El token de reCAPTCHA no es válido dado que el Score es inferior al mínimo aceptado.");
					}


					entrada.Nombre = entrada.Nombre.Trim();
					entrada.Correo = entrada.Correo.Trim();
					entrada.Contenido = entrada.Contenido.Trim();

					// Se obtiene sub si el usuario está autenticado, si no lo está, se deja como null para indicar que es un mensaje anónimo...
					string? sub = (user.Identity?.IsAuthenticated ?? false) ? user.Identity?.Name : null;

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
			}).AllowAnonymous().WithOpenApi();

			return routes;
		}
	}
}
