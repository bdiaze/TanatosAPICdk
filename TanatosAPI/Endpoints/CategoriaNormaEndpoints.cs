using Amazon.Lambda.Core;
using System.Diagnostics;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Endpoints {
	public static class CategoriaNormaEndpoints {
		public static IEndpointRouteBuilder MapCategoriaNormaEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/CategoriaNorma");
			group.MapObtenerVigentes();
			group.MapObtenerPorVigencia();
			group.MapCrearEndpoint();
			group.MapActualizarEndpoint();
			group.MapEliminarEndpoint();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes", async (IHostEnvironment environment, CategoriaNormaDao categoriaNormaDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<CategoriaNorma> retorno = await categoriaNormaDao.ObtenerPorVigencia(true);

					LambdaLogger.Log(
						$"[GET] - [CategoriaNorma] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de las categorías de normas vigentes - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [CategoriaNorma] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener las categorías de normas vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization().WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerPorVigencia(this IEndpointRouteBuilder routes) {
			routes.MapGet("/PorVigencia/{vigencia}", async (bool vigencia, IHostEnvironment environment, CategoriaNormaDao categoriaNormaDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<CategoriaNorma> retorno = await categoriaNormaDao.ObtenerPorVigencia(vigencia);

					LambdaLogger.Log(
						$"[GET] - [CategoriaNorma] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de las categorías de normas por vigencia - Vigencia: {vigencia} - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [CategoriaNorma] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener las categorías de normas por vigencia - Vigencia: {vigencia}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (CategoriaNorma entrada, IHostEnvironment environment, CategoriaNormaDao categoriaNormaDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					CategoriaNorma? existente = await categoriaNormaDao.ObtenerPorId(entrada.Id);

					if (existente != null) {
						LambdaLogger.Log(
							$"[POST] - [CategoriaNorma] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Ya existe una categoría de norma con ID {entrada.Id}.");

						return Results.BadRequest($"Ya existe una categoría de norma con ID {entrada.Id}.");
					}

					await categoriaNormaDao.Insertar(entrada);
					existente = entrada;

					LambdaLogger.Log(
						$"[POST] - [CategoriaNorma] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa de la categoría de norma - ID: {entrada.Id}.");

					return Results.Ok(existente);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [CategoriaNorma] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación de la categoría de norma - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (CategoriaNorma entrada, IHostEnvironment environment, CategoriaNormaDao categoriaNormaDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					CategoriaNorma? existente = await categoriaNormaDao.ObtenerPorId(entrada.Id);

					if (existente == null) {
						LambdaLogger.Log(
							$"[PUT] - [CategoriaNorma] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No existe la categoría de norma con ID {entrada.Id}.");

						return Results.BadRequest($"No existe la categoría de norma con ID {entrada.Id}.");
					}

					await categoriaNormaDao.Actualizar(entrada);
					existente = entrada;

					LambdaLogger.Log(
						$"[PUT] - [CategoriaNorma] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa de la categoría de norma - ID: {entrada.Id}.");

					return Results.Ok(existente);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [CategoriaNorma] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización de la categoría de norma - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, CategoriaNormaDao categoriaNormaDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					CategoriaNorma? existente = await categoriaNormaDao.ObtenerPorId(id);

					if (existente == null) {
						LambdaLogger.Log(
							$"[DELETE] - [CategoriaNorma] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No existe la categoría de norma con ID {id}.");

						return Results.BadRequest($"No existe la categoría de norma con ID {id}.");
					}

					await categoriaNormaDao.Eliminar(id);

					LambdaLogger.Log(
						$"[DELETE] - [CategoriaNorma] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa de la categoría de norma - ID: {id}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [CategoriaNorma] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación de la categoría de norma - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}
	}
}
