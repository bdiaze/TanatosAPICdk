using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces;
using TanatosAPI.Repositories;

namespace TanatosAPI.Test.Business {
	public class PreguntaFrecuenteBcpTest {
		private readonly IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
		private readonly IPreguntaFrecuenteDao preguntaFrecuenteDao = Substitute.For<IPreguntaFrecuenteDao>();
		private readonly PreguntaFrecuenteBcp preguntaFrecuenteBcp;

		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

		public PreguntaFrecuenteBcpTest() {
			dateTimeProvider.UtcNow.Returns(FECHA_DUMMY);
			preguntaFrecuenteBcp = new(dateTimeProvider, preguntaFrecuenteDao);
		}

		public static PreguntaFrecuente PreguntaFrecuenteDummy(
			long id = 1,
			string pregunta = "PreguntaTest",
			string respuesta = "RespuestaTest",
			bool habilitado = true,
			int orden = 1,
			DateTime? fechaCreacion = null,
			DateTime? fechaEliminacion = null,
			bool vigencia = true
		) => new() { 
			Id = id,
			Pregunta = pregunta,
			Respuesta = respuesta,
			Habilitado = habilitado,
			Orden = orden,
			FechaCreacion = fechaCreacion ?? DateTime.UtcNow,
			FechaEliminacion = fechaEliminacion,
			Vigencia = vigencia
		};

		public static TheoryData<PreguntaFrecuente?, bool> EstaVigenteCases => new() {
			{ PreguntaFrecuenteDummy(vigencia: true), true },
			{ PreguntaFrecuenteDummy(vigencia: false), false },
			{ null, false },
		};
		[Theory]
		[MemberData(nameof(EstaVigenteCases))]
		public void EstaVigenteTest(PreguntaFrecuente? preguntaFrecuente, bool expectedResult) {
			Assert.Equal(expectedResult, preguntaFrecuenteBcp.EstaVigente(preguntaFrecuente));
		}

		[Fact]
		public async Task ObtenerVigentesTest() {
			preguntaFrecuenteDao.ObtenerPorVigencia(true).Returns([
				PreguntaFrecuenteDummy(id: 1, vigencia: true),
				PreguntaFrecuenteDummy(id: 2, vigencia: true),
			]);
			preguntaFrecuenteDao.ObtenerPorVigencia(false).Returns([
				PreguntaFrecuenteDummy(id: 3, vigencia: false),
			]);
			preguntaFrecuenteDao.ObtenerPorVigencia(null).Returns([
				PreguntaFrecuenteDummy(id: 1, vigencia: true),
				PreguntaFrecuenteDummy(id: 2, vigencia: true),
				PreguntaFrecuenteDummy(id: 3, vigencia: false),
			]);

			List<PreguntaFrecuente> retorno = await preguntaFrecuenteBcp.ObtenerVigentes();
			Assert.Equal(2, retorno.Count);
			Assert.All(retorno, p => Assert.True(p.Vigencia));
			await preguntaFrecuenteDao.Received(1).ObtenerPorVigencia(true);
		}

		[Fact]
		public async Task InsertarTest() {
			preguntaFrecuenteDao.Insertar(Arg.Any<PreguntaFrecuente>()).Returns(99);

			PreguntaFrecuente preguntaFrecuente = await preguntaFrecuenteBcp.Insertar("pregunta-frecuente-1", "respuesta-pregunta-1", true, 1);

			Assert.Equal(99, preguntaFrecuente.Id);
			Assert.Equal("pregunta-frecuente-1", preguntaFrecuente.Pregunta);
			Assert.Equal("respuesta-pregunta-1", preguntaFrecuente.Respuesta);
			Assert.True(preguntaFrecuente.Habilitado);
			Assert.Equal(1, preguntaFrecuente.Orden);
			Assert.True(preguntaFrecuente.Vigencia);
			Assert.Equal(FECHA_DUMMY, preguntaFrecuente.FechaCreacion);
			Assert.Null(preguntaFrecuente.FechaEliminacion);
			await preguntaFrecuenteDao.Received(1).Insertar(Arg.Any<PreguntaFrecuente>());
		}

		[Fact]
		public async Task ModificarTest() {
			PreguntaFrecuente existente = PreguntaFrecuenteDummy();
			await preguntaFrecuenteBcp.Modificar(existente);
			await preguntaFrecuenteDao.Received(1).Actualizar(Arg.Any<PreguntaFrecuente>());
		}

		[Fact]
		public async Task EliminarTest_Vigente() {
			PreguntaFrecuente existente = PreguntaFrecuenteDummy(vigencia: true);
			await preguntaFrecuenteBcp.Eliminar(existente);
			Assert.Equal(FECHA_DUMMY, existente.FechaEliminacion);
			Assert.False(existente.Vigencia);
			await preguntaFrecuenteDao.Received(1).Actualizar(Arg.Any<PreguntaFrecuente>());
		}

		[Fact]
		public async Task EliminarTest_NoVigente() {
			PreguntaFrecuente existente = PreguntaFrecuenteDummy(vigencia: false);
			await preguntaFrecuenteBcp.Eliminar(existente);
			await preguntaFrecuenteDao.DidNotReceive().Actualizar(Arg.Any<PreguntaFrecuente>());
		}
	}
}
