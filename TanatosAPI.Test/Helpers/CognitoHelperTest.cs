using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime.Internal.Transform;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;
using TanatosAPI.Repositories;
using TanatosAPI.UseCases;

namespace TanatosAPI.Test.Helpers {
	public class CognitoHelperTest {
		private readonly IAmazonCognitoIdentityProvider client = Substitute.For<IAmazonCognitoIdentityProvider>();
		private readonly IVariableEntornoHelper variableEntorno = Substitute.For<IVariableEntornoHelper>();
		private readonly IHttpClientWrapper httpClient = Substitute.For<IHttpClientWrapper>();
		private readonly CognitoHelper cognitoHelper;

		public CognitoHelperTest() {
			cognitoHelper = new CognitoHelper(client, variableEntorno, httpClient);
		}

		[Fact]
		public async Task ObtenerUsuarioTest_Existente() {
			client.AdminGetUserAsync(Arg.Any<AdminGetUserRequest>()).Returns(new AdminGetUserResponse {
				UserAttributes = [
					new AttributeType() { Name = "given_name", Value = "Nombre Test" },
					new AttributeType() { Name = "family_name", Value = "Apellido Test" },
					new AttributeType() { Name = "email", Value = "email@test.com" },
				]
			});

			Dictionary<string, string> atributos = await cognitoHelper.ObtenerUsuario("sub-test-123");
			Assert.Equal("Nombre Test", atributos["given_name"]);
			Assert.Equal("Apellido Test", atributos["family_name"]);
			Assert.Equal("email@test.com", atributos["email"]);
			await client.Received(1).AdminGetUserAsync(Arg.Any<AdminGetUserRequest>());
		}

		[Fact]
		public async Task ObtenerUsuarioTest_ExistenteRepetido() {
			client.AdminGetUserAsync(Arg.Any<AdminGetUserRequest>()).Returns(new AdminGetUserResponse {
				UserAttributes = [
					new AttributeType() { Name = "given_name", Value = "Nombre Test" },
					new AttributeType() { Name = "family_name", Value = "Apellido Test" },
					new AttributeType() { Name = "email", Value = "email@test.com" },
				]
			});

			_ = await cognitoHelper.ObtenerUsuario("sub-test-123");
			client.ClearReceivedCalls();

			Dictionary<string, string> atributos = await cognitoHelper.ObtenerUsuario("sub-test-123");
			Assert.Equal("Nombre Test", atributos["given_name"]);
			Assert.Equal("Apellido Test", atributos["family_name"]);
			Assert.Equal("email@test.com", atributos["email"]);
			await client.DidNotReceive().AdminGetUserAsync(Arg.Any<AdminGetUserRequest>());
		}

		[Fact]
		public async Task ObtenerUsuarioTest_NoExistenteUserAttributes() {
			client.AdminGetUserAsync(Arg.Any<AdminGetUserRequest>()).Returns(new AdminGetUserResponse {
				UserAttributes = null
			});

			await Assert.ThrowsAsync<InvalidOperationException>(() => cognitoHelper.ObtenerUsuario("sub-test-123"));
		}

		[Fact]
		public async Task ObtenerUsuarioTest_NoExistenteResponse() {
			client.AdminGetUserAsync(Arg.Any<AdminGetUserRequest>()).Returns((AdminGetUserResponse?)null);

			await Assert.ThrowsAsync<InvalidOperationException>(() => cognitoHelper.ObtenerUsuario("sub-test-123"));
		}

		[Fact]
		public async Task ObtenerConAuthorizationCodeTest_Valido() {
			variableEntorno.Obtener("COGNITO_REFRESH_TOKEN_VALIDITY_MINUTES").Returns("5000");

			httpClient.SendAsync(Arg.Any<HttpRequestMessage>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(
					JsonSerializer.Serialize(new Dictionary<string, JsonElement> {
						{ "access_token", JsonSerializer.SerializeToElement("access-token-test") },
						{ "refresh_token", JsonSerializer.SerializeToElement("refresh-token-test") },
						{ "expires_in", JsonSerializer.SerializeToElement(1200) },
					}),
					Encoding.UTF8,
					"application/json"
				)
			});

			(string accessToken, string refreshToken, int expiresIn, int refreshExpiresIn) = await cognitoHelper.ObtenerConAuthorizationCode("code-test", "code-verifier-test", "redirect-uri");
			Assert.Equal("access-token-test", accessToken);
			Assert.Equal("refresh-token-test", refreshToken);
			Assert.Equal(1200, expiresIn);
			Assert.Equal(5000, refreshExpiresIn);
			await httpClient.Received(1).SendAsync(Arg.Any<HttpRequestMessage>());
		}

		[Fact]
		public async Task ObtenerConAuthorizationCodeTest_Invalido() {
			httpClient.SendAsync(Arg.Any<HttpRequestMessage>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest,
			});

			await Assert.ThrowsAsync<HttpRequestException>(() =>
				cognitoHelper.ObtenerConAuthorizationCode("code-test", "code-verifier-test", "redirect-uri")
			);
			await httpClient.Received(1).SendAsync(Arg.Any<HttpRequestMessage>());
		}

		[Fact]
		public async Task ObtenerConRefreshTokenTest_Valido() {
			httpClient.SendAsync(Arg.Any<HttpRequestMessage>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(
					JsonSerializer.Serialize(new Dictionary<string, JsonElement> {
						{ "access_token", JsonSerializer.SerializeToElement("access-token-test") },
						{ "expires_in", JsonSerializer.SerializeToElement(1200) },
					}),
					Encoding.UTF8,
					"application/json"
				)
			});

			(string accessToken, int expiresIn) = await cognitoHelper.ObtenerConRefreshToken("refresh-token-test");
			Assert.Equal("access-token-test", accessToken);
			Assert.Equal(1200, expiresIn);
			await httpClient.Received(1).SendAsync(Arg.Any<HttpRequestMessage>());
		}

		[Fact]
		public async Task ObtenerConRefreshTokenTest_Invalido() {
			httpClient.SendAsync(Arg.Any<HttpRequestMessage>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest,
			});

			await Assert.ThrowsAsync<HttpRequestException>(() =>
				cognitoHelper.ObtenerConRefreshToken("refresh-token-test")
			);
			await httpClient.Received(1).SendAsync(Arg.Any<HttpRequestMessage>());
		}
	}
}
