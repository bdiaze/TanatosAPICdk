using Npgsql;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Test.Business {
	public class TipoPeriodicidadBcpTest {
		private readonly ITipoPeriodicidadDao tipoPeriodicidadDao = Substitute.For<ITipoPeriodicidadDao>();
		private readonly TipoPeriodicidadBcp tipoPeriodicidadBcp;

		public TipoPeriodicidadBcpTest() {
			tipoPeriodicidadBcp = new(tipoPeriodicidadDao);
		}

		public static TipoPeriodicidad TipoPeriodicidadDummy(
			long id = 1,
			string nombre = "nombre-test",
			string? descripcion = "descripcion-test",
			string? cron = "MI HO * * ? *",
			int? frecuenciaDias = null,
			int? deltaDias = 1,
			int? deltaMeses = null,
			int? deltaAnnos = null,
			bool vigencia = true
		) => new() { 
			Id = id,
			Nombre = nombre,
			Descripcion = descripcion,
			Cron = cron,
			FrecuenciaDias = frecuenciaDias,
			DeltaDias = deltaDias,
			DeltaMeses = deltaMeses,
			DeltaAnnos = deltaAnnos,
			Vigencia = vigencia
		};

		public static TheoryData<TipoPeriodicidad?, bool> EstaVigenteCases => new() {
			{ TipoPeriodicidadDummy(vigencia: true), true },
			{ TipoPeriodicidadDummy(vigencia: false), false },
			{ null, false },
		};
		[Theory]
		[MemberData(nameof(EstaVigenteCases))]
		public void EstaVigenteTest(TipoPeriodicidad? item, bool expectedResult) {
			Assert.Equal(expectedResult, tipoPeriodicidadBcp.EstaVigente(item));
		}

		public static TheoryData<TipoPeriodicidad, bool> ValidarDeltasCases => new() {
			{ TipoPeriodicidadDummy(deltaAnnos: 1, deltaMeses: 12, deltaDias: 365), true },
			{ TipoPeriodicidadDummy(deltaAnnos: null, deltaMeses: null, deltaDias: null), true },
			{ TipoPeriodicidadDummy(deltaAnnos: -1, deltaMeses: null, deltaDias: null), true },
			{ TipoPeriodicidadDummy(deltaAnnos: null, deltaMeses: -1, deltaDias: null), true },
			{ TipoPeriodicidadDummy(deltaAnnos: null, deltaMeses: null, deltaDias: -1), true },
			{ TipoPeriodicidadDummy(), false },
		};
		[Theory]
		[MemberData(nameof(ValidarDeltasCases))]
		public void ValidarDeltasTest(TipoPeriodicidad item, bool exceptionExpected) {
			if (exceptionExpected) {
				Assert.Throws<InvalidOperationException>(() => tipoPeriodicidadBcp.ValidarDeltas(item));
			} else {
				tipoPeriodicidadBcp.ValidarDeltas(item);
			}
		}

		[Fact]
		public async Task ObtenerPorIdTest() {
			tipoPeriodicidadDao.ObtenerPorId(10).Returns(TipoPeriodicidadDummy(id: 10));
			TipoPeriodicidad? retorno = await tipoPeriodicidadBcp.ObtenerPorId(10);
			Assert.NotNull(retorno);
			Assert.Equal(10, retorno.Id);
			await tipoPeriodicidadDao.Received(1).ObtenerPorId(10);
		}

		[Fact]
		public async Task ObtenerValidandoVigencia_Valido() {
			tipoPeriodicidadDao.ObtenerPorId(10).Returns(TipoPeriodicidadDummy(id: 10));
			TipoPeriodicidad? retorno = await tipoPeriodicidadBcp.ObtenerValidandoVigencia(10);
			Assert.NotNull(retorno);
			Assert.Equal(10, retorno.Id);
			await tipoPeriodicidadDao.Received(1).ObtenerPorId(10);
		}

		[Fact]
		public async Task ObtenerValidandoVigencia_EntradaNula() {
			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => tipoPeriodicidadBcp.ObtenerValidandoVigencia(null));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);
			await tipoPeriodicidadDao.DidNotReceive().ObtenerPorId(10);
		}

		[Fact]
		public async Task ObtenerValidandoVigencia_NoVigente() {
			tipoPeriodicidadDao.ObtenerPorId(10).Returns(TipoPeriodicidadDummy(id: 10, vigencia: false));
		
			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => tipoPeriodicidadBcp.ObtenerValidandoVigencia(10));
			Assert.Equal(TipoErrorValidacion.NoVigente, ex.TipoErrorValidacion);
			await tipoPeriodicidadDao.Received(1).ObtenerPorId(10);
		}

		[Fact]
		public async Task ObtenerVigentesTest() {
			tipoPeriodicidadDao.ObtenerPorVigencia(true, Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoPeriodicidadDummy(id: 1, vigencia: true),
				TipoPeriodicidadDummy(id: 2, vigencia: true)
			]);

			List<TipoPeriodicidad> retorno = await tipoPeriodicidadBcp.ObtenerVigentes();
			Assert.Equal(2, retorno.Count);
			Assert.All(retorno, tp => {
				Assert.True(tp.Id == 1 || tp.Id == 2);
			});
			await tipoPeriodicidadDao.Received(1).ObtenerPorVigencia(true, Arg.Any<NpgsqlTransaction?>());
		}

		[Theory]
		[InlineData(true, 2)]
		[InlineData(false, 1)]
		[InlineData(null, 3)]
		public async Task ObtenerPorVigenciaTest(bool? vigencia, int expectedCount) {
			tipoPeriodicidadDao.ObtenerPorVigencia(true, Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoPeriodicidadDummy(id: 1, vigencia: true),
				TipoPeriodicidadDummy(id: 2, vigencia: true)
			]);
			tipoPeriodicidadDao.ObtenerPorVigencia(false, Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoPeriodicidadDummy(id: 3, vigencia: false),
			]);
			tipoPeriodicidadDao.ObtenerPorVigencia(null, Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoPeriodicidadDummy(id: 1, vigencia: true),
				TipoPeriodicidadDummy(id: 2, vigencia: true),
				TipoPeriodicidadDummy(id: 3, vigencia: false),
			]);

			List<TipoPeriodicidad> retorno = await tipoPeriodicidadBcp.ObtenerPorVigencia(vigencia);
			Assert.Equal(expectedCount, retorno.Count);
			if (vigencia != null) {
				Assert.All(retorno, tp => {
					Assert.Equal(vigencia, tp.Vigencia);
				});
			}
			await tipoPeriodicidadDao.Received(1).ObtenerPorVigencia(vigencia, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task CrearTest_Valido() {
			TipoPeriodicidad retorno = await tipoPeriodicidadBcp.Crear(10, "nombre-test", "descripcion-test", "cron-test", null, 7, null, null, 0, true);
			Assert.Equal(10, retorno.Id);
			Assert.Equal("nombre-test", retorno.Nombre);
			Assert.Equal("descripcion-test", retorno.Descripcion);
			Assert.Equal("cron-test", retorno.Cron);
			Assert.Null(retorno.FrecuenciaDias);
			Assert.Equal(7, retorno.DeltaDias);
			Assert.Null(retorno.DeltaMeses);
			Assert.Null(retorno.DeltaAnnos);
			Assert.Equal(0, retorno.Orden);
			Assert.True(retorno.Vigencia);
			await tipoPeriodicidadDao.Received(1).Insertar(
				Arg.Is<TipoPeriodicidad>(p => 
					p.Id == 10 &&
					p.Nombre == "nombre-test" &&
					p.Descripcion == "descripcion-test" &&
					p.Cron == "cron-test" &&
					p.FrecuenciaDias == null && 
					p.DeltaDias == 7 &&
					p.DeltaMeses == null &&
					p.DeltaAnnos == null &&
					p.Vigencia == true
				), 
				Arg.Any<NpgsqlTransaction?>()
			);
		}

		[Fact]
		public async Task CrearTest_SinCronNiFrecuencia() {
			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => tipoPeriodicidadBcp.Crear(10, "nombre-test", "descripcion-test", null, null, 7, null, null, 0, true));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);
			await tipoPeriodicidadDao.DidNotReceive().Insertar(Arg.Any<TipoPeriodicidad>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task CrearTest_ConCronYFrecuencia() {
			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => tipoPeriodicidadBcp.Crear(10, "nombre-test", "descripcion-test", "cron-test", 7, 7, null, null, 0, true));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);
			await tipoPeriodicidadDao.DidNotReceive().Insertar(Arg.Any<TipoPeriodicidad>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ModificarTest_Valido() {
			TipoPeriodicidad retorno = await tipoPeriodicidadBcp.Modificar(TipoPeriodicidadDummy(id: 10));
			Assert.Equal(10, retorno.Id);
			await tipoPeriodicidadDao.Received(1).Actualizar(
				Arg.Is<TipoPeriodicidad>(p => p.Id == 10),
				Arg.Any<NpgsqlTransaction?>()
			);
		}

		[Fact]
		public async Task ModificarTest_SinCronNiFrecuencia() {
			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => tipoPeriodicidadBcp.Modificar(TipoPeriodicidadDummy(id: 10, cron: null, frecuenciaDias: null)));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);
			await tipoPeriodicidadDao.DidNotReceive().Actualizar(Arg.Any<TipoPeriodicidad>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ModificarTest_ConCronYFrecuencia() {
			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => tipoPeriodicidadBcp.Modificar(TipoPeriodicidadDummy(id: 10, cron: "cron-test", frecuenciaDias: 7)));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);
			await tipoPeriodicidadDao.DidNotReceive().Actualizar(Arg.Any<TipoPeriodicidad>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task EliminarTest() {
			await tipoPeriodicidadBcp.Eliminar(TipoPeriodicidadDummy(id: 10));
			await tipoPeriodicidadDao.Received(1).Eliminar(10, Arg.Any<NpgsqlTransaction?>());
		}
	}
}
