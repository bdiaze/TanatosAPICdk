using Npgsql;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;
using TanatosAPI.UseCases;

namespace TanatosAPI.Test.Business {
	public class PlanBcpTest {
		private readonly IPlanDao planDao = Substitute.For<IPlanDao>();
		private readonly PlanBcp planBcp;

		public PlanBcpTest() {
			planBcp = new(planDao);
		}

		public static Plan PlanDummy(
			long id = 1,
			string nombre = "nombre-plan-test",
			decimal precio = 9990,
			int duracionMeses = 1,
			bool suscripcionUnica = false,
			string? flowPlanId = "flow-plan-id-test",
			bool vigencia = true
		) => new() { 
			Id = id,
			Nombre = nombre,
			Precio = precio,
			DuracionMeses = duracionMeses,
			SuscripcionUnica = suscripcionUnica,
			FlowPlanId = flowPlanId,
			Vigencia = vigencia
		};

		public static TheoryData<Plan?, bool> EstaVigenteCases => new() {
			{ PlanDummy(vigencia: true), true },
			{ PlanDummy(vigencia: false), false },
			{ null, false },
		};
		[Theory]
		[MemberData(nameof(EstaVigenteCases))]
		public void EstaVigenteTest(Plan? plan, bool expectedResult) {
			Assert.Equal(expectedResult, planBcp.EstaVigente(plan));
		}

		[Fact]
		public async Task ObtenerPorIdTest() {
			planDao.Obtener(10, Arg.Any<NpgsqlTransaction?>()).Returns(PlanDummy(id: 10));

			Plan? retorno = await planBcp.ObtenerPorId(10);
			Assert.NotNull(retorno);
			Assert.Equal(10, retorno.Id);
			await planDao.Received(1).Obtener(10, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerPorIdValidandoVigenciaTest_Valido() {
			planDao.Obtener(10, Arg.Any<NpgsqlTransaction?>()).Returns(PlanDummy(id: 10));

			Plan retorno = await planBcp.ObtenerPorIdValidandoVigencia(10);
			Assert.Equal(10, retorno.Id);
			await planDao.Received(1).Obtener(10, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerPorIdValidandoVigenciaTest_Nulo() {
			planDao.Obtener(10, Arg.Any<NpgsqlTransaction?>()).Returns((Plan?)null);
			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => planBcp.ObtenerPorIdValidandoVigencia(10));
			Assert.Equal(TipoErrorValidacion.NoVigente, ex.TipoErrorValidacion);
			await planDao.Received(1).Obtener(10, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerPorIdValidandoVigenciaTest_NoVigente() {
			planDao.Obtener(10, Arg.Any<NpgsqlTransaction?>()).Returns(PlanDummy(id: 10, vigencia: false));
			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => planBcp.ObtenerPorIdValidandoVigencia(10));
			Assert.Equal(TipoErrorValidacion.NoVigente, ex.TipoErrorValidacion);
			await planDao.Received(1).Obtener(10, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerTodosTest() {
			planDao.ObtenerPorVigencia(null).Returns([
				PlanDummy(id: 1, vigencia: true),
				PlanDummy(id: 2, vigencia: false),
			]);

			List<Plan> retorno = await planBcp.ObtenerTodos();
			Assert.Equal(2, retorno.Count);
			await planDao.Received(1).ObtenerPorVigencia(null);
		}

		[Fact]
		public async Task ObtenerVigentesTest() {
			planDao.ObtenerPorVigencia(true).Returns([
				PlanDummy(id: 1, vigencia: true),
				PlanDummy(id: 2, vigencia: true),
				PlanDummy(id: 3, vigencia: true),
			]);

			List<Plan> retorno = await planBcp.ObtenerVigentes();
			Assert.Equal(3, retorno.Count);
			await planDao.Received(1).ObtenerPorVigencia(true);
		}

		[Fact]
		public async Task ObtenerPlanesGratuitosTest() {
			planDao.ObtenerPorVigencia(true).Returns([
				PlanDummy(id: 1, precio: 0, vigencia: true),
				PlanDummy(id: 2, precio: 9990, vigencia: true),
				PlanDummy(id: 3, precio: 24990, vigencia: true),
				PlanDummy(id: 4, precio: 0, vigencia: true),
			]);

			List<Plan> retorno = await planBcp.ObtenerPlanesGratuitos();
			Assert.Equal(2, retorno.Count);
			Assert.All(retorno, p => Assert.Equal(0, p.Precio));
			await planDao.Received(1).ObtenerPorVigencia(true);
		}
	}
}
