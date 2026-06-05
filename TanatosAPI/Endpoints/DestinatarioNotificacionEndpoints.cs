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
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;
using TanatosAPI.Repositories;

namespace TanatosAPI.Endpoints {
	public static class DestinatarioNotificacionEndpoints {
		public static IEndpointRouteBuilder MapDestinatarioNotificacionEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder publicGroup = routes.MapGroup("/public/DestinatarioNotificacion");
			publicGroup.MapValidarEndpoint();

			return routes;
		}

		private static void MapValidarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/Validar/", async (EntDestinatarioNotificacionValidar entrada, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, IDateTimeProvider dateTimeProvider, DestinatarioNotificacionBcp destinatarioNotificacionBcp, DestinatarioNotificacionDao destinatarioNotificacionDao, CryptoHelper cryptoHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					DestinatarioNotificacion? destinatarioNotificacion = await destinatarioNotificacionDao.ObtenerPorCodigoValidacion(cryptoHelper.HashSHA256(entrada.CodigoValidacion));

					// Se valida que el código exista...
					if (destinatarioNotificacion == null || !destinatarioNotificacion.Vigencia) {
						LambdaLogger.Log(
							$"[POST] - [DestinatarioNotificacion] - [Validar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status404NotFound}] - " +
							$"Código ingresado no es válido");

						return Results.NotFound("Código ingresado no es válido");
					}

					// Si el código aun no ha sido validado, se verifica la fecha de caducidad y se valida...
					if (!destinatarioNotificacion.Validado) {
						if (destinatarioNotificacion.FechaCaducidadCodigoValidacion < dateTimeProvider.UtcNow) {
							LambdaLogger.Log(
								$"[POST] - [DestinatarioNotificacion] - [Validar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
								$"El código ingresado ya caducó");

							return Results.BadRequest("El código ingresado ya caducó");
						} else {
							await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
							await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

							try {
								await destinatarioNotificacionBcp.Validar(destinatarioNotificacion, transaction);

								await transaction.CommitAsync();
							} catch {
								await transaction.RollbackAsync();
								throw;
							}
						}
					}

					LambdaLogger.Log(
						$"[POST] - [DestinatarioNotificacion] - [Validar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Se valida exitosamente el destinatario de notificación.");

					return Results.Ok();
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
