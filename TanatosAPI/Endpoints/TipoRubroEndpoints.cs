using Amazon.Lambda.Core;
using System.Diagnostics;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Endpoints {
	public static class TipoRubroEndpoints {
		public static IEndpointRouteBuilder MapTipoRubroEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/TipoRubro");
			group.MapObtenerVigentes();
			group.MapObtenerPorVigencia();
			group.MapCrearEndpoint();
			group.MapActualizarEndpoint();
			group.MapEliminarEndpoint();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes", async (IHostEnvironment environment, TipoRubroDao tipoRubroDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<TipoRubro> retorno = await tipoRubroDao.ObtenerPorVigencia(true);

					LambdaLogger.Log(
						$"[GET] - [TipoRubro] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los tipos de rubros vigentes - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoRubro] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los tipos de rubros vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization().WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerPorVigencia(this IEndpointRouteBuilder routes) {
			routes.MapGet("/PorVigencia/{vigencia?}", async (string? vigencia, IHostEnvironment environment, TipoRubroDao tipoRubroDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					bool? vig = vigencia?.Trim().ToLowerInvariant() switch {
						"true" => true,
						"false" => false,
						_ => null
					};

					List<TipoRubro> retorno = await tipoRubroDao.ObtenerPorVigencia(vig);

					LambdaLogger.Log(
						$"[GET] - [TipoRubro] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los tipos de rubros por vigencia - Vigencia: {vigencia} - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoRubro] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los tipos de rubros por vigencia - Vigencia: {vigencia}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (TipoRubro entrada, IHostEnvironment environment, TipoRubroDao tipoRubroDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoRubro? existente = await tipoRubroDao.ObtenerPorId(entrada.Id);

					if (existente != null) {
						LambdaLogger.Log(
							$"[POST] - [TipoRubro] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Ya existe un tipo de rubro con ID {entrada.Id}.");

						return Results.BadRequest($"Ya existe un tipo de rubro con ID {entrada.Id}.");
					}

					await tipoRubroDao.Insertar(entrada);
					existente = entrada;

					LambdaLogger.Log(
						$"[POST] - [TipoRubro] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa del tipo de rubro - ID: {entrada.Id}.");

					return Results.Ok(existente);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [TipoRubro] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación del tipo de rubro - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (TipoRubro entrada, IHostEnvironment environment, TipoRubroDao tipoRubroDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoRubro? existente = await tipoRubroDao.ObtenerPorId(entrada.Id);

					if (existente == null) {
						LambdaLogger.Log(
							$"[PUT] - [TipoRubro] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No existe el tipo de rubro con ID {entrada.Id}.");

						return Results.BadRequest($"No existe el tipo de rubro con ID {entrada.Id}.");
					}

					await tipoRubroDao.Actualizar(entrada);
					existente = entrada;

					LambdaLogger.Log(
						$"[PUT] - [TipoRubro] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa del tipo de rubro - ID: {entrada.Id}.");

					return Results.Ok(existente);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [TipoRubro] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización del tipo de rubro - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, TipoRubroDao tipoRubroDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoRubro? existente = await tipoRubroDao.ObtenerPorId(id);

					if (existente == null) {
						LambdaLogger.Log(
							$"[DELETE] - [TipoRubro] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No existe el tipo de rubro con ID {id}.");

						return Results.BadRequest($"No existe el tipo de rubro con ID {id}.");
					}

					await tipoRubroDao.Eliminar(id);

					LambdaLogger.Log(
						$"[DELETE] - [TipoRubro] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del tipo de rubro - ID: {id}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [TipoRubro] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del tipo de rubro - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}
	}
}
