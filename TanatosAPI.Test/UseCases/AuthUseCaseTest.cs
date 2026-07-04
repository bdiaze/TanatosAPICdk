using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.UseCases;

namespace TanatosAPI.Test.UseCases {
	public class AuthUseCaseTest {
		private readonly IVariableEntornoHelper variableEntorno = Substitute.For<IVariableEntornoHelper>();
		private readonly ICognitoHelper cognitoHelper = Substitute.For<ICognitoHelper>();
		private readonly AuthUseCase authUseCase;

		public AuthUseCaseTest() {
			authUseCase = new(variableEntorno, cognitoHelper);
		}

		[Fact]
		public async Task ObtenerAccessToken_Valido() {
			variableEntorno.Obtener("COGNITO_CALLBACK_URLS").Returns("https://url-prueba.cl,https://otra-url-prueba.cl");
			cognitoHelper.ObtenerConAuthorizationCode("code-test", "code-verifier-test", "https://url-prueba.cl").Returns(
				("access-token-test", "refresh-token-test", 1200, 5000)	
			);

			(string accessToken, string refreshToken, int expiresIn, int refreshExpiresIn) = await authUseCase.ObtenerAccessToken("code-test", "code-verifier-test", "https://url-prueba.cl");
			Assert.Equal("access-token-test", accessToken);
			Assert.Equal("refresh-token-test", refreshToken);
			Assert.Equal(1200, expiresIn);
			Assert.Equal(5000, refreshExpiresIn);
			await cognitoHelper.Received(1).ObtenerConAuthorizationCode(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
		}

		[Fact]
		public async Task ObtenerAccessToken_Invalido() {
			variableEntorno.Obtener("COGNITO_CALLBACK_URLS").Returns("https://url-prueba.cl,https://otra-url-prueba.cl");

			await Assert.ThrowsAsync<ErrorValidacion>(() => authUseCase.ObtenerAccessToken("code-test", "code-verifier-test", "https://url-no-valida.cl"));
		}

		[Fact]
		public async Task RefreshAccessToken_Valido() {
			authUseCase.RefreshAccessToken("refresh-token-test").Returns(
				("access-token-test", 1200)
			);

			(string accessToken, int expiresIn) = await authUseCase.RefreshAccessToken("refresh-token-test");
			Assert.Equal("access-token-test", accessToken);
			Assert.Equal(1200, expiresIn);
			await cognitoHelper.Received(1).ObtenerConRefreshToken(Arg.Any<string>());
		}

		[Fact]
		public async Task RefreshAccessToken_Invalido() {
			await Assert.ThrowsAsync<ErrorValidacion>(() => authUseCase.RefreshAccessToken(""));
		}
	}
}
