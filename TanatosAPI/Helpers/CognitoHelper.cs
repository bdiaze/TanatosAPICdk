using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Helpers {
    [ExcludeFromCodeCoverage]
    public class CognitoHelper(IAmazonCognitoIdentityProvider client, IVariableEntornoHelper variableEntorno, HttpClient httpClient) {
		private readonly string cognitoBaseUrl = variableEntorno.Obtener("COGNITO_BASE_URL");
		private readonly string cognitoUserPoolClientId = variableEntorno.Obtener("COGNITO_USER_POOL_CLIENT_ID");

		private readonly Dictionary<string, Dictionary<string, string>> atributosUsuarios = [];

		public async Task<Dictionary<string, string>> ObtenerUsuario(string sub) {
			if (!atributosUsuarios.TryGetValue(sub, out Dictionary<string, string>? atributos)) {

			 	AdminGetUserResponse response = await client.AdminGetUserAsync(new AdminGetUserRequest {
					UserPoolId = variableEntorno.Obtener("COGNITO_USER_POOL_ID"),
					Username = sub
				});

				if (response == null || response.UserAttributes == null) {
					throw new InvalidOperationException($"No se pudo rescatar correctamente los atributos del usuario: {sub}");
				}

				atributos = response.UserAttributes.ToDictionary(a => a.Name, a => a.Value);
				atributosUsuarios[sub] = atributos;
			}

			return atributos;
		}

		public async Task<(string accessToken, string refreshToken, int expiresIn, int refreshExpiresIn)> ObtenerConAuthorizationCode(string code, string codeVerifier, string redirectUri) {
			Dictionary<string, string> parametros = new() {
				{ "grant_type", "authorization_code" },
				{ "client_id", cognitoUserPoolClientId },
				{ "redirect_uri", redirectUri },
				{ "code", code },
				{ "code_verifier", codeVerifier }
			};

			HttpRequestMessage request = new(HttpMethod.Post, cognitoBaseUrl + "/oauth2/token") {
				Content = new FormUrlEncodedContent(parametros)
			};
			HttpResponseMessage response = await httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();

			Dictionary<string, JsonElement> tokens = JsonSerializer.Deserialize(await response.Content.ReadAsStringAsync(), AppJsonSerializerContext.Default.DictionaryStringJsonElement)!;
			return (
				tokens["access_token"].ToString(),
				tokens[Constant.CONST_REFRESH_TOKEN].ToString(),
				tokens["expires_in"].GetInt32(),
				int.Parse(variableEntorno.Obtener("COGNITO_REFRESH_TOKEN_VALIDITY_MINUTES"))
			);
		}

		public async Task<(string accessToken, int expiresIn)> ObtenerConRefreshToken(string refreshToken) {
			Dictionary<string, string> parametros = new() {
				{ "grant_type", Constant.CONST_REFRESH_TOKEN },
				{ "client_id", cognitoUserPoolClientId },
				{ Constant.CONST_REFRESH_TOKEN, refreshToken }
			};

			HttpRequestMessage request = new(HttpMethod.Post, cognitoBaseUrl + "/oauth2/token") {
				Content = new FormUrlEncodedContent(parametros)
			};
			HttpResponseMessage response = await httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();

			Dictionary<string, JsonElement> tokens = JsonSerializer.Deserialize(await response.Content.ReadAsStringAsync(), AppJsonSerializerContext.Default.DictionaryStringJsonElement)!;
			return (
				tokens["access_token"].ToString(),
				tokens["expires_in"].GetInt32()
			);
		}
	}
}
