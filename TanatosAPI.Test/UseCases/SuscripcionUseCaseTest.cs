using Npgsql;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Scriban.Parsing;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Flow;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Repositories;
using TanatosAPI.Test.Business;
using TanatosAPI.UseCases;

namespace TanatosAPI.Test.UseCases {
	public class SuscripcionUseCaseTest {
		private readonly IDatabaseConnectionHelper connectionHelper = Substitute.For<IDatabaseConnectionHelper>();
		private readonly IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
		private readonly ISuscripcionBcp suscripcionBcp = Substitute.For<ISuscripcionBcp>();
		private readonly IPlanBcp planBcp = Substitute.For<IPlanBcp>();
		private readonly IUsuarioBcp usuarioBcp = Substitute.For<IUsuarioBcp>();
		private readonly IEventoPagoBcp eventoPagoBcp = Substitute.For<IEventoPagoBcp>();
		private readonly IPagoBcp pagoBcp = Substitute.For<IPagoBcp>();
		private readonly IFlowHelper flowHelper = Substitute.For<IFlowHelper>();
		private readonly SuscripcionUseCase suscripcionUseCase;

		private readonly IDatabaseConnection connection = Substitute.For<IDatabaseConnection>();
		private readonly IDatabaseTransaction transaction = Substitute.For<IDatabaseTransaction>();

		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

		public SuscripcionUseCaseTest() {
			dateTimeProvider.UtcNow.Returns(FECHA_DUMMY);

			connection.BeginTransactionAsync().Returns(transaction);
			connectionHelper.ObtenerConexionWrapper().Returns(connection);

			suscripcionUseCase = new(connectionHelper, dateTimeProvider, suscripcionBcp, planBcp, usuarioBcp, eventoPagoBcp, pagoBcp, flowHelper);
		}

		[Fact]
		public async Task ObtenerVigentesPorSubConPlanTest() {
			suscripcionBcp.ObtenerVigentesPorSub("sub-test").Returns([
				SuscripcionBcpTest.SuscripcionDummy(id: 1, idPlan: 1),
				SuscripcionBcpTest.SuscripcionDummy(id: 2, idPlan: 1),
				SuscripcionBcpTest.SuscripcionDummy(id: 3, idPlan: 2),
				SuscripcionBcpTest.SuscripcionDummy(id: 4, idPlan: 3),
			]);
			planBcp.ObtenerTodos().Returns([
				PlanBcpTest.PlanDummy(id: 1, vigencia: true),
				PlanBcpTest.PlanDummy(id: 2, vigencia: false),
			]);

			List<Suscripcion> retorno = await suscripcionUseCase.ObtenerVigentesPorSubConPlan("sub-test");
			Assert.Equal(4, retorno.Count);
			Assert.All(retorno, s => {
				if (s.IdPlan == 3) {
					Assert.Null(s.Plan);
				} else if (s.IdPlan == 2) {
					Assert.NotNull(s.Plan);
					Assert.Equal(2, s.Plan.Id);
					Assert.False(s.Plan.Vigencia);
				} else if (s.IdPlan == 1) {
					Assert.NotNull(s.Plan);
					Assert.Equal(1, s.Plan.Id);
					Assert.True(s.Plan.Vigencia);
				}
			});
		}

		[Fact]
		public async Task SuscribirseAPlanTest_ConPagoEnCurso() {
			planBcp.ObtenerPorIdValidandoVigencia(Arg.Any<long>(), Arg.Any<NpgsqlTransaction?>()).Returns(PlanBcpTest.PlanDummy(id: 10));
			suscripcionBcp.ObtenerVigentesPorSub(Arg.Any<string>(), Arg.Any<NpgsqlTransaction?>()).Returns([]);
			suscripcionBcp.AlgunaConPagoEnCurso(Arg.Any<List<Suscripcion>>()).Returns(true);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => suscripcionUseCase.SuscribirseAPlan("sub-test", 10));
			Assert.Equal(TipoErrorValidacion.YaExiste, ex.TipoErrorValidacion);
			await connection.Received(1).BeginTransactionAsync();
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task SuscribirseAPlanTest_SuscripcionUnicaExistente() {
			planBcp.ObtenerPorIdValidandoVigencia(Arg.Any<long>(), Arg.Any<NpgsqlTransaction?>()).Returns(PlanBcpTest.PlanDummy(id: 10, suscripcionUnica: true));
			suscripcionBcp.ObtenerVigentesPorSub(Arg.Any<string>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				SuscripcionBcpTest.SuscripcionDummy(idPlan: 10, fechaInicio: FECHA_DUMMY.AddDays(-45), fechaExpiracion: FECHA_DUMMY.AddDays(-15))
			]);
			suscripcionBcp.AlgunaConPagoEnCurso(Arg.Any<List<Suscripcion>>()).Returns(false);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => suscripcionUseCase.SuscribirseAPlan("sub-test", 10));
			Assert.Equal(TipoErrorValidacion.YaExiste, ex.TipoErrorValidacion);
			await connection.Received(1).BeginTransactionAsync();
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task SuscribirseAPlanTest_SuscripcionUnicaNueva() {
			planBcp.ObtenerPorIdValidandoVigencia(Arg.Any<long>(), Arg.Any<NpgsqlTransaction?>()).Returns(PlanBcpTest.PlanDummy(id: 10, suscripcionUnica: true, flowPlanId: null, duracionMeses: 1));
			suscripcionBcp.ObtenerVigentesPorSub(Arg.Any<string>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				SuscripcionBcpTest.SuscripcionDummy(idPlan: 5, fechaInicio: FECHA_DUMMY.AddDays(-45), fechaExpiracion: FECHA_DUMMY.AddDays(-15))
			]);
			suscripcionBcp.AlgunaConPagoEnCurso(Arg.Any<List<Suscripcion>>()).Returns(false);
			suscripcionBcp.ProximaFechaSinSuscripcion(Arg.Any<List<Suscripcion>>()).Returns(FECHA_DUMMY);

