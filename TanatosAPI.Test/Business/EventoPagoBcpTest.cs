using Npgsql;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.UseCases;

namespace TanatosAPI.Test.Business {
	public class EventoPagoBcpTest {
		private readonly IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
		private readonly IEventoPagoDao eventoPagoDao = Substitute.For<IEventoPagoDao>();
		private readonly EventoPagoBcp eventoPagoBcp;

		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

		public EventoPagoBcpTest() {
			dateTimeProvider.UtcNow.Returns(FECHA_DUMMY);

			eventoPagoBcp = new(dateTimeProvider, eventoPagoDao);
		}

		public static EventoPago EventoPagoDummy(
			long id = 1,
			string proveedor = "proveedor-test",
			string evento = "evento-test",
			string payload = "{ 'payload': 'test' }",
			bool procesado = true,
			DateTime? fechaCreacion = null,
			DateTime? fechaEliminacion = null,
			bool vigencia = true
		) => new() { 
			Id = id,
			Proveedor = proveedor,
			Evento = evento,
			Payload = payload,
			Procesado = procesado,
			FechaCreacion = fechaCreacion ?? FECHA_DUMMY,
			FechaEliminacion = fechaEliminacion,
			Vigencia = vigencia
		};

		[Fact]
		public async Task InsertarTest() {
			eventoPagoDao.Insertar(Arg.Any<EventoPago>(), Arg.Any<NpgsqlTransaction?>()).Returns(100);

			EventoPago retorno = await eventoPagoBcp.Insertar("proveedor-test", "evento-test", "payload-test");
			Assert.Equal(100, retorno.Id);
			await eventoPagoDao.Received(1).Insertar(
				Arg.Is<EventoPago>(ep => 
					ep.Proveedor == "proveedor-test" && 
					ep.Evento == "evento-test" &&
					ep.Payload == "payload-test" &&
					ep.Procesado == false &&
					ep.FechaCreacion == FECHA_DUMMY
				), 
				Arg.Any<NpgsqlTransaction?>()
			);
		}

		[Fact]
		public async Task MarcarComoProcesado_Valido() {
			await eventoPagoBcp.MarcarComoProcesado(EventoPagoDummy(id: 100, procesado: false));

			await eventoPagoDao.Received(1).Actualizar(
				Arg.Is<EventoPago>(ep => 
					ep.Id == 100 &&
					ep.Procesado == true
				), 
				Arg.Any<NpgsqlTransaction?>()
			);
		}

		[Fact]
		public async Task MarcarComoProcesado_YaProcesado() {
			await eventoPagoBcp.MarcarComoProcesado(EventoPagoDummy(id: 100, procesado: true));

			await eventoPagoDao.DidNotReceive().Actualizar(Arg.Any<EventoPago>(), Arg.Any<NpgsqlTransaction?>());
		}
	}
}
