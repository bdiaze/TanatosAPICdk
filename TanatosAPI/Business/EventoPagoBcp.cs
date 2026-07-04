using Npgsql;
using System.Text.Json;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
	public class EventoPagoBcp(IDateTimeProvider dateTimeProvider, IEventoPagoDao eventoPagoDao) : IEventoPagoBcp {
		public async Task<EventoPago> Insertar(string proveedor, string evento, string payload, NpgsqlTransaction? transaction = null) {
			EventoPago eventoPago = new() {
				Id = 0,
				Proveedor = proveedor,
				Evento = evento,
				Payload = payload,
				Procesado = false,
				FechaCreacion = dateTimeProvider.UtcNow,
				FechaEliminacion = null,
				Vigencia = true,
			};
			eventoPago.Id = await eventoPagoDao.Insertar(eventoPago, transaction);
			return eventoPago;
		}


		public async Task MarcarComoProcesado(EventoPago eventoPago, NpgsqlTransaction? transaction = null) {
			if (!eventoPago.Procesado) {
				eventoPago.Procesado = true;
				await eventoPagoDao.Actualizar(eventoPago, transaction);
			}
		}
	}
}
