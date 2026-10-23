using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.CuentaFinanciera;
using TanatosAPI.Entities.Others.MovimientoFinanciero;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.UseCases;

namespace TanatosAPI.Endpoints {
	public static class MovimientoFinancieroEndpoints {
		public static void MapMovimientoFinancieroEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/MovimientoFinanciero");
			group.MapObtenerVigentes();
			group.MapCrearEndpoint();
			group.MapActualizarEndpoint();
			group.MapEliminarEndpoint();
		}

		private static void MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes/{idNegocio}/{idCuentaFinanciera}", async(long idNegocio, long idCuentaFinanciera, 
																			  [FromQuery] DateTime fechaDesde, [FromQuery] DateTime fechaHasta, 
																			  [FromQuery] DateTime? primeraFecha, [FromQuery] long? primerId, 
																			  IHostEnvironment environment, ClaimsPrincipal user, 
																			  MovimientoFinancieroUseCase movimientoFinancieroUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					(List<MovimientoFinanciero> items, (DateTime Fecha, long Id)? siguiente) = await movimientoFinancieroUseCase.ObtenerConPaginacion(
						sub, idNegocio, idCuentaFinanciera,
						fechaDesde, fechaHasta,
						primeraFecha, primerId
					);
					SalMovimientoFinancieroObtenerPorPaginacion retorno = new() { 
						Items = [.. items.Select(m => {
							return new SalMovimientoFinanciero() {
								Id = m.Id,
								IdCategoriaFinanciera = m.CategoriaFinanciera?.Id,
								NombreCategoriaFinanciera = m.CategoriaFinanciera?.Nombre,
								TipoMovimiento = m.TipoMovimiento,
								Monto = m.Monto,
								Saldo = m.Saldo,
								FechaMovimiento = m.FechaMovimiento,
								Descripcion = m.Descripcion
							};
						})],
						SiguienteFecha = siguiente?.Fecha,
						SiguienteId = siguiente?.Id,
					};

					LambdaLogger.Log(
						$"[GET] - [MovimientoFinanciero] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los movimientos financieros vigentes - Cant. Registros: {retorno.Items.Count}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [MovimientoFinanciero] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. {ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [MovimientoFinanciero] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los movimientos financieros vigentes. {ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Finanzas.Read.Self");
		}

		private static void MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (EntMovimientoFinancieroCrear entrada, IHostEnvironment environment, ClaimsPrincipal user, MovimientoFinancieroUseCase movimientoFinancieroUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					MovimientoFinanciero nuevo = await movimientoFinancieroUseCase.Crear(
						sub,
						entrada.IdCuentaFinanciera,
						entrada.IdCategoriaFinanciera,
						entrada.TipoMovimiento, 
						entrada.Monto,
						entrada.FechaMovimientoUtc, 
						entrada.Descripcion
					);

					SalMovimientoFinanciero retorno = new() {
						Id = nuevo.Id,
						IdCategoriaFinanciera = nuevo.CategoriaFinanciera?.Id,
						NombreCategoriaFinanciera = nuevo.CategoriaFinanciera?.Nombre,
						TipoMovimiento = nuevo.TipoMovimiento,
						Monto = nuevo.Monto,
						Saldo = nuevo.Saldo,
						FechaMovimiento = nuevo.FechaMovimiento,
						Descripcion = nuevo.Descripcion
					};

					LambdaLogger.Log(
						$"[POST] - [MovimientoFinanciero] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa del movimiento financiero - ID: {retorno.Id}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[POST] - [MovimientoFinanciero] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. {ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [MovimientoFinanciero] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación del movimiento financiero. {ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Finanzas.Read.Self", "Finanzas.Write.Self");
		}

		private static void MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (EntMovimientoFinancieroActualizar entrada, IHostEnvironment environment, ClaimsPrincipal user, MovimientoFinancieroUseCase movimientoFinancieroUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					MovimientoFinanciero existente = await movimientoFinancieroUseCase.Actualizar(
						sub,
						entrada.IdNegocio,
						entrada.IdCuentaFinanciera,
						entrada.IdMovimientoFinanciero,
						entrada.IdCategoriaFinanciera,
						entrada.TipoMovimiento,
						entrada.Monto,
						entrada.FechaMovimiento,
						entrada.Descripcion
					);

					SalMovimientoFinanciero retorno = new() {
						Id = existente.Id,
						IdCategoriaFinanciera = existente.CategoriaFinanciera?.Id,
						NombreCategoriaFinanciera = existente.CategoriaFinanciera?.Nombre,
						TipoMovimiento = existente.TipoMovimiento,
						Monto = existente.Monto,
						Saldo = existente.Saldo,
						FechaMovimiento = existente.FechaMovimiento,
						Descripcion = existente.Descripcion
					};

					LambdaLogger.Log(
						$"[PUT] - [MovimientoFinanciero] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa del movimiento financiero - ID: {entrada.IdMovimientoFinanciero}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[PUT] - [MovimientoFinanciero] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. {ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [MovimientoFinanciero] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización del movimiento financiero - ID: {entrada.IdMovimientoFinanciero}. {ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Finanzas.Read.Self", "Finanzas.Write.Self");
		}

		private static void MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{idNegocio}/{idCuentaFinanciera}/{idMovimientoFinanciero}", async (long idNegocio, long idCuentaFinanciera, long idMovimientoFinanciero, IHostEnvironment environment, ClaimsPrincipal user, MovimientoFinancieroUseCase movimientoFinancieroUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					await movimientoFinancieroUseCase.Eliminar(sub, idNegocio, idCuentaFinanciera, idMovimientoFinanciero);

					LambdaLogger.Log(
						$"[DELETE] - [MovimientoFinanciero] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del movimiento financiero - ID: {idMovimientoFinanciero}.");
					return Results.Ok();
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[DELETE] - [MovimientoFinanciero] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. {ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [MovimientoFinanciero] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del movimiento financiero - ID: {idMovimientoFinanciero}. {ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Finanzas.Write.Self");
		}
	}
}
