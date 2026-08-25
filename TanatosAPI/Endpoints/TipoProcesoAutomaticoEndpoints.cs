using Amazon.Lambda.Core;
using System.Diagnostics;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.TipoProcesoAutomatico;
using TanatosAPI.Entities.Others.VideoTutorial;
using TanatosAPI.Exceptions;
using TanatosAPI.UseCases;

namespace TanatosAPI.Endpoints {
	public static class TipoProcesoAutomaticoEndpoints {
		public static void MapTipoProcesoAutomaticoEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/TipoProcesoAutomatico");
			group.MapObtenerVigentes();
			group.MapCrearEndpoint();
			group.MapActualizarEndpoint();
			group.MapEliminarEndpoint();
		}

		private static void MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes", async (IHostEnvironment environment, TipoProcesoAutomaticoUseCase tipoProcesoAutomaticoUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<TipoProcesoAutomatico> vigentes = await tipoProcesoAutomaticoUseCase.ObtenerVigentes();

					List<SalTipoProcesoAutomatico> retorno = [.. vigentes
						.Select(p => new SalTipoProcesoAutomatico() {
							Id = p.Id,
							Nombre = p.Nombre,
							Descripcion = p.Descripcion,
							Habilitado = p.Habilitado,
							Orden = p.Orden,
						})
					];

					LambdaLogger.Log(
						$"[GET] - [TipoProcesoAutomatico] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los tipos de procesos automáticos vigentes - Cant. Registros: {retorno.Count}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoProcesoAutomatico] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoProcesoAutomatico] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener  los tipos de procesos automáticos vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");
		}

		private static void MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (EntTipoProcesoAutomaticoCrear entrada, IHostEnvironment environment, TipoProcesoAutomaticoUseCase tipoProcesoAutomaticoUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoProcesoAutomatico nuevo = await tipoProcesoAutomaticoUseCase.Registrar(entrada.Nombre, entrada.Descripcion, entrada.Habilitado, entrada.Orden);

					SalTipoProcesoAutomatico retorno = new() {
						Id = nuevo.Id,
						Nombre = nuevo.Nombre,
						Descripcion = nuevo.Descripcion,
						Habilitado = nuevo.Habilitado,
						Orden = nuevo.Orden,
					};

					LambdaLogger.Log(
						$"[POST] - [TipoProcesoAutomatico] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa del tipo de proceso automático - ID: {retorno.Id}.");

					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[POST] - [TipoProcesoAutomatico] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [TipoProcesoAutomatico] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación del tipo de proceso automático. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");
		}

		private static void MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (EntTipoProcesoAutomaticoActualizar entrada, IHostEnvironment environment, TipoProcesoAutomaticoUseCase tipoProcesoAutomaticoUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoProcesoAutomatico existente = await tipoProcesoAutomaticoUseCase.Actualizar(entrada.Id, entrada.Nombre, entrada.Descripcion, entrada.Habilitado, entrada.Orden);

					SalTipoProcesoAutomatico retorno = new() {
						Id = existente.Id,
						Nombre = existente.Nombre,
						Descripcion = existente.Descripcion,
						Habilitado = existente.Habilitado,
						Orden = existente.Orden,
					};

					LambdaLogger.Log(
						$"[PUT] - [TipoProcesoAutomatico] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa del tipo de proceso automático - ID: {entrada.Id}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[PUT] - [TipoProcesoAutomatico] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [TipoProcesoAutomatico] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización del tipo de proceso automático - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");
		}

		private static void MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, TipoProcesoAutomaticoUseCase tipoProcesoAutomaticoUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					await tipoProcesoAutomaticoUseCase.Eliminar(id);

					LambdaLogger.Log(
						$"[DELETE] - [TipoProcesoAutomatico] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del tipo de proceso automático - ID: {id}.");
					return Results.Ok();
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[DELETE] - [TipoProcesoAutomatico] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [TipoProcesoAutomatico] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del tipo de proceso automático - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");
		}
	}
}
