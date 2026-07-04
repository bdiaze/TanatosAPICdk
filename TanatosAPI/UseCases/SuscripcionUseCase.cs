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
		public async Task<List<Suscripcion>> ObtenerVigentesPorSubConPlan(string sub) {
			List<Suscripcion> suscripciones = await suscripcionBcp.ObtenerVigentesPorSub(sub);
			Dictionary<long, Plan> planes = (await planBcp.ObtenerTodos()).ToDictionary(p => p.Id, p => p);
			return [.. suscripciones.Select(s => {
				if (planes.TryGetValue(s.IdPlan, out Plan? plan)) s.Plan = plan;
				return s;
			})];
		}

		public async Task<string?> SuscribirseAPlan(string sub, long idPlan, NpgsqlTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			NpgsqlConnection? connection = null;

			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexion();
					transaction = await connection.BeginTransactionAsync();
				}

				Plan plan = await planBcp.ObtenerPorIdValidandoVigencia(idPlan, transaction);
				List<Suscripcion> suscripciones = await suscripcionBcp.ObtenerVigentesPorSub(sub, transaction);

				if (suscripcionBcp.AlgunaConPagoEnCurso(suscripciones)) {
					throw new ErrorValidacion(TipoErrorValidacion.YaExiste, "El usuario ya tiene una suscripción con pago en curso", "Ya cuentas con una suscripción activa.");
				}

				if (plan.SuscripcionUnica && suscripciones.Any(s => s.IdPlan == plan.Id)) {
					throw new ErrorValidacion(TipoErrorValidacion.YaExiste, "El usuario ya tiene una suscripción anterior con el mismo plan", "Ya te suscribiste con anterioridad a dicho plan.");
				}

				string? urlRedirect = null;

				await suscripcionBcp.EliminarCreacionNoConfirmada(suscripciones, transaction);

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

				Suscripcion nuevo = await suscripcionBcp.Crear(sub, plan.Id, fechaInicio, fechaExpiracion, estado, transaction);

				if (plan.FlowPlanId != null) {
					nuevo.FlowCustomerId = await usuarioBcp.RegistrarUsuarioEnFlow(sub, transaction);
					await suscripcionBcp.Modificar(nuevo, transaction);
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

		public async Task<List<Plan>> SuscribirseAPlanesGratuitos(string sub, NpgsqlTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			NpgsqlConnection? connection = null;

			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexion();
					transaction = await connection.BeginTransactionAsync();
				}

				List<Plan> planesGratuitos = await planBcp.ObtenerPlanesGratuitos(transaction);
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

		public async Task CancelarSuscripcion(string sub, long idSuscripcion, NpgsqlTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			NpgsqlConnection? connection = null;

			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexion();
					transaction = await connection.BeginTransactionAsync();
				}

				Suscripcion? existente = (await suscripcionBcp.ObtenerVigentesPorSub(sub, transaction)).FirstOrDefault(s => s.Id == idSuscripcion);
				if (existente != null && (existente.Estado == 1 /* Activa */ || existente.Estado == 4 /* Pago Pendiente */)) {
					await suscripcionBcp.Cancelar(existente, transaction);
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

		public async Task ProcesarWebhookFlowCustomerRegister(SalFlowCustomerGetRegisterStatus registerStatus, NpgsqlTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			NpgsqlConnection? connection = null;
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexion();
					transaction = await connection.BeginTransactionAsync();
				}

				if (registerStatus.Status?.Trim() == "1" /* Registrado */ && registerStatus.CustomerId != null) {
					Usuario? usuario = await usuarioBcp.ObtenerPorFlowCustomerId(registerStatus.CustomerId, transaction);
					if (usuario != null) {
						List<Suscripcion> suscripciones = await suscripcionBcp.ObtenerVigentesPorSub(usuario.Sub, transaction);
						List<Plan> planesVigentes = await planBcp.ObtenerVigentes(transaction);
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
							if (salFlowSubscriptionCreate.Status == 1 /* Activa */) {
								suscripcionActivar.Estado = 4; // Pago Pendiente
								suscripcionActivar.FlowSubscriptionId = salFlowSubscriptionCreate.SubscriptionId;
								await suscripcionBcp.Modificar(suscripcionActivar, transaction);
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

		public async Task ProcesarWebhookFlowPayment(SalFlowPaymentGetStatus paymentStatus, NpgsqlTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			NpgsqlConnection? connection = null;
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexion();
					transaction = await connection.BeginTransactionAsync();
				}

				if (paymentStatus.Status == 2 /* Pagada */) {
					string[] commerceOrderParts = paymentStatus.CommerceOrder!.Split('_');
					string flowSubscriptionId = $"{commerceOrderParts[0]}_{commerceOrderParts[1]}";
					string flowInvoiceId = commerceOrderParts[2];
					string flowInvoiceDate = commerceOrderParts[3];

					SalFlowInvoiceGet salFlowInvoiceGet = await flowHelper.InvoiceGet(flowInvoiceId);

					Suscripcion? suscripcion = await suscripcionBcp.ObtenerPorFlowSubscriptionId(flowSubscriptionId, transaction);
					if (suscripcion != null) {
						Plan? plan = await planBcp.ObtenerPorId(suscripcion.IdPlan, transaction);
						if (plan != null) {
							Pago? pagoExistente = await pagoBcp.ObtenerPorFlow(suscripcion.FlowSubscriptionId!, flowInvoiceId, transaction);
							if (pagoExistente == null) {
								DateTime ahora = dateTimeProvider.UtcNow;

								DateTime fechaPago = ahora;
								if (salFlowInvoiceGet.Payment?.PaymentData?.Date != null) {
									if (DateTime.TryParseExact(salFlowInvoiceGet.Payment?.PaymentData?.Date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime paymenteDataDate)) {
										fechaPago = DateTimeHelper.TransformarFechaTimezoneAUTC(paymenteDataDate);
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
									transaction
								);

								suscripcion.FechaInicio ??= suscripcionBcp.ProximaFechaSinSuscripcion(await suscripcionBcp.ObtenerVigentesPorSub(suscripcion.Sub, transaction));
								DateTime fechaReferencia = suscripcion.FechaExpiracion == null ? 
									suscripcion.FechaInicio!.Value : 
									(ahora > suscripcion.FechaExpiracion.Value ? ahora : suscripcion.FechaExpiracion.Value);
								suscripcion.FechaExpiracion = DateTimeHelper.SumarMeses(DateTime.SpecifyKind(fechaReferencia, DateTimeKind.Utc), plan.DuracionMeses);
								suscripcion.Estado = 1 /* Activa */;
								await suscripcionBcp.Modificar(suscripcion, transaction);
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
