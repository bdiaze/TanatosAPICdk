using Amazon.Lambda.Core;
using System.Diagnostics;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.VideoTutorial;
using TanatosAPI.Exceptions;
using TanatosAPI.UseCases;

namespace TanatosAPI.Endpoints {
	public static class VideoTutorialEndpoints {
		public static IEndpointRouteBuilder MapVideoTutorialEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder publicGroup = routes.MapGroup("/public/VideoTutorial");
			publicGroup.MapObtenerHabilitados();

			RouteGroupBuilder group = routes.MapGroup("/VideoTutorial");
			group.MapObtenerVigentes();
			group.MapCrearEndpoint();
			group.MapActualizarEndpoint();
			group.MapEliminarEndpoint();

			return routes;
		}

		private static void MapObtenerHabilitados(this IEndpointRouteBuilder routes) {
			routes.MapGet("/", async (IHostEnvironment environment, VideoTutorialUseCase videoTutorialUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<VideoTutorial> habilitados = await videoTutorialUseCase.ObtenerHabilitados();

					List<SalVideoTutorialHabilitado> retorno = [.. habilitados
						.Select(p => new SalVideoTutorialHabilitado() {
							Orden = p.Orden,
							Titulo = p.Titulo,
							Descripcion = p.Descripcion,
							Url = p.Url
						})
					];

					LambdaLogger.Log(
						$"[GET] - [VideoTutorial] - [ObtenerHabilitados] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los videos tutoriales habilitadas - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [VideoTutorial] - [ObtenerHabilitados] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [VideoTutorial] - [ObtenerHabilitados] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener  los videos tutoriales habilitadas. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).AllowAnonymous();
		}

		private static void MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes", async (IHostEnvironment environment, VideoTutorialUseCase videoTutorialUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<VideoTutorial> videosTutoriales = await videoTutorialUseCase.ObtenerVigentes();

					List<SalVideoTutorial> retorno = [.. videosTutoriales
						.Select(d => new SalVideoTutorial() {
							Id = d.Id,
							Titulo = d.Titulo,
							Descripcion = d.Descripcion,
							Url = d.Url,
							Habilitado = d.Habilitado,
							Orden = d.Orden,
						})
					];

					LambdaLogger.Log(
						$"[GET] - [VideoTutorial] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los videos tutoriales vigentes - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [VideoTutorial] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [VideoTutorial] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los videos tutoriales vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");
		}

		private static void MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (EntVideoTutorialCrear entrada, IHostEnvironment environment, VideoTutorialUseCase videoTutorialUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					VideoTutorial nuevo = await videoTutorialUseCase.Registrar(entrada.Titulo, entrada.Descripcion, entrada.Url, entrada.Habilitado, entrada.Orden);

					SalVideoTutorial retorno = new() {
						Id = nuevo.Id,
						Titulo = nuevo.Titulo,
						Descripcion = nuevo.Descripcion,
						Url = nuevo.Url,
						Habilitado = nuevo.Habilitado,
						Orden = nuevo.Orden,
					};

					LambdaLogger.Log(
						$"[POST] - [VideoTutorial] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa del video tutorial - ID: {retorno.Id}.");

					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[POST] - [VideoTutorial] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [VideoTutorial] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación del video tutorial. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");
		}

		private static void MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (EntVideoTutorialActualizar entrada, IHostEnvironment environment, VideoTutorialUseCase videoTutorialUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					VideoTutorial existente = await videoTutorialUseCase.Actualizar(entrada.Id, entrada.Titulo, entrada.Descripcion, entrada.Url, entrada.Habilitado, entrada.Orden);

					SalVideoTutorial retorno = new() {
						Id = existente.Id,
						Titulo = existente.Titulo,
						Descripcion = existente.Descripcion,
						Url = existente.Url,
						Habilitado = existente.Habilitado,
						Orden = existente.Orden,
					};

					LambdaLogger.Log(
						$"[PUT] - [VideoTutorial] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa del video tutorial - ID: {entrada.Id}.");

					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[PUT] - [VideoTutorial] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [VideoTutorial] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización del video tutorial - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");
		}

		private static void MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, VideoTutorialUseCase videoTutorialUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					await videoTutorialUseCase.Eliminar(id);

					LambdaLogger.Log(
						$"[DELETE] - [VideoTutorial] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del video tutorial - ID: {id}.");

					return Results.Ok();
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[DELETE] - [VideoTutorial] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [VideoTutorial] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del video tutorial - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");
		}
	}
}
