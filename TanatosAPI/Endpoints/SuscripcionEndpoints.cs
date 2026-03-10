using Amazon.Lambda.Core;
using System.Diagnostics;
using System.Security.Claims;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;
using static Google.Cloud.RecaptchaEnterprise.V1.TransactionData.Types;

namespace TanatosAPI.Endpoints {
	public static class SuscripcionEndpoints {
		public static IEndpointRouteBuilder MapSuscripcionEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/Suscripcion");
			group.MapCrearEndpoint();

			return routes;
		}

		private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (EntSuscripcionCrear entrada, IHostEnvironment environment, ClaimsPrincipal user, CognitoHelper cognitoHelper, PlanDao planDao, SuscripcionDao suscripcionDao, FlowHelper flowHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					// Se valida que el plan exista y este vigente...
					Plan? planExistente = (await planDao.ObtenerPorVigencia(true)).FirstOrDefault(p => p.Id == entrada.IdPlan);
					if (planExistente == null) {
						LambdaLogger.Log(
							$"[POST] - [Suscripcion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El plan indicado es inválido - ID Plan: {entrada.IdPlan}.");

						return Results.BadRequest($"El plan indicado es inválido - ID Plan: {entrada.IdPlan}.");
					}

					// Se valida que el usuario no tenga otra suscripción vigente...
					List<Suscripcion> suscripciones = await suscripcionDao.ObtenerPorSub(sub, true);
					if (suscripciones.Any(s => s.Estado == 1 /* Activa */)) {
						LambdaLogger.Log(
							$"[POST] - [Suscripcion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El usuario ya cuenta con una suscripción activa.");

						return Results.BadRequest($"El usuario ya cuenta con una suscripción activa.");
					}

					// Si es una suscripción gratuita, se valida que no tenga otra suscripción anterior del mismo tipo...
					if (planExistente.Precio == 0 && suscripciones.Any(s => s.IdPlan == planExistente.Id)) {
						LambdaLogger.Log(
							$"[POST] - [Suscripcion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El usuario ya se suscribió con anterioridad al plan gratuito.");

						return Results.BadRequest($"El usuario ya se suscribió con anterioridad al plan gratuito.");
					}

					// Se crea la suscripción en el sistema interno...
					DateTime ahora = DateTime.UtcNow;
					Suscripcion nuevo = new() {
						Id = 0,
						Sub = sub,
						IdPlan = planExistente.Id,
						FechaInicio = ahora,
						FechaExpiracion = ahora.AddMonths(planExistente.DuracionMeses),
						FechaCancelacion = null,
						Estado = planExistente.Precio == 0 ? (short)1 /* Activa */ : (short)4 /* Pago Pendiente */,
						FlowCustomerId = null,
						FlowSubscriptionId = null,
						FechaCreacion = ahora,
						FechaEliminacion = null,
						Vigencia = true
					};
					nuevo.Id = await suscripcionDao.Insertar(nuevo);

					string? urlRedirect = null;
					if (nuevo.Estado == 4 /* Pago Pendiente */ && planExistente.FlowPlanId != null) {
						// Si es un plan Flow, se crea la suscripción en Flow...
						Dictionary<string, string> atributosUsuario = await cognitoHelper.ObtenerUsuario(sub);
						(string token, string url) = await flowHelper.SuscriptionCreate(atributosUsuario["email"], planExistente.FlowPlanId, nuevo.Id.ToString());
						urlRedirect = $"{url}?token={token}";
					}

					LambdaLogger.Log(
						$"[POST] - [Suscripcion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa de la suscripción.");

					return Results.Ok(new SalSuscripcionCrear {
						UrlSuscripcion = urlRedirect
					});
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [Suscripcion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación de la suscripción - ID Plan: {entrada.IdPlan}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Suscripciones.Write.Self").WithOpenApi();

			return routes;
		}
	}
}
