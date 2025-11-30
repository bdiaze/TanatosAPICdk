using Amazon.Lambda.Core;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Primitives;
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;

namespace TanatosAPI.Endpoints {
	public static class AuthEndpoints {
		public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes) {
			// Endpoints públicos 
			RouteGroupBuilder publicGroup = routes.MapGroup("/public/Auth");
			publicGroup.MapObtenerAccessToken();
			publicGroup.MapRefreshAccessToken();

			// Endpoints autenticados

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerAccessToken(this IEndpointRouteBuilder routes) {
			routes.MapPost("/ObtenerAccessToken", async (EntAuthObtenerAccessToken entrada, HttpResponse httpResponse, IHostEnvironment environment, VariableEntornoHelper variableEntorno) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string baseUrl = variableEntorno.Obtener("COGNITO_BASE_URL");

					string redirectUri;
					if (environment.IsProduction()) {
						redirectUri = variableEntorno.Obtener("COGNITO_CALLBACK_URLS").Split(',').Where(s => !s.Contains("localhost")).First();
					} else {
						redirectUri = variableEntorno.Obtener("COGNITO_CALLBACK_URLS").Split(',').Where(s => s.Contains("localhost")).First();
					}

					Dictionary<string, string> parametros = new() {
						{ "grant_type", "authorization_code" },
						{ "client_id", variableEntorno.Obtener("COGNITO_USER_POOL_CLIENT_ID") },
						{ "redirect_uri", redirectUri },
						{ "code", entrada.Code },
						{ "code_verifier", entrada.CodeVerifier }
					};

					using HttpClient client = new();
					HttpRequestMessage request = new(HttpMethod.Post, baseUrl + "/oauth2/token") { 
						Content = new FormUrlEncodedContent(parametros)
					};

					// Se obtienen los tokens...
					HttpResponseMessage response = await client.SendAsync(request);
					if (!response.IsSuccessStatusCode) {
						throw new Exception($"Ocurrió un error al obtener token. Status Code: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
					}

					// Se arma salida de access token con su expires in...
					Dictionary<string, JsonElement> tokens = JsonSerializer.Deserialize(await response.Content.ReadAsStringAsync(), AppJsonSerializerContext.Default.DictionaryStringJsonElement)!;
					SalAuthObtenerAccessToken retorno = new() {
						AccessToken = tokens["access_token"].ToString(),
						ExpiresIn = tokens["expires_in"].GetInt32()
					};

					DateTimeOffset refreshExpiration = DateTimeOffset.UtcNow.AddMinutes(double.Parse(variableEntorno.Obtener("COGNITO_REFRESH_TOKEN_VALIDITY_MINUTES")));

					httpResponse.Cookies.Append("refresh_token", tokens["refresh_token"].ToString(), new CookieOptions {
						Path = "/public/Auth/RefreshAccessToken",
						IsEssential = true,
						Expires = refreshExpiration,
						HttpOnly = true,
						Secure = true,
						SameSite = SameSiteMode.None
					});

					string csrfToken = Guid.NewGuid().ToString("N");
					httpResponse.Cookies.Append("csrf_token", csrfToken, new CookieOptions {
						Path = "/public/Auth/RefreshAccessToken",
						IsEssential = true,
						Expires = refreshExpiration,
						HttpOnly = false,
						Secure = true,
						SameSite = SameSiteMode.None
					});

					LambdaLogger.Log(
						$"[POST] - [Auth] - [ObtenerAccessToken] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa del access token.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [Auth] - [ObtenerAccessToken] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener access token. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).AllowAnonymous().WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapRefreshAccessToken(this IEndpointRouteBuilder routes) {
			routes.MapPost("/RefreshAccessToken", async (HttpRequest httpRequest, HttpResponse httpResponse, IHostEnvironment environment, VariableEntornoHelper variableEntorno) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					// Se valida CSRF Token...
					if (!httpRequest.Headers.TryGetValue("X-CSRF-Token", out StringValues headerCsrf)) {
						return Results.BadRequest();
					}

					if (!httpRequest.Cookies.TryGetValue("csrf_token", out string? cookieCsrf)) {
						return Results.BadRequest();
					}

					if (string.IsNullOrEmpty(headerCsrf) || string.IsNullOrEmpty(cookieCsrf) || headerCsrf != cookieCsrf) {
						return Results.Unauthorized();
					}

					// Se valida que venga el refresh token...
					if (!httpRequest.Cookies.TryGetValue("refresh_token", out string? refreshToken)) {
						return Results.BadRequest();
					}

					string baseUrl = variableEntorno.Obtener("COGNITO_BASE_URL");

					Dictionary<string, string> parametros = new() {
							{ "grant_type", "refresh_token" },
							{ "client_id", variableEntorno.Obtener("COGNITO_USER_POOL_CLIENT_ID") },
							{ "refresh_token", refreshToken }
						};

					using HttpClient client = new();
					HttpRequestMessage request = new(HttpMethod.Post, baseUrl + "/oauth2/token") {
						Content = new FormUrlEncodedContent(parametros)
					};

					// Se obtienen los tokens...
					HttpResponseMessage response = await client.SendAsync(request);
					if (!response.IsSuccessStatusCode) {
						throw new Exception($"Ocurrió un error al refrescar token. Status Code: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
					}

					// Se arma salida de access token con su expires in...
					Dictionary<string, JsonElement> tokens = JsonSerializer.Deserialize(await response.Content.ReadAsStringAsync(), AppJsonSerializerContext.Default.DictionaryStringJsonElement)!;
					SalAuthObtenerAccessToken retorno = new() {
						AccessToken = tokens["access_token"].ToString(),
						ExpiresIn = tokens["expires_in"].GetInt32()
					};

					LambdaLogger.Log(
							$"[POST] - [Auth] - [RefreshAccessToken] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
							$"Refrescado exitoso del access token.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [Auth] - [RefreshAccessToken] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al refrescar el access token. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}

			}).AllowAnonymous().WithOpenApi(op => {
				op.Parameters.Add(new Microsoft.OpenApi.Models.OpenApiParameter { 
					Name = "X-CSRF-Token",
					In = Microsoft.OpenApi.Models.ParameterLocation.Header,
					Required = true,
					Description = "Token CSRF"
				});

				return op;
			});

			return routes;
		}
	}
}
