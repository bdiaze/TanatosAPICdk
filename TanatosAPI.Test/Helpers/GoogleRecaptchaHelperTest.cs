using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Test.Helpers {
	public class GoogleRecaptchaHelperTest {
		private readonly IHostEnvironment env = Substitute.For<IHostEnvironment>();
		private readonly IVariableEntornoHelper variableEntorno = Substitute.For<IVariableEntornoHelper>();
		private readonly ISecretManagerHelper secretManagerHelper = Substitute.For<ISecretManagerHelper>();
		private readonly IGoogleCredentialHttpClient credentialHttpClient = Substitute.For<IGoogleCredentialHttpClient>();
		private readonly IGoogleRecaptchaHttpClient recaptchaHttpClient = Substitute.For<IGoogleRecaptchaHttpClient>();
		private readonly IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
		private readonly GoogleRecaptchaHelper googleRecaptchaHelper;

		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

		public GoogleRecaptchaHelperTest() {
			dateTimeProvider.UtcNow.Returns(FECHA_DUMMY);

			variableEntorno.Obtener("GOOGLE_OAUTH2_SCOPE").Returns("google-oauth2-scope-test");
			variableEntorno.Obtener("GOOGLE_OAUTH2_GRANT_TYPE").Returns("google-oauth2-grant-type-test");

			using RSA rsa = RSA.Create(2048);
			byte[] pkcs8 = rsa.ExportPkcs8PrivateKey();
			string privateKeyBase64 = Convert.ToBase64String(pkcs8);
			string privateKeyPem = $"-----BEGIN PRIVATE KEY-----\n{privateKeyBase64}\n-----END PRIVATE KEY-----";

			variableEntorno.Obtener("SECRET_ARN_APP").Returns("SecretArnAppTest");
			GoogleRecaptchaCredential credentialDummy = new() { 
				Type = "type-test",
				ProjectId = "project-id-test",
				PrivateKeyId = "private-key-id-test",
				PrivateKey = privateKeyPem,
				ClientEmail = "correo@test.cl",
				ClientId = "client-id-test",
				AuthUri = "https://url.test/auth",
				TokenUri = "https://url.test/token",
				AuthProviderX509CertUrl = "https://url.test/authproviderx509cert",
				ClientX509CertUrl = "https://url.test/clientx509cert",
				UniverseDomain = "universe-domain-test"
			};
			Dictionary<string, string> secretDummy = new() {
				["GoogleRecaptchaCredential"] = JsonSerializer.Serialize(credentialDummy)
			};
			secretManagerHelper.ObtenerSecreto("SecretArnAppTest").Returns(JsonSerializer.Serialize(secretDummy));

			credentialHttpClient.PostAsync("token", Arg.Any<FormUrlEncodedContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(
					JsonSerializer.Serialize(new GoogleTokenResponse {
						AccessToken = "access-token-test"
					}),
					Encoding.UTF8,
					"application/json"
				)
			});

			googleRecaptchaHelper = new(env, variableEntorno, secretManagerHelper, credentialHttpClient, recaptchaHttpClient, dateTimeProvider);
		}

		[Fact]
		public async Task ObtenerAssesment_ProductionValido() {
			env.EnvironmentName = Environments.Production;
			variableEntorno.Obtener("GOOGLE_RECAPTCHA_SITE_KEY").Returns("google-recaptcha-site-key-test");
			variableEntorno.Obtener("GOOGLE_RECAPTCHA_PROJECT_ID").Returns("google-recaptcha-project-id-test");

			recaptchaHttpClient.SendAsync(Arg.Any<HttpRequestMessage>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(
					JsonSerializer.Serialize(new GoogleAssessmentResponse {
						TokenProperties = new GoogleTokenProperties {
							Valid = true,
							InvalidReason = null,
							Action = "action-test"
						},
						RiskAnalysis = new GoogleRiskAnalysis { 
							Score = 0.8f
						}
					}),
					Encoding.UTF8,
					"application/json"
				)
			});

			(bool valid, string invalidReason, string action, float score) = await googleRecaptchaHelper.ObtenerAssesment("recaptcha-token-test", "action-test");
			Assert.True(valid);
			Assert.Empty(invalidReason);
			Assert.Equal("action-test", action);
			Assert.Equal(0.8f, score);
			await credentialHttpClient.Received(1).PostAsync("token", Arg.Any<FormUrlEncodedContent>());
			await recaptchaHttpClient.Received(1).SendAsync(Arg.Any<HttpRequestMessage>());
		}

		[Fact]
		public async Task ObtenerAssesment_ProductionStatusCodeError() {
			env.EnvironmentName = Environments.Production;
			variableEntorno.Obtener("GOOGLE_RECAPTCHA_SITE_KEY").Returns("google-recaptcha-site-key-test");
			variableEntorno.Obtener("GOOGLE_RECAPTCHA_PROJECT_ID").Returns("google-recaptcha-project-id-test");

			recaptchaHttpClient.SendAsync(Arg.Any<HttpRequestMessage>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest
			});

			await Assert.ThrowsAsync<HttpRequestException>(() => googleRecaptchaHelper.ObtenerAssesment("recaptcha-token-test", "action-test"));
			await credentialHttpClient.Received(1).PostAsync("token", Arg.Any<FormUrlEncodedContent>());
			await recaptchaHttpClient.Received(1).SendAsync(Arg.Any<HttpRequestMessage>());
		}

		[Fact]
		public async Task ObtenerAssesment_ProductionStatusCodeErrorAuthentication() {
			env.EnvironmentName = Environments.Production;
			variableEntorno.Obtener("GOOGLE_RECAPTCHA_SITE_KEY").Returns("google-recaptcha-site-key-test");
			variableEntorno.Obtener("GOOGLE_RECAPTCHA_PROJECT_ID").Returns("google-recaptcha-project-id-test");

			credentialHttpClient.PostAsync("token", Arg.Any<FormUrlEncodedContent>()).Returns(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest
			});

			await Assert.ThrowsAsync<HttpRequestException>(() => googleRecaptchaHelper.ObtenerAssesment("recaptcha-token-test", "action-test"));
			await credentialHttpClient.Received(1).PostAsync("token", Arg.Any<FormUrlEncodedContent>());
			await recaptchaHttpClient.DidNotReceive().SendAsync(Arg.Any<HttpRequestMessage>());
		}

		[Fact]
		public async Task ObtenerAssesment_DevelopmentValido() {
			env.EnvironmentName = Environments.Development;

			(bool valid, string invalidReason, string action, float score) = await googleRecaptchaHelper.ObtenerAssesment("recaptcha-token-test", "action-test");
			Assert.True(valid);
			Assert.Empty(invalidReason);
			Assert.Equal("action-test", action);
			Assert.Equal(1, score);
			await credentialHttpClient.DidNotReceive().PostAsync("token", Arg.Any<FormUrlEncodedContent>());
			await recaptchaHttpClient.DidNotReceive().SendAsync(Arg.Any<HttpRequestMessage>());
		}
	}
}
