using Npgsql;
using System.Globalization;
using System.Transactions;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
	public class PagoBcp(IDateTimeProvider dateTimeProvider, IPagoDao pagoDao) : IPagoBcp {
		public async Task<Pago?> ObtenerPorFlow(string flowSubscriptionId, string flowInvoiceId, NpgsqlTransaction? transaction = null) {
			return await pagoDao.ObtenerPorFlow(flowSubscriptionId, flowInvoiceId, transaction);
		}

		public async Task<Pago> Insertar(string sub, long idSuscripcion, decimal monto, string moneda, DateTime fechaPago, string flowSubscriptionId, string flowInvoiceId, NpgsqlTransaction? transaction = null) {
			Pago nuevoPago = new() {
				Id = 0,
				Sub = sub,
				IdSuscripcion = idSuscripcion,
				Monto = monto,
				Moneda = moneda,
				FechaPago = fechaPago,
				Estado = 1, // Pagado
				FlowSubscriptionId = flowSubscriptionId,
				FlowInvoiceId = flowInvoiceId,
				FechaCreacion = dateTimeProvider.UtcNow,
				FechaEliminacion = null,
				Vigencia = true,
			};
			nuevoPago.Id = await pagoDao.Insertar(nuevoPago, transaction);
			return nuevoPago;
		}
	}
}