			string? retorno = await suscripcionUseCase.SuscribirseAPlan("sub-test", 10);
			Assert.Null(retorno);
			await connection.Received(1).BeginTransactionAsync();
			await suscripcionBcp.Received(1).EliminarCreacionNoConfirmada(Arg.Any<List<Suscripcion>>(), Arg.Any<NpgsqlTransaction?>());
			await suscripcionBcp.Received(1).Crear(Arg.Any<string>(), 10, FECHA_DUMMY, FECHA_DUMMY.AddMonths(1), 1, Arg.Any<NpgsqlTransaction?>());
			await usuarioBcp.DidNotReceive().RegistrarUsuarioEnFlow(Arg.Any<string>(), Arg.Any<NpgsqlTransaction?>());
			await suscripcionBcp.DidNotReceive().Modificar(Arg.Any<Suscripcion>(), Arg.Any<NpgsqlTransaction?>());
			await usuarioBcp.DidNotReceive().RegistrarTarjetaEnFlow(Arg.Any<string>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task SuscribirseAPlanTest_PlanSinFlow() {
			planBcp.ObtenerPorIdValidandoVigencia(Arg.Any<long>(), Arg.Any<NpgsqlTransaction?>()).Returns(PlanBcpTest.PlanDummy(id: 10, flowPlanId: null, duracionMeses: 1));
			suscripcionBcp.ObtenerVigentesPorSub(Arg.Any<string>(), Arg.Any<NpgsqlTransaction?>()).Returns([]);
			suscripcionBcp.AlgunaConPagoEnCurso(Arg.Any<List<Suscripcion>>()).Returns(false);
			suscripcionBcp.ProximaFechaSinSuscripcion(Arg.Any<List<Suscripcion>>()).Returns(FECHA_DUMMY);

			string? retorno = await suscripcionUseCase.SuscribirseAPlan("sub-test", 10);
			Assert.Null(retorno);
			await connection.Received(1).BeginTransactionAsync();
			await suscripcionBcp.Received(1).EliminarCreacionNoConfirmada(Arg.Any<List<Suscripcion>>(), Arg.Any<NpgsqlTransaction?>());
			await suscripcionBcp.Received(1).Crear(Arg.Any<string>(), 10, FECHA_DUMMY, FECHA_DUMMY.AddMonths(1), 1 /* Activa */, Arg.Any<NpgsqlTransaction?>());
			await usuarioBcp.DidNotReceive().RegistrarUsuarioEnFlow(Arg.Any<string>(), Arg.Any<NpgsqlTransaction?>());
			await suscripcionBcp.DidNotReceive().Modificar(Arg.Any<Suscripcion>(), Arg.Any<NpgsqlTransaction?>());
			await usuarioBcp.DidNotReceive().RegistrarTarjetaEnFlow(Arg.Any<string>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact] 
		public async Task SuscribirseAPlanTest_PlanConFlow() {
			planBcp.ObtenerPorIdValidandoVigencia(10, Arg.Any<NpgsqlTransaction?>()).Returns(PlanBcpTest.PlanDummy(id: 10, flowPlanId: "flow-plan-id-test", duracionMeses: 1));
			suscripcionBcp.ObtenerVigentesPorSub("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns([]);
			suscripcionBcp.AlgunaConPagoEnCurso(Arg.Any<List<Suscripcion>>()).Returns(false);
			suscripcionBcp.Crear("sub-test", 10, null, null, 5 /* En Creacipon */, Arg.Any<NpgsqlTransaction?>()).Returns(
				SuscripcionBcpTest.SuscripcionDummy(id: 100, sub: "sub-test", idPlan: 10, fechaInicio: null, fechaExpiracion: null, estado: 5, flowCustomerId: null, flowSubscriptionId: null)
			);
			usuarioBcp.RegistrarUsuarioEnFlow("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns("flow-customer-id-test");
			usuarioBcp.RegistrarTarjetaEnFlow("flow-customer-id-test").Returns("url-redirect-test");

			string? retorno = await suscripcionUseCase.SuscribirseAPlan("sub-test", 10);
			Assert.Equal("url-redirect-test", retorno);
			await connection.Received(1).BeginTransactionAsync();
			await suscripcionBcp.Received(1).EliminarCreacionNoConfirmada(Arg.Any<List<Suscripcion>>(), Arg.Any<NpgsqlTransaction?>());
			await suscripcionBcp.Received(1).Crear("sub-test", 10, null, null, 5 /* En Creación */, Arg.Any<NpgsqlTransaction?>());
			await usuarioBcp.Received(1).RegistrarUsuarioEnFlow("sub-test", Arg.Any<NpgsqlTransaction?>());
			await suscripcionBcp.Received(1).Modificar(Arg.Is<Suscripcion>(s => s.FlowCustomerId == "flow-customer-id-test"), Arg.Any<NpgsqlTransaction?>());
			await usuarioBcp.Received(1).RegistrarTarjetaEnFlow("flow-customer-id-test");
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task SuscribirseAPlanesGratuitosTest() {
			planBcp.ObtenerPlanesGratuitos(Arg.Any<NpgsqlTransaction?>()).Returns([
				PlanBcpTest.PlanDummy(id: 1, precio: 0, duracionMeses: 1, suscripcionUnica: true, flowPlanId: null),
				PlanBcpTest.PlanDummy(id: 2, precio: 0, duracionMeses: 2, suscripcionUnica: true, flowPlanId: null),
				PlanBcpTest.PlanDummy(id: 3, precio: 0, duracionMeses: 3, suscripcionUnica: true, flowPlanId: null)
			]);
			planBcp.ObtenerPorIdValidandoVigencia(1, Arg.Any<NpgsqlTransaction?>()).Returns(PlanBcpTest.PlanDummy(id: 1, precio: 0, duracionMeses: 1, suscripcionUnica: true, flowPlanId: null));
			planBcp.ObtenerPorIdValidandoVigencia(2, Arg.Any<NpgsqlTransaction?>()).Returns(PlanBcpTest.PlanDummy(id: 2, precio: 0, duracionMeses: 1, suscripcionUnica: true, flowPlanId: null));
			planBcp.ObtenerPorIdValidandoVigencia(3, Arg.Any<NpgsqlTransaction?>()).Returns(PlanBcpTest.PlanDummy(id: 3, precio: 0, duracionMeses: 1, suscripcionUnica: true, flowPlanId: null));
			suscripcionBcp.ObtenerVigentesPorSub(Arg.Any<string>(), Arg.Any<NpgsqlTransaction?>()).Returns([]);
			suscripcionBcp.AlgunaConPagoEnCurso(Arg.Any<List<Suscripcion>>()).Returns(false);
			suscripcionBcp.ProximaFechaSinSuscripcion(Arg.Any<List<Suscripcion>>()).Returns(FECHA_DUMMY);

			List<Plan> retorno = await suscripcionUseCase.SuscribirseAPlanesGratuitos("sub-test");
			Assert.Equal(3, retorno.Count);
			await connection.Received(1).BeginTransactionAsync();
			await suscripcionBcp.Received(3).EliminarCreacionNoConfirmada(Arg.Any<List<Suscripcion>>(), Arg.Any<NpgsqlTransaction?>());
			await suscripcionBcp.Received(1).Crear(Arg.Any<string>(), 1, FECHA_DUMMY, FECHA_DUMMY.AddMonths(1), 1 /* Activa */, Arg.Any<NpgsqlTransaction?>());
			await suscripcionBcp.Received(1).Crear(Arg.Any<string>(), 2, FECHA_DUMMY, FECHA_DUMMY.AddMonths(1), 1 /* Activa */, Arg.Any<NpgsqlTransaction?>());
			await suscripcionBcp.Received(1).Crear(Arg.Any<string>(), 3, FECHA_DUMMY, FECHA_DUMMY.AddMonths(1), 1 /* Activa */, Arg.Any<NpgsqlTransaction?>());
			await usuarioBcp.DidNotReceive().RegistrarUsuarioEnFlow(Arg.Any<string>(), Arg.Any<NpgsqlTransaction?>());
			await suscripcionBcp.DidNotReceive().Modificar(Arg.Any<Suscripcion>(), Arg.Any<NpgsqlTransaction?>());
			await usuarioBcp.DidNotReceive().RegistrarTarjetaEnFlow(Arg.Any<string>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task SuscribirseAPlanesGratuitosTest_Invalido() {
			planBcp.ObtenerPlanesGratuitos(Arg.Any<NpgsqlTransaction?>()).Returns([
				PlanBcpTest.PlanDummy(id: 1, precio: 0, duracionMeses: 1, suscripcionUnica: true, flowPlanId: null),
			]);
			planBcp.ObtenerPorIdValidandoVigencia(1, Arg.Any<NpgsqlTransaction?>()).Returns(PlanBcpTest.PlanDummy(id: 1, precio: 0, duracionMeses: 1, suscripcionUnica: true, flowPlanId: null));
			suscripcionBcp.ObtenerVigentesPorSub(Arg.Any<string>(), Arg.Any<NpgsqlTransaction?>()).Returns([]);
			suscripcionBcp.AlgunaConPagoEnCurso(Arg.Any<List<Suscripcion>>()).Returns(true);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => suscripcionUseCase.SuscribirseAPlanesGratuitos("sub-test"));
			Assert.Equal(TipoErrorValidacion.YaExiste, ex.TipoErrorValidacion);
			await connection.Received(1).BeginTransactionAsync();
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task CancelarSuscripcionTest() {
			suscripcionBcp.ObtenerVigentesPorSub(Arg.Any<string>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				SuscripcionBcpTest.SuscripcionDummy(id: 100, fechaInicio: FECHA_DUMMY.AddDays(-15), fechaExpiracion: FECHA_DUMMY.AddDays(15))
			]);
			suscripcionBcp.AlgunaConPagoEnCurso(Arg.Any<List<Suscripcion>>(), Arg.Any<DateTime?>()).Returns(true);
			suscripcionBcp.FiltrarPagosEnCurso(Arg.Any<List<Suscripcion>>(), Arg.Any<DateTime?>()).Returns([
				SuscripcionBcpTest.SuscripcionDummy(id: 100, fechaInicio: FECHA_DUMMY.AddDays(-15), fechaExpiracion: FECHA_DUMMY.AddDays(15))
			]);

			await suscripcionUseCase.CancelarSuscripcion("sub-test");
			await connection.Received(1).BeginTransactionAsync();
			await suscripcionBcp.Received(1).Cancelar(Arg.Is<Suscripcion>(s => s.Id == 100), Arg.Any<NpgsqlTransaction?>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task CancelarSuscripcionTest_Invalido() {
			suscripcionBcp.ObtenerVigentesPorSub(Arg.Any<string>(), Arg.Any<NpgsqlTransaction?>()).ThrowsAsync<Exception>();

			await Assert.ThrowsAsync<Exception>(() => suscripcionUseCase.CancelarSuscripcion("sub-test"));
			await connection.Received(1).BeginTransactionAsync();
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ProcesarWebhookFlowCustomerRegisterTest_Valido() {
			usuarioBcp.ObtenerPorFlowCustomerId("flow-customer-id-test", Arg.Any<NpgsqlTransaction?>()).Returns(UsuarioBcpTest.UsuarioDummy(sub: "sub-test", flowCustomerId: "flow-customer-id-test"));
			suscripcionBcp.ObtenerVigentesPorSub("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns([
				SuscripcionBcpTest.SuscripcionDummy(id: 100, sub: "sub-test", idPlan: 10, estado: 5 /* En Creación */, flowCustomerId: "flow-customer-id-test", flowSubscriptionId: null),
			]);
			planBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([
				PlanBcpTest.PlanDummy(id: 10, flowPlanId: "flow-plan-id-test")
			]);
			suscripcionBcp.ProximaFechaExpiracion(Arg.Any<List<Suscripcion>>()).Returns((DateTime?)null);
			flowHelper.SubscriptionCreate("flow-plan-id-test", "flow-customer-id-test", Arg.Any<DateTime?>()).Returns(new SalFlowSubscriptionCreate() {
				Status = 1, // Activa
				SubscriptionId = "flow-subscription-id-test"
			});

			await suscripcionUseCase.ProcesarWebhookFlowCustomerRegister(new SalFlowCustomerGetRegisterStatus() {
				CustomerId = "flow-customer-id-test",
				Status = "1" // Registrado
			});
			await connection.Received(1).BeginTransactionAsync();
			await flowHelper.Received(1).SubscriptionCreate("flow-plan-id-test", "flow-customer-id-test", Arg.Any<DateTime?>());
			await suscripcionBcp.Received(1).Modificar(
				Arg.Is<Suscripcion>(s => 
					s.Id == 100 && 
					s.Estado == 4 /* Pago Pendiente */ && 
					s.FlowSubscriptionId == "flow-subscription-id-test"
				), 
				Arg.Any<NpgsqlTransaction?>()
			);
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ProcesarWebhookFlowCustomerRegisterTest_Invalido() {
			usuarioBcp.ObtenerPorFlowCustomerId("flow-customer-id-test", Arg.Any<NpgsqlTransaction?>()).ThrowsAsync<Exception>();

			await Assert.ThrowsAsync<Exception>(() => suscripcionUseCase.ProcesarWebhookFlowCustomerRegister(new SalFlowCustomerGetRegisterStatus() {
				CustomerId = "flow-customer-id-test",
				Status = "1" // Registrado
			}));
			await connection.Received(1).BeginTransactionAsync();
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}


		[Fact]
		public async Task ProcesarWebhookFlowCustomerRegisterTest_RegisterStatusInvalido() {
			await suscripcionUseCase.ProcesarWebhookFlowCustomerRegister(new SalFlowCustomerGetRegisterStatus() {
				CustomerId = "flow-customer-id-test",
				Status = "0" // No registrado
			});
			await connection.Received(1).BeginTransactionAsync();
			await flowHelper.DidNotReceive().SubscriptionCreate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime?>());
			await suscripcionBcp.DidNotReceive().Modificar(Arg.Any<Suscripcion>(),	Arg.Any<NpgsqlTransaction?>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ProcesarWebhookFlowCustomerRegisterTest_UsuarioNoExistente() {
			usuarioBcp.ObtenerPorFlowCustomerId("flow-customer-id-test", Arg.Any<NpgsqlTransaction?>()).Returns((Usuario?)null);
			
			await suscripcionUseCase.ProcesarWebhookFlowCustomerRegister(new SalFlowCustomerGetRegisterStatus() {
				CustomerId = "flow-customer-id-test",
				Status = "1" // Registrado
			});
			await connection.Received(1).BeginTransactionAsync();
			await flowHelper.DidNotReceive().SubscriptionCreate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime?>());
			await suscripcionBcp.DidNotReceive().Modificar(Arg.Any<Suscripcion>(), Arg.Any<NpgsqlTransaction?>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ProcesarWebhookFlowCustomerRegisterTest_SinSuscripcion() {
			usuarioBcp.ObtenerPorFlowCustomerId("flow-customer-id-test", Arg.Any<NpgsqlTransaction?>()).Returns(UsuarioBcpTest.UsuarioDummy(sub: "sub-test", flowCustomerId: "flow-customer-id-test"));
			suscripcionBcp.ObtenerVigentesPorSub("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns([]);
			planBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([
				PlanBcpTest.PlanDummy(id: 10, flowPlanId: "flow-plan-id-test")
			]);

			await suscripcionUseCase.ProcesarWebhookFlowCustomerRegister(new SalFlowCustomerGetRegisterStatus() {
				CustomerId = "flow-customer-id-test",
				Status = "1" // Registrado
			});
			await connection.Received(1).BeginTransactionAsync();
			await flowHelper.DidNotReceive().SubscriptionCreate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime?>());
			await suscripcionBcp.DidNotReceive().Modificar(Arg.Any<Suscripcion>(), Arg.Any<NpgsqlTransaction?>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ProcesarWebhookFlowCustomerRegisterTest_FlowSubscriptionEstadoInvalido() {
			usuarioBcp.ObtenerPorFlowCustomerId("flow-customer-id-test", Arg.Any<NpgsqlTransaction?>()).Returns(UsuarioBcpTest.UsuarioDummy(sub: "sub-test", flowCustomerId: "flow-customer-id-test"));
			suscripcionBcp.ObtenerVigentesPorSub("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns([
				SuscripcionBcpTest.SuscripcionDummy(id: 100, sub: "sub-test", idPlan: 10, estado: 5 /* En Creación */, flowCustomerId: "flow-customer-id-test", flowSubscriptionId: null),
			]);
			planBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([
				PlanBcpTest.PlanDummy(id: 10, flowPlanId: "flow-plan-id-test")
			]);
			suscripcionBcp.ProximaFechaExpiracion(Arg.Any<List<Suscripcion>>()).Returns((DateTime?)null);
			flowHelper.SubscriptionCreate("flow-plan-id-test", "flow-customer-id-test", Arg.Any<DateTime?>()).Returns(new SalFlowSubscriptionCreate() {
				Status = 0, // Inactiva
				SubscriptionId = "flow-subscription-id-test"
			});

			await suscripcionUseCase.ProcesarWebhookFlowCustomerRegister(new SalFlowCustomerGetRegisterStatus() {
				CustomerId = "flow-customer-id-test",
				Status = "1" // Registrado
			});
			await connection.Received(1).BeginTransactionAsync();
			await flowHelper.Received(1).SubscriptionCreate("flow-plan-id-test", "flow-customer-id-test", Arg.Any<DateTime?>());
			await suscripcionBcp.DidNotReceive().Modificar(Arg.Any<Suscripcion>(), Arg.Any<NpgsqlTransaction?>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ProcesarWebhookFlowPaymentTest_Valido() {
			flowHelper.InvoiceGet("flow-invoice-id-test").Returns(new SalFlowInvoiceGet() {
				Amount = "9990",
				Currency = "CLP",
				Payment = new SalFlowPaymentGetStatus() {
					PaymentData = new SalFlowPaymentData() {
						Date = "2020-06-01 12:30:15" // Formato: yyyy-MM-dd HH:mm:ss - UTC: 2020-06-01 16:30:15
					}
				}
			});
			suscripcionBcp.ObtenerPorFlowSubscriptionId("sus_flow-subscription-id-test", Arg.Any<NpgsqlTransaction?>()).Returns(
				SuscripcionBcpTest.SuscripcionDummy(
					id: 100, sub: "sub-test", idPlan: 10, flowSubscriptionId: "sus_flow-subscription-id-test", 
					fechaInicio: FECHA_DUMMY.AddDays(-15), fechaExpiracion: FECHA_DUMMY.AddDays(15)
				)
			);
			planBcp.ObtenerPorId(10, Arg.Any<NpgsqlTransaction?>()).Returns(PlanBcpTest.PlanDummy(id: 10));
			pagoBcp.ObtenerPorFlow("sus_flow-subscription-id-test", "flow-invoice-id-test", Arg.Any<NpgsqlTransaction?>()).Returns((Pago?)null);
			suscripcionBcp.ObtenerVigentesPorSub("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns([]);
			suscripcionBcp.ProximaFechaSinSuscripcion(Arg.Any<List<Suscripcion>>()).Returns(FECHA_DUMMY.AddDays(15));

			await suscripcionUseCase.ProcesarWebhookFlowPayment(new SalFlowPaymentGetStatus() {
				Status = 2, // Pagada
				CommerceOrder = "sus_flow-subscription-id-test_flow-invoice-id-test_2020-07-10"
			});
			await connection.Received(1).BeginTransactionAsync();
			await pagoBcp.Received(1).Insertar("sub-test", 100, 9990, "CLP", new DateTime(2020, 6, 1, 16, 30, 15, DateTimeKind.Utc), "sus_flow-subscription-id-test", "flow-invoice-id-test", Arg.Any<NpgsqlTransaction?>());
			await suscripcionBcp.Received(1).Modificar(
				Arg.Is<Suscripcion>(s => 
					s.Id == 100 && 
					s.FechaInicio == FECHA_DUMMY.AddDays(-15) && 
					s.FechaExpiracion == FECHA_DUMMY.AddDays(15).AddMonths(1) && 
					s.Estado == 1 /* Activa */), 
				Arg.Any<NpgsqlTransaction?>()
			);
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ProcesarWebhookFlowPaymentTest_Invalido() {
			flowHelper.InvoiceGet("flow-invoice-id-test").ThrowsAsync<Exception>();

			await Assert.ThrowsAsync<Exception>(() => suscripcionUseCase.ProcesarWebhookFlowPayment(new SalFlowPaymentGetStatus() {
				Status = 2, // Pagada
				CommerceOrder = "sus_flow-subscription-id-test_flow-invoice-id-test_2020-07-10"
			}));
			await connection.Received(1).BeginTransactionAsync();
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ProcesarWebhookFlowPaymentTest_PaymentStatusInvalido() {
			await suscripcionUseCase.ProcesarWebhookFlowPayment(new SalFlowPaymentGetStatus() {
				Status = 3, // Rechazado
				CommerceOrder = "sus_flow-subscription-id-test_flow-invoice-id-test_2020-07-10"
			});
			await connection.Received(1).BeginTransactionAsync();
			await pagoBcp.DidNotReceive().Insertar(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<NpgsqlTransaction?>());
			await suscripcionBcp.DidNotReceive().Modificar(Arg.Any<Suscripcion>(), Arg.Any<NpgsqlTransaction?>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ProcesarWebhookFlowPaymentTest_SinSuscripcion() {
			flowHelper.InvoiceGet("flow-invoice-id-test").Returns(new SalFlowInvoiceGet() {
				Amount = "9990",
				Currency = "CLP",
				Payment = new SalFlowPaymentGetStatus() {
					PaymentData = new SalFlowPaymentData() {
						Date = "2020-06-01 12:30:15" // Formato: yyyy-MM-dd HH:mm:ss - UTC: 2020-06-01 16:30:15
					}
				}
			});
			suscripcionBcp.ObtenerPorFlowSubscriptionId("sus_flow-subscription-id-test", Arg.Any<NpgsqlTransaction?>()).Returns((Suscripcion?)null);
			
			await suscripcionUseCase.ProcesarWebhookFlowPayment(new SalFlowPaymentGetStatus() {
				Status = 2, // Pagada
				CommerceOrder = "sus_flow-subscription-id-test_flow-invoice-id-test_2020-07-10"
			});
			await connection.Received(1).BeginTransactionAsync();
			await pagoBcp.DidNotReceive().Insertar(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<NpgsqlTransaction?>());
			await suscripcionBcp.DidNotReceive().Modificar(Arg.Any<Suscripcion>(), Arg.Any<NpgsqlTransaction?>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ProcesarWebhookFlowPaymentTest_SinPlan() {
			flowHelper.InvoiceGet("flow-invoice-id-test").Returns(new SalFlowInvoiceGet() {
				Amount = "9990",
				Currency = "CLP",
				Payment = new SalFlowPaymentGetStatus() {
					PaymentData = new SalFlowPaymentData() {
						Date = "2020-06-01 12:30:15" // Formato: yyyy-MM-dd HH:mm:ss - UTC: 2020-06-01 16:30:15
					}
				}
			});
			suscripcionBcp.ObtenerPorFlowSubscriptionId("sus_flow-subscription-id-test", Arg.Any<NpgsqlTransaction?>()).Returns(
				SuscripcionBcpTest.SuscripcionDummy(
					id: 100, sub: "sub-test", idPlan: 10, flowSubscriptionId: "sus_flow-subscription-id-test",
					fechaInicio: FECHA_DUMMY.AddDays(-15), fechaExpiracion: FECHA_DUMMY.AddDays(15)
				)
			);
			planBcp.ObtenerPorId(10, Arg.Any<NpgsqlTransaction?>()).Returns((Plan?)null);

			await suscripcionUseCase.ProcesarWebhookFlowPayment(new SalFlowPaymentGetStatus() {
				Status = 2, // Pagada
				CommerceOrder = "sus_flow-subscription-id-test_flow-invoice-id-test_2020-07-10"
			});
			await connection.Received(1).BeginTransactionAsync();
			await pagoBcp.DidNotReceive().Insertar(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<NpgsqlTransaction?>());
			await suscripcionBcp.DidNotReceive().Modificar(Arg.Any<Suscripcion>(), Arg.Any<NpgsqlTransaction?>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ProcesarWebhookFlowPaymentTest_PagoDuplicado() {
			flowHelper.InvoiceGet("flow-invoice-id-test").Returns(new SalFlowInvoiceGet() {
				Amount = "9990",
				Currency = "CLP",
				Payment = new SalFlowPaymentGetStatus() {
					PaymentData = new SalFlowPaymentData() {
						Date = "2020-06-01 12:30:15" // Formato: yyyy-MM-dd HH:mm:ss - UTC: 2020-06-01 16:30:15
					}
				}
			});
			suscripcionBcp.ObtenerPorFlowSubscriptionId("sus_flow-subscription-id-test", Arg.Any<NpgsqlTransaction?>()).Returns(
				SuscripcionBcpTest.SuscripcionDummy(
					id: 100, sub: "sub-test", idPlan: 10, flowSubscriptionId: "sus_flow-subscription-id-test",
					fechaInicio: FECHA_DUMMY.AddDays(-15), fechaExpiracion: FECHA_DUMMY.AddDays(15)
				)
			);
			planBcp.ObtenerPorId(10, Arg.Any<NpgsqlTransaction?>()).Returns(PlanBcpTest.PlanDummy(id: 10));
			pagoBcp.ObtenerPorFlow("sus_flow-subscription-id-test", "flow-invoice-id-test", Arg.Any<NpgsqlTransaction?>()).Returns(
				PagoBcpTest.PagoDummy(flowSubscriptionId: "sus_flow-subscription-id-test", flowInvoiceId: "flow-invoice-id-test")
			);

			await suscripcionUseCase.ProcesarWebhookFlowPayment(new SalFlowPaymentGetStatus() {
				Status = 2, // Pagada
				CommerceOrder = "sus_flow-subscription-id-test_flow-invoice-id-test_2020-07-10"
			});
			await connection.Received(1).BeginTransactionAsync();
			await pagoBcp.DidNotReceive().Insertar(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<NpgsqlTransaction?>());
			await suscripcionBcp.DidNotReceive().Modificar(Arg.Any<Suscripcion>(), Arg.Any<NpgsqlTransaction?>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ProcesarWebhookFlowPaymentTest_SinFechaPago() {
			flowHelper.InvoiceGet("flow-invoice-id-test").Returns(new SalFlowInvoiceGet() {
				Amount = "9990",
				Currency = "CLP",
				Payment = new SalFlowPaymentGetStatus() {
					PaymentData = new SalFlowPaymentData() {
						Date = null
					}
				}
			});
			suscripcionBcp.ObtenerPorFlowSubscriptionId("sus_flow-subscription-id-test", Arg.Any<NpgsqlTransaction?>()).Returns(
				SuscripcionBcpTest.SuscripcionDummy(
					id: 100, sub: "sub-test", idPlan: 10, flowSubscriptionId: "sus_flow-subscription-id-test",
					fechaInicio: FECHA_DUMMY.AddDays(-15), fechaExpiracion: FECHA_DUMMY.AddDays(15)
				)
			);
			planBcp.ObtenerPorId(10, Arg.Any<NpgsqlTransaction?>()).Returns(PlanBcpTest.PlanDummy(id: 10));
			pagoBcp.ObtenerPorFlow("sus_flow-subscription-id-test", "flow-invoice-id-test", Arg.Any<NpgsqlTransaction?>()).Returns((Pago?)null);
			suscripcionBcp.ObtenerVigentesPorSub("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns([]);
			suscripcionBcp.ProximaFechaSinSuscripcion(Arg.Any<List<Suscripcion>>()).Returns(FECHA_DUMMY.AddDays(15));

			await suscripcionUseCase.ProcesarWebhookFlowPayment(new SalFlowPaymentGetStatus() {
				Status = 2, // Pagada
				CommerceOrder = "sus_flow-subscription-id-test_flow-invoice-id-test_2020-07-10"
			});
			await connection.Received(1).BeginTransactionAsync();
			await pagoBcp.Received(1).Insertar("sub-test", 100, 9990, "CLP", FECHA_DUMMY, "sus_flow-subscription-id-test", "flow-invoice-id-test", Arg.Any<NpgsqlTransaction?>());
			await suscripcionBcp.Received(1).Modificar(
				Arg.Is<Suscripcion>(s =>
					s.Id == 100 &&
					s.FechaInicio == FECHA_DUMMY.AddDays(-15) &&
					s.FechaExpiracion == FECHA_DUMMY.AddDays(15).AddMonths(1) &&
					s.Estado == 1 /* Activa */),
				Arg.Any<NpgsqlTransaction?>()
			);
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ProcesarWebhookFlowPaymentTest_PagoAtrasado() {
			flowHelper.InvoiceGet("flow-invoice-id-test").Returns(new SalFlowInvoiceGet() {
				Amount = "9990",
				Currency = "CLP",
				Payment = new SalFlowPaymentGetStatus() {
					PaymentData = new SalFlowPaymentData() {
						Date = "2020-06-01 12:30:15" // Formato: yyyy-MM-dd HH:mm:ss - UTC: 2020-06-01 16:30:15
					}
				}
			});
			suscripcionBcp.ObtenerPorFlowSubscriptionId("sus_flow-subscription-id-test", Arg.Any<NpgsqlTransaction?>()).Returns(
				SuscripcionBcpTest.SuscripcionDummy(
					id: 100, sub: "sub-test", idPlan: 10, flowSubscriptionId: "sus_flow-subscription-id-test",
					fechaInicio: FECHA_DUMMY.AddDays(-45), fechaExpiracion: FECHA_DUMMY.AddDays(-15)
				)
			);
			planBcp.ObtenerPorId(10, Arg.Any<NpgsqlTransaction?>()).Returns(PlanBcpTest.PlanDummy(id: 10));
			pagoBcp.ObtenerPorFlow("sus_flow-subscription-id-test", "flow-invoice-id-test", Arg.Any<NpgsqlTransaction?>()).Returns((Pago?)null);
			suscripcionBcp.ObtenerVigentesPorSub("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns([]);
			suscripcionBcp.ProximaFechaSinSuscripcion(Arg.Any<List<Suscripcion>>()).Returns(FECHA_DUMMY.AddDays(15));

			await suscripcionUseCase.ProcesarWebhookFlowPayment(new SalFlowPaymentGetStatus() {
				Status = 2, // Pagada
				CommerceOrder = "sus_flow-subscription-id-test_flow-invoice-id-test_2020-07-10"
			});
			await connection.Received(1).BeginTransactionAsync();
			await pagoBcp.Received(1).Insertar("sub-test", 100, 9990, "CLP", new DateTime(2020, 6, 1, 16, 30, 15, DateTimeKind.Utc), "sus_flow-subscription-id-test", "flow-invoice-id-test", Arg.Any<NpgsqlTransaction?>());
			await suscripcionBcp.Received(1).Modificar(
				Arg.Is<Suscripcion>(s =>
					s.Id == 100 &&
					s.FechaInicio == FECHA_DUMMY.AddDays(-45) &&
					s.FechaExpiracion == FECHA_DUMMY.AddMonths(1) &&
					s.Estado == 1 /* Activa */),
				Arg.Any<NpgsqlTransaction?>()
			);
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ProcesarWebhookFlowTest_TipoCustomerRegister() {
			flowHelper.CustomerGetRegisterStatus("token-test").Returns(new SalFlowCustomerGetRegisterStatus() {
				CustomerId = "flow-customer-id-test",
				Status = "1" // Registrado
			});
			eventoPagoBcp.Insertar(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(EventoPagoBcpTest.EventoPagoDummy(id: 100, procesado: false));

			// Métodos para CustomerRegister...
			usuarioBcp.ObtenerPorFlowCustomerId("flow-customer-id-test", Arg.Any<NpgsqlTransaction?>()).Returns(UsuarioBcpTest.UsuarioDummy(sub: "sub-test", flowCustomerId: "flow-customer-id-test"));
			suscripcionBcp.ObtenerVigentesPorSub("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns([
				SuscripcionBcpTest.SuscripcionDummy(id: 100, sub: "sub-test", idPlan: 10, estado: 5 /* En Creación */, flowCustomerId: "flow-customer-id-test", flowSubscriptionId: null),
			]);
			planBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([
				PlanBcpTest.PlanDummy(id: 10, flowPlanId: "flow-plan-id-test")
			]);
			suscripcionBcp.ProximaFechaExpiracion(Arg.Any<List<Suscripcion>>()).Returns((DateTime?)null);
			flowHelper.SubscriptionCreate("flow-plan-id-test", "flow-customer-id-test", Arg.Any<DateTime?>()).Returns(new SalFlowSubscriptionCreate() {
				Status = 1, // Activa
				SubscriptionId = "flow-subscription-id-test"
			});

			await suscripcionUseCase.ProcesarWebhookFlow("CustomerRegister", "token-test");
			await flowHelper.Received(1).CustomerGetRegisterStatus("token-test");
			await flowHelper.DidNotReceive().PaymentGetStatus(Arg.Any<string>());
			await eventoPagoBcp.Received(1).Insertar(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
			await eventoPagoBcp.Received(1).MarcarComoProcesado(Arg.Is<EventoPago>(ep => ep.Id == 100));
		}

		[Fact]
		public async Task ProcesarWebhookFlowTest_TipoPaymentGetStatus() {
			flowHelper.PaymentGetStatus("token-test").Returns(new SalFlowPaymentGetStatus() {
				Status = 2, // Pagada
				CommerceOrder = "sus_flow-subscription-id-test_flow-invoice-id-test_2020-07-10"
			});
			eventoPagoBcp.Insertar(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(EventoPagoBcpTest.EventoPagoDummy(id: 100, procesado: false));

			// Métodos para PaymentGetStatus...
			flowHelper.InvoiceGet("flow-invoice-id-test").Returns(new SalFlowInvoiceGet() {
				Amount = "9990",
				Currency = "CLP",
				Payment = new SalFlowPaymentGetStatus() {
					PaymentData = new SalFlowPaymentData() {
						Date = "2020-06-01 12:30:15" // Formato: yyyy-MM-dd HH:mm:ss - UTC: 2020-06-01 16:30:15
					}
				}
			});
			suscripcionBcp.ObtenerPorFlowSubscriptionId("sus_flow-subscription-id-test", Arg.Any<NpgsqlTransaction?>()).Returns(
				SuscripcionBcpTest.SuscripcionDummy(
					id: 100, sub: "sub-test", idPlan: 10, flowSubscriptionId: "sus_flow-subscription-id-test",
					fechaInicio: FECHA_DUMMY.AddDays(-15), fechaExpiracion: FECHA_DUMMY.AddDays(15)
				)
			);
			planBcp.ObtenerPorId(10, Arg.Any<NpgsqlTransaction?>()).Returns(PlanBcpTest.PlanDummy(id: 10));
			pagoBcp.ObtenerPorFlow("sus_flow-subscription-id-test", "flow-invoice-id-test", Arg.Any<NpgsqlTransaction?>()).Returns((Pago?)null);
			suscripcionBcp.ObtenerVigentesPorSub("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns([]);
			suscripcionBcp.ProximaFechaSinSuscripcion(Arg.Any<List<Suscripcion>>()).Returns(FECHA_DUMMY.AddDays(15));

			await suscripcionUseCase.ProcesarWebhookFlow("PlanCreate", "token-test");
			await flowHelper.DidNotReceive().CustomerGetRegisterStatus(Arg.Any<string>());
			await flowHelper.Received(1).PaymentGetStatus("token-test");
			await eventoPagoBcp.Received(1).Insertar(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
			await eventoPagoBcp.Received(1).MarcarComoProcesado(Arg.Is<EventoPago>(ep => ep.Id == 100));
		}

		[Fact]
		public async Task ProcesarWebhookFlowTest_TipoInvalido() {
			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => suscripcionUseCase.ProcesarWebhookFlow("TipoProcesoInvalido", "token-test"));
			Assert.Equal(TipoErrorValidacion.TipoNoValido, ex.TipoErrorValidacion);
		}
	}
}
