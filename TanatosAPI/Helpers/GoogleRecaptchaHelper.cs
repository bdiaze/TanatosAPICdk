using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TanatosAPI.Entities.Others.Google;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.Helpers {
    public class GoogleRecaptchaHelper(IHostEnvironment environment, IVariableEntornoHelper variableEntorno, ISecretManagerHelper secretManagerHelper, IGoogleCredentialHttpClient credentialHttpClient, IGoogleRecaptchaHttpClient recaptchaHttpClient, IDateTimeProvider dateTimeProvider) : IGoogleRecaptchaHelper {
        private readonly string GOOGLE_SCOPE = variableEntorno.Obtener("GOOGLE_OAUTH2_SCOPE");
        private readonly string GOOGLE_GRANT_TYPE = variableEntorno.Obtener("GOOGLE_OAUTH2_GRANT_TYPE");

        public async Task<(bool valid, string invalidReason, string action, float score)> ObtenerAssesment(string recaptchaToken, string expectedAction) {
            if (environment.IsDevelopment()) {
                return (true, string.Empty, expectedAction, 1);
            }
            
            GoogleAssessmentParams parametros = new() {
                Event = new GoogleAssesmentEvent {
                    SiteKey = variableEntorno.Obtener("GOOGLE_RECAPTCHA_SITE_KEY"),
                    ExpectedAction = expectedAction,
                    Token = recaptchaToken,
                }
            };

			HttpRequestMessage request = new(HttpMethod.Post, $"v1/projects/{variableEntorno.Obtener("GOOGLE_RECAPTCHA_PROJECT_ID")}/assessments") {
				Content = new StringContent(
		            JsonSerializer.Serialize(parametros, AppJsonSerializerContext.Default.GoogleAssessmentParams),
		            Encoding.UTF8,
		            "application/json"
	            )
			};
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await ObtenerAccessToken());
			HttpResponseMessage response = await recaptchaHttpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();

            GoogleAssessmentResponse assessmentResponse = JsonSerializer.Deserialize(
                await response.Content.ReadAsStringAsync(),
                AppJsonSerializerContext.Default.GoogleAssessmentResponse
            )!;
            
            return (
                assessmentResponse.TokenProperties?.Valid ?? false,
                assessmentResponse.TokenProperties?.InvalidReason ?? string.Empty,
                assessmentResponse.TokenProperties?.Action ?? string.Empty,
                assessmentResponse.RiskAnalysis?.Score ?? 0f
            );
		}

        private async Task<string> ObtenerAccessToken() {
            Dictionary<string, string> secretApp = JsonSerializer.Deserialize(
                await secretManagerHelper.ObtenerSecreto(variableEntorno.Obtener("SECRET_ARN_APP")),
                AppJsonSerializerContext.Default.DictionaryStringString
            ) ?? throw new InvalidOperationException("No se encontraron los secretos de la aplicación");

            GoogleRecaptchaCredential googleRecaptchaCredential = JsonSerializer.Deserialize(
                secretApp["GoogleRecaptchaCredential"],
                AppJsonSerializerContext.Default.GoogleRecaptchaCredential
            ) ?? throw new InvalidOperationException("No se encontraron las credenciales de Google Recaptcha");

            long now = new DateTimeOffset(dateTimeProvider.UtcNow).ToUnixTimeSeconds();

            // Se limpia private key...
            string pem = googleRecaptchaCredential.PrivateKey
                .Replace("-----BEGIN PRIVATE KEY-----", "")
                .Replace("-----END PRIVATE KEY-----", "")
                .Replace("\n", "")
                .Trim();

            // Se carga private key...
            byte[] keyBytes = Convert.FromBase64String(pem);
            using RSA rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(keyBytes, out _);

            // Se crea la firma para generar credenciales...
            SigningCredentials signingCredentials = new(
                new RsaSecurityKey(rsa) { KeyId = googleRecaptchaCredential.PrivateKeyId },
                SecurityAlgorithms.RsaSha256
            );

            SecurityTokenDescriptor tokenDescriptor = new() {
                Claims = new Dictionary<string, object> {
                    ["iss"] = googleRecaptchaCredential.ClientEmail,
                    ["sub"] = googleRecaptchaCredential.ClientEmail,
                    ["aud"] = googleRecaptchaCredential.TokenUri,
                    ["scope"] = GOOGLE_SCOPE,
                    ["iat"] = now,
                    ["exp"] = now + 3600
                },
                SigningCredentials = signingCredentials
            };

            string assertion = new JwtSecurityTokenHandler().CreateEncodedJwt(tokenDescriptor);

            HttpResponseMessage response = await credentialHttpClient.PostAsync("token",
                new FormUrlEncodedContent([
                    new("grant_type", GOOGLE_GRANT_TYPE),
                    new("assertion", assertion)
                ])
            );
            response.EnsureSuccessStatusCode();

            GoogleTokenResponse googleTokenResponse = JsonSerializer.Deserialize(
                await response.Content.ReadAsStringAsync(), 
                AppJsonSerializerContext.Default.GoogleTokenResponse
            ) ?? throw new InvalidOperationException("No se encontró el token de acceso de Google");

            return googleTokenResponse.AccessToken;
        }
    }
}
