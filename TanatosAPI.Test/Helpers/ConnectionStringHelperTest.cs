using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;
using Wrappers_Compile;

namespace TanatosAPI.Test.Helpers {
	public class ConnectionStringHelperTest {
		private readonly IHostEnvironment env = Substitute.For<IHostEnvironment>();
		private readonly IConfiguration config = Substitute.For<IConfiguration>();
		private readonly IVariableEntornoHelper variableEntorno = Substitute.For<IVariableEntornoHelper>();
		private readonly ISecretManagerHelper secretManager = Substitute.For<ISecretManagerHelper>();
		private readonly ConnectionStringHelper connectionStringHelper;

		public ConnectionStringHelperTest() {
			variableEntorno.Obtener("APP_NAME").Returns("AppNameTest");

			connectionStringHelper = new(env, config, variableEntorno, secretManager);
		}

		[Fact]
		public async Task Obtener_ValidoProduction() {
			env.EnvironmentName = Environments.Production;
			variableEntorno.Obtener("SECRET_ARN_CONNECTION_STRING").Returns("SecretArnConnectionStringTest");
			Random rnd = new();
			string passwordDummy = rnd.Next(1000, 10000).ToString();
			Dictionary<string, string> dummySecret = new() {
				["Host"] = "host.test",
				["Port"] = "5432",
				["AppNameTestDatabase"] = "database-test",
				["AppNameTestAppUsername"] = "username-test",
				["AppNameTestAppPassword"] = passwordDummy
			};
			secretManager.ObtenerSecreto(Arg.Any<string>()).Returns(JsonSerializer.Serialize(dummySecret));

			string retorno = await connectionStringHelper.Obtener();
			Assert.Contains("host.test", retorno);
			Assert.Contains("5432", retorno);
			Assert.Contains("database-test", retorno);
			Assert.Contains("username-test", retorno);
			Assert.Contains(passwordDummy, retorno);
			variableEntorno.Received(1).Obtener("APP_NAME");
			variableEntorno.Received(1).Obtener("SECRET_ARN_CONNECTION_STRING");
			await secretManager.Received(1).ObtenerSecreto(Arg.Any<string>());
		}

		[Fact]
		public async Task Obtener_Repetido() {
			env.EnvironmentName = Environments.Production;
			variableEntorno.Obtener("SECRET_ARN_CONNECTION_STRING").Returns("SecretArnConnectionStringTest");
			Random rnd = new();
			string passwordDummy = rnd.Next(1000, 10000).ToString();
			Dictionary<string, string> dummySecret = new() {
				["Host"] = "host.test",
				["Port"] = "5432",
				["AppNameTestDatabase"] = "database-test",
				["AppNameTestAppUsername"] = "username-test",
				["AppNameTestAppPassword"] = passwordDummy
			};
			secretManager.ObtenerSecreto(Arg.Any<string>()).Returns(JsonSerializer.Serialize(dummySecret));

			_ = await connectionStringHelper.Obtener();
			variableEntorno.ClearReceivedCalls();
			secretManager.ClearReceivedCalls();

			string retorno = await connectionStringHelper.Obtener();
			Assert.Contains("host.test", retorno);
			Assert.Contains("5432", retorno);
			Assert.Contains("database-test", retorno);
			Assert.Contains("username-test", retorno);
			Assert.Contains(passwordDummy, retorno);
			variableEntorno.DidNotReceive().Obtener("APP_NAME");
			variableEntorno.DidNotReceive().Obtener("SECRET_ARN_CONNECTION_STRING");
			await secretManager.DidNotReceive().ObtenerSecreto(Arg.Any<string>());
		}

		[Fact]
		public async Task Obtener_ValidoDevelopment() {
			env.EnvironmentName = Environments.Development;
			Random rnd = new();
			string passwordDummy = rnd.Next(1000, 10000).ToString();
			config["ConnectionStrings:Host"].Returns("host.test");
			config["ConnectionStrings:Port"].Returns("5432");
			config["ConnectionStrings:Database"].Returns("database-test");
			config["ConnectionStrings:User Id"].Returns("username-test");
			config["ConnectionStrings:Password"].Returns(passwordDummy);

			string retorno = await connectionStringHelper.Obtener();
			Assert.Contains("host.test", retorno);
			Assert.Contains("5432", retorno);
			Assert.Contains("database-test", retorno);
			Assert.Contains("username-test", retorno);
			Assert.Contains(passwordDummy, retorno);
			variableEntorno.Received(1).Obtener("APP_NAME");
			variableEntorno.DidNotReceive().Obtener("SECRET_ARN_CONNECTION_STRING");
			await secretManager.DidNotReceive().ObtenerSecreto(Arg.Any<string>());
		}
	}
}
