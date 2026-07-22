using Amazon.Lambda.Core;
using System.Diagnostics;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Endpoints {
	public static class TipoFiscalizadorEndpoints {
		public static IEndpointRouteBuilder MapTipoFiscalizadorEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/TipoFiscalizador");
			group.MapObtenerVigentes();
			group.MapObtenerPorVigencia();
			group.MapCrearEndpoint();
			group.MapActualizarEndpoint();
			group.MapEliminarEndpoint();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes", async (IHostEnvironment environment, ITipoFiscalizadorBcp tipoFiscalizadorBcp) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<TipoFiscalizador> retorno = await tipoFiscalizadorBcp.ObtenerPorVigencia(true);

					LambdaLogger.Log(
						$"[GET] - [TipoFiscalizador] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los tipos de fiscalizadores vigentes - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoFiscalizador] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los tipos de fiscalizadores vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Sistema.Read.Public");

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerPorVigencia(this IEndpointRouteBuilder routes) {
			routes.MapGet("/PorVigencia/{vigencia?}", async (string? vigencia, IHostEnvironment environment, ITipoFiscalizadorBcp tipoFiscalizadorBcp) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					bool? vig = vigencia?.Trim().ToLowerInvariant() switch {
						"true" => true,
						"false" => false,
						_ => null
					};

					List<TipoFiscalizador> retorno = await tipoFiscalizadorBcp.ObtenerPorVigencia(vig);

					LambdaLogger.Log(
						$"[GET] - [TipoFiscalizador] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los tipos de fiscalizadores por vigencia - Vigencia: {vigencia} - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [TipoFiscalizador] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los tipos de fiscalizadores por vigencia - Vigencia: {vigencia}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");

			return routes;
		}

		private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (TipoFiscalizador entrada, IHostEnvironment environment, ITipoFiscalizadorBcp tipoFiscalizadorBcp) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoFiscalizador? existente = await tipoFiscalizadorBcp.Obtener(entrada.Id);

					if (existente != null) {
						LambdaLogger.Log(
							$"[POST] - [TipoFiscalizador] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Ya existe un tipo de fiscalizador con ID {entrada.Id}.");

						return Results.BadRequest($"Ya existe un tipo de fiscalizador con ID {entrada.Id}.");
					}

                    existente = await tipoFiscalizadorBcp.Crear(
						entrada.Id,
						entrada.Nombre,
						entrada.NombreCorto,
						entrada.Vigencia
					);

					LambdaLogger.Log(
						$"[POST] - [TipoFiscalizador] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa del tipo de fiscalizador - ID: {entrada.Id}.");

					return Results.Ok(existente);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [TipoFiscalizador] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación del tipo de fiscalizador - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");

			return routes;
		}

		private static IEndpointRouteBuilder MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (TipoFiscalizador entrada, IHostEnvironment environment, ITipoFiscalizadorBcp tipoFiscalizadorBcp) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoFiscalizador? existente = await tipoFiscalizadorBcp.Obtener(entrada.Id);

					if (existente == null) {
						LambdaLogger.Log(
							$"[PUT] - [TipoFiscalizador] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No existe el tipo de fiscalizador con ID {entrada.Id}.");

						return Results.BadRequest($"No existe el tipo de fiscalizador con ID {entrada.Id}.");
					}

					await tipoFiscalizadorBcp.Actualizar(entrada);
					existente = entrada;

					LambdaLogger.Log(
						$"[PUT] - [TipoFiscalizador] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa del tipo de fiscalizador - ID: {entrada.Id}.");

					return Results.Ok(existente);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [TipoFiscalizador] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización del tipo de fiscalizador - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");

			return routes;
		}

		private static IEndpointRouteBuilder MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, ITipoFiscalizadorBcp tipoFiscalizadorBcp) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					TipoFiscalizador? existente = await tipoFiscalizadorBcp.Obtener(id);

					if (existente == null) {
						LambdaLogger.Log(
							$"[DELETE] - [TipoFiscalizador] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No existe el tipo de fiscalizador con ID {id}.");

						return Results.BadRequest($"No existe el tipo de fiscalizador con ID {id}.");
					}

					await tipoFiscalizadorBcp.Eliminar(id);

					LambdaLogger.Log(
						$"[DELETE] - [TipoFiscalizador] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del tipo de fiscalizador - ID: {id}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [TipoFiscalizador] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del tipo de fiscalizador - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");

			return routes;
		}
	}
}
