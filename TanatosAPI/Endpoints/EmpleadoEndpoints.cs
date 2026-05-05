using Amazon.Lambda.Core;
using System.Diagnostics;
using System.Security.Claims;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Repositories;

namespace TanatosAPI.Endpoints {
	public static class EmpleadoEndpoints {
		public static IEndpointRouteBuilder MapEmpleadoEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/Empleado");
			group.MapObtenerVigentes();
			group.MapCrearEndpoint();
			group.MapActualizarEndpoint();
			group.MapEliminarEndpoint();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes/{idNegocio}", async (long idNegocio, IHostEnvironment environment, ClaimsPrincipal user, EmpleadoDao empleadoDao, CargoDao cargoDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					List<Empleado> empleados = await empleadoDao.ObtenerPorSub(sub, idNegocio, true);
					Dictionary<long, Cargo> cargos = (await cargoDao.ObtenerPorSub(sub, idNegocio, true)).ToDictionary(c => c.Id, c => c);

					List<SalEmpleado> retorno = [.. empleados
						.Select(d => {
							Cargo? cargo = null;
							if (d.IdCargo != null) {
								cargo = cargos.TryGetValue(d.IdCargo.Value, out Cargo? v) ? v : null;
							}

							return new SalEmpleado() {
								Id = d.Id,
								Nombre = d.Nombre,
								IdCargo = cargo?.Id,
								NombreCargo = cargo?.Nombre
							};
						})
					];

					LambdaLogger.Log(
						$"[GET] - [Empleado] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los empleados vigentes - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Empleado] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los empleados vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Negocios.Read.Self");

			return routes;
		}

		private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (EntEmpleadoCrear entrada, IHostEnvironment environment, ClaimsPrincipal user, EmpleadoDao empleadoDao, CargoDao cargoDao, NegocioDao negocioDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					entrada.Nombre = entrada.Nombre.Trim();

					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					// Se valida que el negocio sea válido...
					Negocio? negocio = (await negocioDao.ObtenerPorSub(sub)).FirstOrDefault(n => n.Id == entrada.IdNegocio);
					if (negocio == null || !negocio.Vigencia) {
						LambdaLogger.Log(
							$"[POST] - [Empleado] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El negocio es inválido.");

						return Results.BadRequest($"El negocio es inválido.");
					}

					// Se valida que el cargo sea válido...
					Cargo? cargo = (await cargoDao.ObtenerPorSub(sub, entrada.IdNegocio, true)).FirstOrDefault(c => c.Id == entrada.IdCargo);
					if (cargo == null || !cargo.Vigencia) {
						LambdaLogger.Log(
							$"[POST] - [Empleado] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El cargo es inválido.");

						return Results.BadRequest($"El cargo es inválido.");
					}

					Empleado nuevo = new() {
						Id = 0,
						Sub = sub,
						IdNegocio = entrada.IdNegocio,
						Nombre = entrada.Nombre,
						IdCargo = cargo.Id,
						FechaCreacion = DateTime.UtcNow,
						FechaEliminacion = null,
						Vigencia = true
					};
					nuevo.Id = await empleadoDao.Insertar(nuevo);

					SalEmpleado retorno = new() {
						Id = nuevo.Id,
						Nombre = nuevo.Nombre,
						IdCargo = cargo.Id,
						NombreCargo = cargo.Nombre
					};

					LambdaLogger.Log(
						$"[POST] - [Empleado] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa del empleado - ID: {retorno.Id}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [Empleado] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación del empleado. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Negocios.Write.Self");

			return routes;
		}

		private static IEndpointRouteBuilder MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (EntEmpleadoActualizar entrada, IHostEnvironment environment, ClaimsPrincipal user, EmpleadoDao empleadoDao, CargoDao cargoDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					entrada.Nombre = entrada.Nombre.Trim();

					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					// Se valida que el empleado exista...
					Empleado? existente = (await empleadoDao.ObtenerPorSub(sub, null, true)).FirstOrDefault(d => d.Id == entrada.Id);
					if (existente == null || !existente.Vigencia) {
						LambdaLogger.Log(
							$"[PUT] - [Empleado] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El usuario no posee un empleado con ID {entrada.Id}.");

						return Results.BadRequest($"El usuario no posee un empleado con ID {entrada.Id}.");
					}

					// Se valida que el cargo sea válido...
					Cargo? cargo = (await cargoDao.ObtenerPorSub(sub, existente.IdNegocio, true)).FirstOrDefault(c => c.Id == entrada.IdCargo);
					if (cargo == null || !cargo.Vigencia) {
						LambdaLogger.Log(
							$"[PUT] - [Empleado] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El cargo es inválido.");

						return Results.BadRequest($"El cargo es inválido.");
					}

					existente.Nombre = entrada.Nombre;
					existente.IdCargo = entrada.IdCargo;
					await empleadoDao.Actualizar(existente);

					SalEmpleado retorno = new() {
						Id = existente.Id,
						Nombre = existente.Nombre,
						IdCargo = cargo.Id,
						NombreCargo = cargo.Nombre
					};

					LambdaLogger.Log(
						$"[PUT] - [Empleado] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa del empleado - ID: {entrada.Id}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [Empleado] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización del empleado - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Negocios.Write.Self");

			return routes;
		}

		private static IEndpointRouteBuilder MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, ClaimsPrincipal user, EmpleadoDao empleadoDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					Empleado? existente = (await empleadoDao.ObtenerPorSub(sub, null, true)).FirstOrDefault(d => d.Id == id);

					if (existente == null || !existente.Vigencia) {
						LambdaLogger.Log(
							$"[DELETE] - [Empleado] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El usuario no posee un empleado con ID {id}.");

						return Results.BadRequest($"El usuario no posee un empleado con ID {id}.");
					}

					existente.FechaEliminacion = DateTime.UtcNow;
					existente.Vigencia = false;

					await empleadoDao.Actualizar(existente);

					LambdaLogger.Log(
						$"[DELETE] - [Empleado] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del empleado - ID: {id}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [Empleado] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del empleado - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Negocios.Write.Self");

			return routes;
		}
	}
}
