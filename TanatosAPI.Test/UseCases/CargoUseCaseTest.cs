using Microsoft.AspNetCore.SignalR;
using Npgsql;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.UseCases;

namespace TanatosAPI.Test.UseCases {
	public class CargoUseCaseTest {
		private readonly IDatabaseConnectionHelper connectionHelper = Substitute.For<IDatabaseConnectionHelper>();
		private readonly ICargoBcp cargoBcp = Substitute.For<ICargoBcp>();
		private readonly INegocioBcp negocioBcp = Substitute.For<INegocioBcp>();
		private readonly IEmpleadoBcp empleadoBcp = Substitute.For<IEmpleadoBcp>();

		private readonly IDatabaseConnection connection = Substitute.For<IDatabaseConnection>();
		private readonly IDatabaseTransaction transaction = Substitute.For<IDatabaseTransaction>();

		private readonly CargoUseCase cargoUseCase;

		public CargoUseCaseTest() {
			connection.BeginTransactionAsync().Returns(transaction);
			connectionHelper.ObtenerConexionWrapper().Returns(connection);

			cargoUseCase = new(connectionHelper, cargoBcp, negocioBcp, empleadoBcp);
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

		public static Negocio NegocioDummy(
			long id = 1,
			string sub = "SubNegocioTest",
			string nombre = "NombreNegocioTest",
			string direccion = "DireccionNegocioTest",
			long idTipoActividad = 1,
			DateTime? fechaCreacion = null,
			DateTime? fechaEliminacion = null,
			bool vigencia = true
		) => new() {
			Id = id,
			Sub = sub,
			Nombre = nombre,
			Direccion = direccion,
			IdTipoActividad = idTipoActividad,
			FechaCreacion = fechaCreacion ?? DateTime.UtcNow,
			FechaEliminacion = fechaEliminacion,
			Vigencia = vigencia,
		};

		[Fact]
		public async Task ObtenerVigentesTest_Valido() {
			negocioBcp.Obtener(10, validarVigencia: true, validarSub: "sub-test-123", transaction: Arg.Any<NpgsqlTransaction?>()).Returns(NegocioDummy(id: 10, sub: "sub-test-123", vigencia: true));
			cargoBcp.ObtenerPorSubYNegocio("sub-test-123", 10, filtrarVigente: true).Returns([
				CargoDummy(id:1, sub: "sub-test-123", idNegocio: 10, vigencia: true),
				CargoDummy(id:2, sub: "sub-test-123", idNegocio: 10, vigencia: true)
			]);

			List<Cargo> cargos = await cargoUseCase.ObtenerVigentes("sub-test-123", 10);

			Assert.Equal(2, cargos.Count);
			Assert.All(cargos, (cargo) => Assert.True(cargo.Vigencia));
			await negocioBcp.Received(1).Obtener(10, validarVigencia: true, validarSub: "sub-test-123", transaction: Arg.Any<NpgsqlTransaction?>());
			await cargoBcp.Received(1).ObtenerPorSubYNegocio("sub-test-123", 10, filtrarVigente: true);
		}

		[Fact]
		public async Task RegistrarCargoTest_Valido() {
			negocioBcp.Obtener(10, validarVigencia: true, validarSub: "sub-test-123", transaction: Arg.Any<NpgsqlTransaction?>()).Returns(NegocioDummy(id: 10, sub: "sub-test-123", vigencia: true));
			cargoBcp.ObtenerPorSubYNegocio("sub-test-123", 10, filtrarVigente: true).Returns([]);
			cargoBcp.Crear("sub-test-123", "nombre-cargo-test", 10).Returns(CargoDummy(id: 99, sub: "sub-test-123", nombre: "nombre-cargo-test", idNegocio: 10, vigencia: true));
			
			Cargo cargo = await cargoUseCase.Crear("sub-test-123", "nombre-cargo-test", 10);
			Assert.Equal(99, cargo.Id);
			Assert.Equal("sub-test-123", cargo.Sub);
			Assert.Equal("nombre-cargo-test", cargo.Nombre);
			Assert.Equal(10, cargo.IdNegocio);
			Assert.True(cargo.Vigencia);
			await negocioBcp.Received(1).Obtener(10, validarVigencia: true, validarSub: "sub-test-123", transaction: Arg.Any<NpgsqlTransaction?>());
			await cargoBcp.Received(1).ObtenerPorSubYNegocio("sub-test-123", 10, filtrarVigente: true);
			await cargoBcp.Received(1).Crear("sub-test-123", "nombre-cargo-test", 10);
		}

		[Fact]
		public async Task RegistrarCargoTest_Existente() {
			negocioBcp.Obtener(10, validarVigencia: true, validarSub: "sub-test-123", transaction: Arg.Any<NpgsqlTransaction?>()).Returns(NegocioDummy(id: 10, sub: "sub-test-123", vigencia: true));
			cargoBcp.ObtenerPorSubYNegocio("sub-test-123", 10, filtrarVigente: true).Returns([
				CargoDummy(id: 1, sub: "sub-test-123", nombre: "nombre-cargo-test", idNegocio: 10, vigencia: true)
			]);

			Cargo cargo = await cargoUseCase.Crear("sub-test-123", "nombre-cargo-test", 10);
			Assert.Equal(1, cargo.Id);
			Assert.Equal("sub-test-123", cargo.Sub);
			Assert.Equal("nombre-cargo-test", cargo.Nombre);
			Assert.Equal(10, cargo.IdNegocio);
			Assert.True(cargo.Vigencia);
			await negocioBcp.Received(1).Obtener(10, validarVigencia: true, validarSub: "sub-test-123", transaction: Arg.Any<NpgsqlTransaction?>());
			await cargoBcp.Received(1).ObtenerPorSubYNegocio("sub-test-123", 10, filtrarVigente: true);
			await cargoBcp.DidNotReceive().Crear("sub-test-123", "nombre-cargo-test", 10);
		}

		[Fact]
		public async Task ActualizarCargoTest_Valido() {
			cargoBcp.Obtener(1, validarVigencia: true, validarSub: "sub-test-123").Returns(CargoDummy(id: 1, sub: "sub-test-123", nombre: "antiguo-nombre-cargo", idNegocio: 10, vigencia: true));
			negocioBcp.Obtener(10, validarVigencia: true, validarSub: "sub-test-123", transaction: Arg.Any<NpgsqlTransaction?>()).Returns(NegocioDummy(id: 10, sub: "sub-test-123", vigencia: true));
			cargoBcp.ObtenerPorSubYNegocio("sub-test-123", 10, filtrarVigente: true).Returns([
				CargoDummy(id: 1, sub: "sub-test-123", idNegocio: 10, vigencia: true)
			]);
			
			Cargo cargo = await cargoUseCase.Actualizar("sub-test-123", 1, "nuevo-nombre-cargo");
			Assert.Equal(1, cargo.Id);
			Assert.Equal("sub-test-123", cargo.Sub);
			Assert.Equal("nuevo-nombre-cargo", cargo.Nombre);
			Assert.Equal(10, cargo.IdNegocio);
			Assert.True(cargo.Vigencia);
			await cargoBcp.Received(1).Obtener(1, validarVigencia: true, validarSub: "sub-test-123");
			await negocioBcp.Received(1).Obtener(10, validarVigencia: true, validarSub: "sub-test-123", transaction: Arg.Any<NpgsqlTransaction?>());
			await cargoBcp.Received(1).ObtenerPorSubYNegocio("sub-test-123", 10, filtrarVigente: true);
			await cargoBcp.Received(1).Actualizar(Arg.Is<Cargo>(c => c.Id == 1 && c.Nombre == "nuevo-nombre-cargo" && c.Sub == "sub-test-123"));
		}

		[Fact]
		public async Task ActualizarCargoTest_MismoNombreOtroExistente() {
			cargoBcp.Obtener(1, validarVigencia: true, validarSub: "sub-test-123").Returns(CargoDummy(id: 1, sub: "sub-test-123", nombre: "antiguo-nombre-cargo", idNegocio: 10, vigencia: true));
			negocioBcp.Obtener(10, validarVigencia: true, validarSub: "sub-test-123", transaction: Arg.Any<NpgsqlTransaction?>()).Returns(NegocioDummy(id: 10, sub: "sub-test-123", vigencia: true));
			cargoBcp.ObtenerPorSubYNegocio("sub-test-123", 10, filtrarVigente: true).Returns([
				CargoDummy(id: 1, sub: "sub-test-123", idNegocio: 10, vigencia: true),
				CargoDummy(id: 2, sub: "sub-test-123", nombre: "otro-con-mismo-nombre", idNegocio: 10, vigencia: true)
			]);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => cargoUseCase.Actualizar("sub-test-123", 1, nombre: "otro-con-mismo-nombre"));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);
			await cargoBcp.Received(1).Obtener(1, validarVigencia: true, validarSub: "sub-test-123");
			await negocioBcp.Received(1).Obtener(10, validarVigencia: true, validarSub: "sub-test-123", transaction: Arg.Any<NpgsqlTransaction?>());
			await cargoBcp.Received(1).ObtenerPorSubYNegocio("sub-test-123", 10, filtrarVigente: true);
			await cargoBcp.DidNotReceive().Actualizar(Arg.Any<Cargo>());
		}

