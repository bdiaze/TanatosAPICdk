using Npgsql;
using System.Globalization;
using System.Text.Json;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Flow;
using TanatosAPI.Entities.Others.Suscripcion;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.UseCases {
	public class SuscripcionUseCase(IDatabaseConnectionHelper connectionHelper, IDateTimeProvider dateTimeProvider, ISuscripcionBcp suscripcionBcp, IPlanBcp planBcp, IUsuarioBcp usuarioBcp, IEventoPagoBcp eventoPagoBcp, IPagoBcp pagoBcp, IFlowHelper flowHelper) {

		// NombrePlan y PrecioPlan: Información de la suscripción en curso.
		// FechaExpiración: Solo para casos que no tienen renovación automática (en otras palabras, gratuitas o de pago canceladas).
		// FechaPróximoCobro: Solo para casos con renovación automática (en otras palabras, de pago activas, o pago pendiente).
		// RenovaciónAutomática: True para pagos en curso, false para todo lo demás.
		public async Task<(Plan? planEnCurso, Plan? planPagoEnCurso, DateTime? fechaExpiracion, DateTime? fechaProximoCobro, bool renovacionAutomatica)> ObtenerResumenSuscripcion(string sub) {
			DateTime now = dateTimeProvider.UtcNow;
			List<Suscripcion> suscripciones = await suscripcionBcp.ObtenerVigentesPorSub(sub);

			Suscripcion? enCurso = suscripcionBcp.FiltrarEnCurso(suscripciones, now).FirstOrDefault();
			Plan? planEnCurso = enCurso != null ? await planBcp.ObtenerPorId(enCurso.IdPlan) : null;

			Suscripcion? pagoEnCurso = suscripcionBcp.FiltrarPagosEnCurso(suscripciones, now).FirstOrDefault();
			Plan? planPagoEnCurso = pagoEnCurso != null ? await planBcp.ObtenerPorId(pagoEnCurso.IdPlan) : null;

			DateTime? expiracion = null;
			DateTime? proximoCobro = null;
			bool tienePagoEnCurso = false;
			if (suscripcionBcp.AlgunaConPagoEnCurso(suscripciones, now)) {
				tienePagoEnCurso = true;
				proximoCobro = suscripcionBcp.ProximaFechaCobro(suscripciones, now);
			} else {
				expiracion = suscripcionBcp.ProximaFechaExpiracion(suscripciones, now);
			}

			return (
				planEnCurso,
				planPagoEnCurso,
				expiracion,
				proximoCobro,
				tienePagoEnCurso
			);
		}
		
		public async Task<List<Suscripcion>> ObtenerVigentesPorSubConPlan(string sub) {
			List<Suscripcion> suscripciones = await suscripcionBcp.ObtenerVigentesPorSub(sub);
			Dictionary<long, Plan> planes = (await planBcp.ObtenerTodos()).ToDictionary(p => p.Id, p => p);
			return [.. suscripciones.Select(s => {
				if (planes.TryGetValue(s.IdPlan, out Plan? plan)) s.Plan = plan;
				return s;
			})];
		}

		public async Task<string?> SuscribirseAPlan(string sub, long idPlan, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;

			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				Plan plan = await planBcp.ObtenerPorIdValidandoVigencia(idPlan, transaction!.NpgsqlTransaction());
				List<Suscripcion> suscripciones = await suscripcionBcp.ObtenerVigentesPorSub(sub, transaction!.NpgsqlTransaction());

				if (suscripcionBcp.AlgunaConPagoEnCurso(suscripciones)) {
					throw new ErrorValidacion(TipoErrorValidacion.YaExiste, "El usuario ya tiene una suscripción con pago en curso", "Ya cuentas con una suscripción activa.");
				}

				if (plan.SuscripcionUnica && suscripciones.Any(s => s.IdPlan == plan.Id)) {
					throw new ErrorValidacion(TipoErrorValidacion.YaExiste, "El usuario ya tiene una suscripción anterior con el mismo plan", "Ya te suscribiste con anterioridad a dicho plan.");
				}

				string? urlRedirect = null;

				await suscripcionBcp.EliminarCreacionNoConfirmada(suscripciones, transaction!.NpgsqlTransaction());

				short estado;
				DateTime? fechaInicio;
				DateTime? fechaExpiracion;
				if (plan.FlowPlanId == null) {
					estado = 1; // Activa
					fechaInicio = DateTime.SpecifyKind(suscripcionBcp.ProximaFechaSinSuscripcion(suscripciones), DateTimeKind.Utc);
					fechaExpiracion = DateTimeHelper.SumarMeses(fechaInicio.Value, plan.DuracionMeses);
				} else {
					estado = 5; // En Creación
					fechaInicio = null;
					fechaExpiracion = null;
				}

				Suscripcion nuevo = await suscripcionBcp.Crear(sub, plan.Id, fechaInicio, fechaExpiracion, estado, transaction!.NpgsqlTransaction());

				if (plan.FlowPlanId != null) {
					nuevo.FlowCustomerId = await usuarioBcp.RegistrarUsuarioEnFlow(sub, transaction!.NpgsqlTransaction());
					await suscripcionBcp.Modificar(nuevo, transaction!.NpgsqlTransaction());
					urlRedirect = await usuarioBcp.RegistrarTarjetaEnFlow(nuevo.FlowCustomerId!);
				}

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}

				return urlRedirect;
			} catch {
				if (ownsTransaction && transaction != null) {
					await transaction.RollbackAsync();
				}
				throw;
			} finally {
				if (ownsTransaction) {
					if (transaction != null) await transaction.DisposeAsync();
					if (connection != null) await connection.DisposeAsync();
				}
			}
		}

		public async Task<List<Plan>> SuscribirseAPlanesGratuitos(string sub, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;

			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				List<Plan> planesGratuitos = await planBcp.ObtenerPlanesGratuitos(transaction!.NpgsqlTransaction());
				foreach (Plan plan in planesGratuitos) {
					await SuscribirseAPlan(sub, plan.Id, transaction);
				}

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}

				return planesGratuitos;
			} catch {
				if (ownsTransaction && transaction != null) {
					await transaction.RollbackAsync();
				}
				throw;
			} finally {
				if (ownsTransaction) {
					if (transaction != null) await transaction.DisposeAsync();
					if (connection != null) await connection.DisposeAsync();
				}
			}
		}

		public async Task CancelarSuscripcion(string sub, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;

			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				DateTime now = dateTimeProvider.UtcNow;
				List<Suscripcion> suscripciones = await suscripcionBcp.ObtenerVigentesPorSub(sub, transaction!.NpgsqlTransaction());
				if (suscripcionBcp.AlgunaConPagoEnCurso(suscripciones, now)) {
					List<Suscripcion> pagoEnCurso = suscripcionBcp.FiltrarPagosEnCurso(suscripciones, now);
					foreach (Suscripcion enCurso in pagoEnCurso) {
						await suscripcionBcp.Cancelar(enCurso, transaction!.NpgsqlTransaction());
					}
				}

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}
			} catch {
				if (ownsTransaction && transaction != null) {
					await transaction.RollbackAsync();
				}
				throw;
			} finally {
				if (ownsTransaction) {
					if (transaction != null) await transaction.DisposeAsync();
					if (connection != null) await connection.DisposeAsync();
				}
			}
		}

		public async Task<bool> ProcesarWebhookFlow(string tipo, string token) {
			ISalFlow retornoFlow;
			if (tipo == "CustomerRegister") {
				retornoFlow = await flowHelper.CustomerGetRegisterStatus(token);
			} else if (tipo == "PlanCreate") {
				retornoFlow = await flowHelper.PaymentGetStatus(token);
			} else {
				throw new ErrorValidacion(TipoErrorValidacion.TipoNoValido, "Tipo de webhook inválido");
			}

			EventoPago eventoPago = await eventoPagoBcp.Insertar(
				"Flow",
				$"{tipo}Webhook",
				JsonSerializer.Serialize(new EntSuscripcionWebhook() { Token = token }, AppJsonSerializerContext.Default.EntSuscripcionWebhook)
			);

			bool redirect = false;

			if (tipo == "CustomerRegister") {
				redirect = true;
				await ProcesarWebhookFlowCustomerRegister((SalFlowCustomerGetRegisterStatus) retornoFlow);
			} else if (tipo == "PlanCreate") {
				await ProcesarWebhookFlowPayment((SalFlowPaymentGetStatus)retornoFlow);
			}
			
			await eventoPagoBcp.MarcarComoProcesado(eventoPago);

			return redirect;
		}

		public async Task ProcesarWebhookFlowCustomerRegister(SalFlowCustomerGetRegisterStatus registerStatus, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				if (registerStatus.Status?.Trim() == "1" /* Registrado */ && registerStatus.CustomerId != null) {
					Usuario? usuario = await usuarioBcp.ObtenerPorFlowCustomerId(registerStatus.CustomerId, transaction!.NpgsqlTransaction());
					if (usuario != null) {
						List<Suscripcion> suscripciones = await suscripcionBcp.ObtenerVigentesPorSub(usuario.Sub, transaction!.NpgsqlTransaction());
						List<Plan> planesVigentes = await planBcp.ObtenerVigentes(transaction!.NpgsqlTransaction());
						Suscripcion? suscripcionActivar = suscripciones
							.Where(s => s.Estado == 5 /* En Creación */ && planesVigentes.Any(p => p.Id == s.IdPlan))
							.OrderByDescending(s => s.FechaCreacion)
							.FirstOrDefault();

						if (suscripcionActivar != null && suscripcionActivar.FlowSubscriptionId == null) {
							Plan plan = planesVigentes.First(p => p.Id == suscripcionActivar.IdPlan);

							DateTime? fechaInicio = suscripcionBcp.ProximaFechaExpiracion(suscripciones);
							if (fechaInicio != null) fechaInicio = DateTime.SpecifyKind(fechaInicio.Value, DateTimeKind.Utc);

							// Se crea suscripción en Flow...
							SalFlowSubscriptionCreate salFlowSubscriptionCreate = await flowHelper.SubscriptionCreate(plan.FlowPlanId!, usuario.FlowCustomerId!, fechaInicio);

							DateTime? fechaProximoCobro = null;
							if (salFlowSubscriptionCreate.NextInvoiceDate != null) {
								if (DateTime.TryParseExact(salFlowSubscriptionCreate.NextInvoiceDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime subscriptionNextInvoiceDate)) {
									fechaProximoCobro = DateTimeHelper.TransformarFechaTimezoneAUTC(subscriptionNextInvoiceDate);
								}
							}

							if (salFlowSubscriptionCreate.Status == 0 /* Inactivo */ || salFlowSubscriptionCreate.Status == 1 /* Activa */) {
								suscripcionActivar.Estado = 4; // Pago Pendiente
								suscripcionActivar.FlowSubscriptionId = salFlowSubscriptionCreate.SubscriptionId;
								suscripcionActivar.FechaProximoCobro = fechaProximoCobro;
								await suscripcionBcp.Modificar(suscripcionActivar, transaction!.NpgsqlTransaction());
							}
						}
					}
				}

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}
			} catch {
				if (ownsTransaction && transaction != null) {
					await transaction.RollbackAsync();
				}
				throw;
			} finally {
				if (ownsTransaction) {
					if (transaction != null) await transaction.DisposeAsync();
					if (connection != null) await connection.DisposeAsync();
				}
			}
		}

		public async Task ProcesarWebhookFlowPayment(SalFlowPaymentGetStatus paymentStatus, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				if (paymentStatus.Status == 2 /* Pagada */) {
					string[] commerceOrderParts = paymentStatus.CommerceOrder!.Split('_');
					string flowSubscriptionId = $"{commerceOrderParts[0]}_{commerceOrderParts[1]}";
					string flowInvoiceId = commerceOrderParts[2];
					string flowInvoiceDate = commerceOrderParts[3];

					Suscripcion? suscripcion = await suscripcionBcp.ObtenerPorFlowSubscriptionId(flowSubscriptionId, transaction!.NpgsqlTransaction());
					if (suscripcion != null) {
						Plan? plan = await planBcp.ObtenerPorId(suscripcion.IdPlan, transaction!.NpgsqlTransaction());
						if (plan != null) {
							Pago? pagoExistente = await pagoBcp.ObtenerPorFlow(suscripcion.FlowSubscriptionId!, flowInvoiceId, transaction!.NpgsqlTransaction());
							if (pagoExistente == null) {
								DateTime ahora = dateTimeProvider.UtcNow;

								SalFlowInvoiceGet salFlowInvoiceGet = await flowHelper.InvoiceGet(flowInvoiceId);
								DateTime fechaPago = ahora;
								if (salFlowInvoiceGet.Payment?.PaymentData?.Date != null) {
									if (DateTime.TryParseExact(salFlowInvoiceGet.Payment?.PaymentData?.Date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime paymenteDataDate)) {
										fechaPago = DateTimeHelper.TransformarFechaTimezoneAUTC(paymenteDataDate);
									}
								}

								SalFlowSubscriptionGet salFlowSubscriptionGet = await flowHelper.SubscriptionGet(flowSubscriptionId);
								DateTime? fechaProximoCobro = null;
								if (salFlowSubscriptionGet.NextInvoiceDate != null) {
									if (DateTime.TryParseExact(salFlowSubscriptionGet.NextInvoiceDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime subscriptionNextInvoiceDate)) {
										fechaProximoCobro = DateTimeHelper.TransformarFechaTimezoneAUTC(subscriptionNextInvoiceDate);
									}
								}

								// Se crea el pago en el sistema...
								 await pagoBcp.Insertar(
									suscripcion.Sub,
									suscripcion.Id,
									decimal.Parse(salFlowInvoiceGet.Amount!, CultureInfo.InvariantCulture),
									salFlowInvoiceGet.Currency ?? "CLP",
									fechaPago,
									suscripcion.FlowSubscriptionId!,
									flowInvoiceId,
									transaction!.NpgsqlTransaction()
								);

								suscripcion.FechaInicio ??= suscripcionBcp.ProximaFechaSinSuscripcion(await suscripcionBcp.ObtenerVigentesPorSub(suscripcion.Sub, transaction!.NpgsqlTransaction()));
								DateTime fechaReferencia = suscripcion.FechaExpiracion == null ? 
									suscripcion.FechaInicio!.Value : 
									(ahora > suscripcion.FechaExpiracion.Value ? ahora : suscripcion.FechaExpiracion.Value);
								suscripcion.FechaExpiracion = DateTimeHelper.SumarMeses(DateTime.SpecifyKind(fechaReferencia, DateTimeKind.Utc), plan.DuracionMeses);
								suscripcion.FechaProximoCobro = fechaProximoCobro;
								suscripcion.Estado = 1 /* Activa */;
								await suscripcionBcp.Modificar(suscripcion, transaction!.NpgsqlTransaction());
							}
						}
					}
				}

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}
			} catch {
				if (ownsTransaction && transaction != null) {
					await transaction.RollbackAsync();
				}
				throw;
			} finally {
				if (ownsTransaction) {
					if (transaction != null) await transaction.DisposeAsync();
					if (connection != null) await connection.DisposeAsync();
				}
			}
		}
	}
}
