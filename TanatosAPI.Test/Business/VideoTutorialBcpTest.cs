using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Test.Business {
	public class VideoTutorialBcpTest {
		private readonly IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
		private readonly IVideoTutorialDao videoTutorialDao = Substitute.For<IVideoTutorialDao>();
		private readonly VideoTutorialBcp videoTutorialBcp;

		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

		public VideoTutorialBcpTest() {
			dateTimeProvider.UtcNow.Returns(FECHA_DUMMY);
			videoTutorialBcp = new(dateTimeProvider, videoTutorialDao);
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

		public static TheoryData<VideoTutorial?, bool> EstaVigenteCases => new() {
			{ VideoTutorialDummy(vigencia: true), true },
			{ VideoTutorialDummy(vigencia: false), false },
			{ null, false },
		};
		[Theory]
		[MemberData(nameof(EstaVigenteCases))]
		public void EstaVigenteTest(VideoTutorial? videoTutorial, bool expectedResult) {
			Assert.Equal(expectedResult, videoTutorialBcp.EstaVigente(videoTutorial));
		}

		[Fact]
		public async Task ObtenerVigentesTest() {
			videoTutorialDao.ObtenerPorVigencia(true).Returns([
				VideoTutorialDummy(id: 1, vigencia: true),
				VideoTutorialDummy(id: 2, vigencia: true),
			]);
			videoTutorialDao.ObtenerPorVigencia(false).Returns([
				VideoTutorialDummy(id: 3, vigencia: false),
			]);
			videoTutorialDao.ObtenerPorVigencia(null).Returns([
				VideoTutorialDummy(id: 1, vigencia: true),
				VideoTutorialDummy(id: 2, vigencia: true),
				VideoTutorialDummy(id: 3, vigencia: false),
			]);

			List<VideoTutorial> retorno = await videoTutorialBcp.ObtenerVigentes();
			Assert.Equal(2, retorno.Count);
			Assert.All(retorno, p => Assert.True(p.Vigencia));
			await videoTutorialDao.Received(1).ObtenerPorVigencia(true);
		}

		[Fact]
		public async Task InsertarTest() {
			videoTutorialDao.Insertar(Arg.Any<VideoTutorial>()).Returns(99);

			VideoTutorial videoTutorial = await videoTutorialBcp.Insertar("video-tutorial-1", "descricpion-video-tutorial-1", "url-video-tutorial-1", true, 1);

			Assert.Equal(99, videoTutorial.Id);
			Assert.Equal("video-tutorial-1", videoTutorial.Titulo);
			Assert.Equal("descricpion-video-tutorial-1", videoTutorial.Descripcion);
			Assert.Equal("url-video-tutorial-1", videoTutorial.Url);
			Assert.True(videoTutorial.Habilitado);
			Assert.Equal(1, videoTutorial.Orden);
			Assert.True(videoTutorial.Vigencia);
			Assert.Equal(FECHA_DUMMY, videoTutorial.FechaCreacion);
			Assert.Null(videoTutorial.FechaEliminacion);
			await videoTutorialDao.Received(1).Insertar(Arg.Any<VideoTutorial>());
		}

		[Fact]
		public async Task ModificarTest() {
			VideoTutorial existente = VideoTutorialDummy();
			await videoTutorialBcp.Modificar(existente);
			await videoTutorialDao.Received(1).Actualizar(Arg.Any<VideoTutorial>());
		}

		[Fact]
		public async Task EliminarTest_Vigente() {
			VideoTutorial existente = VideoTutorialDummy(vigencia: true);
			await videoTutorialBcp.Eliminar(existente);
			Assert.Equal(FECHA_DUMMY, existente.FechaEliminacion);
			Assert.False(existente.Vigencia);
			await videoTutorialDao.Received(1).Actualizar(Arg.Any<VideoTutorial>());
		}

		[Fact]
		public async Task EliminarTest_NoVigente() {
			VideoTutorial existente = VideoTutorialDummy(vigencia: false);
			await videoTutorialBcp.Eliminar(existente);
			await videoTutorialDao.DidNotReceive().Actualizar(Arg.Any<VideoTutorial>());
		}
	}
}
