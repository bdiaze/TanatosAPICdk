using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Authentication;
using Npgsql;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;
using TanatosAPI.Repositories;
using TanatosAPI.UseCases;

namespace TanatosAPI.Endpoints {
	public static class DestinatarioNotificacionEndpoints {
		public static IEndpointRouteBuilder MapDestinatarioNotificacionEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder publicGroup = routes.MapGroup("/public/DestinatarioNotificacion");
			publicGroup.MapValidarEndpoint();

			return routes;
		}

		private static void MapValidarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/Validar/", async (EntDestinatarioNotificacionValidar entrada, IHostEnvironment environment, IDatabaseConnectionHelper connectionHelper, IDateTimeProvider dateTimeProvider, DestinatarioNotificacionUseCase destinatarioNotificacionUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					await destinatarioNotificacionUseCase.ValidarDestinatario(entrada.CodigoValidacion);

					LambdaLogger.Log(
						$"[POST] - [DestinatarioNotificacion] - [Validar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Se valida exitosamente el destinatario de notificación.");

					return Results.Ok();
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[POST] - [DestinatarioNotificacion] - [Validar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [DestinatarioNotificacion] - [Validar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al validar el destinatario de notificación. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).AllowAnonymous();
		}
	}
}
