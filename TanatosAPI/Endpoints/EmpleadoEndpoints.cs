using Amazon.Lambda.Core;
using Npgsql;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.RegularExpressions;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
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
			routes.MapGet("/Vigentes/{idNegocio}", async (long idNegocio, IHostEnvironment environment, ClaimsPrincipal user, EmpleadoDao empleadoDao, CargoDao cargoDao, DestinatarioNotificacionDao destinatarioNotificacionDao, TipoReceptorNotificacionDao tipoReceptorNotificacionDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					List<Empleado> empleados = await empleadoDao.ObtenerPorSub(sub, idNegocio, true);
					Dictionary<long, Cargo> cargos = (await cargoDao.ObtenerPorSub(sub, idNegocio, true)).ToDictionary(c => c.Id, c => c);

					List<DestinatarioNotificacion> destinatarios = [.. (await destinatarioNotificacionDao.ObtenerPorSub(sub, idNegocio, true)).Where(d => d.IdEmpleado != null)];
					Dictionary<long, TipoReceptorNotificacion> tiposReceptores = (await tipoReceptorNotificacionDao.ObtenerPorVigencia(true)).ToDictionary(r => r.Id, r => r);

					List<SalEmpleado> retorno = [.. empleados
						.Select(e => {
							Cargo? cargo = null;
							if (e.IdCargo != null) {
								cargo = cargos.TryGetValue(e.IdCargo.Value, out Cargo? c) ? c : null;
							}

							List<SalEmpleadoDestinatario> destinatariosEmpleado = [.. destinatarios.Where(dest => dest.IdEmpleado == e.Id)
								.Select(dest => {
									TipoReceptorNotificacion? tipoReceptor = tiposReceptores.TryGetValue(dest.IdTipoReceptor, out TipoReceptorNotificacion? tr) ? tr : null;

									if (tipoReceptor == null) return null;

									return new SalEmpleadoDestinatario() {
										Id = dest.Id,
										IdTipoReceptor = tipoReceptor!.Id,
										NombreTipoReceptor = tipoReceptor!.Nombre,
										TipoReceptorRequierePlanEmpresa = tipoReceptor!.RequierePlanEmpresa,
										Destino = dest.Destino,
										Validado = dest.Validado
									};
								})
								.Where(dest => dest != null)
								.Select(dest => dest!)];

							return new SalEmpleado() {
								Id = e.Id,
								Nombre = e.Nombre,
								IdCargo = cargo?.Id,
								NombreCargo = cargo?.Nombre,
								Destinatarios = destinatariosEmpleado
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
			routes.MapPost("/", async (EntEmpleadoCrear entrada, IHostEnvironment environment, ClaimsPrincipal user, DatabaseConnectionHelper connectionHelper, SuscripcionBcp suscripcionBcp, DestinatarioNotificacionBcp destinatarioNotificacionBcp, EmpleadoDao empleadoDao, CargoDao cargoDao, NegocioDao negocioDao, TipoReceptorNotificacionDao tipoReceptorNotificacionDao, DestinatarioNotificacionDao destinatarioNotificacionDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					entrada.Nombre = entrada.Nombre.Trim();
					entrada.Destinatarios = [.. entrada.Destinatarios.Select(d => {
						d.Destino = d.Destino.Trim();
						return d;
					})];

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

					// Se valida que no vengan dos destinatarios con el mismo destino...
					if (entrada.Destinatarios.GroupBy(d => d.Destino).Any(g => g.Count() > 1)) {
						LambdaLogger.Log(
							$"[POST] - [Empleado] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Se incluyen múltiples veces el mismo destinatario de notificaciones.");

						return Results.BadRequest($"Se incluyen múltiples veces el mismo destinatario de notificaciones.");
					}

					// Se valida que si no tiene plan empresa, no se incluyan destinatarios...
					bool tienePlanEmpresa = await suscripcionBcp.TienePlanEmpresa(sub);
					if (!tienePlanEmpresa && entrada.Destinatarios.Count != 0) {
						LambdaLogger.Log(
							$"[POST] - [Empleado] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Tu plan no permite registrar a tus empleados como receptores de notificaciones.");

						return Results.BadRequest($"Tu plan no permite registrar a tus empleados como receptores de notificaciones.");
					}

					Dictionary<long, TipoReceptorNotificacion> tiposReceptores = (await tipoReceptorNotificacionDao.ObtenerPorVigencia(true)).ToDictionary(r => r.Id, r => r);
					foreach (EntEmpleadoCrearDestinatario destinatario in entrada.Destinatarios) {
						TipoReceptorNotificacion? tipoReceptor = tiposReceptores.TryGetValue(destinatario.IdTipoReceptor, out TipoReceptorNotificacion? tr) ? tr : null;

						// Se valida que el tipo de receptor sea válido...
						if (tipoReceptor == null || !tipoReceptor.Vigencia) {
							LambdaLogger.Log(
								$"[POST] - [Empleado] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
								$"Destinatario con tipo de receptor inválido.");

							return Results.BadRequest($"Destinatario con tipo de receptor inválido.");
						}

						// Se valida que el tipo de receptor seleccionado no se restringa según el plan del usuario...
						if (!tienePlanEmpresa && tipoReceptor.RequierePlanEmpresa) {
							LambdaLogger.Log(
								$"[POST] - [Empleado] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
								$"Tu plan no permite registrar un destinatario de dicho tipo.");

							return Results.BadRequest($"Tu plan no permite registrar un destinatario de dicho tipo.");
						}

						// Se valida regex del tipo de receptor...
						if (!string.IsNullOrEmpty(tipoReceptor.RegexValidacion) && !Regex.IsMatch(destinatario.Destino, tipoReceptor.RegexValidacion)) {
							LambdaLogger.Log(
								$"[POST] - [Empleado] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
								$"{tipoReceptor.Nombre} con formato inválido.");

							return Results.BadRequest($"{tipoReceptor.Nombre} con formato inválido.");
						}
					}

					List<SalEmpleadoDestinatario> nuevosDestinatarios = [];

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

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();
					try {
						nuevo.Id = await empleadoDao.Insertar(nuevo, transaction);

						foreach (EntEmpleadoCrearDestinatario destinatario in entrada.Destinatarios) {
							TipoReceptorNotificacion tipoReceptor = tiposReceptores.TryGetValue(destinatario.IdTipoReceptor, out TipoReceptorNotificacion? tr) ? tr : throw new Exception("No se encontró el tipo de receptor asociado al destinatario a crear.");

							DestinatarioNotificacion nuevoDestinatario = await destinatarioNotificacionBcp.Crear(
								sub,
								negocio.Id,
								nuevo.Id,
								destinatario.IdTipoReceptor,
								$"{tipoReceptor.Nombre} de {nuevo.Nombre}",
								destinatario.Destino,
								false,
								transaction
							);

							nuevosDestinatarios.Add(new SalEmpleadoDestinatario { 
								Id = nuevoDestinatario.Id,
								IdTipoReceptor = tipoReceptor.Id,
								NombreTipoReceptor = tipoReceptor.Nombre,
								TipoReceptorRequierePlanEmpresa = tipoReceptor.RequierePlanEmpresa,
								Destino = nuevoDestinatario.Destino,
								Validado = nuevoDestinatario.Validado
							});
						}

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					SalEmpleado retorno = new() {
						Id = nuevo.Id,
						Nombre = nuevo.Nombre,
						IdCargo = cargo.Id,
						NombreCargo = cargo.Nombre,
						Destinatarios = nuevosDestinatarios
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
			routes.MapPut("/", async (EntEmpleadoActualizar entrada, IHostEnvironment environment, ClaimsPrincipal user, DatabaseConnectionHelper connectionHelper, SuscripcionBcp suscripcionBcp, DestinatarioNotificacionBcp destinatarioNotificacionBcp, EmpleadoDao empleadoDao, CargoDao cargoDao, TipoReceptorNotificacionDao tipoReceptorNotificacionDao, DestinatarioNotificacionDao destinatarioNotificacionDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					entrada.Nombre = entrada.Nombre.Trim();
					entrada.Destinatarios = [.. entrada.Destinatarios.Select(d => {
						d.Destino = d.Destino.Trim();
						return d;
					})];

					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					#region Validaciones
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

					// Se valida que no vengan dos destinatarios con el mismo destino...
					if (entrada.Destinatarios.GroupBy(d => d.Destino).Any(g => g.Count() > 1)) {
						LambdaLogger.Log(
							$"[PUT] - [Empleado] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Se incluyen múltiples veces el mismo destinatario de notificaciones.");

						return Results.BadRequest($"Se incluyen múltiples veces el mismo destinatario de notificaciones.");
					}

					// Se valida que si no tiene plan empresa, no se incluyan destinatarios...
					bool tienePlanEmpresa = await suscripcionBcp.TienePlanEmpresa(sub);
					if (!tienePlanEmpresa && entrada.Destinatarios.Count != 0) {
						LambdaLogger.Log(
							$"[PUT] - [Empleado] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Tu plan no permite registrar a tus empleados como receptores de notificaciones.");

						return Results.BadRequest($"Tu plan no permite registrar a tus empleados como receptores de notificaciones.");
					}

					Dictionary<long, TipoReceptorNotificacion> tiposReceptores = (await tipoReceptorNotificacionDao.ObtenerPorVigencia(true)).ToDictionary(r => r.Id, r => r);
					foreach (EntEmpleadoActualizarDestinatario destinatario in entrada.Destinatarios) {
						TipoReceptorNotificacion? tipoReceptor = tiposReceptores.TryGetValue(destinatario.IdTipoReceptor, out TipoReceptorNotificacion? tr) ? tr : null;

						// Se valida que el tipo de receptor sea válido...
						if (tipoReceptor == null || !tipoReceptor.Vigencia) {
							LambdaLogger.Log(
								$"[PUT] - [Empleado] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
								$"Destinatario con tipo de receptor inválido.");

							return Results.BadRequest($"Destinatario con tipo de receptor inválido.");
						}

						// Se valida que el tipo de receptor seleccionado no se restringa según el plan del usuario...
						if (!tienePlanEmpresa && tipoReceptor.RequierePlanEmpresa) {
							LambdaLogger.Log(
								$"[PUT] - [Empleado] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
								$"Tu plan no permite registrar un destinatario de dicho tipo.");

							return Results.BadRequest($"Tu plan no permite registrar un destinatario de dicho tipo.");
						}

						// Se valida regex del tipo de receptor...
						if (!string.IsNullOrEmpty(tipoReceptor.RegexValidacion) && !Regex.IsMatch(destinatario.Destino, tipoReceptor.RegexValidacion)) {
							LambdaLogger.Log(
								$"[PUT] - [Empleado] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
								$"{tipoReceptor.Nombre} con formato inválido.");

							return Results.BadRequest($"{tipoReceptor.Nombre} con formato inválido.");
						}
					}
					#endregion

					List<SalEmpleadoDestinatario> nuevosDestinatarios = [];

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();
					try {
						if (existente.Nombre != entrada.Nombre || existente.IdCargo != entrada.IdCargo) {
							existente.Nombre = entrada.Nombre;
							existente.IdCargo = entrada.IdCargo;
							await empleadoDao.Actualizar(existente, transaction);
						}

						List<DestinatarioNotificacion> destinatariosExistentes = [.. (await destinatarioNotificacionDao.ObtenerPorSub(sub, existente.IdNegocio, true, transaction)).Where(d => d.IdEmpleado == existente.Id)];
						// Se eliminan los destinatarios existentes que no se encuentran en la entrada...
						foreach (DestinatarioNotificacion destinatario in destinatariosExistentes.Where(de => !entrada.Destinatarios.Any(d => d.IdTipoReceptor == de.IdTipoReceptor && d.Destino == de.Destino))) {
							await destinatarioNotificacionBcp.Eliminar(destinatario, transaction);
						}

						// Se insertan los destinatarios de la entrada que no se encuentran entre los existentes...
						foreach (EntEmpleadoActualizarDestinatario destinatario in entrada.Destinatarios.Where(d => !destinatariosExistentes.Any(de => de.IdTipoReceptor == d.IdTipoReceptor && de.Destino == d.Destino))) {
							TipoReceptorNotificacion tipoReceptor = tiposReceptores.TryGetValue(destinatario.IdTipoReceptor, out TipoReceptorNotificacion? tr) ? tr : throw new Exception("No se encontró el tipo de receptor asociado al destinatario a crear.");

							DestinatarioNotificacion nuevoDestinatario = await destinatarioNotificacionBcp.Crear(
								sub,
								existente.IdNegocio,
								existente.Id,
								destinatario.IdTipoReceptor,
								$"{tipoReceptor.Nombre} de {existente.Nombre}",
								destinatario.Destino,
								false,
								transaction
							);

							nuevosDestinatarios.Add(new SalEmpleadoDestinatario {
								Id = nuevoDestinatario.Id,
								IdTipoReceptor = tipoReceptor.Id,
								NombreTipoReceptor = tipoReceptor.Nombre,
								TipoReceptorRequierePlanEmpresa = tipoReceptor.RequierePlanEmpresa,
								Destino = nuevoDestinatario.Destino,
								Validado = nuevoDestinatario.Validado
							});
						}

						// Se añaden a la salida los destinatarios que se mantuvieron igual...
						foreach (DestinatarioNotificacion destinatario in destinatariosExistentes.Where(de => entrada.Destinatarios.Any(d => d.IdTipoReceptor == de.IdTipoReceptor && d.Destino == de.Destino))) {
							TipoReceptorNotificacion? tipoReceptor = tiposReceptores.TryGetValue(destinatario.IdTipoReceptor, out TipoReceptorNotificacion? tr) ? tr : null;
							
							if (tipoReceptor != null) {
								nuevosDestinatarios.Add(new SalEmpleadoDestinatario {
									Id = destinatario.Id,
									IdTipoReceptor = tipoReceptor.Id,
									NombreTipoReceptor = tipoReceptor.Nombre,
									TipoReceptorRequierePlanEmpresa = tipoReceptor.RequierePlanEmpresa,
									Destino = destinatario.Destino,
									Validado = destinatario.Validado
								});
							}
						}

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					SalEmpleado retorno = new() {
						Id = existente.Id,
						Nombre = existente.Nombre,
						IdCargo = cargo.Id,
						NombreCargo = cargo.Nombre,
						Destinatarios = nuevosDestinatarios
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
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, ClaimsPrincipal user, DatabaseConnectionHelper connectionHelper, DestinatarioNotificacionBcp destinatarioNotificacionBcp, EmpleadoDao empleadoDao, DestinatarioNotificacionDao destinatarioNotificacionDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					Empleado? existente = (await empleadoDao.ObtenerPorSub(sub, null, null)).FirstOrDefault(d => d.Id == id);
					if (existente == null) {
						LambdaLogger.Log(
							$"[DELETE] - [Empleado] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El usuario no posee un empleado con ID {id}.");

						return Results.BadRequest($"El usuario no posee un empleado con ID {id}.");
					}

					if (existente.Vigencia) {
						await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
						await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();
						
						try {
							existente.FechaEliminacion = DateTime.UtcNow;
							existente.Vigencia = false;
							await empleadoDao.Actualizar(existente, transaction);

							List<DestinatarioNotificacion> destinatariosExistentes = [.. (await destinatarioNotificacionDao.ObtenerPorSub(sub, existente.IdNegocio, true, transaction)).Where(d => d.IdEmpleado == existente.Id)];
							foreach (DestinatarioNotificacion destinatario in destinatariosExistentes) {
								await destinatarioNotificacionBcp.Eliminar(destinatario, transaction);
							}

							await transaction.CommitAsync();
						} catch {
							await transaction.RollbackAsync();
							throw;
						}
					}

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
