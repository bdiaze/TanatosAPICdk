using Npgsql;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;
using TanatosAPI.Test.Business;
using TanatosAPI.UseCases;

namespace TanatosAPI.Test.UseCases {
	public class EvaluacionUseCaseTest {
		private readonly IEvaluacionBcp evaluacionBcp = Substitute.For<IEvaluacionBcp>();
		private readonly EvaluacionUseCase evaluacionUseCase;

		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

		public EvaluacionUseCaseTest() {
			evaluacionUseCase = new(evaluacionBcp);
		}

		[Fact]
		public async Task ObtenerTest() {
			evaluacionBcp.Obtener(FECHA_DUMMY.AddDays(-1), FECHA_DUMMY.AddDays(1), Arg.Any<NpgsqlTransaction?>()).Returns([
				EvaluacionBcpTest.EvaluacionDummy(id: 10, fechaCreacion: FECHA_DUMMY),
				EvaluacionBcpTest.EvaluacionDummy(id: 20, fechaCreacion: FECHA_DUMMY),
			]);

			List<Evaluacion> retorno = await evaluacionUseCase.Obtener(FECHA_DUMMY.AddDays(-1), FECHA_DUMMY.AddDays(1));
			Assert.Equal(2, retorno.Count);
			Assert.All(retorno, e => {
				Assert.True(e.Id == 10 || e.Id == 20);
			});
			await evaluacionBcp.Received(1).Obtener(FECHA_DUMMY.AddDays(-1), FECHA_DUMMY.AddDays(1));
		}

		[Fact]
		public async Task InsertarTest() {
			evaluacionBcp.Insertar("sub-test", 5, "comentario-test", Arg.Any<NpgsqlTransaction?>()).Returns(
				EvaluacionBcpTest.EvaluacionDummy(id: 100, sub: "sub-test", puntaje: 5, comentario: "comentario-test", fechaCreacion: FECHA_DUMMY)
			);

			Evaluacion retorno = await evaluacionUseCase.Insertar("sub-test", 5, "comentario-test");
			Assert.Equal(100, retorno.Id);
			await evaluacionBcp.Received(1).Insertar("sub-test", 5, "comentario-test", Arg.Any<NpgsqlTransaction?>());
		}
	}
}
