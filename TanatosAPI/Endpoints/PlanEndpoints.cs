using Amazon.Lambda.Core;
using Microsoft.IdentityModel.Logging;
using Npgsql;
using System.Diagnostics;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;

namespace TanatosAPI.Endpoints {
	public static class PlanEndpoints {
		public static IEndpointRouteBuilder MapPlanEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/Plan");
			group.MapObtenerVigentes();
			group.MapObtenerPorVigencia();
			group.MapCrearEndpoint();
			group.MapActualizarEndpoint();
			group.MapEliminarEndpoint();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes", async (IHostEnvironment environment, PlanDao planDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<Plan> planes = await planDao.ObtenerPorVigencia(true);
					List<SalPlan> retorno = [.. planes.Select(p => new SalPlan() { 
						Id = p.Id,
						Nombre = p.Nombre,
						Precio = p.Precio,
						DuracionMeses = p.DuracionMeses,
						SuscripcionUnica = p.SuscripcionUnica,
					})];

					LambdaLogger.Log(
						$"[GET] - [Plan] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los planes vigentes - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Plan] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los planes vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Sistema.Read.Public");

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerPorVigencia(this IEndpointRouteBuilder routes) {
			routes.MapGet("/PorVigencia/{vigencia?}", async (string? vigencia, IHostEnvironment environment, PlanDao planDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					bool? vig = vigencia?.Trim().ToLowerInvariant() switch {
						"true" => true,
						"false" => false,
						_ => null
					};

					List<Plan> retorno = await planDao.ObtenerPorVigencia(vig);

					LambdaLogger.Log(
						$"[GET] - [Plan] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los planes por vigencia - Vigencia: {vigencia} - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Plan] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los planes por vigencia - Vigencia: {vigencia}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");

			return routes;
		}

		private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (EntPlanCrearEditar entrada, IHostEnvironment environment, VariableEntornoHelper variableEntorno, DatabaseConnectionHelper connectionHelper, PlanDao planDao, FlowHelper flowHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					entrada.Nombre = entrada.Nombre.Trim();

					Plan? existente = (await planDao.ObtenerPorVigencia(null)).FirstOrDefault(p => p.Id == entrada.Id);

					if (existente != null) {
						LambdaLogger.Log(
							$"[POST] - [Plan] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Ya existe un plan con ID {entrada.Id}.");

						return Results.BadRequest($"Ya existe un plan con ID {entrada.Id}.");
					}

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();
					try {
						Plan nuevo = new() {
							Id = entrada.Id,
							Nombre = entrada.Nombre,
							Precio = entrada.Precio,
							DuracionMeses = entrada.DuracionMeses,
							SuscripcionUnica = entrada.SuscripcionUnica,
							Vigencia = true
						};
						await planDao.Insertar(nuevo, transaction);

						if (entrada.Precio > 0) {
							SalFlowPlanCreate salFlowPlanCreate = await flowHelper.PlanCreate(
								$"{variableEntorno.Obtener("APP_NAME")}-{entrada.Id}-{Guid.NewGuid():N}",
								entrada.Nombre,
								entrada.Precio,
								entrada.DuracionMeses
							);
							nuevo.FlowPlanId = salFlowPlanCreate.PlanId;
							await planDao.Actualizar(nuevo, transaction);
						}

						existente = nuevo;

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					LambdaLogger.Log(
						$"[POST] - [Plan] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa del plan - ID: {entrada.Id}.");

					return Results.Ok(existente);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [Plan] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación del plan - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");

			return routes;
		}

		private static IEndpointRouteBuilder MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (EntPlanCrearEditar entrada, IHostEnvironment environment, VariableEntornoHelper variableEntorno, DatabaseConnectionHelper connectionHelper, PlanDao planDao, FlowHelper flowHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					entrada.Nombre = entrada.Nombre;

					Plan? existente = (await planDao.ObtenerPorVigencia(null)).FirstOrDefault(p => p.Id == entrada.Id);

					if (existente == null) {
						LambdaLogger.Log(
							$"[PUT] - [Plan] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No existe el plan con ID {entrada.Id}.");

						return Results.BadRequest($"No existe el plan con ID {entrada.Id}.");
					}

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();
					try {
						existente.Nombre = entrada.Nombre;
						existente.Precio = entrada.Precio;
						existente.DuracionMeses = entrada.DuracionMeses;
						existente.SuscripcionUnica = entrada.SuscripcionUnica;
						existente.Vigencia = entrada.Vigencia;
						await planDao.Actualizar(existente, transaction);

						if (existente.FlowPlanId != null) {
							if (existente.Precio > 0) {
								// Se modifica plan de flow...
								SalFlowPlanEdit salFlowPlanEdit = await flowHelper.PlanEdit(
									existente.FlowPlanId,
									existente.Nombre,
									existente.Precio,
									existente.DuracionMeses
								);
							} else {
								// Se elimina plan de flow, y se quita de tabla de Plan...
								SalFlowPlanDelete salFlowPlanDelete = await flowHelper.PlanDelete(existente.FlowPlanId);
								existente.FlowPlanId = null;
								await planDao.Actualizar(existente, transaction);
							}
						} else {
							if (existente.Precio > 0) {
								// Se crea plan en flow...
								SalFlowPlanCreate salFlowPlanCreate = await flowHelper.PlanCreate(
									$"{variableEntorno.Obtener("APP_NAME")}-{entrada.Id}-{Guid.NewGuid():N}",
									existente.Nombre,
									existente.Precio,
									existente.DuracionMeses
								);
								existente.FlowPlanId = salFlowPlanCreate.PlanId;
								await planDao.Actualizar(existente, transaction);
							}
						}

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					LambdaLogger.Log(
						$"[PUT] - [Plan] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa del plan - ID: {entrada.Id}.");

					return Results.Ok(existente);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [Plan] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización del plan - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");

			return routes;
		}

		private static IEndpointRouteBuilder MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, PlanDao planDao, FlowHelper flowHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					Plan? existente = (await planDao.ObtenerPorVigencia(null)).FirstOrDefault(p => p.Id == id);

					if (existente == null) {
						LambdaLogger.Log(
							$"[DELETE] - [Plan] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No existe el plan con ID {id}.");

						return Results.BadRequest($"No existe el plan con ID {id}.");
					}

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();
					try {
						await planDao.Eliminar(id, transaction);

						if (existente.FlowPlanId != null) {
							SalFlowPlanDelete salFlowPlanDelete = await flowHelper.PlanDelete(existente.FlowPlanId);
						}

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					LambdaLogger.Log(
						$"[DELETE] - [Plan] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del plan - ID: {id}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [Plan] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del plan - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin");

			return routes;
		}
	}
}
