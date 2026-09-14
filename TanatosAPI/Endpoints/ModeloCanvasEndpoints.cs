using Amazon.Lambda.Core;
using System.Diagnostics;
using System.Security.Claims;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.ModeloCanvas;
using TanatosAPI.Entities.Others.NormaSuscrita;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.UseCases;

namespace TanatosAPI.Endpoints {
	public static class ModeloCanvasEndpoints {
		public static void MapModeloCanvasEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/ModeloCanvas");
			group.MapObtenerVigentes();
			group.MapCrearEndpoint();
			group.MapActualizarEndpoint();
			group.MapEliminarEndpoint();
		}

		private static void MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes/{idNegocio}", async (long idNegocio, IHostEnvironment environment, ClaimsPrincipal user, ModeloCanvasUseCase modeloCanvasUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					List<ModeloCanvas> modelos = await modeloCanvasUseCase.ObtenerVigentes(sub, idNegocio);

					List<SalModeloCanvas> retorno = [.. modelos.Select(m => {
							return new SalModeloCanvas() {
								Id = m.Id,
								SociosClave = m.SociosClave,
								ActividadesClave = m.ActividadesClave,
								RecursosClave = m.RecursosClave,
								PropuestaValor = m.PropuestaValor,
								RelacionesClientes = m.RelacionesClientes,
								Canales = m.Canales,
								SegmentosClientes = m.SegmentosClientes,
								EstructuraCostos = m.EstructuraCostos,
								FuentesIngresos = m.FuentesIngresos,
								FechaCreacion = m.FechaCreacion,
							};
						})
					];

					LambdaLogger.Log(
						$"[GET] - [ModeloCanvas] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los modelos Canvas vigentes - Cant. Registros: {retorno.Count}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [ModeloCanvas] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [ModeloCanvas] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los modelos Canvas vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Negocios.Read.Self");
		}

		private static void MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (EntModeloCanvasCrear entrada, IHostEnvironment environment, ClaimsPrincipal user, ModeloCanvasUseCase modeloCanvasUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					ModeloCanvas nuevo = await modeloCanvasUseCase.Crear(
						sub,
						entrada.IdNegocio,
						entrada.SociosClave,
						entrada.ActividadesClave,
						entrada.RecursosClave,
						entrada.PropuestaValor,
						entrada.RelacionesClientes,
						entrada.Canales,
						entrada.SegmentosClientes,
						entrada.EstructuraCostos,
						entrada.FuentesIngresos
					);

					SalModeloCanvas retorno = new() {
						Id = nuevo.Id,
						SociosClave = nuevo.SociosClave,
						ActividadesClave = nuevo.ActividadesClave,
						RecursosClave = nuevo.RecursosClave,
						PropuestaValor = nuevo.PropuestaValor,
						RelacionesClientes = nuevo.RelacionesClientes,
						Canales = nuevo.Canales,
						SegmentosClientes = nuevo.SegmentosClientes,
						EstructuraCostos = nuevo.EstructuraCostos,
						FuentesIngresos = nuevo.FuentesIngresos,
						FechaCreacion = nuevo.FechaCreacion,
					};

					LambdaLogger.Log(
						$"[POST] - [ModeloCanvas] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa del modelo Canvas - ID: {retorno.Id}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[POST] - [ModeloCanvas] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [ModeloCanvas] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación del modelo Canvas. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Negocios.Read.Self", "Negocios.Write.Self");
		}

		private static void MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (EntModeloCanvasActualizar entrada, IHostEnvironment environment, ClaimsPrincipal user, ModeloCanvasUseCase modeloCanvasUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					ModeloCanvas existente = await modeloCanvasUseCase.Actualizar(
						sub,
						entrada.Id,
						entrada.SociosClave,
						entrada.ActividadesClave,
						entrada.RecursosClave,
						entrada.PropuestaValor,
						entrada.RelacionesClientes,
						entrada.Canales,
						entrada.SegmentosClientes,
						entrada.EstructuraCostos,
						entrada.FuentesIngresos
					);

					SalModeloCanvas retorno = new() {
						Id = existente.Id,
						SociosClave = existente.SociosClave,
						ActividadesClave = existente.ActividadesClave,
						RecursosClave = existente.RecursosClave,
						PropuestaValor = existente.PropuestaValor,
						RelacionesClientes = existente.RelacionesClientes,
						Canales = existente.Canales,
						SegmentosClientes = existente.SegmentosClientes,
						EstructuraCostos = existente.EstructuraCostos,
						FuentesIngresos = existente.FuentesIngresos,
						FechaCreacion = existente.FechaCreacion,
					};

					LambdaLogger.Log(
						$"[PUT] - [ModeloCanvas] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa del modelo Canvas - ID: {entrada.Id}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[PUT] - [ModeloCanvas] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [ModeloCanvas] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización del modelo Canvas - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Negocios.Read.Self", "Negocios.Write.Self");
		}

		private static void MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, ClaimsPrincipal user, ModeloCanvasUseCase modeloCanvasUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					await modeloCanvasUseCase.Eliminar(sub, id);

					LambdaLogger.Log(
						$"[DELETE] - [ModeloCanvas] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del modelo Canvas - ID: {id}.");
					return Results.Ok();
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[DELETE] - [ModeloCanvas] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [ModeloCanvas] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del modelo Canvas - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Negocios.Write.Self");
		}
	}
}
