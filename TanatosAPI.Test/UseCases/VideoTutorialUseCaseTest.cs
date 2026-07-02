using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces;
using TanatosAPI.UseCases;

namespace TanatosAPI.Test.UseCases {
	public class VideoTutorialUseCaseTest {
		private readonly IVideoTutorialBcp videoTutorialBcp = Substitute.For<IVideoTutorialBcp>();
		private readonly VideoTutorialUseCase videoTutorialUseCase;

		public VideoTutorialUseCaseTest() {
			videoTutorialUseCase = new(videoTutorialBcp);
		}

		public static VideoTutorial VideoTutorialDummy(
			long id = 1,
			string titulo = "TituloTest",
			string? descripcion = "DescripcionTest",
			string url = "UrlTest",
			bool habilitado = true,
			int orden = 1,
			DateTime? fechaCreacion = null,
			DateTime? fechaEliminacion = null,
			bool vigencia = true
		) => new() {
			Id = id,
			Titulo = titulo,
			Descripcion = descripcion,
			Url = url,
			Habilitado = habilitado,
			Orden = orden,
			FechaCreacion = fechaCreacion ?? DateTime.UtcNow,
			FechaEliminacion = fechaEliminacion,
			Vigencia = vigencia
		};

		[Fact]
		public async Task ObtenerVigentesTest() {
			videoTutorialBcp.ObtenerVigentes().Returns([
				VideoTutorialDummy(id: 1, vigencia: true),
				VideoTutorialDummy(id: 2, vigencia: true),
			]);

			List<VideoTutorial> retorno = await videoTutorialUseCase.ObtenerVigentes();
			Assert.Equal(2, retorno.Count);
			Assert.All(retorno, p => Assert.True(p.Vigencia));
			await videoTutorialBcp.Received(1).ObtenerVigentes();
		}

		[Fact]
		public async Task ObtenerHabilitadosTest() {
			videoTutorialBcp.ObtenerVigentes().Returns([
				VideoTutorialDummy(id: 1, habilitado: true, vigencia: true),
				VideoTutorialDummy(id: 2, habilitado: true, vigencia: true),
				VideoTutorialDummy(id: 3, habilitado: false, vigencia: true),
			]);

			List<VideoTutorial> retorno = await videoTutorialUseCase.ObtenerHabilitados();
			Assert.Equal(2, retorno.Count);
			Assert.All(retorno, p => Assert.True(p.Vigencia));
			Assert.All(retorno, p => Assert.True(p.Habilitado));
			await videoTutorialBcp.Received(1).ObtenerVigentes();
		}

		[Fact]
		public async Task RegistrarTest_Válido() {
			videoTutorialBcp.ObtenerVigentes().Returns([]);
			videoTutorialBcp.Insertar(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>()).Returns(callInfo =>
				VideoTutorialDummy(
					id: 99,
					titulo: callInfo.ArgAt<string>(0),
					descripcion: callInfo.ArgAt<string?>(1),
					url: callInfo.ArgAt<string>(2),
					habilitado: callInfo.ArgAt<bool>(3),
					orden: callInfo.ArgAt<int>(4)
				)
			);

			VideoTutorial videoTutorial = await videoTutorialUseCase.Registrar("video-tutorial-1", "descripcion-video-tutorial-1", "url-video-tutorial-1", true, 1);

			Assert.Equal(99, videoTutorial.Id);
			Assert.Equal("video-tutorial-1", videoTutorial.Titulo);
			Assert.Equal("descripcion-video-tutorial-1", videoTutorial.Descripcion);
			Assert.Equal("url-video-tutorial-1", videoTutorial.Url);
			Assert.True(videoTutorial.Habilitado);
			Assert.Equal(1, videoTutorial.Orden);
			await videoTutorialBcp.Received(1).Insertar(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>());
		}

		[Fact]
		public async Task RegistrarTest_VálidoDescripcionNulo() {
			videoTutorialBcp.ObtenerVigentes().Returns([]);
			videoTutorialBcp.Insertar(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>()).Returns(callInfo =>
				VideoTutorialDummy(
					id: 99,
					titulo: callInfo.ArgAt<string>(0),
					descripcion: callInfo.ArgAt<string?>(1),
					url: callInfo.ArgAt<string>(2),
					habilitado: callInfo.ArgAt<bool>(3),
					orden: callInfo.ArgAt<int>(4)
				)
			);

			VideoTutorial videoTutorial = await videoTutorialUseCase.Registrar("video-tutorial-1", null, "url-video-tutorial-1", true, 1);

			Assert.Equal(99, videoTutorial.Id);
			Assert.Equal("video-tutorial-1", videoTutorial.Titulo);
			Assert.Null(videoTutorial.Descripcion);
			Assert.Equal("url-video-tutorial-1", videoTutorial.Url);
			Assert.True(videoTutorial.Habilitado);
			Assert.Equal(1, videoTutorial.Orden);
			await videoTutorialBcp.Received(1).Insertar(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>());
		}

