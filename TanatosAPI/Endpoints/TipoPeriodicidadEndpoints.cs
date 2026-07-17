using Amazon.Lambda.Core;
using System.Diagnostics;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.TipoPeriodicidad;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;
using TanatosAPI.UseCases;

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
			routes.MapGet("/Vigentes", async (IHostEnvironment environment, TipoPeriodicidadUseCase tipoPeriodicidadUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<TipoPeriodicidad> periodicidades = await tipoPeriodicidadUseCase.ObtenerVigentes();
					List<SalTipoPeriodicidad> retorno = [.. periodicidades.Select(p => new SalTipoPeriodicidad() {
						Id = p.Id,
						Nombre = p.Nombre,
						Descripcion = p.Descripcion
					})];

					LambdaLogger.Log(
						$"[GET] - [TipoPeriodicidad] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los tipos de periodicidad vigentes - Cant. Registros: {retorno.Count}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoPeriodicidad] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoPeriodicidad] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los tipos de periodicidad vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Sistema.Read.Public");

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerPorVigencia(this IEndpointRouteBuilder routes) {
			routes.MapGet("/PorVigencia/{vigencia?}", async (string? vigencia, IHostEnvironment environment, TipoPeriodicidadUseCase tipoPeriodicidadUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					bool? vig = vigencia?.Trim().ToLowerInvariant() switch {
						"true" => true,
						"false" => false,
						_ => null
					};

					List<TipoPeriodicidad> retorno = await tipoPeriodicidadUseCase.ObtenerPorVigencia(vig);

					LambdaLogger.Log(
						$"[GET] - [TipoPeriodicidad] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los tipos de periodicidad por vigencia - Vigencia: {vigencia} - Cant. Registros: {retorno.Count}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoPeriodicidad] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoPeriodicidad] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los tipos de periodicidad por vigencia - Vigencia: {vigencia}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");

			return routes;
		}

		private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (TipoPeriodicidad entrada, IHostEnvironment environment, TipoPeriodicidadUseCase tipoPeriodicidadUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoPeriodicidad nuevo = await tipoPeriodicidadUseCase.Crear(
						entrada.Id,
						entrada.Nombre,
						entrada.Descripcion,
						entrada.Cron,
						entrada.FrecuenciaDias,
						entrada.DeltaDias,
						entrada.DeltaMeses,
						entrada.DeltaAnnos,
						entrada.Vigencia
					);

					LambdaLogger.Log(
						$"[POST] - [TipoPeriodicidad] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa del tipo de periodicidad - ID: {entrada.Id}.");
					return Results.Ok(nuevo);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[POST] - [TipoPeriodicidad] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [TipoPeriodicidad] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación del tipo de periodicidad - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");

			return routes;
		}

		private static IEndpointRouteBuilder MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (TipoPeriodicidad entrada, IHostEnvironment environment, TipoPeriodicidadUseCase tipoPeriodicidadUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoPeriodicidad existente = await tipoPeriodicidadUseCase.Modificar(
						entrada.Id,
						entrada.Nombre,
						entrada.Descripcion,
						entrada.Cron,
						entrada.FrecuenciaDias,
						entrada.DeltaDias,
						entrada.DeltaMeses,
						entrada.DeltaAnnos,
						entrada.Vigencia
					);

					LambdaLogger.Log(
						$"[PUT] - [TipoPeriodicidad] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa del tipo de periodicidad - ID: {entrada.Id}.");
					return Results.Ok(existente);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[PUT] - [TipoPeriodicidad] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [TipoPeriodicidad] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización del tipo de periodicidad - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");

			return routes;
		}

		private static IEndpointRouteBuilder MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, TipoPeriodicidadUseCase tipoPeriodicidadUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					await tipoPeriodicidadUseCase.Eliminar(id);

					LambdaLogger.Log(
						$"[DELETE] - [TipoPeriodicidad] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del tipo de periodicidad - ID: {id}.");
					return Results.Ok();
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[DELETE] - [TipoPeriodicidad] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [TipoPeriodicidad] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del tipo de periodicidad - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");

			return routes;
		}
	}
}
