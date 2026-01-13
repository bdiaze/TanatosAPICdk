using Amazon.Lambda.Core;
using Npgsql;
using System.Diagnostics;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;

namespace TanatosAPI.Endpoints {
	public static class TemplateEndpoints {
		public static IEndpointRouteBuilder MapTemplateEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/Template");
			group.MapObtener();
			group.MapObtenerVigentes();
			group.MapObtenerVigentesConNormas();
			group.MapObtenerVigentesConNormasYRecomendacion();
			group.MapObtenerPorVigencia();
			group.MapCrearEndpoint();
			group.MapActualizarEndpoint();
			group.MapEliminarEndpoint();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtener(this IEndpointRouteBuilder routes) {
			routes.MapGet("/{id}", async (long id, IHostEnvironment environment, TemplateDao templateDao, TemplateNormaDao templateNormaDao, TemplateNormaFiscalizadorDao templateNormaFiscalizadorDao, TemplateNormaNotificacionDao templateNormaNotificacionDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					Template? retorno = await templateDao.ObtenerPorId(id);
					if (retorno == null) {
						LambdaLogger.Log(
							$"[GET] - [Template] - [Obtener] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status404NotFound}] - " +
							$"No existe el template con ID {id}.");

						return Results.NotFound($"No existe el template con ID {id}.");
					}

					retorno.TemplateNormas = await templateNormaDao.ObtenerPorTemplate(retorno.Id);
					List<TemplateNormaFiscalizador> fiscalizadores = await templateNormaFiscalizadorDao.ObtenerPorTemplateNorma(retorno.Id);
					List<TemplateNormaNotificacion> notificaciones = await templateNormaNotificacionDao.ObtenerPorTemplateNorma(retorno.Id);
					foreach (TemplateNorma norma in retorno.TemplateNormas) {
						norma.TemplateNormaFiscalizadores = [.. fiscalizadores.Where(f => f.IdTemplate == norma.IdTemplate && f.IdNorma == norma.IdNorma)];
						norma.TemplateNormaNotificaciones = [.. notificaciones.Where(n => n.IdTemplate == norma.IdTemplate && n.IdNorma == norma.IdNorma)];	
					}

					LambdaLogger.Log(
						$"[GET] - [Template] - [Obtener] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa del template con ID {id}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Template] - [Obtener] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener el template con ID {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization().WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes", async (IHostEnvironment environment, TemplateDao templateDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<Template> retorno = await templateDao.ObtenerPorVigencia(true);

					LambdaLogger.Log(
						$"[GET] - [Template] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los templates vigentes - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Template] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los templates vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization().WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerVigentesConNormas(this IEndpointRouteBuilder routes) {
			routes.MapGet("/VigentesConNormas", async (IHostEnvironment environment, TemplateDao templateDao, TemplateNormaDao templateNormaDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<Template> retorno = await templateDao.ObtenerPorVigencia(true);

					foreach (Template template in retorno) {
						template.TemplateNormas = await templateNormaDao.ObtenerPorTemplate(template.Id);
					}

					LambdaLogger.Log(
						$"[GET] - [Template] - [ObtenerVigentesConNormas] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los templates vigentes con normas - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Template] - [ObtenerVigentesConNormas] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los templates vigentes con normas. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization().WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerVigentesConNormasYRecomendacion(this IEndpointRouteBuilder routes) {
			routes.MapGet("/VigentesConNormasYRecomendacion/{idTipoActividad}", async (long idTipoActividad, IHostEnvironment environment, TemplateDao templateDao, TemplateNormaDao templateNormaDao, TemplateActividadDao templateActividadDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					List<Template> retorno = await templateDao.ObtenerPorVigencia(true);
					List<TemplateActividad> recomendacionesPorTipoActividad = await templateActividadDao.ObtenerPorActividad(idTipoActividad);

					foreach (Template template in retorno) {
						template.TemplateNormas = await templateNormaDao.ObtenerPorTemplate(template.Id);

						template.TemplateActividades = [.. recomendacionesPorTipoActividad.Where(r => r.IdTemplate == template.Id && r.IdTipoActividad == idTipoActividad)];
					}				

					LambdaLogger.Log(
						$"[GET] - [Template] - [ObtenerVigentesConNormasYRecomendacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los templates vigentes con normas y recomendación - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Template] - [ObtenerVigentesConNormasYRecomendacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los templates vigentes con normas y recomendación. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization().WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerPorVigencia(this IEndpointRouteBuilder routes) {
			routes.MapGet("/PorVigencia/{vigencia?}", async (string? vigencia, IHostEnvironment environment, TemplateDao templateDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					bool? vig = vigencia?.Trim().ToLowerInvariant() switch {
						"true" => true,
						"false" => false,
						_ => null
					};

					List<Template> retorno = await templateDao.ObtenerPorVigencia(vig);

					LambdaLogger.Log(
						$"[GET] - [Template] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los templates por vigencia - Vigencia: {vigencia} - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [Template] - [ObtenerPorVigencia] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los templates por vigencia - Vigencia: {vigencia}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (Template entrada, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, TemplateDao templateDao, TemplateNormaDao templateNormaDao, TemplateNormaFiscalizadorDao templateNormaFiscalizadorDao, TemplateNormaNotificacionDao templateNormaNotificacionDao, TemplateActividadDao templateActividadDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					// Se valida que no exista un template con el mismo ID...
					Template? existente = await templateDao.ObtenerPorId(entrada.Id);
					if (existente != null) {
						LambdaLogger.Log(
							$"[POST] - [Template] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Ya existe un template con ID {entrada.Id}.");

						return Results.BadRequest($"Ya existe un template con ID {entrada.Id}.");
					}

					// Se valida que todas las normas pertenezcan al template...
					if (entrada.TemplateNormas != null && entrada.TemplateNormas.Any(tn => tn.IdTemplate != entrada.Id)) {
						LambdaLogger.Log(
							$"[POST] - [Template] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No todas las normas pertenecen al template con ID {entrada.Id}.");

						return Results.BadRequest($"No todas las normas pertenecen al template con ID {entrada.Id}.");
					}

					// Además se valida que no existan normas con el ID duplicado...
					if (entrada.TemplateNormas != null && entrada.TemplateNormas.GroupBy(n => n.IdNorma).Any(g => g.Count() > 1)) {
						LambdaLogger.Log(
							$"[POST] - [Template] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Existen normas con el mismo ID, este debe ser único.");

						return Results.BadRequest($"Existen normas con el mismo ID, este debe ser único.");
					}

					// Se valida que todas las relaciones de fiscalizadores y notificaciones pertenezcan a sus respectivas normas...
					foreach (TemplateNorma templateNorma in entrada.TemplateNormas ?? []) {
						if (templateNorma.TemplateNormaFiscalizadores != null && templateNorma.TemplateNormaFiscalizadores.Any(tnf => tnf.IdTemplate != templateNorma.IdTemplate || tnf.IdNorma != templateNorma.IdNorma)) {
							LambdaLogger.Log(
								$"[POST] - [Template] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
								$"No todos los fiscalizadores pertenecen a la norma con ID Norma {templateNorma.IdNorma}.");

							return Results.BadRequest($"No todos los fiscalizadores pertenecen a la norma con ID Norma {templateNorma.IdNorma}.");
						}

						if (templateNorma.TemplateNormaNotificaciones != null && templateNorma.TemplateNormaNotificaciones.Any(tnn => tnn.IdTemplate != templateNorma.IdTemplate || tnn.IdNorma != templateNorma.IdNorma)) {
							LambdaLogger.Log(
								$"[POST] - [Template] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
								$"No todas las notificaciones pertenecen a la norma con ID Norma {templateNorma.IdNorma}.");

							return Results.BadRequest($"No todas las notificaciones pertenecen a la norma con ID Norma {templateNorma.IdNorma}.");
						}
					}

					// Se valida que todas las actividades pertenezcan al template...
					if (entrada.TemplateActividades != null && entrada.TemplateActividades.Any(ta => ta.IdTemplate != entrada.Id)) {
						LambdaLogger.Log(
							$"[POST] - [Template] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No todas las actividades pertenecen al template con ID {entrada.Id}.");

						return Results.BadRequest($"No todas las actividades pertenecen al template con ID {entrada.Id}.");
					}

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

					try {
						// Se graba la cabecera del template...
						await templateDao.Insertar(entrada, transaction);
						foreach (TemplateNorma templateNorma in entrada.TemplateNormas ?? []) {
							// Se graba cada norma del template...
							await templateNormaDao.Insertar(templateNorma, transaction);

							foreach (TemplateNormaFiscalizador templateNormaFiscalizador in templateNorma.TemplateNormaFiscalizadores ?? []) {
								// Se graba cada fiscalizador de la norma...
								await templateNormaFiscalizadorDao.Insertar(templateNormaFiscalizador, transaction);
							}

							foreach (TemplateNormaNotificacion templateNormaNotificacion in templateNorma.TemplateNormaNotificaciones ?? []) {
								// Se graba cada notificación de la norma...
								await templateNormaNotificacionDao.Insertar(templateNormaNotificacion, transaction);
							}
						}

						// Se graba cada tipo de actividad...
						foreach(TemplateActividad templateActividad in entrada.TemplateActividades ?? []) {
							await templateActividadDao.Insertar(templateActividad, transaction);
						}

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					existente = entrada;

					LambdaLogger.Log(
						$"[POST] - [Template] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa del template - ID: {entrada.Id}.");

					return Results.Ok(existente);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [Template] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación del template - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async (Template entrada, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, TemplateDao templateDao, TemplateNormaDao templateNormaDao, TemplateNormaFiscalizadorDao templateNormaFiscalizadorDao, TemplateNormaNotificacionDao templateNormaNotificacionDao, TemplateActividadDao templateActividadDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					// Se valida que exista el template...
					Template? existente = await templateDao.ObtenerPorId(entrada.Id);
					if (existente == null) {
						LambdaLogger.Log(
							$"[PUT] - [Template] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No existe el template con ID {entrada.Id}.");

						return Results.BadRequest($"No existe el template con ID {entrada.Id}.");
					}

					// Se valida que todas las normas pertenezcan al template...
					if (entrada.TemplateNormas != null && entrada.TemplateNormas.Any(tn => tn.IdTemplate != entrada.Id)) {
						LambdaLogger.Log(
							$"[PUT] - [Template] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No todas las normas pertenecen al template con ID {entrada.Id}.");

						return Results.BadRequest($"No todas las normas pertenecen al template con ID {entrada.Id}.");
					}

					// Además se valida que no existan normas con el ID duplicado...
					if (entrada.TemplateNormas != null && entrada.TemplateNormas.GroupBy(n => n.IdNorma).Any(g => g.Count() > 1)) {
						LambdaLogger.Log(
							$"[PUT] - [Template] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Existen normas con el mismo ID, este debe ser único.");

						return Results.BadRequest($"Existen normas con el mismo ID, este debe ser único.");
					}

					// Se valida que todas las relaciones de fiscalizadores y notificaciones pertenezcan a sus respectivas normas...
					foreach (TemplateNorma templateNormaEntrada in entrada.TemplateNormas ?? []) {
						if (templateNormaEntrada.TemplateNormaFiscalizadores != null && templateNormaEntrada.TemplateNormaFiscalizadores.Any(tnf => tnf.IdTemplate != templateNormaEntrada.IdTemplate || tnf.IdNorma != templateNormaEntrada.IdNorma)) {
							LambdaLogger.Log(
								$"[PUT] - [Template] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
								$"No todos los fiscalizadores pertenecen a la norma con ID Norma {templateNormaEntrada.IdNorma}.");

							return Results.BadRequest($"No todos los fiscalizadores pertenecen a la norma con ID Norma {templateNormaEntrada.IdNorma}.");
						}

						if (templateNormaEntrada.TemplateNormaNotificaciones != null && templateNormaEntrada.TemplateNormaNotificaciones.Any(tnn => tnn.IdTemplate != templateNormaEntrada.IdTemplate || tnn.IdNorma != templateNormaEntrada.IdNorma)) {
							LambdaLogger.Log(
								$"[PUT] - [Template] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
								$"No todas las notificaciones pertenecen a la norma con ID Norma {templateNormaEntrada.IdNorma}.");

							return Results.BadRequest($"No todas las notificaciones pertenecen a la norma con ID Norma {templateNormaEntrada.IdNorma}.");
						}
					}

					// Se valida que todas las actividades pertenezcan al template...
					if (entrada.TemplateActividades != null && entrada.TemplateActividades.Any(ta => ta.IdTemplate != entrada.Id)) {
						LambdaLogger.Log(
							$"[PUT] - [Template] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No todas las actividades pertenecen al template con ID {entrada.Id}.");

						return Results.BadRequest($"No todas las actividades pertenecen al template con ID {entrada.Id}.");
					}

					existente.TemplateNormas = await templateNormaDao.ObtenerPorTemplate(existente.Id);
					List<TemplateNormaFiscalizador> fiscalizadoresExistentes = await templateNormaFiscalizadorDao.ObtenerPorTemplateNorma(existente.Id);
					List<TemplateNormaNotificacion> notificacionesExistentes = await templateNormaNotificacionDao.ObtenerPorTemplateNorma(existente.Id);
					foreach (TemplateNorma normaExistente in existente.TemplateNormas) {
						normaExistente.TemplateNormaFiscalizadores = [.. fiscalizadoresExistentes.Where(f => f.IdTemplate == normaExistente.IdTemplate && f.IdNorma == normaExistente.IdNorma)];
						normaExistente.TemplateNormaNotificaciones = [.. notificacionesExistentes.Where(n => n.IdTemplate == normaExistente.IdTemplate && n.IdNorma == normaExistente.IdNorma)];	
					}

					existente.TemplateActividades = await templateActividadDao.ObtenerPorTemplate(existente.Id);

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

					try {
						// Se eliminan las normas que ya no existen...
						foreach(TemplateNorma normaEliminar in existente.TemplateNormas.Where(tne => (!entrada.TemplateNormas?.Any(tni => tni.IdTemplate == tne.IdTemplate && tni.IdNorma == tne.IdNorma)) ?? true)) {
							await templateNormaFiscalizadorDao.Eliminar(normaEliminar.IdTemplate, normaEliminar.IdNorma, null, transaction);
							await templateNormaNotificacionDao.Eliminar(normaEliminar.IdTemplate, normaEliminar.IdNorma, null, null, transaction);
							await templateNormaDao.Eliminar(normaEliminar.IdTemplate, normaEliminar.IdNorma, transaction);
						}

						
						foreach(TemplateNorma normaEntrada in entrada.TemplateNormas ?? []) {
							TemplateNorma? normaExistente = existente.TemplateNormas.FirstOrDefault(tn => tn.IdTemplate == normaEntrada.IdTemplate && tn.IdNorma == normaEntrada.IdNorma);

							if (normaExistente != null) {
								// Se eliminan los fiscalizadores que ya no existen...
								foreach (TemplateNormaFiscalizador fiscalizadorEliminar in normaExistente.TemplateNormaFiscalizadores?.Except(normaEntrada.TemplateNormaFiscalizadores ?? []) ?? []) {
									await templateNormaFiscalizadorDao.Eliminar(fiscalizadorEliminar.IdTemplate, fiscalizadorEliminar.IdNorma, fiscalizadorEliminar.IdTipoFiscalizador, transaction);
								}

								// Se eliminan las notificaciones que ya no existen...
								foreach (TemplateNormaNotificacion notificacionEliminar in normaExistente.TemplateNormaNotificaciones?.Except(normaEntrada.TemplateNormaNotificaciones ?? []) ?? []) {
									await templateNormaNotificacionDao.Eliminar(notificacionEliminar.IdTemplate, notificacionEliminar.IdNorma, notificacionEliminar.IdTipoUnidadTiempoAntelacion, notificacionEliminar.CantAntelacion, transaction);
								}

								// Se agregan los fiscalizadores faltantes...
								foreach (TemplateNormaFiscalizador fiscalizadorCrear in normaEntrada.TemplateNormaFiscalizadores?.Except(normaExistente.TemplateNormaFiscalizadores ?? []) ?? []) {
									await templateNormaFiscalizadorDao.Insertar(fiscalizadorCrear, transaction);
								}

								// Se agregan las notificaciones faltantes...
								foreach (TemplateNormaNotificacion notificacionCrear in normaEntrada.TemplateNormaNotificaciones?.Except(normaExistente.TemplateNormaNotificaciones ?? []) ?? []) {
									await templateNormaNotificacionDao.Insertar(notificacionCrear, transaction);
								}

								// Si existen diferencias entre la norma existente y la de entrada, se actualiza...
								if (!normaEntrada.Equals(normaExistente)) {
									await templateNormaDao.Actualizar(normaEntrada, transaction);
								}
							} else {
								// Se crea la norma que no existía...
								await templateNormaDao.Insertar(normaEntrada, transaction);

								foreach (TemplateNormaFiscalizador templateNormaFiscalizador in normaEntrada.TemplateNormaFiscalizadores ?? []) {
									// Se graba cada fiscalizador de la norma...
									await templateNormaFiscalizadorDao.Insertar(templateNormaFiscalizador, transaction);
								}
								foreach (TemplateNormaNotificacion templateNormaNotificacion in normaEntrada.TemplateNormaNotificaciones ?? []) {
									// Se graba cada notificación de la norma...
									await templateNormaNotificacionDao.Insertar(templateNormaNotificacion, transaction);
								}
							}
						}

						// Se eliminan las actividades que ya no existen...
						foreach (TemplateActividad actividadEliminar in existente.TemplateActividades.Where(ta => (!entrada.TemplateActividades?.Any(ea => ea.IdTemplate == ta.IdTemplate && ea.IdTipoActividad == ta.IdTipoActividad)) ?? true)) {
							await templateActividadDao.Eliminar(actividadEliminar.IdTemplate, actividadEliminar.IdTipoActividad, transaction);
						}

						// Se crean las nuevas actividades que no existen...
						foreach (TemplateActividad actividadCrear in entrada.TemplateActividades?.Where(ea => !existente.TemplateActividades.Any(ta => ta.IdTemplate == ea.IdTemplate && ta.IdTipoActividad == ea.IdTipoActividad)) ?? []) {
							await templateActividadDao.Insertar(actividadCrear, transaction);
						}

						// Se actualiza el template si existen diferencias...
						if (!entrada.Equals(existente)) {
							await templateDao.Actualizar(entrada, transaction);
						}

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					existente = entrada;

					LambdaLogger.Log(
						$"[PUT] - [Template] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa de la categoría de norma - ID: {entrada.Id}.");

					return Results.Ok(existente);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [Template] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización de la categoría de norma - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, TemplateDao templateDao, TemplateNormaDao templateNormaDao, TemplateNormaFiscalizadorDao templateNormaFiscalizadorDao, TemplateNormaNotificacionDao templateNormaNotificacionDao, TemplateActividadDao templateActividadDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					Template? existente = await templateDao.ObtenerPorId(id);

					if (existente == null) {
						LambdaLogger.Log(
							$"[DELETE] - [Template] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No existe el template con ID {id}.");

						return Results.BadRequest($"No existe el template con ID {id}.");
					}

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

					try {
						await templateNormaNotificacionDao.Eliminar(id, null, null, null, transaction);
						await templateNormaFiscalizadorDao.Eliminar(id, null, null, transaction);
						await templateNormaDao.Eliminar(id, null, transaction);
						await templateActividadDao.Eliminar(id, null, transaction);
						await templateDao.Eliminar(id, transaction);

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					LambdaLogger.Log(
						$"[DELETE] - [Template] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del template - ID: {id}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [Template] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del template - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Admin").WithOpenApi();

			return routes;
		}
	}
}