		[Fact]
		public async Task RegistrarTest_MenorQue0() {
			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => videoTutorialUseCase.Registrar("video-tutorial-1", "descripcion-video-tutorial-1", "url-video-tutorial-1", true, -1));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);

			await videoTutorialBcp.DidNotReceive().Insertar(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>());
		}

		[Fact]
		public async Task RegistrarTest_Existente() {
			videoTutorialBcp.ObtenerVigentes().Returns([
				VideoTutorialDummy(titulo: "video-tutorial-1")
			]);
			videoTutorialBcp.Insertar(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>()).Returns(callInfo =>
				VideoTutorialDummy(
					id: 99,
					titulo: callInfo.ArgAt<string>(0),
					descripcion: callInfo.ArgAt<string?>(1),
					url: callInfo.ArgAt<string>(2),
					habilitado: callInfo.ArgAt<bool>(3),
					orden: callInfo.ArgAt<int>(4)
				)
			);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => videoTutorialUseCase.Registrar("video-tutorial-1", "descripcion", "url", true, 1));
			Assert.Equal(TipoErrorValidacion.YaExiste, ex.TipoErrorValidacion);

			await videoTutorialBcp.Received(1).ObtenerVigentes();
			await videoTutorialBcp.DidNotReceive().Insertar(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>());
		}

		[Fact]
		public async Task RegistrarTest_OrdenRepetido() {
			videoTutorialBcp.ObtenerVigentes().Returns([
				VideoTutorialDummy(orden: 1)
			]);
			videoTutorialBcp.Insertar(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>()).Returns(callInfo =>
				VideoTutorialDummy(
					id: 99,
					titulo: callInfo.ArgAt<string>(0),
					descripcion: callInfo.ArgAt<string?>(1),
					url: callInfo.ArgAt<string>(2),
					habilitado: callInfo.ArgAt<bool>(3),
					orden: callInfo.ArgAt<int>(4)
				)
			);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => videoTutorialUseCase.Registrar("video-tutorial", "descripcion", "url", true, 1));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);

			await videoTutorialBcp.Received(1).ObtenerVigentes();
			await videoTutorialBcp.DidNotReceive().Insertar(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>());
		}

		[Fact]
		public async Task ActualizarTest_Válido() {
			videoTutorialBcp.ObtenerVigentes().Returns([
				VideoTutorialDummy(id: 1, titulo: "valor-antiguo-video-1", descripcion: "valor-antiguo-descripcion-1", url: "valor-antiguo-url-1", habilitado: true, orden: 5),
				VideoTutorialDummy(id: 2),
			]);

			VideoTutorial videoTutorial = await videoTutorialUseCase.Actualizar(1, "nuevo-valor-video", "nueva-descripcion-video", "nueva-url-video", false, 10);

			Assert.Equal(1, videoTutorial.Id);
			Assert.Equal("nuevo-valor-video", videoTutorial.Titulo);
			Assert.Equal("nueva-descripcion-video", videoTutorial.Descripcion);
			Assert.Equal("nueva-url-video", videoTutorial.Url);
			Assert.False(videoTutorial.Habilitado);
			Assert.Equal(10, videoTutorial.Orden);
			await videoTutorialBcp.Received(1).Modificar(Arg.Any<VideoTutorial>());
		}

		[Fact]
		public async Task ActualizarTest_VálidoDescripcionNulo() {
			videoTutorialBcp.ObtenerVigentes().Returns([
				VideoTutorialDummy(id: 1, titulo: "valor-antiguo-video-1", descripcion: "valor-antiguo-descripcion-1", url: "valor-antiguo-url-1", habilitado: true, orden: 5),
				VideoTutorialDummy(id: 2),
			]);

			VideoTutorial videoTutorial = await videoTutorialUseCase.Actualizar(1, "nuevo-valor-video", null, "nueva-url-video", false, 10);

			Assert.Equal(1, videoTutorial.Id);
			Assert.Equal("nuevo-valor-video", videoTutorial.Titulo);
			Assert.Null(videoTutorial.Descripcion);
			Assert.Equal("nueva-url-video", videoTutorial.Url);
			Assert.False(videoTutorial.Habilitado);
			Assert.Equal(10, videoTutorial.Orden);
			await videoTutorialBcp.Received(1).Modificar(Arg.Any<VideoTutorial>());
		}

		[Fact]
		public async Task ActualizarTest_MismosValores() {
			videoTutorialBcp.ObtenerVigentes().Returns([
				VideoTutorialDummy(id: 1, titulo: "mismo-valor-video-1", descripcion: "mismo-valor-descripcion-1", url: "mismo-valor-url-1", habilitado: true, orden: 5),
				VideoTutorialDummy(id: 2),
			]);

			VideoTutorial videoTutorial = await videoTutorialUseCase.Actualizar(1, "mismo-valor-video-1", "mismo-valor-descripcion-1", "mismo-valor-url-1", true, 5);

			Assert.Equal(1, videoTutorial.Id);
			Assert.Equal("mismo-valor-video-1", videoTutorial.Titulo);
			Assert.Equal("mismo-valor-descripcion-1", videoTutorial.Descripcion);
			Assert.Equal("mismo-valor-url-1", videoTutorial.Url);
			Assert.True(videoTutorial.Habilitado);
			Assert.Equal(5, videoTutorial.Orden);
			await videoTutorialBcp.DidNotReceive().Modificar(Arg.Any<VideoTutorial>());
		}

		[Fact]
		public async Task ActualizarTest_MenorQue0() {
			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => videoTutorialUseCase.Actualizar(1, "mismo-valor-video-1", "mismo-valor-descripcion-1", "mismo-valor-url-1", false, -10));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);
			await videoTutorialBcp.DidNotReceive().Modificar(Arg.Any<VideoTutorial>());
		}

		[Fact]
		public async Task ActualizarTest_Existente() {
			videoTutorialBcp.ObtenerVigentes().Returns([
				VideoTutorialDummy(id: 1, titulo: "valor-antiguo-titulo-1", descripcion: "valor-antiguo-descripcion-1", habilitado: true, orden: 5),
				VideoTutorialDummy(id: 2, titulo: "mismo-valor-titulo"),
			]);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => videoTutorialUseCase.Actualizar(1, "mismo-valor-titulo", "nueva-descripcion-video", "nueva-url-video", false, 10));
			Assert.Equal(TipoErrorValidacion.YaExiste, ex.TipoErrorValidacion);
			await videoTutorialBcp.DidNotReceive().Modificar(Arg.Any<VideoTutorial>());
		}

		[Fact]
		public async Task ActualizarTest_MismoOrden() {
			videoTutorialBcp.ObtenerVigentes().Returns([
				VideoTutorialDummy(id: 1, titulo: "valor-antiguo-video-1", descripcion: "valor-antiguo-descripcion-1", habilitado: true, orden: 5),
				VideoTutorialDummy(id: 2, orden: 10),
			]);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => videoTutorialUseCase.Actualizar(1, "mismo-valor-titulo", "nueva-descripcion-video", "nueva-url-video", false, 10));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);
			await videoTutorialBcp.DidNotReceive().Modificar(Arg.Any<VideoTutorial>());
		}

		[Fact]
		public async Task EliminarTest_Valido() {
			videoTutorialBcp.EstaVigente(Arg.Any<VideoTutorial>()).Returns(true);
			videoTutorialBcp.ObtenerVigentes().Returns([
				VideoTutorialDummy(id: 1),
				VideoTutorialDummy(id: 2),
			]);

			await videoTutorialUseCase.Eliminar(1);

			videoTutorialBcp.Received(1).EstaVigente(Arg.Any<VideoTutorial>());
			await videoTutorialBcp.Received(1).Eliminar(Arg.Any<VideoTutorial>());
		}

		[Fact]
		public async Task EliminarTest_NoVigente() {
			videoTutorialBcp.EstaVigente(Arg.Any<VideoTutorial>()).Returns(false);
			videoTutorialBcp.ObtenerVigentes().Returns([
				VideoTutorialDummy(id: 1),
				VideoTutorialDummy(id: 2),
			]);

			await videoTutorialUseCase.Eliminar(1);

			videoTutorialBcp.Received(1).EstaVigente(Arg.Any<VideoTutorial>());
			await videoTutorialBcp.DidNotReceive().Eliminar(Arg.Any<VideoTutorial>());
		}
	}
}
