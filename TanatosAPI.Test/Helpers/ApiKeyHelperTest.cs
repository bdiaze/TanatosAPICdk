using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Test.Helpers {
	public class ApiKeyHelperTest {
		private readonly IAmazonAPIGateway apiClient = Substitute.For<IAmazonAPIGateway>();
		private readonly ApiKeyHelper apiKeyHelper;

		public ApiKeyHelperTest() {
			apiKeyHelper = new(apiClient);
		}

		[Fact]
		public async Task ObtenerApiKeyTest_Existente() {
			apiClient.GetApiKeyAsync(Arg.Any<GetApiKeyRequest>(), Arg.Any<CancellationToken>()).Returns(new GetApiKeyResponse {
				Value = "API-KEY-TEST"
			});

			string retorno = await apiKeyHelper.ObtenerApiKey("api-key-id-test");
			Assert.Equal("API-KEY-TEST", retorno);
			await apiClient.Received(1).GetApiKeyAsync(Arg.Any<GetApiKeyRequest>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ObtenerApiKeyTest_ExistenteRepetido() {
			apiClient.GetApiKeyAsync(Arg.Any<GetApiKeyRequest>(), Arg.Any<CancellationToken>()).Returns(new GetApiKeyResponse {
				Value = "API-KEY-TEST"
			});

			_ = await apiKeyHelper.ObtenerApiKey("api-key-id-test");
			apiClient.ClearReceivedCalls();

			string retorno = await apiKeyHelper.ObtenerApiKey("api-key-id-test");
			Assert.Equal("API-KEY-TEST", retorno);
			await apiClient.DidNotReceive().GetApiKeyAsync(Arg.Any<GetApiKeyRequest>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ObtenerApiKeyTest_NoExistenteValue() {
			apiClient.GetApiKeyAsync(Arg.Any<GetApiKeyRequest>(), Arg.Any<CancellationToken>()).Returns(new GetApiKeyResponse {
				Value = null
			});

			await Assert.ThrowsAsync<InvalidOperationException>(() => apiKeyHelper.ObtenerApiKey("api-key-id-test"));
			await apiClient.Received(1).GetApiKeyAsync(Arg.Any<GetApiKeyRequest>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ObtenerApiKeyTest_NoExistenteResponse() {
			apiClient.GetApiKeyAsync(Arg.Any<GetApiKeyRequest>(), Arg.Any<CancellationToken>()).Returns((GetApiKeyResponse?)null);

			await Assert.ThrowsAsync<InvalidOperationException>(() => apiKeyHelper.ObtenerApiKey("api-key-id-test"));
			await apiClient.Received(1).GetApiKeyAsync(Arg.Any<GetApiKeyRequest>(), Arg.Any<CancellationToken>());
		}

	}
}
