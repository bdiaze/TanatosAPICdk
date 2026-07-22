using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;
using TanatosAPI.UseCases;

namespace TanatosAPI.Test.Business {
	public class CargoBcpTest {
		private readonly IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
		private readonly ICargoDao cargoDao = Substitute.For<ICargoDao>();
		private readonly CargoBcp cargoBcp;

		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

		public CargoBcpTest() {
			dateTimeProvider.UtcNow.Returns(FECHA_DUMMY);

			cargoBcp = new(dateTimeProvider, cargoDao);
		}

		public static Cargo CargoDummy(
			long id = 1,
			string sub = "SubCargoTest",
			long idNegocio = 10,
			string nombre = "NombreCargoTest",
			DateTime? fechaCreacion = null,
			DateTime? fechaEliminacion = null,
			bool vigencia = true
		) => new() { 
			Id = id,
			Sub = sub,
			IdNegocio = idNegocio,
			Nombre = nombre,
			FechaCreacion = fechaCreacion ?? DateTime.UtcNow,
			FechaEliminacion = fechaEliminacion,
			Vigencia = vigencia,
		};

		public static TheoryData<Cargo?, bool> EstaVigenteCases => new() {
			{ CargoDummy(vigencia: true), true },
			{ CargoDummy(vigencia: false), false },
			{ null, false },
		};
		[Theory]
		[MemberData(nameof(EstaVigenteCases))]
		public void EstaVigenteTest(Cargo? cargo, bool expectedResult) {
			Assert.Equal(expectedResult, cargoBcp.EstaVigente(cargo));
		}

		public static TheoryData<Cargo, bool> PerteneceAlUsuarioCases => new() {
			{ CargoDummy(sub: "sub-test-123"), true },
			{ CargoDummy(sub: "otro-sub-test-123"), false }
		};
		[Theory]
		[MemberData(nameof(PerteneceAlUsuarioCases))]
		public void PerteneceAlUsuarioTest(Cargo cargo, bool expectedResult) {
			Assert.Equal(expectedResult, cargoBcp.PerteneceAlUsuario(cargo, "sub-test-123"));
		}

		[Theory]
		[InlineData(1L, 1L)]
		[InlineData(2L, 2L)]
		[InlineData(3L, null)]
		public async Task ObtenerPorIdTest(long idCargo, long? expectedIdResult) {
			cargoDao.Obtener(1).Returns(CargoDummy(id: 1));
			cargoDao.Obtener(2).Returns(CargoDummy(id: 2));
			cargoDao.Obtener(3).Returns((Cargo?)null);

			Cargo? cargo = await cargoBcp.Obtener(idCargo);
			Assert.Equal(expectedIdResult, cargo?.Id);
			await cargoDao.Received(1).Obtener(idCargo);
		}

		[Fact]
		public async Task ObtenerPorIdValidandoTest_Valido() {
			cargoDao.Obtener(1).Returns(CargoDummy(id: 1, sub: "sub-test-123", vigencia: true));

			Cargo cargo = await cargoBcp.ObtenerValidandoVigenciaYPertenencia(1, "sub-test-123");
			Assert.Equal(1, cargo.Id);
			Assert.Equal("sub-test-123", cargo.Sub);
			Assert.True(cargo.Vigencia);
			await cargoDao.Received(1).Obtener(1);
		}

		[Fact]
		public async Task ObtenerPorIdValidandoTest_NoVigente() {
			cargoDao.Obtener(1).Returns(CargoDummy(id: 1, sub: "sub-test-123", vigencia: false));

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => cargoBcp.ObtenerValidandoVigenciaYPertenencia(1, "sub-test-123"));
			Assert.Equal(TipoErrorValidacion.NoVigente, ex.TipoErrorValidacion);
		}

		[Fact]
		public async Task ObtenerPorIdValidandoTest_NoPertenece() {
			cargoDao.Obtener(1).Returns(CargoDummy(id: 1, sub: "sub-test-123", vigencia: true));

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => cargoBcp.ObtenerValidandoVigenciaYPertenencia(1, "otro-sub-test"));
			Assert.Equal(TipoErrorValidacion.NoPertenece, ex.TipoErrorValidacion);
		}

		
		[Theory]
		[InlineData("sub-test-1", 1L, 2L)]
		[InlineData("sub-test-1", 2L, 1L)]
		[InlineData("sub-test-1", 3L, 0L)]
		[InlineData("sub-test-2", 1L, 0L)]
		public async Task ObtenerVigentes(string sub, long idNegocio, long? expectedCount) {
			cargoDao.ObtenerPorSub(Arg.Any<string>(), Arg.Any<long>(), true).Returns([]);
			cargoDao.ObtenerPorSub("sub-test-1", 1, true).Returns([
				CargoDummy(sub: "sub-test-1", idNegocio: 1, id: 1),
				CargoDummy(sub: "sub-test-1", idNegocio: 1, id: 2),
			]);
			cargoDao.ObtenerPorSub("sub-test-1", 2, true).Returns([
				CargoDummy(sub: "sub-test-1", idNegocio: 2, id: 3),
			]);

			List<Cargo> cargos = await cargoBcp.ObtenerVigentes(sub, idNegocio);
			Assert.Equal(expectedCount, cargos.Count);
			Assert.All(cargos, (cargo) => Assert.True(cargo.Vigencia));
			await cargoDao.Received(1).ObtenerPorSub(sub, idNegocio, true);
		}

		[Fact]
		public async Task RegistrarCargoTest() {
			cargoDao.Insertar(Arg.Any<Cargo>()).Returns(99);

			Cargo cargo = await cargoBcp.Crear("sub-test-1", "nombre-cargo-1", 10);

			Assert.Equal(99, cargo.Id);
			Assert.Equal("sub-test-1", cargo.Sub);
			Assert.Equal("nombre-cargo-1", cargo.Nombre);
			Assert.Equal(10, cargo.IdNegocio);
			Assert.Equal(FECHA_DUMMY, cargo.FechaCreacion);
			Assert.True(cargo.Vigencia);
			await cargoDao.Received(1).Insertar(Arg.Any<Cargo>());
		}

		[Fact]
		public async Task ModificarCargoTest() {
			Cargo cargo = CargoDummy();
			await cargoBcp.Actualizar(cargo);
			await cargoDao.Received(1).Actualizar(Arg.Any<Cargo>());
		}

		[Fact]
		public async Task EliminarTest_Vigente() {
			Cargo cargo = CargoDummy(vigencia: true);
			await cargoBcp.Eliminar(cargo);
			Assert.Equal(FECHA_DUMMY, cargo.FechaEliminacion);
			Assert.False(cargo.Vigencia);
			await cargoDao.Received(1).Actualizar(Arg.Any<Cargo>());
		}

		[Fact]
		public async Task EliminarTest_NoVigente() {
			Cargo cargo = CargoDummy(vigencia: false);
			await cargoBcp.Eliminar(cargo);
			await cargoDao.DidNotReceive().Actualizar(Arg.Any<Cargo>());
		}
	}
}
