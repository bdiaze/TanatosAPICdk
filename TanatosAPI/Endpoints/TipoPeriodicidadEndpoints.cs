using Amazon.Lambda.Core;
using System.Diagnostics;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Endpoints {
	public static class TipoPeriodicidadEndpoints {
		public static IEndpointRouteBuilder MapTipoPeriodicidadEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/TipoPeriodicidad");
			group.MapObtenerVigentes();
			group.MapObtenerPorVigencia();
			group.MapCrearEndpoint();
			group.MapActualizarEndpoint();
			group.MapEliminarEndpoint();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes", async (IHostEnvironment environment, TipoPeriodicidadDao tipoPeriodicidadDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<TipoPeriodicidad> retorno = await tipoPeriodicidadDao.ObtenerPorVigencia(true);

					LambdaLogger.Log(
						$"[GET] - [TipoPeriodicidad] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los tipos de periodicidad vigentes - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoPeriodicidad] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los tipos de periodicidad vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization().WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerPorVigencia(this IEndpointRouteBuilder routes) {
			routes.MapGet("/PorVigencia/{vigencia}", async (bool vigencia, IHostEnvironment environment, TipoPeriodicidadDao tipoPeriodicidadDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<TipoPeriodicidad> retorno = await tipoPeriodicidadDao.ObtenerPorVigencia(vigencia);

					LambdaLogger.Log(
						$"[GET] - [TipoPeriodicidad] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los tipos de periodicidad por vigencia - Vigencia: {vigencia} - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoPeriodicidad] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los tipos de periodicidad por vigencia - Vigencia: {vigencia}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (TipoPeriodicidad entrada, IHostEnvironment environment, TipoPeriodicidadDao tipoPeriodicidadDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoPeriodicidad? existente = await tipoPeriodicidadDao.ObtenerPorId(entrada.Id);

					if (existente != null) {
						LambdaLogger.Log(
							$"[POST] - [TipoPeriodicidad] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Ya existe un tipo de periodicidad con ID {entrada.Id}.");

						return Results.BadRequest($"Ya existe un tipo de periodicidad con ID {entrada.Id}.");
					}

					await tipoPeriodicidadDao.Insertar(entrada);
					existente = entrada;

					LambdaLogger.Log(
						$"[POST] - [TipoPeriodicidad] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa del tipo de periodicidad - ID: {entrada.Id}.");

					return Results.Ok(existente);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [TipoPeriodicidad] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación del tipo de periodicidad - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (TipoPeriodicidad entrada, IHostEnvironment environment, TipoPeriodicidadDao tipoPeriodicidadDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoPeriodicidad? existente = await tipoPeriodicidadDao.ObtenerPorId(entrada.Id);

					if (existente == null) {
						LambdaLogger.Log(
							$"[PUT] - [TipoPeriodicidad] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No existe el tipo de periodicidad con ID {entrada.Id}.");

						return Results.BadRequest($"No existe el tipo de periodicidad con ID {entrada.Id}.");
					}

					await tipoPeriodicidadDao.Actualizar(entrada);
					existente = entrada;

					LambdaLogger.Log(
						$"[PUT] - [TipoPeriodicidad] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa del tipo de periodicidad - ID: {entrada.Id}.");

					return Results.Ok(existente);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [TipoPeriodicidad] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización del tipo de periodicidad - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, TipoPeriodicidadDao tipoPeriodicidadDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoPeriodicidad? existente = await tipoPeriodicidadDao.ObtenerPorId(id);

					if (existente == null) {
						LambdaLogger.Log(
							$"[DELETE] - [TipoPeriodicidad] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No existe el tipo de periodicidad con ID {id}.");

						return Results.BadRequest($"No existe el tipo de periodicidad con ID {id}.");
					}

					await tipoPeriodicidadDao.Eliminar(id);

					LambdaLogger.Log(
						$"[DELETE] - [TipoPeriodicidad] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del tipo de periodicidad - ID: {id}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [TipoPeriodicidad] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del tipo de periodicidad - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}
	}
}
