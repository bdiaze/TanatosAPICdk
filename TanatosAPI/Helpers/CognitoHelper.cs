using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.AspNetCore.ResponseCompression;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.Helpers {
    public class CognitoHelper(IAmazonCognitoIdentityProvider client, IVariableEntornoHelper variableEntorno, ICognitoHttpClient httpClient) : ICognitoHelper {
		private readonly string cognitoUserPoolClientId = variableEntorno.Obtener("COGNITO_USER_POOL_CLIENT_ID");

		private readonly Dictionary<string, Dictionary<string, string>> atributosUsuarios = [];

		public async Task<Dictionary<string, string>> ObtenerUsuario(string username) {
			if (!atributosUsuarios.TryGetValue(username, out Dictionary<string, string>? atributos)) {

			 	AdminGetUserResponse response = await client.AdminGetUserAsync(new AdminGetUserRequest {
					UserPoolId = variableEntorno.Obtener("COGNITO_USER_POOL_ID"),
					Username = username
				});

				if (response == null || response.UserAttributes == null) {
					throw new InvalidOperationException($"No se pudo rescatar correctamente los atributos del usuario: {username}");
				}

				atributos = response.UserAttributes.ToDictionary(a => a.Name, a => a.Value);
				atributosUsuarios[username] = atributos;
			}

			return atributos;
		}

		public async Task ConfirmarRegistro(string username, string confirmationCode) {
			try {
				ConfirmSignUpResponse response = await client.ConfirmSignUpAsync(new ConfirmSignUpRequest {
					ClientId = variableEntorno.Obtener("COGNITO_USER_POOL_CLIENT_ID"),
					Username = username,
					ConfirmationCode = confirmationCode
				});

				if (response.HttpStatusCode != HttpStatusCode.OK) {
					throw new InvalidOperationException("Ocurrió un error al verificar cuenta con código");
				}
			} catch (CodeMismatchException) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "El código de verificación es inválido.");
			} catch (ExpiredCodeException) {
				throw new ErrorValidacion(TipoErrorValidacion.AccesoCaducado, "El código ha caducado, favor solicitar nuevo código.");
			} catch (NotAuthorizedException) {
				throw new ErrorValidacion(TipoErrorValidacion.EstadoNoValido, "La cuenta ya se encuentra verificada.");
			}
		}

		public async Task ReenviarCodigoVerificacion(string username) {
			try {
				ResendConfirmationCodeResponse response = await client.ResendConfirmationCodeAsync(new ResendConfirmationCodeRequest {
					ClientId = variableEntorno.Obtener("COGNITO_USER_POOL_CLIENT_ID"),
					Username = username,
				});

                if (response.HttpStatusCode != HttpStatusCode.OK) {
                    throw new InvalidOperationException("Ocurrió un error al reenviar código de verificación");
                }
            } catch (LimitExceededException) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "Has alcanzado el límite de nuevos códigos de verificación que te podemos enviar, favor intenta más tarde.");
			}
		}

		public async Task<(string accessToken, string refreshToken, int expiresIn, int refreshExpiresIn)> ObtenerConAuthorizationCode(string code, string codeVerifier, string redirectUri) {
			Dictionary<string, string> parametros = new() {
				{ "grant_type", "authorization_code" },
				{ "client_id", cognitoUserPoolClientId },
				{ "redirect_uri", redirectUri },
				{ "code", code },
				{ "code_verifier", codeVerifier }
			};

			HttpRequestMessage request = new(HttpMethod.Post, "oauth2/token") {
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

			HttpRequestMessage request = new(HttpMethod.Post, "oauth2/token") {
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
