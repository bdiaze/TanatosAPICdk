using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Repositories;
using TanatosAPI.UseCases;

namespace TanatosAPI.Test.UseCases {
	public class PreguntaFrecuenteUseCaseTest {
		private readonly IPreguntaFrecuenteBcp preguntaFrecuenteBcp = Substitute.For<IPreguntaFrecuenteBcp>();
		private readonly PreguntaFrecuenteUseCase preguntaFrecuenteUseCase;

		public PreguntaFrecuenteUseCaseTest() {
			preguntaFrecuenteUseCase = new(preguntaFrecuenteBcp);
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

		[Fact]
		public async Task ObtenerVigentesTest() {
			preguntaFrecuenteBcp.ObtenerVigentes().Returns([
				PreguntaFrecuenteDummy(id: 1, vigencia: true),
				PreguntaFrecuenteDummy(id: 2, vigencia: true),
			]);

			List<PreguntaFrecuente> retorno = await preguntaFrecuenteUseCase.ObtenerVigentes();
			Assert.Equal(2, retorno.Count);
			Assert.All(retorno, p => Assert.True(p.Vigencia));
			await preguntaFrecuenteBcp.Received(1).ObtenerVigentes();
		}

		[Fact]
		public async Task ObtenerHabilitadosTest() {
			preguntaFrecuenteBcp.ObtenerVigentes().Returns([
				PreguntaFrecuenteDummy(id: 1, habilitado: true, vigencia: true),
				PreguntaFrecuenteDummy(id: 2, habilitado: true, vigencia: true),
				PreguntaFrecuenteDummy(id: 3, habilitado: false, vigencia: true),
			]);

			List<PreguntaFrecuente> retorno = await preguntaFrecuenteUseCase.ObtenerHabilitados();
			Assert.Equal(2, retorno.Count);
			Assert.All(retorno, p => Assert.True(p.Vigencia));
			Assert.All(retorno, p => Assert.True(p.Habilitado));
			await preguntaFrecuenteBcp.Received(1).ObtenerVigentes();
		}

		[Fact]
		public async Task RegistrarTest_Válido() {
			preguntaFrecuenteBcp.ObtenerVigentes().Returns([]);
			preguntaFrecuenteBcp.Insertar(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>()).Returns(callInfo =>
				PreguntaFrecuenteDummy(
					id: 99,
					pregunta: callInfo.ArgAt<string>(0),
					respuesta: callInfo.ArgAt<string>(1),
					habilitado: callInfo.ArgAt<bool>(2),
					orden: callInfo.ArgAt<int>(3)
				)
			);

			PreguntaFrecuente preguntaFrecuente = await preguntaFrecuenteUseCase.Registrar("pregunta-frecuente-1", "respuesta-pregunta-1", true, 1);

			Assert.Equal(99, preguntaFrecuente.Id);
			Assert.Equal("pregunta-frecuente-1", preguntaFrecuente.Pregunta);
			Assert.Equal("respuesta-pregunta-1", preguntaFrecuente.Respuesta);
			Assert.True(preguntaFrecuente.Habilitado);
			Assert.Equal(1, preguntaFrecuente.Orden);
			await preguntaFrecuenteBcp.Received(1).Insertar(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>());
		}

		[Fact]
		public async Task RegistrarTest_MenorQue0() {
			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => preguntaFrecuenteUseCase.Registrar("pregunta-existente-1", "respuesta-pregunta-1", true, -1));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);

			await preguntaFrecuenteBcp.DidNotReceive().Insertar(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>());
		}

		[Fact]
		public async Task RegistrarTest_Existente() {
			preguntaFrecuenteBcp.ObtenerVigentes().Returns([
				PreguntaFrecuenteDummy(pregunta: "pregunta-existente-1")
			]);
			preguntaFrecuenteBcp.Insertar(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>()).Returns(callInfo =>
				PreguntaFrecuenteDummy(
					id: 99,
					pregunta: callInfo.ArgAt<string>(0),
					respuesta: callInfo.ArgAt<string>(1),
					habilitado: callInfo.ArgAt<bool>(2),
					orden: callInfo.ArgAt<int>(3)
				)
			);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => preguntaFrecuenteUseCase.Registrar("pregunta-existente-1", "respuesta-pregunta-1", true, 1));
			Assert.Equal(TipoErrorValidacion.YaExiste, ex.TipoErrorValidacion);

