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
			publicGroup.MapPrueba();

			// Endpoints autenticados
			RouteGroupBuilder privateGroup = routes.MapGroup("/Auth");
			privateGroup.MapLimpiarAuthCookies();

			return routes;
		}

		private static IEndpointRouteBuilder MapPrueba(this IEndpointRouteBuilder routes) {
			routes.MapGet("/", async () => {
				return new Dictionary<string, string>() {
					{ "Hola", "Mundo" }
				};
			}).AllowAnonymous().WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerAccessToken(this IEndpointRouteBuilder routes) {
			routes.MapPost("/ObtenerAccessToken", async (EntAuthObtenerAccessToken entrada, HttpResponse httpResponse, IHostEnvironment environment, VariableEntornoHelper variableEntorno) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string baseUrl = variableEntorno.Obtener("COGNITO_BASE_URL");

					// Se valida que el redirect uri se encuentre entre los permitidos...
					if (!variableEntorno.Obtener("COGNITO_CALLBACK_URLS").Split(',').Contains(entrada.RedirectUri)) {
						LambdaLogger.Log(
							$"[POST] - [Auth] - [ObtenerAccessToken] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No es válido el RedirectUri recibido.");

						Results.BadRequest();
					}

					string apiMapping = $"/{variableEntorno.Obtener("API_GATEWAY_MAPPING_KEY")}";
					if (environment.IsDevelopment()) {
						apiMapping = "";
					}

					Dictionary<string, string> parametros = new() {
						{ "grant_type", "authorization_code" },
						{ "client_id", variableEntorno.Obtener("COGNITO_USER_POOL_CLIENT_ID") },
						{ "redirect_uri", entrada.RedirectUri },
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
						throw new Exception($"Ocurrio un error al obtener token. Status Code: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
					}

					// Se arma salida de access token con su expires in...
					Dictionary<string, JsonElement> tokens = JsonSerializer.Deserialize(await response.Content.ReadAsStringAsync(), AppJsonSerializerContext.Default.DictionaryStringJsonElement)!;
					
					DateTimeOffset refreshExpiration = DateTimeOffset.UtcNow.AddMinutes(double.Parse(variableEntorno.Obtener("COGNITO_REFRESH_TOKEN_VALIDITY_MINUTES")));

					httpResponse.Cookies.Append("refresh_token", tokens["refresh_token"].ToString(), new CookieOptions {
						// Path = $"{apiMapping}/public/Auth/RefreshAccessToken",
						Path = "/",
						IsEssential = true,
						Expires = refreshExpiration,
						//HttpOnly = true,
						//Secure = true,
						SameSite = SameSiteMode.None
					});

					string csrfToken = Guid.NewGuid().ToString("N");
					httpResponse.Cookies.Append("csrf_token", csrfToken, new CookieOptions {
						// Path = $"{apiMapping}/public/Auth/RefreshAccessToken",
						Path = "/",
						IsEssential = true,
						Expires = refreshExpiration,
						//HttpOnly = true,
						//Secure = true,
						SameSite = SameSiteMode.None
					});

					SalAuthObtenerAccessToken retorno = new() {
						AccessToken = tokens["access_token"].ToString(),
						ExpiresIn = tokens["expires_in"].GetInt32(),
						CsrfToken = csrfToken,
						CsrfTokenExpiration = refreshExpiration
					};

					LambdaLogger.Log(
						$"[POST] - [Auth] - [ObtenerAccessToken] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtencion exitosa del access token.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [Auth] - [ObtenerAccessToken] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrio un error al obtener access token. " +
						$"{ex}");
					return Results.Problem($"Ocurrio un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
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
						LambdaLogger.Log(
							$"[POST] - [Auth] - [RefreshAccessToken] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No se incluyo header X-CSRF-Token.");

						return Results.BadRequest();
					}

					if (!httpRequest.Cookies.TryGetValue("csrf_token", out string? cookieCsrf)) {
						LambdaLogger.Log(
							$"[POST] - [Auth] - [RefreshAccessToken] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No se incluyo cookie csrf_token.");

						return Results.BadRequest();
					}

					if (string.IsNullOrEmpty(headerCsrf) || string.IsNullOrEmpty(cookieCsrf) || headerCsrf != cookieCsrf) {
						LambdaLogger.Log(
							$"[POST] - [Auth] - [RefreshAccessToken] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status401Unauthorized}] - " +
							$"No coinciden los valores del header X-CSRF-Token con la cookie csrf_token.");

						return Results.Unauthorized();
					}

					// Se valida que venga el refresh token...
					if (!httpRequest.Cookies.TryGetValue("refresh_token", out string? refreshToken)) {
						LambdaLogger.Log(
							$"[POST] - [Auth] - [RefreshAccessToken] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No se incluyo cookie refresh_token.");

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
						throw new Exception($"Ocurrio un error al refrescar token. Status Code: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
					}

					// Se arma salida de access token con su expires in...
					Dictionary<string, JsonElement> tokens = JsonSerializer.Deserialize(await response.Content.ReadAsStringAsync(), AppJsonSerializerContext.Default.DictionaryStringJsonElement)!;
					SalAuthRefreshAccessToken retorno = new() {
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
						$"Ocurrio un error al refrescar el access token. " +
						$"{ex}");
					return Results.Problem($"Ocurrio un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
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

		private static IEndpointRouteBuilder MapLimpiarAuthCookies(this IEndpointRouteBuilder routes) {
			routes.MapPost("/LimpiarAuthCookies", async (HttpRequest httpRequest, HttpResponse httpResponse, IHostEnvironment environment, VariableEntornoHelper variableEntorno) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string apiMapping = $"/{variableEntorno.Obtener("API_GATEWAY_MAPPING_KEY")}";
					if (environment.IsDevelopment()) {
						apiMapping = "";
					}

					httpResponse.Cookies.Delete("refresh_token", new CookieOptions {
						Path = $"{apiMapping}/public/Auth/RefreshAccessToken",
						IsEssential = true,
						HttpOnly = true,
						Secure = true,
						SameSite = SameSiteMode.None
					});

					httpResponse.Cookies.Delete("csrf_token", new CookieOptions {
						Path = $"{apiMapping}/public/Auth/RefreshAccessToken",
						IsEssential = true,
						HttpOnly = false,
						Secure = true,
						SameSite = SameSiteMode.None
					});

					LambdaLogger.Log(
							$"[POST] - [Auth] - [LimpiarAuthCookies] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
							$"Se limpian exitosamente las cookies auth.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [Auth] - [LimpiarAuthCookies] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrio un error al limpiar las cookies auth. " +
						$"{ex}");
					return Results.Problem($"Ocurrio un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}

			}).RequireAuthorization().WithOpenApi();

			return routes;
		}
	}
}
