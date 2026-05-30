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
	public static class NegocioEndpoints {
		public static IEndpointRouteBuilder MapNegocioEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/Negocio");
			group.MapObtenerInformacionUsuario();
			group.MapObtenerVigentes();
			group.MapCrearEndpoint();
			group.MapActualizarEndpoint();
			group.MapEliminarEndpoint();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerInformacionUsuario(this IEndpointRouteBuilder routes) {
			routes.MapGet("/InformacionUsuario", async (IHostEnvironment environment, ClaimsPrincipal user, SuscripcionBcp suscripcionBcp, UsuarioBcp usuarioBcp) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					Usuario usuario = await usuarioBcp.ObtenerInformacionUsuario(sub);

					SalNegocioInformacionUsuario retorno = new() {
						Nombre = usuario.Nombre,
						Apellido = usuario.Apellido,
						Email = usuario.CorreoElectronico,
						TienePlanEmpresa = await suscripcionBcp.TienePlanEmpresa(sub)
					};

					LambdaLogger.Log(
						$"[GET] - [Negocio] - [InformacionUsuario] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de la información del usuario.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Negocio] - [InformacionUsuario] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener la información del usuario. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Negocios.Read.Self");

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes", async (IHostEnvironment environment, ClaimsPrincipal user, NegocioDao negocioDao, TipoActividadDao tipoActividadDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					List<Negocio> negocios = await negocioDao.ObtenerPorSub(sub, true);

					List<TipoActividad> tiposActividades = [];
					if (negocios.Count > 0) {
						tiposActividades = await tipoActividadDao.ObtenerPorVigencia(true);
					}

					List<SalNegocio> retorno = [.. negocios
						.Select(d => {
							TipoActividad? tipoActividad = tiposActividades.FirstOrDefault(ta => ta.Id == d.IdTipoActividad);

							return new SalNegocio() {
								Id = d.Id,
								Nombre = d.Nombre,
								Direccion = d.Direccion,
								IdTipoActividad = tipoActividad?.Id,
								NombreTipoActividad = tipoActividad?.Nombre,
								FechaCreacion = d.FechaCreacion,
							}; 
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
			}).RequireAuthorization("Negocios.Read.Self", "Sistema.Read.Public");

			return routes;
		}

		private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (EntNegocioCrear entrada, IHostEnvironment environment, ClaimsPrincipal user, SuscripcionBcp suscripcionBcp, NegocioDao negocioDao, TipoActividadDao tipoActividadDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					entrada.Nombre = entrada.Nombre.Trim();
					entrada.Direccion = entrada.Direccion?.Trim();

					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					// Se valida que el usuario no tenga otro negocio igual...
					List<Negocio> negociosVigentes = await negocioDao.ObtenerPorSub(sub, true);
					if (negociosVigentes.Any(d => d.Sub == sub && d.Nombre == entrada.Nombre)) {
						LambdaLogger.Log(
							$"[POST] - [Negocio] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Ya tienes registrado dicho negocio.");

						return Results.BadRequest($"Ya tienes registrado dicho negocio.");
					}

					// Se valida que el usuario tenga plan empresa si este no es su único negocio...
					if (negociosVigentes.Count > 0 && !await suscripcionBcp.TienePlanEmpresa(sub)) {
						LambdaLogger.Log(
							$"[POST] - [Negocio] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Tu plan no permite registrar un negocio adicional.");

						return Results.BadRequest($"Tu plan no permite registrar un negocio adicional.");
					}

					// Se valida que el tipo de actividad sea válido...
					TipoActividad? tipoActividad = null;
					if (entrada.IdTipoActividad != null) {
						tipoActividad = await tipoActividadDao.ObtenerPorId(entrada.IdTipoActividad.Value);
						if (tipoActividad == null || !tipoActividad.Vigencia) {
							LambdaLogger.Log(
							$"[POST] - [Negocio] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El tipo de actividad es inválido.");

							return Results.BadRequest($"El tipo de actividad es inválido.");
						}
					}

					Negocio nuevo = new() {
						Id = 0,
						Sub = sub,
						Nombre = entrada.Nombre,
						Direccion = entrada.Direccion,
						IdTipoActividad = entrada.IdTipoActividad,
						FechaCreacion = DateTime.UtcNow,
						Vigencia = true
					};
					nuevo.Id = await negocioDao.Insertar(nuevo);

					SalNegocio retorno = new() {
						Id = nuevo.Id,
						Nombre = nuevo.Nombre,
						Direccion = nuevo.Direccion,
						IdTipoActividad = nuevo.IdTipoActividad,
						NombreTipoActividad = tipoActividad?.Nombre,
						FechaCreacion = nuevo.FechaCreacion,
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
			}).RequireAuthorization("Negocios.Write.Self", "Sistema.Read.Public");

			return routes;
		}

		private static IEndpointRouteBuilder MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (EntNegocioActualizar entrada, IHostEnvironment environment, ClaimsPrincipal user, SuscripcionBcp suscripcionBcp, NegocioDao negocioDao, TipoActividadDao tipoActividadDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					entrada.Nombre = entrada.Nombre.Trim();
					entrada.Direccion = entrada.Direccion?.Trim();

					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					// Se valida que el negocio a actualizar pertenezca al usuario y este vigente...
					List<Negocio> negociosVigentes = await negocioDao.ObtenerPorSub(sub, true);
					Negocio? existente = negociosVigentes.FirstOrDefault(n => n.Id == entrada.Id);
					if (existente == null) {
						LambdaLogger.Log(
							$"[PUT] - [Negocio] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No existe el negocio con ID {entrada.Id}.");

						return Results.BadRequest($"No existe el negocio con ID {entrada.Id}.");
					}

					// Se valida que el usuario tenga plan empresa si no esta editando su primer negocio...
					Negocio primerNegocio = negociosVigentes.OrderBy(n => n.FechaCreacion).First();
					if (primerNegocio.Id != existente!.Id && !await suscripcionBcp.TienePlanEmpresa(sub)) {
						LambdaLogger.Log(
							$"[PUT] - [Negocio] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Tu plan no permite actualizar la información de este negocio.");

						return Results.BadRequest($"Tu plan no permite actualizar la información de este negocio.");
					}

					// Se valida que el tipo de actividad sea válido...
					TipoActividad? tipoActividad = null;
					if (entrada.IdTipoActividad != null) {
						tipoActividad = await tipoActividadDao.ObtenerPorId(entrada.IdTipoActividad.Value);
						if (tipoActividad == null || !tipoActividad.Vigencia) {
							LambdaLogger.Log(
							$"[PUT] - [Negocio] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El tipo de actividad es inválido.");

							return Results.BadRequest($"El tipo de actividad es inválido.");
						}
					}

					if (existente.Nombre != entrada.Nombre || existente.Direccion != entrada.Direccion || existente.IdTipoActividad != entrada.IdTipoActividad) {
						existente.Nombre = entrada.Nombre;
						existente.Direccion = entrada.Direccion;
						existente.IdTipoActividad = entrada.IdTipoActividad;

						await negocioDao.Actualizar(existente);
					}

					SalNegocio retorno = new() {
						Id = existente.Id,
						Nombre = existente.Nombre,
						Direccion = existente.Direccion,
						IdTipoActividad = existente.IdTipoActividad,
						NombreTipoActividad = tipoActividad?.Nombre,
						FechaCreacion = existente.FechaCreacion,
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
			}).RequireAuthorization("Negocios.Write.Self", "Sistema.Read.Public");

			return routes;
		}

		private static IEndpointRouteBuilder MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, ClaimsPrincipal user, DatabaseConnectionHelper connectionHelper, NegocioBcp negocioBcp, NegocioDao negocioDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					List<Negocio> negociosVigentes = await negocioDao.ObtenerPorSub(sub, true);
					Negocio? existente = negociosVigentes.FirstOrDefault(d => d.Id == id);

					if (existente == null) {
						LambdaLogger.Log(
							$"[DELETE] - [Negocio] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El usuario no posee un negocio con ID {id}.");

						return Results.BadRequest($"El usuario no posee un negocio con ID {id}.");
					}

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();
					try {
						await negocioBcp.EliminarNegocio(existente, transaction);

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

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
			}).RequireAuthorization("Negocios.Write.Self");

			return routes;
		}
	}
}