			await preguntaFrecuenteBcp.Received(1).ObtenerVigentes();
			await preguntaFrecuenteBcp.DidNotReceive().Insertar(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>());
		}

		[Fact]
		public async Task RegistrarTest_OrdenRepetido() {
			preguntaFrecuenteBcp.ObtenerVigentes().Returns([
				PreguntaFrecuenteDummy(orden: 1)
			]);
			preguntaFrecuenteBcp.Insertar(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>()).Returns(callInfo =>
				PreguntaFrecuenteDummy(
					id: 99,
					pregunta: callInfo.ArgAt<string>(0),
					respuesta: callInfo.ArgAt<string>(1),
					habilitado: callInfo.ArgAt<bool>(2),
					orden: callInfo.ArgAt<int>(3)
				)
			);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => preguntaFrecuenteUseCase.Registrar("pregunta-existente-1", "respuesta-pregunta-1", true, 1));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);

			await preguntaFrecuenteBcp.Received(1).ObtenerVigentes();
			await preguntaFrecuenteBcp.DidNotReceive().Insertar(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>());
		}

		[Fact]
		public async Task ActualizarTest_Válido() {
			preguntaFrecuenteBcp.ObtenerVigentes().Returns([
				PreguntaFrecuenteDummy(id: 1, pregunta: "valor-antiguo-pregunta-1", respuesta: "valor-antiguo-respuesta-1", habilitado: true, orden: 5),
				PreguntaFrecuenteDummy(id: 2),
			]);

			PreguntaFrecuente preguntaFrecuente = await preguntaFrecuenteUseCase.Actualizar(1, "nuevo-valor-pregunta", "nueva-respuesta-pregunta", false, 10);

			Assert.Equal(1, preguntaFrecuente.Id);
			Assert.Equal("nuevo-valor-pregunta", preguntaFrecuente.Pregunta);
			Assert.Equal("nueva-respuesta-pregunta", preguntaFrecuente.Respuesta);
			Assert.False (preguntaFrecuente.Habilitado);
			Assert.Equal(10, preguntaFrecuente.Orden);
			await preguntaFrecuenteBcp.Received(1).Modificar(Arg.Any<PreguntaFrecuente>());
		}

		[Fact]
		public async Task ActualizarTest_MismosValores() {
			preguntaFrecuenteBcp.ObtenerVigentes().Returns([
				PreguntaFrecuenteDummy(id: 1, pregunta: "mismo-valor-pregunta-1", respuesta: "mismo-valor-respuesta-1", habilitado: true, orden: 5),
				PreguntaFrecuenteDummy(id: 2),
			]);

			PreguntaFrecuente preguntaFrecuente = await preguntaFrecuenteUseCase.Actualizar(1, "mismo-valor-pregunta-1", "mismo-valor-respuesta-1", true, 5);

			Assert.Equal(1, preguntaFrecuente.Id);
			Assert.Equal("mismo-valor-pregunta-1", preguntaFrecuente.Pregunta);
			Assert.Equal("mismo-valor-respuesta-1", preguntaFrecuente.Respuesta);
			Assert.True(preguntaFrecuente.Habilitado);
			Assert.Equal(5, preguntaFrecuente.Orden);
			await preguntaFrecuenteBcp.DidNotReceive().Modificar(Arg.Any<PreguntaFrecuente>());
		}

		[Fact]
		public async Task ActualizarTest_MenorQue0() {
			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => preguntaFrecuenteUseCase.Actualizar(1, "mismo-valor-pregunta", "nueva-respuesta-pregunta", false, -10));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);
			await preguntaFrecuenteBcp.DidNotReceive().Modificar(Arg.Any<PreguntaFrecuente>());
		}

		[Fact]
		public async Task ActualizarTest_Existente() {
			preguntaFrecuenteBcp.ObtenerVigentes().Returns([
				PreguntaFrecuenteDummy(id: 1, pregunta: "valor-antiguo-pregunta-1", respuesta: "valor-antiguo-respuesta-1", habilitado: true, orden: 5),
				PreguntaFrecuenteDummy(id: 2, pregunta: "mismo-valor-pregunta"),
			]);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => preguntaFrecuenteUseCase.Actualizar(1, "mismo-valor-pregunta", "nueva-respuesta-pregunta", false, 10));
			Assert.Equal(TipoErrorValidacion.YaExiste, ex.TipoErrorValidacion);
			await preguntaFrecuenteBcp.DidNotReceive().Modificar(Arg.Any<PreguntaFrecuente>());
		}

		[Fact]
		public async Task ActualizarTest_MismoOrden() {
			preguntaFrecuenteBcp.ObtenerVigentes().Returns([
				PreguntaFrecuenteDummy(id: 1, pregunta: "valor-antiguo-pregunta-1", respuesta: "valor-antiguo-respuesta-1", habilitado: true, orden: 5),
				PreguntaFrecuenteDummy(id: 2, orden: 10),
			]);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => preguntaFrecuenteUseCase.Actualizar(1, "mismo-valor-pregunta", "nueva-respuesta-pregunta", false, 10));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);
			await preguntaFrecuenteBcp.DidNotReceive().Modificar(Arg.Any<PreguntaFrecuente>());
		}

		[Fact]
		public async Task EliminarTest_Valido() {
			preguntaFrecuenteBcp.EstaVigente(Arg.Any<PreguntaFrecuente>()).Returns(true);
			preguntaFrecuenteBcp.ObtenerVigentes().Returns([
				PreguntaFrecuenteDummy(id: 1),
				PreguntaFrecuenteDummy(id: 2),
			]);

			await preguntaFrecuenteUseCase.Eliminar(1);

			preguntaFrecuenteBcp.Received(1).EstaVigente(Arg.Any<PreguntaFrecuente>());
			await preguntaFrecuenteBcp.Received(1).Eliminar(Arg.Any<PreguntaFrecuente>());
		}

		[Fact]
		public async Task EliminarTest_NoVigente() {
			preguntaFrecuenteBcp.EstaVigente(Arg.Any<PreguntaFrecuente>()).Returns(false);
			preguntaFrecuenteBcp.ObtenerVigentes().Returns([
				PreguntaFrecuenteDummy(id: 1),
				PreguntaFrecuenteDummy(id: 2),
			]);

			await preguntaFrecuenteUseCase.Eliminar(1);

			preguntaFrecuenteBcp.Received(1).EstaVigente(Arg.Any<PreguntaFrecuente>());
			await preguntaFrecuenteBcp.DidNotReceive().Eliminar(Arg.Any<PreguntaFrecuente>());
		}
	}
}
