using Amazon.Lambda.Core;
using Microsoft.Extensions.Primitives;
using System;
using System.Diagnostics;
using System.Text.Json;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;

namespace TanatosAPI.Endpoints {
	public static class AuthEndpoints {
		public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes) {
			// Endpoints públicos 
			RouteGroupBuilder publicGroup = routes.MapGroup("/public/Auth");
			publicGroup.MapObtenerAccessToken();
			publicGroup.MapRefreshAccessToken();

			// Endpoints autenticados
			RouteGroupBuilder privateGroup = routes.MapGroup("/Auth");
			privateGroup.MapLimpiarAuthCookies();

			return routes;
		}

		private static void MapObtenerAccessToken(this IEndpointRouteBuilder routes) {
			routes.MapPost("/ObtenerAccessToken", async (EntAuthObtenerAccessToken entrada, HttpContext httpContext, HttpResponse httpResponse, IHostEnvironment environment, VariableEntornoHelper variableEntorno) => {
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
						throw new HttpRequestException(
							$"Ocurrio un error al obtener token. Status Code: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}",
							inner: null,
							statusCode: response.StatusCode
						);
					}

					// Se arma salida de access token con su expires in...
					Dictionary<string, JsonElement> tokens = JsonSerializer.Deserialize(await response.Content.ReadAsStringAsync(), AppJsonSerializerContext.Default.DictionaryStringJsonElement)!;
					
					DateTimeOffset refreshExpiration = DateTimeOffset.UtcNow.AddMinutes(double.Parse(variableEntorno.Obtener("COGNITO_REFRESH_TOKEN_VALIDITY_MINUTES")));

					// Se revisa si request llega desde localhost para setear cookies como SameSiteMode.None...
					bool sameSiteStrict = true;
					if (httpContext.Request.Headers.TryGetValue("Origin", out StringValues originHeader) && Uri.TryCreate(originHeader.ToString(), UriKind.Absolute, out Uri? uri) && uri.IsLoopback) {
						sameSiteStrict = false;
					}

					httpResponse.Cookies.Append(Constant.CONST_REFRESH_TOKEN, tokens[Constant.CONST_REFRESH_TOKEN].ToString(), new CookieOptions {
						Path = $"{apiMapping}/public/Auth/RefreshAccessToken",
						IsEssential = true,
						Expires = refreshExpiration,
						HttpOnly = true,
						Secure = true,
						SameSite = sameSiteStrict ? SameSiteMode.Strict : SameSiteMode.None
					});

					SalAuthObtenerAccessToken retorno = new() {
						AccessToken = tokens["access_token"].ToString(),
						ExpiresIn = tokens["expires_in"].GetInt32()
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
			}).AllowAnonymous();
		}

		private static void MapRefreshAccessToken(this IEndpointRouteBuilder routes) {
			routes.MapPost("/RefreshAccessToken", async(HttpContext httpContext, HttpRequest httpRequest, HttpResponse httpResponse, IHostEnvironment environment, VariableEntornoHelper variableEntorno) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					// Se valida que venga el refresh token...
					if (!httpRequest.Cookies.TryGetValue(Constant.CONST_REFRESH_TOKEN, out string? refreshToken) || string.IsNullOrWhiteSpace(refreshToken)) {
						LambdaLogger.Log(
							$"[POST] - [Auth] - [RefreshAccessToken] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No se incluyo cookie {Constant.CONST_REFRESH_TOKEN}.");

						return Results.BadRequest();
					}

					string baseUrl = variableEntorno.Obtener("COGNITO_BASE_URL");

					Dictionary<string, string> parametros = new() {
							{ "grant_type", Constant.CONST_REFRESH_TOKEN },
							{ "client_id", variableEntorno.Obtener("COGNITO_USER_POOL_CLIENT_ID") },
							{ Constant.CONST_REFRESH_TOKEN, refreshToken }
						};

					using HttpClient client = new();
					HttpRequestMessage request = new(HttpMethod.Post, baseUrl + "/oauth2/token") {
						Content = new FormUrlEncodedContent(parametros)
					};

					// Se obtienen los tokens...
					HttpResponseMessage response = await client.SendAsync(request);
					if (!response.IsSuccessStatusCode) {

						// Si no se logra efectuar el refresh, se manda request con limpieza de cookies...
						string apiMapping = $"/{variableEntorno.Obtener("API_GATEWAY_MAPPING_KEY")}";
						if (environment.IsDevelopment()) {
							apiMapping = "";
						}

						bool sameSiteStrict = true;
						if (httpContext.Request.Headers.TryGetValue("Origin", out StringValues originHeader) && Uri.TryCreate(originHeader.ToString(), UriKind.Absolute, out Uri? uri) && uri.IsLoopback) {
							sameSiteStrict = false;
						}

						httpResponse.Cookies.Delete(Constant.CONST_REFRESH_TOKEN, new CookieOptions {
							Path = $"{apiMapping}/public/Auth/RefreshAccessToken",
							IsEssential = true,
							HttpOnly = true,
							Secure = true,
							SameSite = sameSiteStrict ? SameSiteMode.Strict : SameSiteMode.None
						});

						throw new HttpRequestException(
							$"Ocurrio un error al refrescar token. Status Code: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}",
							inner: null,
							statusCode: response.StatusCode
						);
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

			}).AllowAnonymous();
		}

		private static void MapLimpiarAuthCookies(this IEndpointRouteBuilder routes) {
			routes.MapPost("/LimpiarAuthCookies", (HttpContext httpContext, HttpResponse httpResponse, IHostEnvironment environment, VariableEntornoHelper variableEntorno) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string apiMapping = $"/{variableEntorno.Obtener("API_GATEWAY_MAPPING_KEY")}";
					if (environment.IsDevelopment()) {
						apiMapping = "";
					}

					// Se revisa si request llega desde localhost para setear cookies como SameSiteMode.None...
					bool sameSiteStrict = true;
					if (httpContext.Request.Headers.TryGetValue("Origin", out StringValues originHeader) && Uri.TryCreate(originHeader.ToString(), UriKind.Absolute, out Uri? uri) && uri.IsLoopback) {
						sameSiteStrict = false;
					}

					httpResponse.Cookies.Delete(Constant.CONST_REFRESH_TOKEN, new CookieOptions {
						Path = $"{apiMapping}/public/Auth/RefreshAccessToken",
						IsEssential = true,
						HttpOnly = true,
						Secure = true,
						SameSite = sameSiteStrict ? SameSiteMode.Strict : SameSiteMode.None
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

			}).RequireAuthorization();
		}
	}
}
