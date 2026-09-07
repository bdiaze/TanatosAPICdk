using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.ProcesoAutomatico;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.UseCases;

namespace TanatosAPI.Endpoints {
	public static class ProcesoAutomaticoEndpoints {
		public static void MapProcesoAutomaticoEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/ProcesoAutomatico");
			group.MapObtenerConPaginacion();
		}

		private static void MapObtenerConPaginacion(this IEndpointRouteBuilder routes) {
			routes.MapGet("/ObtenerConPaginacion", async ([FromQuery] long? primerId, [FromQuery] int? cantidad, [FromQuery] string? nombre, [FromQuery] string? vigencia, IHostEnvironment environment, ProcesoAutomaticoUseCase procesoAutomaticoUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					bool? vig = vigencia?.Trim().ToLowerInvariant() switch {
						"true" => true,
						"false" => false,
						_ => null
					};

					(List<ProcesoAutomatico> procesos, long? siguienteId) = await procesoAutomaticoUseCase.ObtenerConPaginacion(primerId, cantidad, nombre, vig);
					SalProcesoAutomaticoObtenerPorPaginacion retorno = new() {
						Items = procesos,
						SiguienteId = siguienteId,
					};

					LambdaLogger.Log(
						$"[GET] - [ProcesoAutomatico] - [ObtenerConPaginacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los procesos automáticos con paginación - Primer ID: {primerId} - Cantidad: {cantidad} - Nombre: {nombre} - Vigencia: {vigencia} - Cant. Registros: {retorno.Items.Count}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [ProcesoAutomatico] - [ObtenerConPaginacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [ProcesoAutomatico] - [ObtenerConPaginacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los procesos automáticos con paginación - Primer ID: {primerId} - Cantidad: {cantidad} - Nombre: {nombre} - Vigencia: {vigencia}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");
		}
	}
}
