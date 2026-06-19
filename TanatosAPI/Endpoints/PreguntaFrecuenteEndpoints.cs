using Amazon.Lambda.Core;
using System.Diagnostics;
using System.Security.Claims;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Exceptions;
using TanatosAPI.UseCases;

namespace TanatosAPI.Endpoints {
	public static class PreguntaFrecuenteEndpoints {
		public static IEndpointRouteBuilder MapPreguntaFrecuenteEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder publicGroup = routes.MapGroup("/public/PreguntaFrecuente");
			publicGroup.MapObtenerHabilitados();

			RouteGroupBuilder group = routes.MapGroup("/PreguntaFrecuente");
			group.MapObtenerVigentes();
			group.MapCrearEndpoint();
			group.MapActualizarEndpoint();
			group.MapEliminarEndpoint();

			return routes;
		}

		private static void MapObtenerHabilitados(this IEndpointRouteBuilder routes) {
			routes.MapGet("/", async (IHostEnvironment environment, PreguntaFrecuenteUseCase preguntaFrecuenteUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<PreguntaFrecuente> habilitados = await preguntaFrecuenteUseCase.ObtenerHabilitados();

					List<SalPreguntaFrecuenteHabilitado> retorno = [.. habilitados
						.Select(p => new SalPreguntaFrecuenteHabilitado() {
							Orden = p.Orden,
							Pregunta = p.Pregunta,
							Respuesta = p.Respuesta
						})
					];

					LambdaLogger.Log(
						$"[GET] - [PreguntaFrecuente] - [ObtenerHabilitados] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de las preguntas frecuentes habilitadas - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [PreguntaFrecuente] - [ObtenerHabilitados] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [PreguntaFrecuente] - [ObtenerHabilitados] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener  las preguntas frecuentes habilitadas. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).AllowAnonymous();
		}

		private static void MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes", async (IHostEnvironment environment, PreguntaFrecuenteUseCase preguntaFrecuenteUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<PreguntaFrecuente> preguntaFrecuentes = await preguntaFrecuenteUseCase.ObtenerVigentes();

					List<SalPreguntaFrecuente> retorno = [.. preguntaFrecuentes
						.Select(d => new SalPreguntaFrecuente() {
							Id = d.Id,
							Pregunta = d.Pregunta,
							Respuesta = d.Respuesta,
							Habilitado = d.Habilitado,
							Orden = d.Orden,
						})
					];

					LambdaLogger.Log(
						$"[GET] - [PreguntaFrecuente] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de las preguntas frecuentes vigentes - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [PreguntaFrecuente] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [PreguntaFrecuente] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener las preguntas frecuentes vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");
		}

		private static void MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (EntPreguntaFrecuenteCrear entrada, IHostEnvironment environment, PreguntaFrecuenteUseCase preguntaFrecuenteUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					PreguntaFrecuente nuevo = await preguntaFrecuenteUseCase.Registrar(entrada.Pregunta, entrada.Respuesta, entrada.Habilitado, entrada.Orden);

					SalPreguntaFrecuente retorno = new() {
						Id = nuevo.Id,
						Pregunta = nuevo.Pregunta,
						Respuesta = nuevo.Respuesta,
						Habilitado = nuevo.Habilitado,
						Orden = nuevo.Orden,
					};

					LambdaLogger.Log(
						$"[POST] - [PreguntaFrecuente] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa de la pregunta frecuente - ID: {retorno.Id}.");

					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[POST] - [PreguntaFrecuente] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [PreguntaFrecuente] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación de la pregunta frecuente. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");
		}

		private static void MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (EntPreguntaFrecuenteActualizar entrada, IHostEnvironment environment, PreguntaFrecuenteUseCase preguntaFrecuenteUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					PreguntaFrecuente existente = await preguntaFrecuenteUseCase.Actualizar(entrada.Id, entrada.Pregunta, entrada.Respuesta, entrada.Habilitado, entrada.Orden);

					SalPreguntaFrecuente retorno = new() {
						Id = existente.Id,
						Pregunta = existente.Pregunta,
						Respuesta = existente.Respuesta,
						Habilitado = existente.Habilitado,
						Orden = existente.Orden,
					};

					LambdaLogger.Log(
						$"[PUT] - [PreguntaFrecuente] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa de la pregunta frecuente - ID: {entrada.Id}.");

					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[PUT] - [PreguntaFrecuente] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [PreguntaFrecuente] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización de la pregunta frecuente - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");
		}

		private static void MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, PreguntaFrecuenteUseCase preguntaFrecuenteUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					await preguntaFrecuenteUseCase.Eliminar(id);

					LambdaLogger.Log(
						$"[DELETE] - [PreguntaFrecuente] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa de la pregunta frecuente - ID: {id}.");

					return Results.Ok();
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[DELETE] - [PreguntaFrecuente] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [PreguntaFrecuente] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación de la pregunta frecuente - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");
		}
	}
}
