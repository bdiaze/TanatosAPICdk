using Npgsql;
using Scriban.Runtime;
using System.Globalization;
using System.Net;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Hermes;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
	public class SuscripcionBcp(IVariableEntornoHelper variableEntorno, IDateTimeProvider dateTimeProvider, ISuscripcionDao suscripcionDao, IFlowHelper flowHelper, IHermesHelper hermesHelper, IHtmlRenderer renderer) : ISuscripcionBcp {
		public bool EstaVigente(Suscripcion? suscripcion) {
			return suscripcion != null && suscripcion.Vigencia;
		}

		public bool PerteneceAlUsuario(Suscripcion suscripcion, string sub) {
			return suscripcion.Sub == sub;
		}

		public List<Suscripcion> FiltrarExpiradas(List<Suscripcion> suscripciones, DateTime? fechaReferencia = null) {
			fechaReferencia ??= dateTimeProvider.UtcNow;
			return [.. suscripciones.Where(s =>
				(s.Estado == 1 /* Activa */ || s.Estado == 2 /* Cancelada */) &&
				s.FechaExpiracion != null &&
				s.FechaExpiracion.Value < fechaReferencia
			)];
		}

		public List<Suscripcion> FiltrarExpiradasConFlow(List<Suscripcion> suscripciones, DateTime? fechaReferencia = null) {
			fechaReferencia ??= dateTimeProvider.UtcNow;
			return [.. FiltrarExpiradas(suscripciones, fechaReferencia).Where(s => !string.IsNullOrEmpty(s.FlowSubscriptionId))];
		}

		public List<Suscripcion> FiltrarEnCurso(List<Suscripcion> suscripciones, DateTime? fechaReferencia = null) {
			fechaReferencia ??= dateTimeProvider.UtcNow;

			return [.. suscripciones.Where(s => 
				(s.Estado == 1 /* Activa */ || s.Estado == 2 /* Cancelada */) &&
				s.FechaInicio != null &&
				s.FechaInicio.Value <= fechaReferencia &&
				s.FechaExpiracion != null &&
				s.FechaExpiracion.Value >= fechaReferencia
			)];
		}

		public List<Suscripcion> FiltrarEnCursoConFlow(List<Suscripcion> suscripciones, DateTime? fechaReferencia = null) {
			fechaReferencia ??= dateTimeProvider.UtcNow;
			return [.. FiltrarEnCurso(suscripciones, fechaReferencia).Where(s => !string.IsNullOrEmpty(s.FlowSubscriptionId))];
		}

		public List<Suscripcion> FiltrarFuturas(List<Suscripcion> suscripciones, DateTime? fechaReferencia = null) {
			fechaReferencia ??= dateTimeProvider.UtcNow;

			return [.. suscripciones.Where(s =>
				(s.Estado == 1 /* Activa */ || s.Estado == 2 /* Cancelada */ || s.Estado == 4 /* Pago Pendiente */) &&
				(s.FechaInicio == null || s.FechaInicio.Value > fechaReferencia) 
			)];
		}

		public List<Suscripcion> FiltrarFuturasConFlow(List<Suscripcion> suscripciones, DateTime? fechaReferencia = null) {
			fechaReferencia ??= dateTimeProvider.UtcNow;
			return [.. FiltrarFuturas(suscripciones, fechaReferencia).Where(s => !string.IsNullOrEmpty(s.FlowSubscriptionId))];
		}

		public List<Suscripcion> FiltrarPagosEnCurso(List<Suscripcion> suscripciones, DateTime? fechaReferencia = null) {
			fechaReferencia ??= dateTimeProvider.UtcNow;
			List<Suscripcion> expiradasConFlowNoCancelada = [.. FiltrarExpiradasConFlow(suscripciones, fechaReferencia).Where(s => s.Estado != 2 /* Cancelada */)];
			List<Suscripcion> enCursoConFlowNoCancelada = [.. FiltrarEnCursoConFlow(suscripciones, fechaReferencia).Where(s => s.Estado != 2 /* Cancelada */)];
			List<Suscripcion> futurasConFlowNoCancelada = [.. FiltrarFuturasConFlow(suscripciones, fechaReferencia).Where(s => s.Estado != 2 /* Cancelada */)];
			return [.. expiradasConFlowNoCancelada, .. enCursoConFlowNoCancelada, .. futurasConFlowNoCancelada];
		}

		public bool AlgunaConPagoEnCurso(List<Suscripcion> suscripciones, DateTime? fechaReferencia = null) {
			fechaReferencia ??= dateTimeProvider.UtcNow;
			return FiltrarPagosEnCurso(suscripciones, fechaReferencia).Count != 0;
		}

		public DateTime? ProximaFechaCobro(List<Suscripcion> suscripciones, DateTime? fechaReferencia = null) {
			fechaReferencia ??= dateTimeProvider.UtcNow;
			List<Suscripcion> todas = FiltrarPagosEnCurso(suscripciones, fechaReferencia);
			if (!todas.Any(s => s.FechaProximoCobro != null)) {
				return null;
			}
			return todas.Where(s => s.FechaProximoCobro != null).Max(s => s.FechaProximoCobro!.Value);
		}

		public DateTime? ProximaFechaExpiracion(List<Suscripcion> suscripciones, DateTime? fechaReferencia = null) {
			fechaReferencia ??= dateTimeProvider.UtcNow;
			List<Suscripcion> enCurso = FiltrarEnCurso(suscripciones, fechaReferencia);
			List<Suscripcion> futuras = FiltrarFuturas(suscripciones, fechaReferencia);
			List<Suscripcion> todas = [.. enCurso, .. futuras];
			if (!todas.Any(s => s.FechaExpiracion != null)) {
				return null;
			}
			return todas.Where(s => s.FechaExpiracion != null).Max(s => s.FechaExpiracion!.Value);
		}

		public DateTime ProximaFechaSinSuscripcion(List<Suscripcion> suscripciones, DateTime? fechaReferencia = null) {
			fechaReferencia ??= dateTimeProvider.UtcNow; 
			return ProximaFechaExpiracion(suscripciones, fechaReferencia) ?? fechaReferencia.Value;
		}

		public async Task<List<Suscripcion>> ObtenerVigentesPorSub(string sub, NpgsqlTransaction? transaction = null) {
			return await suscripcionDao.ObtenerPorSub(sub, true, transaction);
		}

		public async Task<Suscripcion?> ObtenerPorFlowSubscriptionId(string flowSubscriptionId, NpgsqlTransaction? transaction = null) {
			return await suscripcionDao.ObtenerPorFlowSubscriptionId(flowSubscriptionId, transaction);
		}

		public bool TienePlanEmpresa(List<Suscripcion> suscripciones, DateTime? fechaReferencia = null) {
			return FiltrarEnCurso(suscripciones).Count != 0;
		}

		public async Task<bool> ConsultaTienePlanEmpresa(string sub, NpgsqlTransaction? transaction = null) {
			// Se obtienen las suscripciones del usuario...
			List<Suscripcion> suscripciones = await suscripcionDao.ObtenerPorSub(sub, true, transaction);
			return TienePlanEmpresa(suscripciones);
		}

		public async Task Cancelar(Suscripcion suscripcion, NpgsqlTransaction? transaction = null) {
			if (suscripcion.FlowSubscriptionId == null) {
				throw new ErrorValidacion(TipoErrorValidacion.TipoNoValido, "La suscripción no está asociada a un subscription de Flow", "No se puede cancelar una suscripción gratuita.");
			}

			if (suscripcion.Estado == 5 /* En Creación */) {
				throw new ErrorValidacion(TipoErrorValidacion.EstadoNoValido, "No se puede cancelar una suscripción en creación", "No se puede cancelar la suscripción dado su estado.");
			}

			if (suscripcion.Estado != 2) {
				suscripcion.Estado = 2; // Cancelada
				suscripcion.FechaCancelacion = dateTimeProvider.UtcNow;

				await flowHelper.SubscriptionCancel(suscripcion.FlowSubscriptionId);

				await suscripcionDao.Actualizar(suscripcion, transaction);
			}
		}

		public async Task Eliminar(Suscripcion suscripcion, NpgsqlTransaction? transaction = null) {
			if (suscripcion.Vigencia) {
				suscripcion.FechaEliminacion = dateTimeProvider.UtcNow;
				suscripcion.Vigencia = false;
				await suscripcionDao.Actualizar(suscripcion, transaction);
			}
		}

		public async Task EliminarCreacionNoConfirmada(List<Suscripcion> suscripciones, NpgsqlTransaction? transaction = null) {
			foreach (Suscripcion suscripcion in suscripciones.Where(s => s.Estado == 5 /* En Creación */)) {
				await Eliminar(suscripcion, transaction);
			}
		}

		public async Task<Suscripcion> Crear(string sub, long idPlan, DateTime? fechaInicio, DateTime? fechaExpiracion, short estado, NpgsqlTransaction? transaction = null) {
			Suscripcion nuevo = new() {
				Id = 0,
				Sub = sub,
				IdPlan = idPlan,
				FechaInicio = fechaInicio,
				FechaExpiracion = fechaExpiracion,
				FechaCancelacion = null,
				Estado = estado,
				FlowCustomerId = null,
				FlowSubscriptionId = null,
				FechaCreacion = dateTimeProvider.UtcNow,
				FechaEliminacion = null,
				Vigencia = true
			};
			nuevo.Id = await suscripcionDao.Insertar(nuevo, transaction);
			return nuevo;
		}

		public async Task Modificar(Suscripcion suscripcion, NpgsqlTransaction? transaction = null) {
			await suscripcionDao.Actualizar(suscripcion, transaction);
		}

		public async Task<string> EnviarNotificacionContratacion(string correoUsuario, string? nombreUsuario, Plan plan, DateTime? fechaInicioUtc) {
			string cadaCuanto = plan.DuracionMeses switch {
				1 => "mensuales",
				12 => "anuales",
				_ => $"cada {plan.DuracionMeses} meses"
			};

			DateTime? fechaInicioChile = fechaInicioUtc != null ? DateTimeHelper.TransformarFechaUTCATimezone(fechaInicioUtc.Value) : null;

			NumberFormatInfo formatoNumero = new() {
				NumberGroupSeparator = ".",
				NumberDecimalSeparator = ","
			};

			SalHermesEnviar retorno = await hermesHelper.EnviarCorreo(new EntHermesCorreoEnviar() {
				De = new DireccionCorreo() {
					Nombre = variableEntorno.Obtener("HERMES_DE_NOMBRE"),
					Correo = variableEntorno.Obtener("HERMES_DE_CORREO"),
				},
				Para = [
					new DireccionCorreo() {
						Correo = correoUsuario
					}
				],
				Asunto = $"Contratación de {plan.Nombre} - Todo en Orden",
				Cuerpo = await renderer.GenerarHtml("ContratacionPlan.html", new ScriptObject() {
					["NOMBRE_USUARIO"] = nombreUsuario != null ? WebUtility.HtmlEncode(nombreUsuario) : null,
					["NOMBRE_PLAN"] = WebUtility.HtmlEncode(plan.Nombre),
					["PRECIO_PLAN"] = WebUtility.HtmlEncode(plan.Precio.ToString("N0", formatoNumero)),
					["CADA_CUANTO"] = WebUtility.HtmlEncode(cadaCuanto),
					["FECHA_INICIO"] = fechaInicioChile != null ? WebUtility.HtmlEncode(fechaInicioChile.Value.ToString("dd/MM/yyyy HH:mm")) : null,
				})
			});

			return retorno.IdMensaje;
		}
	}
}
