using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Cargo;
using TanatosAPI.Entities.Others.Evaluacion;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.UseCases;

namespace TanatosAPI.Endpoints {
	public static class EvaluacionEndpoints {
		public static IEndpointRouteBuilder MapEvaluacionEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/Evaluacion");
			group.MapObtenerEndpoint();
			group.MapCrearEndpoint();

			return routes;
		}

		private static void MapObtenerEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapGet("/", async ([FromQuery] DateTime fechaDesde, [FromQuery] DateTime fechaHasta, IHostEnvironment environment, ClaimsPrincipal user, EvaluacionUseCase evaluacionUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					List<Evaluacion> evaluaciones = await evaluacionUseCase.Obtener(fechaDesde, fechaHasta);
					List<SalEvaluacion> retorno = [.. evaluaciones.Select(e => new SalEvaluacion() {
						Sub = e.Sub,
						Puntaje = e.Puntaje,
						Comentario = e.Comentario,
						FechaCreacion = e.FechaCreacion
					})];

					LambdaLogger.Log(
						$"[GET] - [Evaluacion] - [Obtener] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de las evaluaciones - Fecha Desde: {fechaDesde:O} - Fecha Hasta: {fechaHasta:O} - Cant. Registros: {retorno.Count}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [Evaluacion] - [Obtener] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Evaluacion] - [Obtener] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener las evaluaciones. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");
		}

		private static void MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (EntEvaluacionCrear entrada, IHostEnvironment environment, ClaimsPrincipal user, EvaluacionUseCase evaluacionUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					Evaluacion nuevo = await evaluacionUseCase.Insertar(sub, entrada.Puntaje, entrada.Comentario);

					SalEvaluacionCrear retorno = new() {
						Puntaje = nuevo.Puntaje,
						Comentario = nuevo.Comentario,
						FechaCreacion = nuevo.FechaCreacion
					};

					LambdaLogger.Log(
						$"[POST] - [Evaluacion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa de la evaluación - ID: {nuevo.Id}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[POST] - [Evaluacion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [Evaluacion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación de la evaluación. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Perfil.Write.Self");
		}
	}
}
