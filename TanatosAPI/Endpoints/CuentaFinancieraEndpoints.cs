using Amazon.Lambda.Core;
using System.Diagnostics;
using System.Security.Claims;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.CuentaFinanciera;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.UseCases;

namespace TanatosAPI.Endpoints {
	public static class CuentaFinancieraEndpoints {
		public static void MapCuentaFinancieraEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/CuentaFinanciera");
			group.MapObtenerVigentes();
			group.MapCrearEndpoint();
			group.MapActualizarEndpoint();
			group.MapEliminarEndpoint();
		}

		private static void MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes/{idNegocio}", async (long idNegocio, IHostEnvironment environment, ClaimsPrincipal user, CuentaFinancieraUseCase cuentaFinancieraUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					List<CuentaFinanciera> cuentas = await cuentaFinancieraUseCase.ObtenerVigentes(sub, idNegocio);
					List<SalCuentaFinanciera> retorno = [.. cuentas.Select(m => {
							return new SalCuentaFinanciera() {
								Id = m.Id,
								Nombre = m.Nombre,
								IdTipoCuentaFinanciera = m.TipoCuentaFinanciera?.Id,
								NombreTipoCuentaFinanciera = m.TipoCuentaFinanciera?.Nombre,
								SaldoActual = m.SaldoActual
							};
						})
					];

					LambdaLogger.Log(
						$"[GET] - [CuentaFinanciera] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de las cuentas financieras vigentes - Cant. Registros: {retorno.Count}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [CuentaFinanciera] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [CuentaFinanciera] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener las cuentas financieras vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Finanzas.Read.Self", "Sistema.Read.Public");
		}

		private static void MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (EntCuentaFinancieraCrear entrada, IHostEnvironment environment, ClaimsPrincipal user, CuentaFinancieraUseCase cuentaFinancieraUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					CuentaFinanciera nuevo = await cuentaFinancieraUseCase.Crear(
						sub,
						entrada.IdNegocio,
						entrada.Nombre,
						entrada.IdTipoCuentaFinanciera,
						entrada.SaldoInicial,
						entrada.FechaSaldoInicialUtc
					);

					SalCuentaFinanciera retorno = new() {
						Id = nuevo.Id,
						Nombre = nuevo.Nombre,
						IdTipoCuentaFinanciera = nuevo.TipoCuentaFinanciera?.Id,
						NombreTipoCuentaFinanciera = nuevo.TipoCuentaFinanciera?.Nombre,
						SaldoActual = nuevo.SaldoActual
					};

					LambdaLogger.Log(
						$"[POST] - [CuentaFinanciera] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa de la cuenta financiera - ID: {retorno.Id}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[POST] - [CuentaFinanciera] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [CuentaFinanciera] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación de la cuenta financiera. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Finanzas.Read.Self", "Finanzas.Write.Self", "Sistema.Read.Public");
		}

		private static void MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (EntCuentaFinancieraActualizar entrada, IHostEnvironment environment, ClaimsPrincipal user, CuentaFinancieraUseCase cuentaFinancieraUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					CuentaFinanciera existente = await cuentaFinancieraUseCase.Actualizar(
						sub,
						entrada.Id,
						entrada.Nombre,
						entrada.IdTipoCuentaFinanciera
					);

					SalCuentaFinanciera retorno = new() {
						Id = existente.Id,
						Nombre = existente.Nombre,
						IdTipoCuentaFinanciera = existente.TipoCuentaFinanciera?.Id,
						NombreTipoCuentaFinanciera = existente.TipoCuentaFinanciera?.Nombre,
						SaldoActual = existente.SaldoActual
					};

					LambdaLogger.Log(
						$"[PUT] - [CuentaFinanciera] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa de la cuenta financiera - ID: {entrada.Id}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[PUT] - [CuentaFinanciera] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [CuentaFinanciera] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización de la cuenta financiera - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Finanzas.Read.Self", "Finanzas.Write.Self", "Sistema.Read.Public");
		}

		private static void MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, ClaimsPrincipal user, CuentaFinancieraUseCase cuentaFinancieraUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					await cuentaFinancieraUseCase.Eliminar(sub, id);

					LambdaLogger.Log(
						$"[DELETE] - [CuentaFinanciera] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa de la cuenta financiera - ID: {id}.");
					return Results.Ok();
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[DELETE] - [CuentaFinanciera] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [CuentaFinanciera] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación de la cuenta financiera - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Finanzas.Write.Self");
		}
	}
}
