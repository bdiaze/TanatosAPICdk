using Npgsql;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;
using TanatosAPI.Test.Business;
using TanatosAPI.UseCases;

namespace TanatosAPI.Test.UseCases {
	public class TipoPeriodicidadUseCaseTest {
		private readonly ITipoPeriodicidadBcp tipoPeriodicidadBcp = Substitute.For<ITipoPeriodicidadBcp>();
		private readonly TipoPeriodicidadUseCase tipoPeriodicidadUseCase;

		public TipoPeriodicidadUseCaseTest() {
			tipoPeriodicidadUseCase = new(tipoPeriodicidadBcp);
		}

		[Fact]
		public async Task ObtenerVigentesTest() {
			tipoPeriodicidadBcp.ObtenerVigentes().Returns([
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 1, vigencia: true),
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 2, vigencia: true),
			]);

			List<TipoPeriodicidad> retorno = await tipoPeriodicidadUseCase.ObtenerVigentes();
			Assert.Equal(2, retorno.Count);
			Assert.All(retorno, p => {
				Assert.True(p.Id == 1 || p.Id == 2);
			});
			await tipoPeriodicidadBcp.Received(1).ObtenerVigentes();
		}

		[Theory]
		[InlineData(true, 2)]
		[InlineData(false, 1)]
		[InlineData(null, 3)]
		public async Task ObtenerPorVigenciaTest(bool? vigencia, int expectedCount) {
			tipoPeriodicidadBcp.ObtenerPorVigencia(true, Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 1, vigencia: true),
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 2, vigencia: true)
			]);
			tipoPeriodicidadBcp.ObtenerPorVigencia(false, Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 3, vigencia: false),
			]);
			tipoPeriodicidadBcp.ObtenerPorVigencia(null, Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 1, vigencia: true),
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 2, vigencia: true),
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 3, vigencia: false),
			]);

			List<TipoPeriodicidad> retorno = await tipoPeriodicidadUseCase.ObtenerPorVigencia(vigencia);
			Assert.Equal(expectedCount, retorno.Count);
			if (vigencia != null) {
				Assert.All(retorno, tp => {
					Assert.Equal(vigencia, tp.Vigencia);
				});
			}
			await tipoPeriodicidadBcp.Received(1).ObtenerPorVigencia(vigencia, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task CrearTest_Valido() {
			tipoPeriodicidadBcp.ObtenerPorId(10, Arg.Any<NpgsqlTransaction?>()).Returns((TipoPeriodicidad?)null);
			tipoPeriodicidadBcp.Crear(10, "nombre-test", "descripcion-test", "cron-test", null, 7, null, null, true, Arg.Any<NpgsqlTransaction?>()).Returns(
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(
					id: 10, nombre: "nombre-test", descripcion: "descripcion-test", cron: "cron-test", frecuenciaDias: null, 
					deltaDias: 7, deltaMeses: null, deltaAnnos: null, vigencia: true
				)
			);

			TipoPeriodicidad retorno = await tipoPeriodicidadUseCase.Crear(10, "nombre-test", "descripcion-test", "cron-test", null, 7, null, null, true);
			Assert.Equal(10, retorno.Id);
			Assert.Equal("nombre-test", retorno.Nombre);
			Assert.Equal("descripcion-test", retorno.Descripcion);
			Assert.Equal("cron-test", retorno.Cron);
			Assert.Null(retorno.FrecuenciaDias);
			Assert.Equal(7, retorno.DeltaDias);
			Assert.Null(retorno.DeltaMeses);
			Assert.Null(retorno.DeltaAnnos);
			Assert.True(retorno.Vigencia);
			await tipoPeriodicidadBcp.Received(1).Crear(10, "nombre-test", "descripcion-test", "cron-test", null, 7, null, null, true, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task CrearTest_Existente() {
			tipoPeriodicidadBcp.ObtenerPorId(10, Arg.Any<NpgsqlTransaction?>()).Returns(TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 10));

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => tipoPeriodicidadUseCase.Crear(10, "nombre-test", "descripcion-test", "cron-test", null, 7, null, null, true));
			Assert.Equal(TipoErrorValidacion.YaExiste, ex.TipoErrorValidacion);
			await tipoPeriodicidadBcp.DidNotReceive().Crear(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ModificarTest_Valido() {
			tipoPeriodicidadBcp.ObtenerPorId(10, Arg.Any<NpgsqlTransaction?>()).Returns(TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 10));
			tipoPeriodicidadBcp.Modificar(Arg.Is<TipoPeriodicidad>(p => p.Id == 10 && p.Nombre == "otro-nombre-test"), Arg.Any<NpgsqlTransaction?>()).Returns(
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 10, nombre: "otro-nombre-test")
			);

			TipoPeriodicidad retorno = await tipoPeriodicidadUseCase.Modificar(10, "otro-nombre-test", "descripcion-test", "cron-test", null, 7, null, null, true);
			Assert.Equal(10, retorno.Id);
			Assert.Equal("otro-nombre-test", retorno.Nombre);
			await tipoPeriodicidadBcp.Received(1).Modificar(Arg.Is<TipoPeriodicidad>(p => p.Id == 10 && p.Nombre == "otro-nombre-test"), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ModificarTest_NoExistente() {
			tipoPeriodicidadBcp.ObtenerPorId(10, Arg.Any<NpgsqlTransaction?>()).Returns((TipoPeriodicidad?)null);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => tipoPeriodicidadUseCase.Modificar(10, "nombre-test", "descripcion-test", "cron-test", null, 7, null, null, true));
			Assert.Equal(TipoErrorValidacion.NoVigente, ex.TipoErrorValidacion);
			await tipoPeriodicidadBcp.DidNotReceive().Modificar(Arg.Any<TipoPeriodicidad>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ModificarTest_NoEditado() {
			TipoPeriodicidad existente = TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 10);
			tipoPeriodicidadBcp.ObtenerPorId(10, Arg.Any<NpgsqlTransaction?>()).Returns(existente);

			TipoPeriodicidad retorno = await tipoPeriodicidadUseCase.Modificar(
				existente.Id, existente.Nombre, existente.Descripcion, existente.Cron, existente.FrecuenciaDias,
				existente.DeltaDias, existente.DeltaMeses, existente.DeltaAnnos, existente.Vigencia
			);
			Assert.Equal(10, retorno.Id);
			Assert.Equal(existente.Nombre, retorno.Nombre);
			await tipoPeriodicidadBcp.DidNotReceive().Modificar(Arg.Any<TipoPeriodicidad>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task EliminarTest_Valido() {
			tipoPeriodicidadBcp.ObtenerPorId(10, Arg.Any<NpgsqlTransaction?>()).Returns(TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 10));

			await tipoPeriodicidadUseCase.Eliminar(10);
			await tipoPeriodicidadBcp.Received(1).Eliminar(Arg.Is<TipoPeriodicidad>(p => p.Id == 10));
		}

		[Fact]
		public async Task EliminarTest_NoExistente() {
			tipoPeriodicidadBcp.ObtenerPorId(10, Arg.Any<NpgsqlTransaction?>()).Returns((TipoPeriodicidad?)null);

			await tipoPeriodicidadUseCase.Eliminar(10);
			await tipoPeriodicidadBcp.DidNotReceive().Eliminar(Arg.Any<TipoPeriodicidad>());
		}
	}
}
