using Amazon.Lambda.Core;
using System;
using System.Diagnostics;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Endpoints {
    public static class TipoReceptorNotificacionEndpoints {
        public static IEndpointRouteBuilder MapTipoReceptorNotificacionEndpoints(this IEndpointRouteBuilder routes) {
            RouteGroupBuilder group = routes.MapGroup("/TipoReceptorNotificacion");
            group.MapObtenerPorVigencia();
			group.MapCrearEndpoint();
			group.MapActualizarEndpoint();
			group.MapEliminarEndpoint();

			return routes;
        }

        private static IEndpointRouteBuilder MapObtenerPorVigencia(this IEndpointRouteBuilder routes) {
            routes.MapGet("/PorVigencia/{vigencia}", async (bool vigencia, IHostEnvironment environment, TipoReceptorNotificacionDao tipoReceptorNotificacionDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<TipoReceptorNotificacion> retorno = await tipoReceptorNotificacionDao.ObtenerPorVigencia(vigencia);

					LambdaLogger.Log(
						$"[GET] - [TipoReceptorNotificacion] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los tipos de receptores de notificación por vigencia - Vigencia: {vigencia} - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoReceptorNotificacion] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los tipos de receptores de notificación por vigencia - Vigencia: {vigencia}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPost("/", async (TipoReceptorNotificacion entrada, IHostEnvironment environment, TipoReceptorNotificacionDao tipoReceptorNotificacionDao) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    TipoReceptorNotificacion? existente = await tipoReceptorNotificacionDao.ObtenerPorId(entrada.Id);

					if (existente == null) {
                        await tipoReceptorNotificacionDao.Insertar(entrada);
						existente = entrada;
                    }

                    LambdaLogger.Log(
                        $"[POST] - [TipoReceptorNotificacion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Creación exitosa del tipo de receptor de notificación - ID: {entrada.Id}.");

                    return Results.Ok(existente);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[POST] - [TipoReceptorNotificacion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error en la creación del tipo de receptor de notificación - ID: {entrada.Id}. " +
                        $"{ex}");
                    return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
                }
            }).WithOpenApi();

            return routes;
        }

		private static IEndpointRouteBuilder MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (TipoReceptorNotificacion entrada, IHostEnvironment environment, TipoReceptorNotificacionDao tipoReceptorNotificacionDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoReceptorNotificacion? existente = await tipoReceptorNotificacionDao.ObtenerPorId(entrada.Id);

					if (existente != null) {
						await tipoReceptorNotificacionDao.Actualizar(entrada);
						existente = entrada;
					}

					LambdaLogger.Log(
						$"[PUT] - [TipoReceptorNotificacion] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa del tipo de receptor de notificación - ID: {entrada.Id}.");

					return Results.Ok(existente);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [TipoReceptorNotificacion] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización del tipo de receptor de notificación - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, TipoReceptorNotificacionDao tipoReceptorNotificacionDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoReceptorNotificacion? existente = await tipoReceptorNotificacionDao.ObtenerPorId(id);

					if (existente != null) {
						await tipoReceptorNotificacionDao.Eliminar(id);
					}

					LambdaLogger.Log(
						$"[DELETE] - [TipoReceptorNotificacion] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del tipo de receptor de notificación - ID: {id}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [TipoReceptorNotificacion] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del tipo de receptor de notificación - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).WithOpenApi();

			return routes;
		}
	}
}
