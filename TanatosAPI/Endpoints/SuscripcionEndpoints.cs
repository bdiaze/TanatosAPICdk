using Amazon.Lambda.Core;
using Npgsql;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
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

			RouteGroupBuilder publicGroup = routes.MapGroup("/public/Suscripcion");
			publicGroup.MapWebhookEndpoint();

			return routes;
		}

		private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (EntSuscripcionCrear entrada, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, ClaimsPrincipal user, CognitoHelper cognitoHelper, PlanDao planDao, SuscripcionDao suscripcionDao, FlowHelper flowHelper) => {
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

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

					string? urlRedirect = null;
					try {
						short estado;
						DateTime? fechaInicio;
						DateTime? fechaExpiracion;
						if (planExistente.Precio == 0) {
							estado = 1; // Activa
							fechaInicio = DateTime.UtcNow;
							fechaExpiracion = fechaInicio.Value.AddMonths(planExistente.DuracionMeses);
						} else {
							estado = 4; // Pago Pendiente
							fechaInicio = null;
							fechaExpiracion = null;
						}

						// Se crea la suscripción en el sistema interno...
						Suscripcion nuevo = new() {
							Id = 0,
							Sub = sub,
							IdPlan = planExistente.Id,
							FechaInicio = fechaInicio,
							FechaExpiracion = fechaExpiracion,
							FechaCancelacion = null,
							Estado = estado,
							FlowCustomerId = null,
							FlowSubscriptionId = null,
							FechaCreacion = DateTime.UtcNow,
							FechaEliminacion = null,
							Vigencia = true
						};
						nuevo.Id = await suscripcionDao.Insertar(nuevo, transaction);

						if (nuevo.Estado == 4 /* Pago Pendiente */ && planExistente.FlowPlanId != null) {
							// Si es un plan Flow, se crea la suscripción en Flow...
							Dictionary<string, string> atributosUsuario = await cognitoHelper.ObtenerUsuario(sub);
							SalFlowSubscriptionCreate salida = await flowHelper.SuscriptionCreate(atributosUsuario["email"], planExistente.FlowPlanId, nuevo.Id.ToString());
							urlRedirect = $"{salida.Url}?token={salida.Token}";
						}

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
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

		private static IEndpointRouteBuilder MapWebhookEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/flow-webhook", async (HttpRequest request, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, EventoPagoDao eventoPagoDao, SuscripcionDao suscripcionDao, PlanDao planDao, PagoDao pagoDao, FlowHelper flowHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					using StreamReader reader = new(request.Body);
					string cuerpo = await reader.ReadToEndAsync();

					// Se registra evento recepcionado en webhook...
					EventoPago eventoPago = new() { 
						Id = 0,
						Proveedor = "Flow",
						Evento = "Subscription Webhook",
						Payload = cuerpo,
						Procesado = false,
						FechaCreacion = DateTime.UtcNow,
						FechaEliminacion = null,
						Vigencia = true,
					};
					eventoPago.Id = await eventoPagoDao.Insertar(eventoPago);

					EntSuscripcionWebhook entrada = JsonSerializer.Deserialize(cuerpo, AppJsonSerializerContext.Default.EntSuscripcionWebhook)!;

					// Se obtiene estado de la suscripción de Flow...
					SalFlowSubscriptionStatus salida = await flowHelper.SubscriptionStatus(entrada.Token);

					// Se valida que exista una suscripción para el token recepcionado...
					Suscripcion? existente = await suscripcionDao.Obtener(long.Parse(salida.ExternalId));
					if (existente == null) {
						LambdaLogger.Log(
							$"[POST] - [Suscripcion] - [Webhook] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
							$"No existe una suscripción para el token recepcionado - External ID: {salida.ExternalId}");

						return Results.Ok();
					}

					// Se valida que el plan exista...
					Plan? planExistente = (await planDao.ObtenerPorVigencia(null)).FirstOrDefault(p => p.Id == existente.IdPlan);
					if (planExistente == null) {
						LambdaLogger.Log(
							$"[POST] - [Suscripcion] - [Webhook] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
							$"No existe un plan para la suscripción - ID Plan: {existente.IdPlan}.");

						return Results.Ok();
					}

					// Se valida que el pago no este registrado previamente...
					Pago? pagoExistente = await pagoDao.ObtenerPorFlow(salida.SubscriptionId, salida.Period);
					if (pagoExistente != null) {
						LambdaLogger.Log(
							$"[POST] - [Suscripcion] - [Webhook] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
							$"El pago ya se encuentra registrado - Flow Subscription ID: {salida.SubscriptionId} - Flow Period: {salida.Period}.");

						return Results.Ok();
					}

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();
					try {
						// Si llega activa, se procesa el pago...
						if (salida.Status == 1 /* Activa */) {
							existente.Estado = 1;
							existente.FechaInicio ??= DateTime.UtcNow;
							if (existente.FechaExpiracion == null) {
								existente.FechaExpiracion = existente.FechaInicio.Value.AddMonths(planExistente.DuracionMeses);
							} else {
								DateTime desde = existente.FechaExpiracion.Value > DateTime.UtcNow ? existente.FechaExpiracion.Value : DateTime.UtcNow;
								existente.FechaExpiracion = desde.AddMonths(planExistente.DuracionMeses);
							}
							existente.FlowCustomerId ??= salida.CustomerId;
							existente.FlowSubscriptionId ??= salida.SubscriptionId.ToString();
							await suscripcionDao.Actualizar(existente, transaction);

							// Se crea pago...
							Pago nuevoPago = new() { 
								Id = 0,
								Sub = existente.Sub,
								IdSuscripcion = existente.Id,
								Monto = salida.Amount,
								Moneda = salida.Currency,
								FechaPago = DateTime.UtcNow,
								Estado = 1, // Pagado
								FlowSubscriptionId = salida.SubscriptionId,
								FlowPeriod = salida.Period,
								FechaCreacion = DateTime.UtcNow,
								FechaEliminacion = null,
								Vigencia = true
							};
							nuevoPago.Id = await pagoDao.Insertar(nuevoPago, transaction);

						// Si llega cancelada, se cancela la suscripción...
						} else if (salida.Status == 2 /* Cancelada */) {
							if (existente.Estado != 2 /* Cancelada */) {
								existente.Estado = 2; // Cancelada
								existente.FechaCancelacion = DateTime.UtcNow;
								await suscripcionDao.Actualizar(existente, transaction);
							}
						}

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					eventoPago.Procesado = true;
					await eventoPagoDao.Actualizar(eventoPago);

					LambdaLogger.Log(
						$"[POST] - [Suscripcion] - [Webhook] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Ejecución exitosa del webhook de suscripción.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [Suscripcion] - [Webhook] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la ejecución del webhook de suscripción. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).AllowAnonymous().WithOpenApi();

			return routes;
		}
	}
}
