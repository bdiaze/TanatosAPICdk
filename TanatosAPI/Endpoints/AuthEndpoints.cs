using Amazon.Lambda.Core;
using Microsoft.Extensions.Primitives;
using System;
using System.Diagnostics;
using System.Text.Json;
using TanatosAPI.Entities.Others;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;
using TanatosAPI.UseCases;

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
			routes.MapPost("/ObtenerAccessToken", async (EntAuthObtenerAccessToken entrada, HttpContext httpContext, HttpResponse httpResponse, IHostEnvironment environment, IVariableEntornoHelper variableEntorno, IDateTimeProvider dateTimeProvider, AuthUseCase authUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					DateTime now = dateTimeProvider.UtcNow;

					(string accessToken, string refreshToken, int expiresIn, int refreshExpiresIn) = await authUseCase.ObtenerAccessToken(
						entrada.Code, 
						entrada.CodeVerifier, 
						entrada.RedirectUri
					);

					string apiMapping = $"/{variableEntorno.Obtener("API_GATEWAY_MAPPING_KEY")}";
					if (environment.IsDevelopment()) {
						apiMapping = "";
					}

					// Se revisa si request llega desde localhost para setear cookies como SameSiteMode.None...
					bool sameSiteStrict = true;
					if (httpContext.Request.Headers.TryGetValue("Origin", out StringValues originHeader) && Uri.TryCreate(originHeader.ToString(), UriKind.Absolute, out Uri? uri) && uri.IsLoopback) {
						sameSiteStrict = false;
					}

					httpResponse.Cookies.Append(Constant.CONST_REFRESH_TOKEN, refreshToken, new CookieOptions {
						Path = $"{apiMapping}/public/Auth/RefreshAccessToken",
						IsEssential = true,
						Expires = new(now.AddMinutes(refreshExpiresIn), TimeSpan.Zero),
						HttpOnly = true,
						Secure = true,
						SameSite = sameSiteStrict ? SameSiteMode.Strict : SameSiteMode.None
					});

					SalAuthObtenerAccessToken retorno = new() {
						AccessToken = accessToken,
						ExpiresIn = expiresIn
					};

					LambdaLogger.Log(
						$"[POST] - [Auth] - [ObtenerAccessToken] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtencion exitosa del access token.");

					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[POST] - [Auth] - [ObtenerAccessToken] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
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
			routes.MapPost("/RefreshAccessToken", async(HttpContext httpContext, HttpRequest httpRequest, HttpResponse httpResponse, IHostEnvironment environment, IVariableEntornoHelper variableEntorno, AuthUseCase authUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					if (!httpRequest.Cookies.TryGetValue(Constant.CONST_REFRESH_TOKEN, out string? refreshToken)) refreshToken = null;

					(string accesToken, int expiresIn) = await authUseCase.RefreshAccessToken(refreshToken);

					SalAuthRefreshAccessToken retorno = new() {
						AccessToken = accesToken,
						ExpiresIn = expiresIn
					};

					LambdaLogger.Log(
							$"[POST] - [Auth] - [RefreshAccessToken] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
							$"Refrescado exitoso del access token.");

					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
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

					LambdaLogger.Log(
						$"[POST] - [Auth] - [RefreshAccessToken] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
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
			routes.MapPost("/LimpiarAuthCookies", (HttpContext httpContext, HttpResponse httpResponse, IHostEnvironment environment, IVariableEntornoHelper variableEntorno) => {
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
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[POST] - [Auth] - [LimpiarAuthCookies] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
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
