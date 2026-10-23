using Amazon.Lambda.Core;
using System.Diagnostics;
using System.Security.Claims;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.CategoriaFinanciera;
using TanatosAPI.Entities.Others.CuentaFinanciera;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.UseCases;

namespace TanatosAPI.Endpoints {
	public static class CategoriaFinancieraEndpoints {
		public static void MapCategoriaFinancieraEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/CategoriaFinanciera");
			group.MapObtenerVigentes();
			group.MapCrearEndpoint();
			group.MapActualizarEndpoint();
			group.MapEliminarEndpoint();
		}

		private static void MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes/{idNegocio}", async (long idNegocio, IHostEnvironment environment, ClaimsPrincipal user, CategoriaFinancieraUseCase categoriaFinancieraUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					List<CategoriaFinanciera> items = await categoriaFinancieraUseCase.ObtenerVigentes(sub, idNegocio);
					List<SalCategoriaFinanciera> retorno = [.. items.Select(m => {
							return new SalCategoriaFinanciera() {
								Id = m.Id,
								Nombre = m.Nombre,
								TipoMovimiento = m.TipoMovimiento
							};
						})
					];

					LambdaLogger.Log(
						$"[GET] - [CategoriaFinanciera] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de las categorías financieras vigentes - Cant. Registros: {retorno.Count}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [CategoriaFinanciera] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. {ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [CategoriaFinanciera] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener las categorías financieras vigentes. {ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Finanzas.Read.Self");
		}

		private static void MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (EntCategoriaFinancieraCrear entrada, IHostEnvironment environment, ClaimsPrincipal user, CategoriaFinancieraUseCase categoriaFinancieraUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					CategoriaFinanciera nuevo = await categoriaFinancieraUseCase.Crear(
						sub,
						entrada.IdNegocio,
						entrada.Nombre,
						entrada.TipoMovimiento
					);

					SalCategoriaFinanciera retorno = new() {
						Id = nuevo.Id,
						Nombre = nuevo.Nombre,
						TipoMovimiento = nuevo.TipoMovimiento
					};

					LambdaLogger.Log(
						$"[POST] - [CategoriaFinanciera] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa de la categoría financiera - ID: {retorno.Id}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[POST] - [CategoriaFinanciera] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. {ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [CategoriaFinanciera] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación de la categoría financiera. {ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Finanzas.Read.Self", "Finanzas.Write.Self");
		}

		private static void MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (EntCategoriaFinancieraActualizar entrada, IHostEnvironment environment, ClaimsPrincipal user, CategoriaFinancieraUseCase categoriaFinancieraUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					CategoriaFinanciera existente = await categoriaFinancieraUseCase.Actualizar(
						sub,
						entrada.Id,
						entrada.Nombre,
						entrada.TipoMovimiento
					);

					SalCategoriaFinanciera retorno = new() {
						Id = existente.Id,
						Nombre = existente.Nombre,
						TipoMovimiento = existente.TipoMovimiento
					};

					LambdaLogger.Log(
						$"[PUT] - [CategoriaFinanciera] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa de la categoría financiera - ID: {entrada.Id}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[PUT] - [CategoriaFinanciera] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [CategoriaFinanciera] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización de la categoría financiera - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Finanzas.Read.Self", "Finanzas.Write.Self");
		}

		private static void MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, ClaimsPrincipal user, CategoriaFinancieraUseCase categoriaFinancieraUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					await categoriaFinancieraUseCase.Eliminar(sub, id);

					LambdaLogger.Log(
						$"[DELETE] - [CategoriaFinanciera] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa de la categoría financiera - ID: {id}.");
					return Results.Ok();
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[DELETE] - [CategoriaFinanciera] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [CategoriaFinanciera] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación de la categoría financiera - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Finanzas.Write.Self");
		}
	}
}
