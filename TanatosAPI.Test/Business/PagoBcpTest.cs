using Npgsql;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;
using TanatosAPI.UseCases;

namespace TanatosAPI.Test.Business {
	public class PagoBcpTest {
		private readonly IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
		private readonly IPagoDao pagoDao = Substitute.For<IPagoDao>();
		private readonly PagoBcp pagoBcp;

		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

		public PagoBcpTest() {
			pagoBcp = new(dateTimeProvider, pagoDao);
		}

		public static Pago PagoDummy(
			long id = 1,
			string sub = "sub-test",
			long idSuscripcion = 100,
			decimal monto = 9990,
			string moneda = "CLP",
			DateTime? fechaPago = null,
			short estado = 1, // Pagado
			string flowSubscriptionId = "flow-subscription-id-test",
			string flowInvoiceId = "flow-invoice-id-test",
			DateTime? fechaCreacion = null,
			DateTime? fechaEliminacion = null,
			bool vigencia = true
		) => new() { 
			Id = id,
			Sub = sub,
			IdSuscripcion = idSuscripcion,
			Monto = monto,
			Moneda = moneda,
			FechaPago = fechaPago,
			Estado = estado,
			FlowSubscriptionId = flowSubscriptionId,
			FlowInvoiceId = flowInvoiceId,
			FechaCreacion = fechaCreacion ?? FECHA_DUMMY,
			FechaEliminacion = fechaEliminacion,
			Vigencia = vigencia
		};

		[Fact]
		public async Task ObtenerPorFlowTest() {
			pagoDao.ObtenerPorFlow("flow-subscription-id-test", "flow-invoice-id-test", Arg.Any<NpgsqlTransaction?>()).Returns(
				PagoDummy(id: 1, flowSubscriptionId: "flow-subscription-id-test", flowInvoiceId: "flow-invoice-id-test")
			);

			Pago? retorno = await pagoBcp.ObtenerPorFlow("flow-subscription-id-test", "flow-invoice-id-test");
			Assert.NotNull(retorno);
			Assert.Equal(1, retorno.Id);
			Assert.Equal("flow-subscription-id-test", retorno.FlowSubscriptionId);
			Assert.Equal("flow-invoice-id-test", retorno.FlowInvoiceId);
			await pagoDao.Received(1).ObtenerPorFlow("flow-subscription-id-test", "flow-invoice-id-test", Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task InsertarTest() {
			pagoDao.Insertar(Arg.Any<Pago>(), Arg.Any<NpgsqlTransaction?>()).Returns(100);

			Pago retorno = await pagoBcp.Insertar("sub-test", 10, 9990, "CLP", FECHA_DUMMY, "flow-subscription-id-test", "flow-invoice-id-test");
			Assert.Equal(100, retorno.Id);
			await pagoDao.Received(1).Insertar(
				Arg.Is<Pago>(p => 
					p.Sub == "sub-test" &&
					p.IdSuscripcion == 10 &&
					p.Monto == 9990 &&
					p.Moneda == "CLP" &&
					p.FechaPago == FECHA_DUMMY &&
					p.FlowSubscriptionId == "flow-subscription-id-test" &&
					p.FlowInvoiceId == "flow-invoice-id-test"
				), 
				Arg.Any<NpgsqlTransaction?>()
			);
		}
	}
}
