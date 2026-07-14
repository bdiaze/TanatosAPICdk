using Npgsql;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Test.Business {
	public class EvaluacionBcpTest {
		private readonly IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
		private readonly IEvaluacionDao evaluacionDao = Substitute.For<IEvaluacionDao>();
		private readonly EvaluacionBcp evaluacionBcp;

		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

		public EvaluacionBcpTest() {
			dateTimeProvider.UtcNow.Returns(FECHA_DUMMY);

			evaluacionBcp = new(dateTimeProvider, evaluacionDao);
		}

		public static Evaluacion EvaluacionDummy(
			long id = 1,
			string sub = "sub-test",
			short puntaje = 5,
			string? comentario = "comentario-test",
			DateTime? fechaCreacion = null
		) => new() { 
			Id = id,
			Sub = sub,
			Puntaje = puntaje,
			Comentario = comentario,
			FechaCreacion = fechaCreacion ?? FECHA_DUMMY
		};

		[Fact]
		public async Task ObtenerTest() {
			evaluacionDao.Obtener(FECHA_DUMMY.AddDays(-1), FECHA_DUMMY.AddDays(1), Arg.Any<NpgsqlTransaction?>()).Returns([
				EvaluacionDummy(id: 10, fechaCreacion: FECHA_DUMMY),
				EvaluacionDummy(id: 20, fechaCreacion: FECHA_DUMMY),
			]);

			List<Evaluacion> retorno = await evaluacionBcp.Obtener(FECHA_DUMMY.AddDays(-1), FECHA_DUMMY.AddDays(1));
			Assert.Equal(2, retorno.Count);
			Assert.All(retorno, e => {
				Assert.True(e.Id == 10 || e.Id == 20);
			});
			await evaluacionDao.Received(1).Obtener(FECHA_DUMMY.AddDays(-1), FECHA_DUMMY.AddDays(1));
		}

		[Fact]
		public async Task InsertarTest() {
			evaluacionDao.Insertar(Arg.Any<Evaluacion>(), Arg.Any<NpgsqlTransaction?>()).Returns(100);

			Evaluacion retorno = await evaluacionBcp.Insertar("sub-test", 5, "comentario-test");
			Assert.Equal(100, retorno.Id);
			await evaluacionDao.Received(1).Insertar(
				Arg.Is<Evaluacion>(e => 
					e.Id == 100 &&
					e.Sub == "sub-test" &&
					e.Puntaje == 5 &&
					e.Comentario == "comentario-test"
				), 
				Arg.Any<NpgsqlTransaction?>()
			);
		}

		[Fact]
		public async Task InsertarTest_PuntajeMenor() {
			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => evaluacionBcp.Insertar("sub-test", 0, "comentario-test"));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);
			await evaluacionDao.DidNotReceive().Insertar(Arg.Any<Evaluacion>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task InsertarTest_PuntajeMayor() {
			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => evaluacionBcp.Insertar("sub-test", 100, "comentario-test"));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);
			await evaluacionDao.DidNotReceive().Insertar(Arg.Any<Evaluacion>(), Arg.Any<NpgsqlTransaction?>());
		}
	}
}
