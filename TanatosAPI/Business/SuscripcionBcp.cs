using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Logging;
using Npgsql;
using System.Transactions;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class SuscripcionBcp(IDateTimeProvider dateTimeProvider, ISuscripcionDao suscripcionDao, IFlowHelper flowHelper) : ISuscripcionBcp {
		public bool EstaVigente(Suscripcion? suscripcion) {
			return suscripcion != null && suscripcion.Vigencia;
		}

		public bool PerteneceAlUsuario(Suscripcion suscripcion, string sub) {
			return suscripcion.Sub == sub;
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
				(s.Estado == 4 /* Pago Pendiente */) &&
				(s.FechaInicio == null || s.FechaInicio.Value > fechaReferencia) 
			)];
		}

		public List<Suscripcion> FiltrarPagosEnCurso(List<Suscripcion> suscripciones, DateTime? fechaReferencia = null) {
			fechaReferencia ??= dateTimeProvider.UtcNow;
			List<Suscripcion> enCursoConFlowNoCancelada = [.. FiltrarEnCursoConFlow(suscripciones, fechaReferencia).Where(s => s.Estado != 2 /* Cancelada */)];
			List<Suscripcion> futuras = FiltrarFuturas(suscripciones, fechaReferencia);
			return [..enCursoConFlowNoCancelada, ..futuras];
		}

		public bool AlgunaConPagoEnCurso(List<Suscripcion> suscripciones, DateTime? fechaReferencia = null) {
			fechaReferencia ??= dateTimeProvider.UtcNow;
			return FiltrarPagosEnCurso(suscripciones, fechaReferencia).Count != 0;
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

		public async Task<bool> TienePlanEmpresa(string sub, NpgsqlTransaction? transaction = null) {
			// Se obtienen las suscripciones del usuario...
			List<Suscripcion> suscripciones = await suscripcionDao.ObtenerPorSub(sub, true, transaction);

			if (suscripciones.Any(s => s.FechaExpiracion != null && s.FechaExpiracion > dateTimeProvider.UtcNow)) {
				return true;
			}

			return false;
		}

		public async Task Cancelar(Suscripcion suscripcion, NpgsqlTransaction? transaction = null) {
			if (suscripcion.Estado != 2) {
				suscripcion.Estado = 2;
				suscripcion.FechaExpiracion = dateTimeProvider.UtcNow;

				if (suscripcion.FlowSubscriptionId != null) {
					await flowHelper.SubscriptionCancel(suscripcion.FlowSubscriptionId);
				}

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
	}
}
