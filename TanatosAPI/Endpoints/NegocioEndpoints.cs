using Amazon.Lambda.Core;
using System.Diagnostics;
using System.Security.Claims;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;

namespace TanatosAPI.Endpoints {
	public static class NegocioEndpoints {
		public static IEndpointRouteBuilder MapNegocioEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/Negocio");
			group.MapObtenerVigentes();
			group.MapCrearEndpoint();
			group.MapActualizarEndpoint();
			group.MapEliminarEndpoint();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes", async (IHostEnvironment environment, ClaimsPrincipal user, NegocioDao negocioDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					List<SalNegocio> retorno = [.. (await negocioDao.ObtenerPorSub(sub))
						.Select(d => new SalNegocio() {
							Id = d.Id,
							Nombre = d.Nombre,
							Direccion = d.Direccion
						})
					];

					LambdaLogger.Log(
						$"[GET] - [Negocio] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los negocios vigentes - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Negocio] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los negocios vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization().WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (EntNegocioCrear entrada, IHostEnvironment environment, ClaimsPrincipal user, NegocioDao negocioDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					entrada.Nombre = entrada.Nombre.Trim();
					entrada.Direccion = entrada.Direccion?.Trim();

					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					// Se valida que el usuario no tenga otro negocio igual...
					List<Negocio> negociosVigentes = await negocioDao.ObtenerPorSub(sub, true);
					if (negociosVigentes.Any(d => d.Sub == sub && d.Nombre == entrada.Nombre)) {
						LambdaLogger.Log(
							$"[POST] - [Negocio] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Ya tienes registrado dicho negocio.");

						return Results.BadRequest($"Ya tienes registrado dicho negocio.");
					}

					Negocio nuevo = new() {
						Id = 0,
						Sub = sub,
						Nombre = entrada.Nombre,
						Direccion = entrada.Direccion,
						FechaCreacion = DateTime.UtcNow,
						Vigencia = true
					};
					nuevo.Id = await negocioDao.Insertar(nuevo);

					SalNegocio retorno = new() {
						Id = nuevo.Id,
						Nombre = nuevo.Nombre,
						Direccion = nuevo.Direccion,
					};

					LambdaLogger.Log(
						$"[POST] - [Negocio] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa del negocio - ID: {retorno.Id}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [Negocio] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación del negocio. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization().WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (EntNegocioActualizar entrada, IHostEnvironment environment, ClaimsPrincipal user, NegocioDao negocioDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					entrada.Nombre = entrada.Nombre.Trim();
					entrada.Direccion = entrada.Direccion?.Trim();

					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					Negocio? existente = (await negocioDao.ObtenerPorSub(sub, true)).FirstOrDefault(n => n.Id == entrada.Id);

					if (existente == null) {
						LambdaLogger.Log(
							$"[PUT] - [Negocio] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No existe el negocio con ID {entrada.Id}.");

						return Results.BadRequest($"No existe el negocio con ID {entrada.Id}.");
					}

					existente.Nombre = entrada.Nombre;
					existente.Direccion = entrada.Direccion;

					await negocioDao.Actualizar(existente);

					SalNegocio retorno = new() {
						Id = existente.Id,
						Nombre = existente.Nombre,
						Direccion = existente.Direccion,
					};

					LambdaLogger.Log(
						$"[PUT] - [Negocio] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa del negocio - ID: {entrada.Id}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [Negocio] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización del negocio - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization().WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, ClaimsPrincipal user, NegocioDao negocioDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					Negocio? existente = (await negocioDao.ObtenerPorSub(sub)).FirstOrDefault(d => d.Id == id);

					if (existente == null) {
						LambdaLogger.Log(
							$"[DELETE] - [Negocio] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El usuario no posee un negocio con ID {id}.");

						return Results.BadRequest($"El usuario no posee un negocio con ID {id}.");
					}

					existente.FechaEliminacion = DateTime.UtcNow;
					existente.Vigencia = false;
					await negocioDao.Actualizar(existente);

					LambdaLogger.Log(
						$"[DELETE] - [Negocio] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del negocio - ID: {id}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [Negocio] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del negocio - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization().WithOpenApi();

			return routes;
		}
	}
}
