using Amazon.Lambda.Core;
using System.Diagnostics;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.TipoCuentaFinanciera;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.UseCases;

namespace TanatosAPI.Endpoints {
	public static class TipoCuentaFinancieraEndpoints {
		public static void MapTipoCuentaFinancieraEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/TipoCuentaFinanciera");
			group.MapObtenerVigentes();
			group.MapObtenerTodos();
			group.MapCrear();
			group.MapActualizar();
			group.MapEliminar();
		}

		private static void MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes", async (IHostEnvironment environment, TipoCuentaFinancieraUseCase tipoCuentaFinancieraUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<TipoCuentaFinanciera> vigentes = await tipoCuentaFinancieraUseCase.ObtenerVigentes();
					List<SalTipoCuentaFinanciera> retorno = [.. vigentes.Select(v => new SalTipoCuentaFinanciera() { 
						Id = v.Id,
						Nombre = v.Nombre
					})];

					LambdaLogger.Log(
						$"[GET] - [TipoCuentaFinanciera] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los tipos de cuentas financieras vigentes - Cant. Registros: {retorno.Count}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoCuentaFinanciera] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoCuentaFinanciera] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los tipos de cuentas financieras vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Sistema.Read.Public");
		}

		private static void MapObtenerTodos(this IEndpointRouteBuilder routes) {
			routes.MapGet("/", async (IHostEnvironment environment, TipoCuentaFinancieraUseCase tipoCuentaFinancieraUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<TipoCuentaFinanciera> retorno = await tipoCuentaFinancieraUseCase.ObtenerTodos();

					LambdaLogger.Log(
						$"[GET] - [TipoCuentaFinanciera] - [ObtenerTodos] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los tipos de cuentas financieras - Cant. Registros: {retorno.Count}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoCuentaFinanciera] - [ObtenerTodos] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoCuentaFinanciera] - [ObtenerTodos] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los tipos de cuentas financieras. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Sistema.Read.All");
		}

		private static void MapCrear(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (TipoCuentaFinanciera entrada, IHostEnvironment environment, TipoCuentaFinancieraUseCase tipoCuentaFinancieraUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoCuentaFinanciera nuevo = await tipoCuentaFinancieraUseCase.Crear(entrada.Id, entrada.Nombre, entrada.Vigencia);

					LambdaLogger.Log(
						$"[POST] - [TipoCuentaFinanciera] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa del tipo de cuenta financiera - ID: {entrada.Id}.");
					return Results.Ok(nuevo);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[POST] - [TipoCuentaFinanciera] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [TipoCuentaFinanciera] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación del tipo de cuenta financiera - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Sistema.Read.All", "Sistema.Write.All");
		}

		private static void MapActualizar(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (TipoCuentaFinanciera entrada, IHostEnvironment environment, TipoCuentaFinancieraUseCase tipoCuentaFinancieraUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoCuentaFinanciera existente = await tipoCuentaFinancieraUseCase.Actualizar(entrada.Id, entrada.Nombre, entrada.Vigencia);

					LambdaLogger.Log(
						$"[PUT] - [TipoCuentaFinanciera] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa del tipo de cuenta financiera - ID: {entrada.Id}.");
					return Results.Ok(existente);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[PUT] - [TipoCuentaFinanciera] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [TipoCuentaFinanciera] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización del tipo de cuenta financiera - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Sistema.Read.All", "Sistema.Write.All");
		}

		private static void MapEliminar(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, TipoCuentaFinancieraUseCase tipoCuentaFinancieraUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					await tipoCuentaFinancieraUseCase.Eliminar(id);

					LambdaLogger.Log(
						$"[DELETE] - [TipoCuentaFinanciera] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del tipo de cuenta financiera - ID: {id}.");
					return Results.Ok();
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[DELETE] - [TipoCuentaFinanciera] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [TipoCuentaFinanciera] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del tipo de cuenta financiera - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Sistema.Write.All");
		}
	}
}
