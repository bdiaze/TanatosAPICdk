using Amazon.Lambda.Core;
using Microsoft.AspNetCore.SignalR;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

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
			routes.MapPost("/", async (EntSuscripcionCrear entrada, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, ClaimsPrincipal user, CognitoHelper cognitoHelper, PlanDao planDao, SuscripcionDao suscripcionDao, UsuarioDao usuarioDao, FlowHelper flowHelper) => {
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
					List<Suscripcion> suscripciones = await suscripcionDao.ObtenerPorSub(sub);
					if (suscripciones.Any(s => s.Estado == 1 /* Activa */)) {
						LambdaLogger.Log(
							$"[POST] - [Suscripcion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El usuario ya cuenta con una suscripción activa.");

						return Results.BadRequest($"El usuario ya cuenta con una suscripción activa.");
					}

					// Si es una suscripción gratuita, se valida que no tenga otra suscripción anterior del mismo tipo...
					if (planExistente.SuscripcionUnica && suscripciones.Any(s => s.IdPlan == planExistente.Id)) {
						LambdaLogger.Log(
							$"[POST] - [Suscripcion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El usuario ya se suscribió con anterioridad al plan.");

						return Results.BadRequest($"El usuario ya se suscribió con anterioridad al plan.");
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

						// Si es un plan Flow, se crea la suscripción en Flow...
						if (nuevo.Estado == 4 /* Pago Pendiente */ && planExistente.FlowPlanId != null) {
							Dictionary<string, string> atributosUsuario = await cognitoHelper.ObtenerUsuario(sub);
							string nombre = atributosUsuario.TryGetValue("given_name", out string? givenName) ? givenName : "";
							string apellido = atributosUsuario.TryGetValue("family_name", out string? familyName) ? familyName : "";
							string correo = atributosUsuario.TryGetValue("email", out string? email) ? email : throw new Exception("No se encuentra registro del correo electrónico del usuario.");

							// Se crea el usuario en el sistema interno si no existe...
							Usuario? usuarioExistente = await usuarioDao.Obtener(sub);
							if (usuarioExistente == null) {
								usuarioExistente = new Usuario() {
									Sub = sub,
									CorreoElectronico = correo
								};
								await usuarioDao.Insertar(usuarioExistente, transaction);
							}

							// Se crea el usuario en flow si no existe...
							if (usuarioExistente.FlowCustomerId == null) {
								SalFlowCustomerCreate salFlowCustomerCreate = await flowHelper.CustomerCreate($"{nombre} {apellido}".Trim(), correo, sub);
								usuarioExistente.FlowCustomerId = salFlowCustomerCreate.CustomerId;
								await usuarioDao.Actualizar(usuarioExistente, transaction);
							}

							nuevo.FlowCustomerId = usuarioExistente.FlowCustomerId;
							await suscripcionDao.Actualizar(nuevo, transaction);

							// Se valida si el usuario ya tiene registrada su tarjeta...
							SalFlowUrlToken salFlowUrlToken =  await flowHelper.CustomerRegister(usuarioExistente.FlowCustomerId!);
							urlRedirect = $"{salFlowUrlToken.Url}?token={salFlowUrlToken.Token}";
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
			routes.MapPost("/flow-webhook/{tipo}", async (string tipo, HttpRequest request, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, EventoPagoDao eventoPagoDao, SuscripcionDao suscripcionDao, PlanDao planDao, PagoDao pagoDao, UsuarioDao usuarioDao, FlowHelper flowHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					using StreamReader reader = new(request.Body);
					string cuerpo = await reader.ReadToEndAsync();

					// Se registra evento recepcionado en webhook...
					EventoPago eventoPago = new() { 
						Id = 0,
						Proveedor = "Flow",
						Evento = $"{tipo}Webhook",
						Payload = cuerpo,
						Procesado = false,
						FechaCreacion = DateTime.UtcNow,
						FechaEliminacion = null,
						Vigencia = true,
					};
					eventoPago.Id = await eventoPagoDao.Insertar(eventoPago);

					EntSuscripcionWebhook entrada = JsonSerializer.Deserialize(cuerpo, AppJsonSerializerContext.Default.EntSuscripcionWebhook)!;

					if (tipo == "CustomerRegister") {
						SalFlowCustomerGetRegisterStatus salFlow = await flowHelper.CustomerGetRegisterStatus(entrada.Token);
						if (salFlow.Status?.Trim() == "1" /* Registrado */ && salFlow.CustomerId != null) {
							Usuario? usuario = await usuarioDao.ObtenerPorFlowCustomerId(salFlow.CustomerId);
							if (usuario != null) {
								List<Suscripcion> suscripciones = await suscripcionDao.ObtenerPorSub(usuario.Sub);
								List<Plan> planesVigentes = await planDao.ObtenerPorVigencia(true);
								Suscripcion? suscripcionActivar = null;
								foreach (Suscripcion suscripcion in suscripciones.Where(s => s.Estado == 4 /* Pago Pendiente */).OrderByDescending(s => s.FechaCreacion)) {
									if (planesVigentes.Any(p => p.Id == suscripcion.IdPlan)) {
										suscripcionActivar = suscripcion;
										break;
									}
								}

								if (suscripcionActivar != null) {
									Plan plan = planesVigentes.First(p => p.Id == suscripcionActivar.IdPlan);

									// Se crea suscripción en Flow...
									SalFlowSubscriptionCreate salFlowSubscriptionCreate = await flowHelper.SuscriptionCreate(plan.FlowPlanId!, usuario.CorreoElectronico);
									if (salFlowSubscriptionCreate.Status == 1 /* Activa */) {
										suscripcionActivar.FechaInicio = DateTime.UtcNow;
										suscripcionActivar.FechaExpiracion = suscripcionActivar.FechaInicio.Value.AddMonths(plan.DuracionMeses);
										suscripcionActivar.Estado = 1; // Activa
										suscripcionActivar.FlowSubscriptionId = salFlowSubscriptionCreate.SubscriptionId;
										await suscripcionDao.Actualizar(suscripcionActivar);
									}
								}
							}
						}
					} else if (tipo == "PlanCreate" || tipo == "PlanEdit") {
						SalFlowPaymentGetStatus salFlow = await flowHelper.PaymentGetStatus(entrada.Token);
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
