using Amazon.Lambda.Core;
using Npgsql;
using System.Diagnostics;
using System.Security.Claims;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;

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

        private static IEndpointRouteBuilder MapObtenerVigentes(this IEndpointRouteBuilder routes) {
            routes.MapGet("/Vigentes/{idNegocio}", async (long idNegocio, IHostEnvironment environment, ClaimsPrincipal user, CargoDao cargoDao) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

                    List<Cargo> cargos = await cargoDao.ObtenerPorSub(sub,idNegocio, true);

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
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[GET] - [Cargo] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al obtener los cargos vigentes. " +
                        $"{ex}");
                    return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
                }
            }).RequireAuthorization("Negocios.Read.Self");

            return routes;
        }

        private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPost("/", async (EntCargoCrear entrada, IHostEnvironment environment, ClaimsPrincipal user, CargoDao cargoDao, NegocioDao negocioDao) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    entrada.Nombre = entrada.Nombre.Trim();

                    string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

                    // Se valida que el negocio sea válido...
                    Negocio? negocio = (await negocioDao.ObtenerPorSub(sub)).FirstOrDefault(n => n.Id == entrada.IdNegocio);
                    if (negocio == null || !negocio.Vigencia) {
                        LambdaLogger.Log(
                            $"[POST] - [Cargo] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                            $"El negocio es inválido.");

                        return Results.BadRequest($"El negocio es inválido.");
                    }

                    Cargo nuevo = new() { 
                        Id = 0,
                        Sub = sub,
                        Nombre = entrada.Nombre,
                        IdNegocio = entrada.IdNegocio,
                        FechaCreacion = DateTime.UtcNow,
                        FechaEliminacion = null,
                        Vigencia = true
                    };
                    nuevo.Id = await cargoDao.Insertar(nuevo);

                    SalCargo retorno = new() { 
                        Id = nuevo.Id,
                        Nombre = nuevo.Nombre,
                    };

                    LambdaLogger.Log(
                        $"[POST] - [Cargo] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Creación exitosa del cargo - ID: {retorno.Id}.");

                    return Results.Ok(retorno);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[POST] - [Cargo] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error en la creación del cargo. " +
                        $"{ex}");
                    return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
                }
            }).RequireAuthorization("Negocios.Write.Self");

            return routes;
        }

        private static IEndpointRouteBuilder MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPut("/", async (EntCargoActualizar entrada, IHostEnvironment environment, ClaimsPrincipal user, CargoDao cargoDao) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    entrada.Nombre = entrada.Nombre.Trim();

                    string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

                    Cargo? existente = (await cargoDao.ObtenerPorSub(sub, null, true)).FirstOrDefault(d => d.Id == entrada.Id);
                    if (existente == null) {
                        LambdaLogger.Log(
                            $"[PUT] - [Cargo] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                            $"El usuario no posee un cargo con ID {entrada.Id}.");

                        return Results.BadRequest($"El usuario no posee un cargo con ID {entrada.Id}.");
                    }

                    existente.Nombre = entrada.Nombre;
                    await cargoDao.Actualizar(existente);

                    SalCargo retorno = new() {
                        Id = existente.Id,
                        Nombre = existente.Nombre,
                    };

                    LambdaLogger.Log(
                        $"[PUT] - [Cargo] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Actualización exitosa del cargo - ID: {entrada.Id}.");

                    return Results.Ok(retorno);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[PUT] - [Cargo] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error en la actualización del cargo - ID: {entrada.Id}. " +
                        $"{ex}");
                    return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
                }
            }).RequireAuthorization("Negocios.Write.Self");

            return routes;
        }

        private static IEndpointRouteBuilder MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, ClaimsPrincipal user, CargoDao cargoDao, EmpleadoDao empleadoDao) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					// Se valida que el cargo exista y pertenezca al usuario...
					Cargo? existente = (await cargoDao.ObtenerPorSub(sub, null, true)).FirstOrDefault(d => d.Id == id);
                    if (existente == null) {
                        LambdaLogger.Log(
                            $"[DELETE] - [Cargo] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                            $"El usuario no posee un cargo con ID {id}.");

                        return Results.BadRequest($"El usuario no posee un cargo con ID {id}.");
                    }

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

					try {
                        // Se quitan los cargos a los empleados que tenga el cargo a eliminar...
                        List<Empleado> empleados = await empleadoDao.ObtenerPorSub(sub, existente.IdNegocio, true, transaction);
                        foreach (Empleado empleado in empleados) {
                            empleado.IdCargo = null;
                            await empleadoDao.Actualizar(empleado, transaction);
						}

                        // Se elimina el cargo...
						existente.FechaEliminacion = DateTime.UtcNow;
						existente.Vigencia = false;
						await cargoDao.Actualizar(existente, transaction);

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

                    LambdaLogger.Log(
                        $"[DELETE] - [Cargo] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Eliminación exitosa del cargo - ID: {id}.");

                    return Results.Ok();
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[DELETE] - [Cargo] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error en la eliminación del cargo - ID: {id}. " +
                        $"{ex}");
                    return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
                }
            }).RequireAuthorization("Negocios.Write.Self");

            return routes;
        }
    }
}
