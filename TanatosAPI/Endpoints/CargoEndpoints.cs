using Amazon.Lambda.Core;
using Npgsql;
using System.Diagnostics;
using System.Security.Claims;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Cargo;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;
using TanatosAPI.Repositories;
using TanatosAPI.UseCases;

namespace TanatosAPI.Endpoints {
    public static class CargoEndpoints {
        public static IEndpointRouteBuilder MapCargoEndpoints(this IEndpointRouteBuilder routes) {
            RouteGroupBuilder group = routes.MapGroup("/Cargo");
            group.MapObtenerVigentes();
            group.MapCrearEndpoint();
            group.MapActualizarEndpoint();
            group.MapEliminarEndpoint();

            return routes;
        }

        private static void MapObtenerVigentes(this IEndpointRouteBuilder routes) {
            routes.MapGet("/Vigentes/{idNegocio}", async (long idNegocio, IHostEnvironment environment, ClaimsPrincipal user, CargoUseCase cargoUseCase) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

                    List<Cargo> cargos = await cargoUseCase.ObtenerVigentes(sub, idNegocio);

                    List<SalCargo> retorno = [.. cargos
                        .Select(d => new SalCargo() {
                            Id = d.Id,
                            Nombre = d.Nombre,
                        })
                    ];

                    LambdaLogger.Log(
                        $"[GET] - [Cargo] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Obtención exitosa de los cargos vigentes - Cant. Registros: {retorno.Count}.");

                    return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[GET] - [Cargo] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[GET] - [Cargo] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al obtener los cargos vigentes. " +
                        $"{ex}");
                    return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
                }
            }).RequireAuthorization("Negocios.Read.Self");
        }

        private static void MapCrearEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPost("/", async (EntCargoCrear entrada, IHostEnvironment environment, ClaimsPrincipal user, CargoUseCase cargoUseCase) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    entrada.Nombre = entrada.Nombre.Trim();

                    string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

                    Cargo nuevo = await cargoUseCase.RegistrarCargo(sub, entrada.Nombre, entrada.IdNegocio);

                    SalCargo retorno = new() { 
                        Id = nuevo.Id,
                        Nombre = nuevo.Nombre,
                    };

                    LambdaLogger.Log(
                        $"[POST] - [Cargo] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Creación exitosa del cargo - ID: {retorno.Id}.");

                    return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[POST] - [Cargo] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[POST] - [Cargo] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error en la creación del cargo. " +
                        $"{ex}");
                    return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
                }
            }).RequireAuthorization("Negocios.Write.Self");
        }

        private static void MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPut("/", async (EntCargoActualizar entrada, IHostEnvironment environment, ClaimsPrincipal user, CargoUseCase cargoUseCase) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

                    Cargo cargo = await cargoUseCase.ActualizarCargo(sub, entrada.Id, entrada.Nombre);

                    SalCargo retorno = new() {
                        Id = cargo.Id,
                        Nombre = cargo.Nombre,
                    };

                    LambdaLogger.Log(
                        $"[PUT] - [Cargo] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Actualización exitosa del cargo - ID: {entrada.Id}.");

                    return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[PUT] - [Cargo] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[PUT] - [Cargo] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error en la actualización del cargo - ID: {entrada.Id}. " +
                        $"{ex}");
                    return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
                }
            }).RequireAuthorization("Negocios.Write.Self");
        }

        private static void MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, ClaimsPrincipal user, CargoUseCase cargoUseCase) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

                    await cargoUseCase.EliminarCargo(sub, id);

                    LambdaLogger.Log(
                        $"[DELETE] - [Cargo] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Eliminación exitosa del cargo - ID: {id}.");

                    return Results.Ok();
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[DELETE] - [Cargo] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[DELETE] - [Cargo] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error en la eliminación del cargo - ID: {id}. " +
                        $"{ex}");
                    return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
                }
            }).RequireAuthorization("Negocios.Write.Self");
        }
    }
}
