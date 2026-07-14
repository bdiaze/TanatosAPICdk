using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Npgsql;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Suscripcion;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Repositories;
using TanatosAPI.UseCases;

namespace TanatosAPI.Endpoints {
	public static class SuscripcionEndpoints {
		public static IEndpointRouteBuilder MapSuscripcionEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/Suscripcion");
			group.MapObtenerResumenEndpoint();
			group.MapObtenerVigentesEndpoint();
			group.MapCrearEndpoint();
			group.MapCancelarEndpoint();
			group.MapActivarSuscripcionGratuitaEndpoint();

			RouteGroupBuilder publicGroup = routes.MapGroup("/public/Suscripcion");
			publicGroup.MapWebhookEndpoint();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerResumenEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Resumen", async (IHostEnvironment environment, ClaimsPrincipal user, SuscripcionUseCase suscripcionUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					(bool tienePlanEmpresa, Plan? planEnCurso, Plan? planPagoEnCurso, DateTime? fechaExpiracion, DateTime? fechaProximoCobro, bool renovacionAutomatica) = 
						await suscripcionUseCase.ObtenerResumenSuscripcion(sub);

					SalSuscripcionResumen retorno = new() {
						TienePlanEmpresa = tienePlanEmpresa,
						NombrePlanEnCurso = planEnCurso?.Nombre,
						PrecioPlanEnCurso = planEnCurso?.Precio,
						NombrePlanPagoEnCurso = planPagoEnCurso?.Nombre,
						PrecioPlanPagoEnCurso = planPagoEnCurso?.Precio,
						FechaExpiracion = fechaExpiracion,
						FechaProximoCobro = fechaProximoCobro,
						RenovacionAutomatica = renovacionAutomatica
					};

					LambdaLogger.Log(
						$"[GET] - [Suscripcion] - [ObtenerResumen] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa del resumen de suscripción del cliente.");

					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [Suscripcion] - [ObtenerResumen] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Suscripcion] - [ObtenerResumen] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener el resumen de suscripción del cliente. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Suscripciones.Read.Self");

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerVigentesEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes", async (IHostEnvironment environment, ClaimsPrincipal user, SuscripcionUseCase suscripcionUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					List<Suscripcion> suscripciones = await suscripcionUseCase.ObtenerVigentesPorSubConPlan(sub);
					List<SalSuscripcion> retorno = [.. suscripciones.Select(s => {
						return new SalSuscripcion {
							Id = s.Id,
							IdPlan = s.Plan!.Id,
							NombrePlan = s.Plan!.Nombre,
							PrecioPlan = s.Plan!.Precio,
							DuracionMesesPlan = s.Plan!.DuracionMeses,
							FechaInicio = s.FechaInicio,
							FechaExpiracion = s.FechaExpiracion,
							FechaCancelacion = s.FechaCancelacion,
							Estado = s.Estado,
							TieneFlowSubscriptionId = !string.IsNullOrWhiteSpace(s.FlowSubscriptionId)
						};
					})];

					LambdaLogger.Log(
						$"[GET] - [Suscripcion] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de las suscripciones del cliente - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [Suscripcion] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Suscripcion] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener las suscripciones del cliente. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Suscripciones.Read.Self");

			return routes;
		}

		private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (EntSuscripcionCrear entrada, IHostEnvironment environment, ClaimsPrincipal user, SuscripcionUseCase suscripcionUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					SalSuscripcionCrear retorno = new() {
						UrlSuscripcion = await suscripcionUseCase.SuscribirseAPlan(sub, entrada.IdPlan)
					};

					LambdaLogger.Log(
						$"[POST] - [Suscripcion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa de la suscripción.");

					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[POST] - [Suscripcion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [Suscripcion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación de la suscripción - ID Plan: {entrada.IdPlan}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Suscripciones.Write.Self");

			return routes;
		}

		private static IEndpointRouteBuilder MapActivarSuscripcionGratuitaEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/ActivarSuscripcionGratuita", async (EntSuscripcionActivarSuscripcionGratuita entrada, IHostEnvironment environment, ClaimsPrincipal user, SuscripcionUseCase suscripcionUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<Plan> planesGratuitosSuscritos = await suscripcionUseCase.SuscribirseAPlanesGratuitos(entrada.Sub);

					LambdaLogger.Log(
						$"[POST] - [Suscripcion] - [ActivarSuscripcionGratuita] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Se activó exitosamente {planesGratuitosSuscritos.Count} planes gratuitos - Sub: {entrada.Sub}.");

					return Results.Ok();
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[POST] - [Suscripcion] - [ActivarSuscripcionGratuita] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [Suscripcion] - [ActivarSuscripcionGratuita] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la activación de suscripciones gratuitas - Sub: {entrada.Sub}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Suscripciones.Read.All", "Suscripciones.Write.All", "Sistema.Read.Public");

			return routes;
		}


		private static IEndpointRouteBuilder MapCancelarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/", async (IHostEnvironment environment, ClaimsPrincipal user, SuscripcionUseCase suscripcionUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					await suscripcionUseCase.CancelarSuscripcion(sub);

					LambdaLogger.Log(
						$"[DELETE] - [Suscripcion] - [Cancelar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Cancelación exitosa de la suscripción.");

					return Results.Ok();
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[DELETE] - [Suscripcion] - [Cancelar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [Suscripcion] - [Cancelar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al cancelar la suscripción. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Suscripciones.Write.Self");

			return routes;
		}

		private static IEndpointRouteBuilder MapWebhookEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/flow-webhook/{tipo}", async (string tipo, [FromForm] string token, IHostEnvironment environment, IVariableEntornoHelper variableEntorno, SuscripcionUseCase suscripcionUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					bool redirect = await suscripcionUseCase.ProcesarWebhookFlow(tipo, token);

					LambdaLogger.Log(
						$"[POST] - [Suscripcion] - [Webhook] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Ejecución exitosa del webhook de suscripción - Tipo: {tipo}.");

					if (redirect) {
						return Results.Redirect(variableEntorno.Obtener("FLOW_URL_RETORNO"));
					} else {
						return Results.Ok();
					}
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[POST] - [Suscripcion] - [Webhook] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [Suscripcion] - [Webhook] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la ejecución del webhook de suscripción  - Tipo: {tipo}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).AllowAnonymous().DisableAntiforgery();

			return routes;
		}
	}
}
