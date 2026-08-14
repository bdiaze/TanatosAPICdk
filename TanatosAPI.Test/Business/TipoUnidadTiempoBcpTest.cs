using Npgsql;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Transactions;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Test.Business {
	public class TipoUnidadTiempoBcpTest {
		private readonly ITipoUnidadTiempoDao tipoUnidadTiempoDao = Substitute.For<ITipoUnidadTiempoDao>();
		private readonly TipoUnidadTiempoBcp tipoUnidadTiempoBcp;

		public TipoUnidadTiempoBcpTest() {
			tipoUnidadTiempoBcp = new(tipoUnidadTiempoDao);
		}

		public static TipoUnidadTiempo TipoUnidadTiempoDummy(
			long id = 1,
			string nombre = "nombre-test",
			string nombrePlural = "nombre-plural-test",
			long cantSegundos = 3600,
			long? cantMinutos = 60,
			long? cantHoras = 1,
			long? cantDias = null,
			bool vigencia = true
		) => new() {
			Id = id,
			Nombre = nombre,
			NombrePlural = nombrePlural,
			CantSegundos = cantSegundos,
			CantMinutos = cantMinutos,
			CantHoras = cantHoras,
			CantDias = cantDias,
			Vigencia = vigencia,
		};

		public static TheoryData<TipoUnidadTiempo?, bool> EstaVigenteCases => new() {
			{ TipoUnidadTiempoDummy(vigencia: true), true },
			{ TipoUnidadTiempoDummy(vigencia: false), false },
			{ null, false },
		};
		[Theory]
		[MemberData(nameof(EstaVigenteCases))]
		public void EstaVigenteTest(TipoUnidadTiempo? item, bool expectedResult) {
			Assert.Equal(expectedResult, tipoUnidadTiempoBcp.EstaVigente(item));
		}

		[Fact]
		public async Task ValidarTodosVigentesTest() {
			tipoUnidadTiempoDao.ObtenerPorVigencia(true, Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoDummy(id: 1),
				TipoUnidadTiempoDummy(id: 2),
				TipoUnidadTiempoDummy(id: 3),
			]);

			List<TipoUnidadTiempo> retorno = await tipoUnidadTiempoBcp.ValidarTodosVigentes([2, 3]);
			Assert.Equal(2, retorno.Count);
			Assert.DoesNotContain(1, retorno.Select(u => u.Id));
			Assert.Contains(2, retorno.Select(u => u.Id));
			Assert.Contains(3, retorno.Select(u => u.Id));
		}

		[Fact]
		public async Task ValidarTodosVigentesTest_ElementoNoVigente() {
			tipoUnidadTiempoDao.ObtenerPorVigencia(true, Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoDummy(id: 1),
				TipoUnidadTiempoDummy(id: 2),
				TipoUnidadTiempoDummy(id: 3),
			]);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => tipoUnidadTiempoBcp.ValidarTodosVigentes([2, 3, 4]));
			Assert.Equal(TipoErrorValidacion.NoVigente, ex.TipoErrorValidacion);
		}

		[Fact]
		public async Task ObtenerTest_SinParametros() {
			tipoUnidadTiempoDao.ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>()).Returns(TipoUnidadTiempoDummy(id: 1));

			TipoUnidadTiempo? retorno = await tipoUnidadTiempoBcp.Obtener(1);
			Assert.NotNull(retorno);
			Assert.Equal(1, retorno.Id);
			await tipoUnidadTiempoDao.Received(1).ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerTest_FiltrandoNoVigente() {
			tipoUnidadTiempoDao.ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>()).Returns(TipoUnidadTiempoDummy(id: 1, vigencia: false));

			TipoUnidadTiempo? retorno = await tipoUnidadTiempoBcp.Obtener(1, filtrarVigente: true);
			Assert.Null(retorno);
			await tipoUnidadTiempoDao.Received(1).ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerPorVigencia_Vigentes() {
			tipoUnidadTiempoDao.ObtenerPorVigencia(true, Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoDummy(id: 1),
				TipoUnidadTiempoDummy(id: 2)
			]);

			List<TipoUnidadTiempo> retorno = await tipoUnidadTiempoBcp.ObtenerPorVigencia(true);
			Assert.Equal(2, retorno.Count);
			await tipoUnidadTiempoDao.Received(1).ObtenerPorVigencia(true, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerPorVigencia_NoVigentes() {
			tipoUnidadTiempoDao.ObtenerPorVigencia(false, Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoDummy(id: 1),
				TipoUnidadTiempoDummy(id: 2)
			]);

			List<TipoUnidadTiempo> retorno = await tipoUnidadTiempoBcp.ObtenerPorVigencia(false);
			Assert.Equal(2, retorno.Count);
			await tipoUnidadTiempoDao.Received(1).ObtenerPorVigencia(false, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerPorVigencia_Nulos() {
			tipoUnidadTiempoDao.ObtenerPorVigencia(null, Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoDummy(id: 1),
				TipoUnidadTiempoDummy(id: 2)
			]);

			List<TipoUnidadTiempo> retorno = await tipoUnidadTiempoBcp.ObtenerPorVigencia(null);
			Assert.Equal(2, retorno.Count);
			await tipoUnidadTiempoDao.Received(1).ObtenerPorVigencia(null, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerVigentesTest() {
			tipoUnidadTiempoDao.ObtenerPorVigencia(true, Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoDummy(id: 1),
				TipoUnidadTiempoDummy(id: 2)
			]);

			List<TipoUnidadTiempo> retorno = await tipoUnidadTiempoBcp.ObtenerVigentes();
			Assert.Equal(2, retorno.Count);
			await tipoUnidadTiempoDao.Received(1).ObtenerPorVigencia(true, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task InsertarTest_Valido() {
			tipoUnidadTiempoDao.ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>()).Returns((TipoUnidadTiempo?)null);

			TipoUnidadTiempo retorno = await tipoUnidadTiempoBcp.Insertar(1, "nombre-test", "nombre-plural-test", 24 * 60 * 60, 24 * 60, 24, 1, true);
			Assert.Equal(1, retorno.Id);
			Assert.Equal("nombre-test", retorno.Nombre);
			Assert.Equal("nombre-plural-test", retorno.NombrePlural);
			Assert.Equal(24 * 60 * 60, retorno.CantSegundos);
			Assert.Equal(24 * 60, retorno.CantMinutos);
			Assert.Equal(24, retorno.CantHoras);
			Assert.Equal(1, retorno.CantDias);
			Assert.True(retorno.Vigencia);
			await tipoUnidadTiempoDao.Received(1).ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>());
			await tipoUnidadTiempoDao.Received(1).Insertar(Arg.Any<TipoUnidadTiempo>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task InsertarTest_Existente() {
			tipoUnidadTiempoDao.ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>()).Returns(TipoUnidadTiempoDummy(id: 1));

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => tipoUnidadTiempoBcp.Insertar(1, "nombre-test", "nombre-plural-test", 24 * 60 * 60, 24 * 60, 24, 1, true));
			Assert.Equal(TipoErrorValidacion.YaExiste, ex.TipoErrorValidacion);

			await tipoUnidadTiempoDao.Received(1).ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>());
			await tipoUnidadTiempoDao.DidNotReceive().Insertar(Arg.Any<TipoUnidadTiempo>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task InsertarTest_SinNombrePlural() {
			tipoUnidadTiempoDao.ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>()).Returns((TipoUnidadTiempo?)null);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => tipoUnidadTiempoBcp.Insertar(1, "nombre-test", null, 24 * 60 * 60, 24 * 60, 24, 1, true));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);

			await tipoUnidadTiempoDao.Received(1).ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>());
			await tipoUnidadTiempoDao.DidNotReceive().Insertar(Arg.Any<TipoUnidadTiempo>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task InsertarTest_ConDiasSinHoras() {
			tipoUnidadTiempoDao.ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>()).Returns((TipoUnidadTiempo?)null);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => tipoUnidadTiempoBcp.Insertar(1, "nombre-test", "nombre-plural-test", 24 * 60 * 60, 24 * 60, null, 1, true));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);

			await tipoUnidadTiempoDao.Received(1).ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>());
			await tipoUnidadTiempoDao.DidNotReceive().Insertar(Arg.Any<TipoUnidadTiempo>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task InsertarTest_ConHorasSinMinutos() {
			tipoUnidadTiempoDao.ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>()).Returns((TipoUnidadTiempo?)null);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => tipoUnidadTiempoBcp.Insertar(1, "nombre-test", "nombre-plural-test", 24 * 60 * 60, null, 24, 1, true));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);

			await tipoUnidadTiempoDao.Received(1).ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>());
			await tipoUnidadTiempoDao.DidNotReceive().Insertar(Arg.Any<TipoUnidadTiempo>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ActualizarTest_ValidoConCambios() {
			tipoUnidadTiempoDao.ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>()).Returns(
				TipoUnidadTiempoDummy(id: 1, nombre: "nombre-test", nombrePlural: "nombre-plural-test", cantSegundos: 3600, cantMinutos: 60, cantHoras: 1, cantDias: null, vigencia: true)
			);

			TipoUnidadTiempo retorno = await tipoUnidadTiempoBcp.Actualizar(1, "nombre-test", "nombre-plural-test", 24 * 60 * 60, 24 * 60, 24, 1, true);
			Assert.Equal(1, retorno.Id);
			Assert.Equal("nombre-test", retorno.Nombre);
			Assert.Equal("nombre-plural-test", retorno.NombrePlural);
			Assert.Equal(24 * 60 * 60, retorno.CantSegundos);
			Assert.Equal(24 * 60, retorno.CantMinutos);
			Assert.Equal(24, retorno.CantHoras);
			Assert.Equal(1, retorno.CantDias);
			Assert.True(retorno.Vigencia);
			await tipoUnidadTiempoDao.Received(1).ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>());
			await tipoUnidadTiempoDao.Received(1).Actualizar(Arg.Any<TipoUnidadTiempo>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ActualizarTest_ValidoSinCambios() {
			tipoUnidadTiempoDao.ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>()).Returns(
				TipoUnidadTiempoDummy(id: 1, nombre: "nombre-test", nombrePlural: "nombre-plural-test", cantSegundos: 3600, cantMinutos: 60, cantHoras: 1, cantDias: null, vigencia: true)
			);

			TipoUnidadTiempo retorno = await tipoUnidadTiempoBcp.Actualizar(1, "nombre-test", "nombre-plural-test", 3600, 60, 1, null, true);
			Assert.Equal(1, retorno.Id);
			Assert.Equal("nombre-test", retorno.Nombre);
			Assert.Equal("nombre-plural-test", retorno.NombrePlural);
			Assert.Equal(3600, retorno.CantSegundos);
			Assert.Equal(60, retorno.CantMinutos);
			Assert.Equal(1, retorno.CantHoras);
			Assert.Null(retorno.CantDias);
			Assert.True(retorno.Vigencia);
			await tipoUnidadTiempoDao.Received(1).ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>());
			await tipoUnidadTiempoDao.DidNotReceive().Actualizar(Arg.Any<TipoUnidadTiempo>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ActualizarTest_NoExistente() {
			tipoUnidadTiempoDao.ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>()).Returns(
				(TipoUnidadTiempo?)null
			);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => tipoUnidadTiempoBcp.Actualizar(1, "nombre-test", "nombre-plural-test", 24 * 60 * 60, 24 * 60, 24, 1, true));
			Assert.Equal(TipoErrorValidacion.NoVigente, ex.TipoErrorValidacion);

			await tipoUnidadTiempoDao.Received(1).ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>());
			await tipoUnidadTiempoDao.DidNotReceive().Actualizar(Arg.Any<TipoUnidadTiempo>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ActualizarTest_SinNombrePlural() {
			tipoUnidadTiempoDao.ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>()).Returns(
				TipoUnidadTiempoDummy(id: 1, nombre: "nombre-test", nombrePlural: "nombre-plural-test", cantSegundos: 3600, cantMinutos: 60, cantHoras: 1, cantDias: null, vigencia: true)
			);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => tipoUnidadTiempoBcp.Actualizar(1, "nombre-test", null, 24 * 60 * 60, 24 * 60, 24, 1, true));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);

			await tipoUnidadTiempoDao.Received(1).ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>());
			await tipoUnidadTiempoDao.DidNotReceive().Actualizar(Arg.Any<TipoUnidadTiempo>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ActualizarTest_ConDiasSinHoras() {
			tipoUnidadTiempoDao.ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>()).Returns(
				TipoUnidadTiempoDummy(id: 1, nombre: "nombre-test", nombrePlural: "nombre-plural-test", cantSegundos: 3600, cantMinutos: 60, cantHoras: 1, cantDias: null, vigencia: true)
			);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => tipoUnidadTiempoBcp.Actualizar(1, "nombre-test", null, 24 * 60 * 60, 24 * 60, null, 1, true));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);

			await tipoUnidadTiempoDao.Received(1).ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>());
			await tipoUnidadTiempoDao.DidNotReceive().Actualizar(Arg.Any<TipoUnidadTiempo>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ActualizarTest_ConHorasSinMinutos() {
			tipoUnidadTiempoDao.ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>()).Returns(
				TipoUnidadTiempoDummy(id: 1, nombre: "nombre-test", nombrePlural: "nombre-plural-test", cantSegundos: 3600, cantMinutos: 60, cantHoras: 1, cantDias: null, vigencia: true)
			);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => tipoUnidadTiempoBcp.Actualizar(1, "nombre-test", null, 24 * 60 * 60, null, 24, 1, true));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);

			await tipoUnidadTiempoDao.Received(1).ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>());
			await tipoUnidadTiempoDao.DidNotReceive().Actualizar(Arg.Any<TipoUnidadTiempo>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task EliminarTest_Valido() {
			tipoUnidadTiempoDao.ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>()).Returns(
				TipoUnidadTiempoDummy(id: 1)
			);

			await tipoUnidadTiempoBcp.Eliminar(1);
			await tipoUnidadTiempoDao.Received(1).ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>());
			await tipoUnidadTiempoDao.Received(1).Eliminar(1, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task EliminarTest_NoExistente() {
			tipoUnidadTiempoDao.ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>()).Returns(
				(TipoUnidadTiempo?)null
			);

			await tipoUnidadTiempoBcp.Eliminar(1);
			await tipoUnidadTiempoDao.Received(1).ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>());
			await tipoUnidadTiempoDao.DidNotReceive().Eliminar(Arg.Any<long>(), Arg.Any<NpgsqlTransaction?>());
		}
	}
}
