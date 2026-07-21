using Amazon.Lambda.Core;
using System.Diagnostics;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;
using TanatosAPI.UseCases;

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
			routes.MapGet("/Vigentes", async (IHostEnvironment environment, TipoUnidadTiempoUseCase tipoUnidadTiempoUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<TipoUnidadTiempo> retorno = await tipoUnidadTiempoUseCase.ObtenerVigentes();

					LambdaLogger.Log(
						$"[GET] - [TipoUnidadTiempo] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los tipos de unidad de tiempo vigentes - Cant. Registros: {retorno.Count}.");
					return Results.Ok(retorno);
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[GET] - [TipoUnidadTiempo] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoUnidadTiempo] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los tipos de unidad de tiempo vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Sistema.Read.Public");

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerPorVigencia(this IEndpointRouteBuilder routes) {
			routes.MapGet("/PorVigencia/{vigencia?}", async (string? vigencia, IHostEnvironment environment, TipoUnidadTiempoUseCase tipoUnidadTiempoUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					bool? vig = vigencia?.Trim().ToLowerInvariant() switch {
						"true" => true,
						"false" => false,
						_ => null
					};

					List<TipoUnidadTiempo> retorno = await tipoUnidadTiempoUseCase.ObtenerPorVigencia(vig);

					LambdaLogger.Log(
						$"[GET] - [TipoUnidadTiempo] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los tipos de unidad de tiempo por vigencia - Vigencia: {vigencia} - Cant. Registros: {retorno.Count}.");
					return Results.Ok(retorno);
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[GET] - [TipoUnidadTiempo] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoUnidadTiempo] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los tipos de unidad de tiempo por vigencia - Vigencia: {vigencia}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");

			return routes;
		}

		private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (TipoUnidadTiempo entrada, IHostEnvironment environment, TipoUnidadTiempoUseCase tipoUnidadTiempoUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoUnidadTiempo nuevo = await tipoUnidadTiempoUseCase.Insertar(
                        entrada.Id,
                        entrada.Nombre,
                        entrada.NombrePlural,
                        entrada.CantSegundos,
                        entrada.CantMinutos,
                        entrada.CantHoras,
                        entrada.CantDias,
                        entrada.Vigencia
                    );

					LambdaLogger.Log(
						$"[POST] - [TipoUnidadTiempo] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa del tipo de unidad de tiempo - ID: {entrada.Id}.");
					return Results.Ok(nuevo);
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[POST] - [TipoUnidadTiempo] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [TipoUnidadTiempo] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación del tipo de unidad de tiempo - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");

			return routes;
		}

		private static IEndpointRouteBuilder MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (TipoUnidadTiempo entrada, IHostEnvironment environment, TipoUnidadTiempoUseCase tipoUnidadTiempoUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
                    TipoUnidadTiempo existente = await tipoUnidadTiempoUseCase.Actualizar(
						entrada.Id, 
						entrada.Nombre, 
						entrada.NombrePlural, 
						entrada.CantSegundos, 
						entrada.CantMinutos,
						entrada.CantHoras,
						entrada.CantDias,
						entrada.Vigencia
					);

					LambdaLogger.Log(
						$"[PUT] - [TipoUnidadTiempo] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa del tipo de unidad de tiempo - ID: {entrada.Id}.");
					return Results.Ok(existente);
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[PUT] - [TipoUnidadTiempo] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [TipoUnidadTiempo] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización del tipo de unidad de tiempo - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");

			return routes;
		}

		private static IEndpointRouteBuilder MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, TipoUnidadTiempoUseCase tipoUnidadTiempoUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					await tipoUnidadTiempoUseCase.Eliminar(id);

					LambdaLogger.Log(
						$"[DELETE] - [TipoUnidadTiempo] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del tipo de unidad de tiempo - ID: {id}.");
					return Results.Ok();
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[DELETE] - [TipoUnidadTiempo] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [TipoUnidadTiempo] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del tipo de unidad de tiempo - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");

			return routes;
		}
	}
}
