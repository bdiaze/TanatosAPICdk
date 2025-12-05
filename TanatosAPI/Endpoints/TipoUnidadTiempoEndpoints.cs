using Amazon.Lambda.Core;
using System.Diagnostics;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Endpoints {
	public static class TipoUnidadTiempoEndpoints {
		public static IEndpointRouteBuilder MapTipoUnidadTiempoEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/TipoUnidadTiempo");
			group.MapObtenerVigentes();
			group.MapObtenerPorVigencia();
			group.MapCrearEndpoint();
			group.MapActualizarEndpoint();
			group.MapEliminarEndpoint();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes", async (IHostEnvironment environment, TipoUnidadTiempoDao tipoUnidadTiempoDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<TipoUnidadTiempo> retorno = await tipoUnidadTiempoDao.ObtenerPorVigencia(true);

					LambdaLogger.Log(
						$"[GET] - [TipoUnidadTiempo] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los tipos de unidad de tiempo vigentes - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoUnidadTiempo] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los tipos de unidad de tiempo vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization().WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerPorVigencia(this IEndpointRouteBuilder routes) {
			routes.MapGet("/PorVigencia/{vigencia}", async (bool vigencia, IHostEnvironment environment, TipoUnidadTiempoDao tipoUnidadTiempoDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<TipoUnidadTiempo> retorno = await tipoUnidadTiempoDao.ObtenerPorVigencia(vigencia);

					LambdaLogger.Log(
						$"[GET] - [TipoUnidadTiempo] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los tipos de unidad de tiempo por vigencia - Vigencia: {vigencia} - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoUnidadTiempo] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los tipos de unidad de tiempo por vigencia - Vigencia: {vigencia}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (TipoUnidadTiempo entrada, IHostEnvironment environment, TipoUnidadTiempoDao tipoUnidadTiempoDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoUnidadTiempo? existente = await tipoUnidadTiempoDao.ObtenerPorId(entrada.Id);

					if (existente != null) {
						LambdaLogger.Log(
							$"[POST] - [TipoUnidadTiempo] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Ya existe un tipo de unidad de tiempo con ID {entrada.Id}.");

						return Results.BadRequest($"Ya existe un tipo de unidad de tiempo con ID {entrada.Id}.");
					}

					await tipoUnidadTiempoDao.Insertar(entrada);
					existente = entrada;

					LambdaLogger.Log(
						$"[POST] - [TipoUnidadTiempo] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa del tipo de unidad de tiempo - ID: {entrada.Id}.");

					return Results.Ok(existente);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [TipoUnidadTiempo] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación del tipo de unidad de tiempo - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (TipoUnidadTiempo entrada, IHostEnvironment environment, TipoUnidadTiempoDao tipoUnidadTiempoDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoUnidadTiempo? existente = await tipoUnidadTiempoDao.ObtenerPorId(entrada.Id);

					if (existente == null) {
						LambdaLogger.Log(
							$"[PUT] - [TipoUnidadTiempo] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No existe el tipo de unidad de tiempo con ID {entrada.Id}.");

						return Results.BadRequest($"No existe el tipo de unidad de tiempo con ID {entrada.Id}.");
					}

					await tipoUnidadTiempoDao.Actualizar(entrada);
					existente = entrada;

					LambdaLogger.Log(
						$"[PUT] - [TipoUnidadTiempo] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa del tipo de unidad de tiempo - ID: {entrada.Id}.");

					return Results.Ok(existente);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [TipoUnidadTiempo] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización del tipo de unidad de tiempo - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, TipoUnidadTiempoDao tipoUnidadTiempoDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoUnidadTiempo? existente = await tipoUnidadTiempoDao.ObtenerPorId(id);

					if (existente == null) {
						LambdaLogger.Log(
							$"[DELETE] - [TipoUnidadTiempo] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No existe el tipo de unidad de tiempo con ID {id}.");

						return Results.BadRequest($"No existe el tipo de unidad de tiempo con ID {id}.");
					}

					await tipoUnidadTiempoDao.Eliminar(id);

					LambdaLogger.Log(
						$"[DELETE] - [TipoUnidadTiempo] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del tipo de unidad de tiempo - ID: {id}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [TipoUnidadTiempo] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del tipo de unidad de tiempo - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}
	}
}
