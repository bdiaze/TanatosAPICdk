using Amazon.Lambda.Core;
using System.Diagnostics;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Endpoints {
	public static class TipoFiscalizadorEndpoints {
		public static IEndpointRouteBuilder MapTipoFiscalizadorEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/TipoFiscalizador");
			group.MapObtenerVigentes();
			group.MapObtenerPorVigencia();
			group.MapCrearEndpoint();
			group.MapActualizarEndpoint();
			group.MapEliminarEndpoint();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes", async (IHostEnvironment environment, TipoFiscalizadorDao tipoFiscalizadorDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<TipoFiscalizador> retorno = await tipoFiscalizadorDao.ObtenerPorVigencia(true);

					LambdaLogger.Log(
						$"[GET] - [TipoFiscalizador] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los tipos de fiscalizadores vigentes - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoFiscalizador] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los tipos de fiscalizadores vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization().WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerPorVigencia(this IEndpointRouteBuilder routes) {
			routes.MapGet("/PorVigencia/{vigencia}", async (bool vigencia, IHostEnvironment environment, TipoFiscalizadorDao tipoFiscalizadorDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<TipoFiscalizador> retorno = await tipoFiscalizadorDao.ObtenerPorVigencia(vigencia);

					LambdaLogger.Log(
						$"[GET] - [TipoFiscalizador] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los tipos de fiscalizadores por vigencia - Vigencia: {vigencia} - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoFiscalizador] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los tipos de fiscalizadores por vigencia - Vigencia: {vigencia}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (TipoFiscalizador entrada, IHostEnvironment environment, TipoFiscalizadorDao tipoFiscalizadorDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoFiscalizador? existente = await tipoFiscalizadorDao.ObtenerPorId(entrada.Id);

					if (existente != null) {
						LambdaLogger.Log(
							$"[POST] - [TipoFiscalizador] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Ya existe un tipo de fiscalizador con ID {entrada.Id}.");

						return Results.BadRequest($"Ya existe un tipo de fiscalizador con ID {entrada.Id}.");
					}

					await tipoFiscalizadorDao.Insertar(entrada);
					existente = entrada;

					LambdaLogger.Log(
						$"[POST] - [TipoFiscalizador] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa del tipo de fiscalizador - ID: {entrada.Id}.");

					return Results.Ok(existente);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [TipoFiscalizador] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación del tipo de fiscalizador - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (TipoFiscalizador entrada, IHostEnvironment environment, TipoFiscalizadorDao tipoFiscalizadorDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoFiscalizador? existente = await tipoFiscalizadorDao.ObtenerPorId(entrada.Id);

					if (existente == null) {
						LambdaLogger.Log(
							$"[PUT] - [TipoFiscalizador] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No existe el tipo de fiscalizador con ID {entrada.Id}.");

						return Results.BadRequest($"No existe el tipo de fiscalizador con ID {entrada.Id}.");
					}

					await tipoFiscalizadorDao.Actualizar(entrada);
					existente = entrada;

					LambdaLogger.Log(
						$"[PUT] - [TipoFiscalizador] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa del tipo de fiscalizador - ID: {entrada.Id}.");

					return Results.Ok(existente);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [TipoFiscalizador] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización del tipo de fiscalizador - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, TipoFiscalizadorDao tipoFiscalizadorDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoFiscalizador? existente = await tipoFiscalizadorDao.ObtenerPorId(id);

					if (existente == null) {
						LambdaLogger.Log(
							$"[DELETE] - [TipoFiscalizador] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No existe el tipo de fiscalizador con ID {id}.");

						return Results.BadRequest($"No existe el tipo de fiscalizador con ID {id}.");
					}

					await tipoFiscalizadorDao.Eliminar(id);

					LambdaLogger.Log(
						$"[DELETE] - [TipoFiscalizador] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del tipo de fiscalizador - ID: {id}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [TipoFiscalizador] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del tipo de fiscalizador - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}
	}
}
