using Npgsql;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Flow;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.UseCases;

namespace TanatosAPI.Test.Business {
	public class UsuarioBcpTest {
		private readonly IUsuarioDao usuarioDao = Substitute.For<IUsuarioDao>();
		private readonly ICognitoHelper cognitoHelper = Substitute.For<ICognitoHelper>();
		private readonly IFlowHelper flowHelper = Substitute.For<IFlowHelper>();
		private readonly UsuarioBcp usuarioBcp;

		public UsuarioBcpTest() {
			usuarioBcp = new(usuarioDao, cognitoHelper, flowHelper);
		}

		public static Usuario UsuarioDummy(
			string sub = "sub-test",
			string? flowCustomerId = "flow-customer-id-test",
			string? nombre = "nombre-test",
			string? apellido = "apellido-test",
			string? correoElectronico = "correo@test.cl"
		) => new() {
			Sub = sub,
			FlowCustomerId = flowCustomerId,
			Nombre = nombre,
			Apellido = apellido,
			CorreoElectronico = correoElectronico
		};

		[Fact]
		public async Task ObtenerPorFlowCustomerIdTest() {
			usuarioDao.ObtenerPorFlowCustomerId("flow-customer-id-test", Arg.Any<NpgsqlTransaction?>()).Returns(UsuarioDummy(flowCustomerId: "flow-customer-id-test"));

			Usuario? retorno = await usuarioBcp.ObtenerPorFlowCustomerId("flow-customer-id-test");
			Assert.NotNull(retorno);
			Assert.Equal("flow-customer-id-test", retorno.FlowCustomerId);
			await usuarioDao.Received(1).ObtenerPorFlowCustomerId("flow-customer-id-test", Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerInformacionUsuarioTest_Existente() {
			usuarioDao.Obtener("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(UsuarioDummy(sub: "sub-test"));

			Usuario usuario = await usuarioBcp.ObtenerInformacionUsuario("sub-test");
			Assert.Equal("sub-test", usuario.Sub);
			await usuarioDao.Received(1).Obtener("sub-test", Arg.Any<NpgsqlTransaction?>());
			await cognitoHelper.DidNotReceive().ObtenerUsuario(Arg.Any<string>());
			await usuarioDao.DidNotReceive().Insertar(Arg.Any<Usuario>(), Arg.Any<NpgsqlTransaction?>());
			await usuarioDao.DidNotReceive().Actualizar(Arg.Any<Usuario>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerInformacionUsuarioTest_AtributoNulo() {
			usuarioDao.Obtener("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(UsuarioDummy(sub: "sub-test", nombre: null, apellido: null, correoElectronico: null));
			cognitoHelper.ObtenerUsuario("sub-test").Returns(new Dictionary<string, string>() {
				["given_name"] = "nombre-test",
				["family_name"] = "apellido-test",
				["email"] = "correo@test.cl"
			});

			Usuario usuario = await usuarioBcp.ObtenerInformacionUsuario("sub-test");
			Assert.Equal("sub-test", usuario.Sub);
			Assert.Equal("nombre-test", usuario.Nombre);
			Assert.Equal("apellido-test", usuario.Apellido);
			Assert.Equal("correo@test.cl", usuario.CorreoElectronico);
			await usuarioDao.Received(1).Obtener("sub-test", Arg.Any<NpgsqlTransaction?>());
			await cognitoHelper.Received(1).ObtenerUsuario(Arg.Any<string>());
			await usuarioDao.DidNotReceive().Insertar(Arg.Any<Usuario>(), Arg.Any<NpgsqlTransaction?>());
			await usuarioDao.Received(1).Actualizar(Arg.Is<Usuario>(u => 
				u.Sub == "sub-test" && 
				u.Nombre == "nombre-test" && 
				u.Apellido == "apellido-test" && 
				u.CorreoElectronico == "correo@test.cl"), 
				Arg.Any<NpgsqlTransaction?>()
			);
		}

		[Fact]
		public async Task ObtenerInformacionUsuarioTest_NoExistente() {
			usuarioDao.Obtener("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns((Usuario?)null);
			cognitoHelper.ObtenerUsuario("sub-test").Returns(new Dictionary<string, string>() {
				["given_name"] = "nombre-test",
				["family_name"] = "apellido-test",
				["email"] = "correo@test.cl"
			});

			Usuario usuario = await usuarioBcp.ObtenerInformacionUsuario("sub-test");
			Assert.Equal("sub-test", usuario.Sub);
			Assert.Equal("nombre-test", usuario.Nombre);
			Assert.Equal("apellido-test", usuario.Apellido);
			Assert.Equal("correo@test.cl", usuario.CorreoElectronico);
			await usuarioDao.Received(1).Obtener("sub-test", Arg.Any<NpgsqlTransaction?>());
			await cognitoHelper.Received(1).ObtenerUsuario(Arg.Any<string>());
			await usuarioDao.Received(1).Insertar(
				Arg.Is<Usuario>(u =>
					u.Sub == "sub-test" &&
					u.Nombre == "nombre-test" &&
					u.Apellido == "apellido-test" &&
					u.CorreoElectronico == "correo@test.cl"
				),
				Arg.Any<NpgsqlTransaction?>()
			);
			await usuarioDao.DidNotReceive().Actualizar(Arg.Any<Usuario>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task RegistrarUsuarioEnFlowTest_ConFlowCustomerId() {
			usuarioDao.Obtener("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(UsuarioDummy(sub: "sub-test", flowCustomerId: "flow-customer-id-test", correoElectronico: "correo@test.cl"));

			string retorno = await usuarioBcp.RegistrarUsuarioEnFlow("sub-test");
			Assert.Equal("flow-customer-id-test", retorno);
			await flowHelper.DidNotReceive().CustomerCreate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
			await usuarioDao.DidNotReceive().Actualizar(Arg.Any<Usuario>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task RegistrarUsuarioEnFlowTest_SinFlowCustomerId() {
			usuarioDao.Obtener("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(UsuarioDummy(sub: "sub-test", flowCustomerId: null, correoElectronico: "correo@test.cl"));
			flowHelper.CustomerCreate(Arg.Any<string>(), "correo@test.cl", "sub-test").Returns(new SalFlowCustomerCreate() { 
				CustomerId = "flow-customer-id-test"
			});

			string retorno = await usuarioBcp.RegistrarUsuarioEnFlow("sub-test");
			Assert.Equal("flow-customer-id-test", retorno);
			await flowHelper.Received(1).CustomerCreate(Arg.Any<string>(), "correo@test.cl", "sub-test");
			await usuarioDao.Received(1).Actualizar(Arg.Is<Usuario>(u => u.Sub == "sub-test" && u.FlowCustomerId == "flow-customer-id-test"), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task RegistrarUsuarioEnFlowTest_SinCorreoElectronico() {
			usuarioDao.Obtener("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(UsuarioDummy(sub: "sub-test", correoElectronico: null));
			cognitoHelper.ObtenerUsuario("sub-test").Returns(new Dictionary<string, string>() {
				["given_name"] = "nombre-test",
				["family_name"] = "apellido-test",
			});

			await Assert.ThrowsAsync<InvalidOperationException>(() => usuarioBcp.RegistrarUsuarioEnFlow("sub-test"));
			await usuarioDao.Received(1).Obtener("sub-test", Arg.Any<NpgsqlTransaction?>());
			await usuarioDao.Received(1).Actualizar(Arg.Any<Usuario>(), Arg.Any<NpgsqlTransaction?>());
			await flowHelper.DidNotReceive().CustomerCreate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
		}

		[Fact]
		public async Task RegistrarTarjetaEnFlowTest() {
			flowHelper.CustomerRegister("flow-customer-id-test").Returns(new SalFlowUrlToken() {
				Url = "https://url.test",
				Token = "token-test"
			});
			string retorno = await usuarioBcp.RegistrarTarjetaEnFlow("flow-customer-id-test");
			Assert.Contains("https://url.test", retorno);
			Assert.Contains("token-test", retorno);
		}
	}
}
