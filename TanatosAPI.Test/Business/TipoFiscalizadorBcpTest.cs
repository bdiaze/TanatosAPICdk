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

namespace TanatosAPI.Test.Business {
	public class TipoFiscalizadorBcpTest {
		private readonly ITipoFiscalizadorDao tipoFiscalizadorDao = Substitute.For<ITipoFiscalizadorDao>();
		private readonly TipoFiscalizadorBcp tipoFiscalizadorBcp;

		public TipoFiscalizadorBcpTest() {
			tipoFiscalizadorBcp = new(tipoFiscalizadorDao);
		}

		public static TipoFiscalizador TipoFiscalizadorDummy(
			long id = 1,
			string nombre = "nombre-test",
			string? nombreCorto = "nombre-corto-test",
			bool vigencia = true
		) => new() {
			Id = id,
			Nombre = nombre,
			NombreCorto = nombreCorto,
			Vigencia = vigencia,
		};

		public static TheoryData<TipoFiscalizador?, bool> EstaVigenteCases => new() {
			{ TipoFiscalizadorDummy(vigencia: true), true },
			{ TipoFiscalizadorDummy(vigencia: false), false },
			{ null, false },
		};
		[Theory]
		[MemberData(nameof(EstaVigenteCases))]
		public void EstaVigenteTest(TipoFiscalizador? item, bool expectedResult) {
			Assert.Equal(expectedResult, tipoFiscalizadorBcp.EstaVigente(item));
		}

		[Fact]
		public async Task ValidarTodosVigentesTest() {
			tipoFiscalizadorDao.ObtenerPorVigencia(true, Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorDummy(id: 1),
				TipoFiscalizadorDummy(id: 2),
				TipoFiscalizadorDummy(id: 3),
			]);

			List<TipoFiscalizador> retorno = await tipoFiscalizadorBcp.ValidarTodosVigentes([2, 3]);
			Assert.Equal(2, retorno.Count);
			Assert.DoesNotContain(1, retorno.Select(u => u.Id));
			Assert.Contains(2, retorno.Select(u => u.Id));
			Assert.Contains(3, retorno.Select(u => u.Id));
		}

		[Fact]
		public async Task ValidarTodosVigentesTest_ElementoNoVigente() {
			tipoFiscalizadorDao.ObtenerPorVigencia(true, Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorDummy(id: 1),
				TipoFiscalizadorDummy(id: 2),
				TipoFiscalizadorDummy(id: 3),
			]);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => tipoFiscalizadorBcp.ValidarTodosVigentes([2, 3, 4]));
			Assert.Equal(TipoErrorValidacion.NoVigente, ex.TipoErrorValidacion);
		}

		[Fact]
		public async Task ObtenerTest() {
			tipoFiscalizadorDao.ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>()).Returns(TipoFiscalizadorDummy(id: 1));

			TipoFiscalizador? retorno = await tipoFiscalizadorBcp.Obtener(1);
			Assert.NotNull(retorno);
			Assert.Equal(1, retorno.Id);
			await tipoFiscalizadorDao.Received(1).ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerVigentesTest() {
			tipoFiscalizadorDao.ObtenerPorVigencia(true, Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorDummy(id: 1),
				TipoFiscalizadorDummy(id: 2)
			]);

			List<TipoFiscalizador> retorno = await tipoFiscalizadorBcp.ObtenerVigentes();
			Assert.Equal(2, retorno.Count);
			await tipoFiscalizadorDao.Received(1).ObtenerPorVigencia(true, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerPorVigencia_Vigentes() {
			tipoFiscalizadorDao.ObtenerPorVigencia(true, Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorDummy(id: 1),
				TipoFiscalizadorDummy(id: 2)
			]);

			List<TipoFiscalizador> retorno = await tipoFiscalizadorBcp.ObtenerPorVigencia(true);
			Assert.Equal(2, retorno.Count);
			await tipoFiscalizadorDao.Received(1).ObtenerPorVigencia(true, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerPorVigencia_NoVigentes() {
			tipoFiscalizadorDao.ObtenerPorVigencia(false, Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorDummy(id: 1),
				TipoFiscalizadorDummy(id: 2)
			]);

			List<TipoFiscalizador> retorno = await tipoFiscalizadorBcp.ObtenerPorVigencia(false);
			Assert.Equal(2, retorno.Count);
			await tipoFiscalizadorDao.Received(1).ObtenerPorVigencia(false, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerPorVigencia_Nulos() {
			tipoFiscalizadorDao.ObtenerPorVigencia(null, Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorDummy(id: 1),
				TipoFiscalizadorDummy(id: 2)
			]);

			List<TipoFiscalizador> retorno = await tipoFiscalizadorBcp.ObtenerPorVigencia(null);
			Assert.Equal(2, retorno.Count);
			await tipoFiscalizadorDao.Received(1).ObtenerPorVigencia(null, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task CrearTest() {
			TipoFiscalizador retorno = await tipoFiscalizadorBcp.Crear(1, "nombre-test", "nombre-corto-test", true);
			Assert.Equal(1, retorno.Id);
			Assert.Equal("nombre-test", retorno.Nombre);
			Assert.Equal("nombre-corto-test", retorno.NombreCorto);
			Assert.True(retorno.Vigencia);
			await tipoFiscalizadorDao.Received(1).Insertar(Arg.Any<TipoFiscalizador>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ActualizarTest() {
			await tipoFiscalizadorBcp.Actualizar(TipoFiscalizadorDummy(id: 1));
			await tipoFiscalizadorDao.Received(1).Actualizar(
				Arg.Is<TipoFiscalizador>(f => f.Id == 1), 
				Arg.Any<NpgsqlTransaction?>()
			);
		}

		[Fact]
		public async Task EliminarTest() {
			await tipoFiscalizadorBcp.Eliminar(1);
			await tipoFiscalizadorDao.Received(1).Eliminar(1, Arg.Any<NpgsqlTransaction?>());
		}
	}
}