		[Fact]
		public async Task ActualizarCargoTest_MismoNombreExistente() {
			cargoBcp.Obtener(1, validarVigencia: true, validarSub: "sub-test-123").Returns(CargoDummy(id: 1, sub: "sub-test-123", nombre: "antiguo-nombre-cargo", idNegocio: 10, vigencia: true));
			negocioBcp.Obtener(10, validarVigencia: true, validarSub: "sub-test-123", transaction: Arg.Any<NpgsqlTransaction?>()).Returns(NegocioDummy(id: 10, sub: "sub-test-123", vigencia: true));
			cargoBcp.ObtenerPorSubYNegocio("sub-test-123", 10, filtrarVigente: true).Returns([
				CargoDummy(id: 1, sub: "sub-test-123", nombre: "antiguo-nombre-cargo", idNegocio: 10, vigencia: true),
			]);

			Cargo cargo = await cargoUseCase.Actualizar("sub-test-123", 1, "antiguo-nombre-cargo");
			Assert.Equal(1, cargo.Id);
			Assert.Equal("sub-test-123", cargo.Sub);
			Assert.Equal("antiguo-nombre-cargo", cargo.Nombre);
			Assert.Equal(10, cargo.IdNegocio);
			Assert.True(cargo.Vigencia);
			await cargoBcp.Received(1).Obtener(1, validarVigencia: true, validarSub: "sub-test-123");
			await negocioBcp.Received(1).Obtener(10, validarVigencia: true, validarSub: "sub-test-123", transaction: Arg.Any<NpgsqlTransaction?>());
			await cargoBcp.Received(1).ObtenerPorSubYNegocio("sub-test-123", 10, filtrarVigente: true);
			await cargoBcp.DidNotReceive().Actualizar(Arg.Any<Cargo>());
		}

