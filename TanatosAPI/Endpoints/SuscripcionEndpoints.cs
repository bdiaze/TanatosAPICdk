using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Npgsql;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Security.Claims;
using System.Text.Json;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;
using TimeZoneConverter;
using static Google.Cloud.RecaptchaEnterprise.V1.TransactionData.Types;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TanatosAPI.Endpoints {
	public static class SuscripcionEndpoints {
		public static IEndpointRouteBuilder MapSuscripcionEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/Suscripcion");
			group.MapObtenerVigentesEndpoint();
			group.MapCrearEndpoint();
			group.MapCancelarEndpoint();

			RouteGroupBuilder publicGroup = routes.MapGroup("/public/Suscripcion");
			publicGroup.MapWebhookEndpoint();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerVigentesEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes", async (IHostEnvironment environment, ClaimsPrincipal user, PlanDao planDao, SuscripcionDao suscripcionDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					List<Suscripcion> suscripciones = await suscripcionDao.ObtenerPorSub(sub, true);
					List<Plan> planes = await planDao.ObtenerPorVigencia(null);
					List<SalSuscripcion> retorno = [.. suscripciones.Select(s => {
						Plan plan = planes.First(p => p.Id == s.IdPlan);

						return new SalSuscripcion {
							Id = s.Id,
							IdPlan = plan.Id,
							NombrePlan = plan.Nombre,
							PrecioPlan = plan.Precio,
							DuracionMesesPlan = plan.DuracionMeses,
							FechaInicio = s.FechaInicio,
							FechaExpiracion = s.FechaExpiracion,
							FechaCancelacion = s.FechaCancelacion,
							Estado = s.Estado
							};
						})];

					LambdaLogger.Log(
						$"[GET] - [Suscripcion] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de las suscripciones del cliente - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Suscripcion] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener las suscripciones del cliente. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Suscripciones.Read.Self").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (EntSuscripcionCrear entrada, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, ClaimsPrincipal user, CognitoHelper cognitoHelper, PlanDao planDao, SuscripcionDao suscripcionDao, UsuarioDao usuarioDao, FlowHelper flowHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					// Se valida que el plan exista y este vigente...
					Plan? planExistente = await planDao.Obtener(entrada.IdPlan);
					if (planExistente == null || !planExistente.Vigencia) {
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

					// Si es una suscripción única, se valida que no tenga otra suscripción anterior del mismo tipo...
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

							// Nos aseguramos de que la fecha esté en UTC...
							DateTime fechaUTC = DateTime.SpecifyKind(fechaInicio.Value, DateTimeKind.Utc);

							// Se transforma la fecha a zona horaria de Chile...
							string timezone = "America/Santiago";
							if (OperatingSystem.IsWindows()) {
								timezone = TZConvert.IanaToWindows(timezone);
							}
							TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
							DateTime fechaTimezone = TimeZoneInfo.ConvertTimeFromUtc(fechaUTC, timeZoneInfo);

							// Se añade la duración en meses del plan...
							fechaTimezone = fechaTimezone.AddMonths(planExistente.DuracionMeses);

							// Se transforma de vuelta a UTC...
							fechaTimezone = TimeZoneInfo.ConvertTimeToUtc(fechaTimezone, timeZoneInfo);

							fechaExpiracion = fechaTimezone;
						} else {
							estado = 4; // Pago Pendiente
							fechaInicio = null;
							fechaExpiracion = null;

							// Se eliminan todas las suscripciones del cliente que tienen su pago pendiente...
							foreach (Suscripcion suscripcionEliminar in suscripciones.Where(s => s.Estado == 4 /* Pago Pendiente */)) {
								suscripcionEliminar.FechaEliminacion = DateTime.UtcNow;
								suscripcionEliminar.Vigencia = false;
								await suscripcionDao.Actualizar(suscripcionEliminar, transaction);
							}
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

						// Si es un plan Flow, se crea el usuario y se solicita el registro del medio de pago...
						if (nuevo.Estado == 4 /* Pago Pendiente */ && planExistente.FlowPlanId != null) {
							Dictionary<string, string> atributosUsuario = await cognitoHelper.ObtenerUsuario(sub);
							string nombre = atributosUsuario.TryGetValue("given_name", out string? givenName) ? givenName : "";
							string apellido = atributosUsuario.TryGetValue("family_name", out string? familyName) ? familyName : "";
							string correo = atributosUsuario.TryGetValue("email", out string? email) ? email : throw new Exception("No se encuentra registro del correo electrónico del usuario.");

							// Se crea el usuario en el sistema interno si no existe...
							Usuario? usuarioExistente = await usuarioDao.Obtener(sub);
							if (usuarioExistente == null) {
								usuarioExistente = new Usuario() {
									Sub = sub
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

		private static IEndpointRouteBuilder MapCancelarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{idSuscripcion}", async (long idSuscripcion, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, ClaimsPrincipal user, SuscripcionDao suscripcionDao, FlowHelper flowHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					// Se valida que la suscripción exista, este vigente, pertenezca al cliente y que no esté "Cancelada"...
					Suscripcion? existente = await suscripcionDao.Obtener(idSuscripcion);
					if (existente == null || !existente.Vigencia || existente.Sub != sub || existente.Estado == 2 /* Cancelada */) {
						LambdaLogger.Log(
							$"[DELETE] - [Suscripcion] - [Cancelar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El ID de suscripción es inválido.");

						return Results.BadRequest($"El ID de suscripción es inválido.");
					}

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

					try {
						existente.Estado = 2; // Cancelada
						existente.FechaCancelacion = DateTime.UtcNow;
						await suscripcionDao.Actualizar(existente, transaction);

						if (existente.FlowSubscriptionId != null) {
							SalFlowSubscriptionCancel salFlowSubscriptionCancel = await flowHelper.SubscriptionCancel(existente.FlowSubscriptionId);
						}

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					LambdaLogger.Log(
						$"[DELETE] - [Suscripcion] - [Cancelar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Cancelación exitosa de la suscripción - ID Suscripcion: {idSuscripcion}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [Suscripcion] - [Cancelar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al cancelar la suscripción - ID Suscripcion: {idSuscripcion}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Suscripciones.Write.Self").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapWebhookEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/flow-webhook/{tipo}", async (string tipo, [FromForm] string token, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, EventoPagoDao eventoPagoDao, SuscripcionDao suscripcionDao, PlanDao planDao, PagoDao pagoDao, UsuarioDao usuarioDao, FlowHelper flowHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					EntSuscripcionWebhook entrada = new() {
						Token = token
					};

					// Se registra evento recepcionado en webhook...
					EventoPago eventoPago = new() { 
						Id = 0,
						Proveedor = "Flow",
						Evento = $"{tipo}Webhook",
						Payload = JsonSerializer.Serialize(entrada, AppJsonSerializerContext.Default.EntSuscripcionWebhook),
						Procesado = false,
						FechaCreacion = DateTime.UtcNow,
						FechaEliminacion = null,
						Vigencia = true,
					};
					eventoPago.Id = await eventoPagoDao.Insertar(eventoPago);

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

					try {
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

									if (suscripcionActivar != null && suscripcionActivar.FlowSubscriptionId == null) {
										Plan plan = planesVigentes.First(p => p.Id == suscripcionActivar.IdPlan);

										// Se crea suscripción en Flow...
										SalFlowSubscriptionCreate salFlowSubscriptionCreate = await flowHelper.SubscriptionCreate(plan.FlowPlanId!, usuario.FlowCustomerId!);
										if (salFlowSubscriptionCreate.Status == 1 /* Activa */) {
											suscripcionActivar.FlowSubscriptionId = salFlowSubscriptionCreate.SubscriptionId;
											await suscripcionDao.Actualizar(suscripcionActivar, transaction);
										}
									}
								}
							}
						} else if (tipo == "PlanCreate") {
							SalFlowPaymentGetStatus salFlow = await flowHelper.PaymentGetStatus(entrada.Token);
							if (salFlow.Status == 2 /* Pagada */) {
								string[] commerceOrderParts = salFlow.CommerceOrder!.Split('_');
								string flowSubscriptionId = $"{commerceOrderParts[0]}_{commerceOrderParts[1]}";
								string flowInvoiceId = commerceOrderParts[2];
								string flowInvoiceDate = commerceOrderParts[3];

								SalFlowInvoiceGet salFlowInvoiceGet = await flowHelper.InvoiceGet(flowInvoiceId);

								Suscripcion? suscripcion = await suscripcionDao.ObtenerPorFlowSubscriptionId(flowSubscriptionId);
								if (suscripcion != null) {
									Plan? plan = await planDao.Obtener(suscripcion.IdPlan);
									if (plan != null) {
										Pago? pagoExistente = await pagoDao.ObtenerPorFlow(suscripcion.FlowSubscriptionId!, flowInvoiceId);
										if (pagoExistente == null) {
											DateTime ahora = DateTime.UtcNow;

											// Se crea el pago en el sistema...
											Pago nuevoPago = new() {
												Id = 0,
												Sub = suscripcion.Sub,
												IdSuscripcion = suscripcion.Id,
												Monto = decimal.Parse(salFlow.Amount!, CultureInfo.InvariantCulture),
												Moneda = salFlow.Currency ?? "CLP",
												FechaPago = ahora,
												Estado = 1, // Pagado
												FlowSubscriptionId = suscripcion.FlowSubscriptionId!,
												FlowInvoiceId = flowInvoiceId,
												FechaCreacion = ahora,
												FechaEliminacion = null,
												Vigencia = true,
											};
											nuevoPago.Id = await pagoDao.Insertar(nuevoPago, transaction);

											// Se actualiza fecha de expiración de la suscripción...
											suscripcion.FechaInicio ??= ahora;
											DateTime fechaReferencia = suscripcion.FechaExpiracion == null ? suscripcion.FechaInicio.Value : (ahora > suscripcion.FechaExpiracion.Value ? ahora : suscripcion.FechaExpiracion.Value);
											
											// Nos aseguramos de que la fecha esté en UTC...
											DateTime fechaUTC = DateTime.SpecifyKind(fechaReferencia, DateTimeKind.Utc);

											// Se transforma la fecha a zona horaria de Chile...
											string timezone = "America/Santiago";
											if (OperatingSystem.IsWindows()) {
												timezone = TZConvert.IanaToWindows(timezone);
											}
											TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
											DateTime fechaTimezone = TimeZoneInfo.ConvertTimeFromUtc(fechaUTC, timeZoneInfo);

											// Se añade la duración en meses del plan...
											fechaTimezone = fechaTimezone.AddMonths(plan.DuracionMeses);

											// Se transforma de vuelta a UTC...
											fechaTimezone = TimeZoneInfo.ConvertTimeToUtc(fechaTimezone, timeZoneInfo);


											suscripcion.FechaExpiracion = fechaTimezone;
											suscripcion.Estado = 1 /* Activa */;
											await suscripcionDao.Actualizar(suscripcion, transaction);
										}
									}
								}
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
						$"Ejecución exitosa del webhook de suscripción - Tipo: {tipo}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [Suscripcion] - [Webhook] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la ejecución del webhook de suscripción  - Tipo: {tipo}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).AllowAnonymous().DisableAntiforgery().WithOpenApi();

			return routes;
		}
	}
}
