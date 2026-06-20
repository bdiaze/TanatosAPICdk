using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Test.Helpers {
	public class SecretManagerHelperTest {
		private readonly IAmazonSecretsManager client = Substitute.For<IAmazonSecretsManager>();
		private readonly SecretManagerHelper secretManagerHelper;

		public SecretManagerHelperTest() {
			secretManagerHelper = new(client);
		}

		[Fact]
		public async Task ObtenerSecretoTest_Existente() {
			client.GetSecretValueAsync(Arg.Any<GetSecretValueRequest>()).Returns(new GetSecretValueResponse {
				SecretString = "SECRET-STRING-TEST"
			});

			string retorno = await secretManagerHelper.ObtenerSecreto("secret-arn-test");
			Assert.Equal("SECRET-STRING-TEST", retorno);
			await client.Received(1).GetSecretValueAsync(Arg.Any<GetSecretValueRequest>());
		}

		[Fact]
		public async Task ObtenerSecretoTest_ExistenteRepetido() {
			client.GetSecretValueAsync(Arg.Any<GetSecretValueRequest>()).Returns(new GetSecretValueResponse {
				SecretString = "SECRET-STRING-TEST"
			});

			_ = await secretManagerHelper.ObtenerSecreto("secret-arn-test");
			client.ClearReceivedCalls();

			string retorno = await secretManagerHelper.ObtenerSecreto("secret-arn-test");
			Assert.Equal("SECRET-STRING-TEST", retorno);
			await client.DidNotReceive().GetSecretValueAsync(Arg.Any<GetSecretValueRequest>());
		}

		[Fact]
		public async Task ObtenerSecretoTest_NoExistenteValue() {
			client.GetSecretValueAsync(Arg.Any<GetSecretValueRequest>()).Returns(new GetSecretValueResponse {
				SecretString = null
			});

			await Assert.ThrowsAsync<InvalidOperationException>(() => secretManagerHelper.ObtenerSecreto("secret-arn-test"));
			await client.Received(1).GetSecretValueAsync(Arg.Any<GetSecretValueRequest>());
		}

		[Fact]
		public async Task ObtenerSecretoTest_NoExistenteResponse() {
			client.GetSecretValueAsync(Arg.Any<GetSecretValueRequest>()).Returns((GetSecretValueResponse?)null);

			await Assert.ThrowsAsync<InvalidOperationException>(() => secretManagerHelper.ObtenerSecreto("secret-arn-test"));
			await client.Received(1).GetSecretValueAsync(Arg.Any<GetSecretValueRequest>());
		}
	}
}