		[Fact]
		public async Task EliminarCargoTest_Valido() {
			cargoBcp.Obtener(1, transaction: Arg.Any<NpgsqlTransaction>()).Returns(CargoDummy(id: 1, sub: "sub-test-123", nombre: "antiguo-nombre-cargo", idNegocio: 10, vigencia: true));
			cargoBcp.PerteneceAlUsuario(Arg.Any<Cargo>(), Arg.Any<string>()).Returns(true);
			cargoBcp.EstaVigente(Arg.Any<Cargo>()).Returns(true);

			await cargoUseCase.Eliminar("sub-test-123", 1, null);

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await cargoBcp.Received(1).Obtener(1, transaction: Arg.Any<NpgsqlTransaction>());
			cargoBcp.Received(1).PerteneceAlUsuario(Arg.Any<Cargo>(), Arg.Any<string>());
			cargoBcp.Received(1).EstaVigente(Arg.Any<Cargo>());
			await empleadoBcp.Received(1).DesasociarCargo(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<NpgsqlTransaction>());
			await cargoBcp.Received(1).Eliminar(Arg.Any<Cargo>(), Arg.Any<NpgsqlTransaction>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task EliminarCargoTest_NoPerteneciente() {
			cargoBcp.Obtener(1, transaction: Arg.Any<NpgsqlTransaction>()).Returns(CargoDummy(id: 1, sub: "sub-test-123", nombre: "antiguo-nombre-cargo", idNegocio: 10, vigencia: true));
			cargoBcp.PerteneceAlUsuario(Arg.Any<Cargo>(), Arg.Any<string>()).Returns(false);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => cargoUseCase.Eliminar("sub-test-123", 1, null));
			Assert.Equal(TipoErrorValidacion.NoPertenece, ex.TipoErrorValidacion);

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await cargoBcp.Received(1).Obtener(1, transaction: Arg.Any<NpgsqlTransaction>());
			cargoBcp.Received(1).PerteneceAlUsuario(Arg.Any<Cargo>(), Arg.Any<string>());
			await empleadoBcp.DidNotReceive().DesasociarCargo(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<NpgsqlTransaction>());
			await cargoBcp.DidNotReceive().Eliminar(Arg.Any<Cargo>(), Arg.Any<NpgsqlTransaction>());
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task EliminarCargoTest_NoVigente() {
			cargoBcp.Obtener(1, transaction: Arg.Any<NpgsqlTransaction>()).Returns(CargoDummy(id: 1, sub: "sub-test-123", nombre: "antiguo-nombre-cargo", idNegocio: 10, vigencia: true));
			cargoBcp.PerteneceAlUsuario(Arg.Any<Cargo>(), Arg.Any<string>()).Returns(true);
			cargoBcp.EstaVigente(Arg.Any<Cargo>()).Returns(false);

			await cargoUseCase.Eliminar("sub-test-123", 1, null);

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await cargoBcp.Received(1).Obtener(1, transaction: Arg.Any<NpgsqlTransaction>());
			cargoBcp.Received(1).PerteneceAlUsuario(Arg.Any<Cargo>(), Arg.Any<string>());
			cargoBcp.Received(1).EstaVigente(Arg.Any<Cargo>());
			await empleadoBcp.DidNotReceive().DesasociarCargo(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<NpgsqlTransaction>());
			await cargoBcp.DidNotReceive().Eliminar(Arg.Any<Cargo>(), Arg.Any<NpgsqlTransaction>());
			await transaction.DidNotReceive().CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task EliminarCargoTest_Excepcion() {
			cargoBcp.Obtener(1, transaction: Arg.Any<NpgsqlTransaction>()).Returns(CargoDummy(id: 1, sub: "sub-test-123", nombre: "antiguo-nombre-cargo", idNegocio: 10, vigencia: true));
			cargoBcp.PerteneceAlUsuario(Arg.Any<Cargo>(), Arg.Any<string>()).Returns(true);
			cargoBcp.EstaVigente(Arg.Any<Cargo>()).Returns(true);
			cargoBcp.Eliminar(Arg.Any<Cargo>(), Arg.Any<NpgsqlTransaction>()).ThrowsAsync(new Exception("error-test"));

			await Assert.ThrowsAsync<Exception>(() => cargoUseCase.Eliminar("sub-test-123", 1, null));

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await cargoBcp.Received(1).Obtener(1, transaction: Arg.Any<NpgsqlTransaction>());
			cargoBcp.Received(1).PerteneceAlUsuario(Arg.Any<Cargo>(), Arg.Any<string>());
			cargoBcp.Received(1).EstaVigente(Arg.Any<Cargo>());
			await empleadoBcp.Received(1).DesasociarCargo(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<NpgsqlTransaction>());
			await cargoBcp.Received(1).Eliminar(Arg.Any<Cargo>(), Arg.Any<NpgsqlTransaction>());
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task EliminarCargoTest_Inexistente() {
			cargoBcp.Obtener(1, transaction: Arg.Any<NpgsqlTransaction>()).Returns((Cargo?)null);
			cargoBcp.EstaVigente(Arg.Any<Cargo>()).Returns(false);

			await cargoUseCase.Eliminar("sub-test-123", 1, null);

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await cargoBcp.Received(1).Obtener(1, transaction: Arg.Any<NpgsqlTransaction>());
			cargoBcp.DidNotReceive().PerteneceAlUsuario(Arg.Any<Cargo>(), Arg.Any<string>());
			cargoBcp.Received(1).EstaVigente(Arg.Any<Cargo>());
			await empleadoBcp.DidNotReceive().DesasociarCargo(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<NpgsqlTransaction>());
			await cargoBcp.DidNotReceive().Eliminar(Arg.Any<Cargo>(), Arg.Any<NpgsqlTransaction>());
			await transaction.DidNotReceive().CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task EliminarCargoTest_ExcepcionSinConexion() {
			connection.BeginTransactionAsync().ThrowsAsync(new Exception("error-test"));

			await Assert.ThrowsAsync<Exception>(() => cargoUseCase.Eliminar("sub-test-123", 1, null));

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await cargoBcp.DidNotReceive().Obtener(1, transaction: Arg.Any<NpgsqlTransaction>());
			cargoBcp.DidNotReceive().PerteneceAlUsuario(Arg.Any<Cargo>(), Arg.Any<string>());
			cargoBcp.DidNotReceive().EstaVigente(Arg.Any<Cargo>());
			await empleadoBcp.DidNotReceive().DesasociarCargo(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<NpgsqlTransaction>());
			await cargoBcp.DidNotReceive().Eliminar(Arg.Any<Cargo>(), Arg.Any<NpgsqlTransaction>());
			await transaction.DidNotReceive().CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.DidNotReceive().DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task EliminarCargoTest_ExcepcionConConexionExterna() {
			cargoBcp.Obtener(1, transaction: Arg.Any<NpgsqlTransaction>()).Returns(CargoDummy(id: 1, sub: "sub-test-123", nombre: "antiguo-nombre-cargo", idNegocio: 10, vigencia: true));
			cargoBcp.PerteneceAlUsuario(Arg.Any<Cargo>(), Arg.Any<string>()).Returns(true);
			cargoBcp.EstaVigente(Arg.Any<Cargo>()).Returns(true);
			cargoBcp.Eliminar(Arg.Any<Cargo>(), Arg.Any<NpgsqlTransaction>()).ThrowsAsync(new Exception("error-test"));

			await Assert.ThrowsAsync<Exception>(() => cargoUseCase.Eliminar("sub-test-123", 1, transaction));

			await connectionHelper.DidNotReceive().ObtenerConexionWrapper();
			await connection.DidNotReceive().BeginTransactionAsync();
			await cargoBcp.Received(1).Obtener(1, transaction: Arg.Any<NpgsqlTransaction>());
			cargoBcp.Received(1).PerteneceAlUsuario(Arg.Any<Cargo>(), Arg.Any<string>());
			cargoBcp.Received(1).EstaVigente(Arg.Any<Cargo>());
			await empleadoBcp.Received(1).DesasociarCargo(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<NpgsqlTransaction>());
			await cargoBcp.Received(1).Eliminar(Arg.Any<Cargo>(), Arg.Any<NpgsqlTransaction>());
			await transaction.DidNotReceive().CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.DidNotReceive().DisposeAsync();
			await connection.DidNotReceive().DisposeAsync();
		}
	}
}
