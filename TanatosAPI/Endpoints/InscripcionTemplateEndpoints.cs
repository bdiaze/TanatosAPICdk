using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;
using System.Diagnostics;
using System.Security.Claims;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;

namespace TanatosAPI.Endpoints {
	public static class InscripcionTemplateEndpoints {
		public static IEndpointRouteBuilder MapInscripcionTemplateEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/InscripcionTemplate");
			group.MapObtenerVigentes();
			group.MapActivarEndpoint();
			group.MapDesactivarEndpoint();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes/{idNegocio}", async (long idNegocio, IHostEnvironment environment, ClaimsPrincipal user, InscripcionTemplateDao inscripcionTemplateDao, TemplateDao templateDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					List<InscripcionTemplate> inscripciones = await inscripcionTemplateDao.ObtenerPorSub(sub, idNegocio,  true);

					List<Template> templates = [];
					if (inscripciones.Count > 0) {
						templates = await templateDao.ObtenerPorVigencia(null);
					}

					List<SalInscripcionTemplate> retorno = [];
					retorno = [.. inscripciones.Select(i => new SalInscripcionTemplate { 
						IdTemplate = i.IdTemplate,
						NombreTemplate = templates.FirstOrDefault(t => t.Id == i.IdTemplate)?.Nombre
					})];

					LambdaLogger.Log(
						$"[GET] - [InscripcionTemplate] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de las inscripciones a templates vigentes - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [InscripcionTemplate] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener las inscripciones a templates vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Obligaciones.Read.Self", "Templates.Read.Public").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapActivarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/Activar", async (EntInscripcionTemplateActivar entrada, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, ClaimsPrincipal user, InscripcionTemplateDao inscripcionTemplateDao, NormaSuscritaDao normaSuscritaDao, TemplateDao templateDao, TemplateNormaDao templateNormaDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					// Se valida que el template esté vigente...
					List<Template> templatesVigentes = await templateDao.ObtenerPorVigencia(true);
					Template? templateExistente = templatesVigentes.FirstOrDefault(t => t.Id == entrada.IdTemplate);
					if (templateExistente == null) {
						LambdaLogger.Log(
							$"[POST] - [InscripcionTemplate] - [Activar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El template es inválido.");

						return Results.BadRequest($"El template es inválido.");
					}

					// Se valida que el cliente no cuente con esa inscripción ya activa...
					List<InscripcionTemplate> inscripcionesExistentes = await inscripcionTemplateDao.ObtenerPorSub(sub, entrada.IdNegocio, null);
					InscripcionTemplate? inscripcionExistente = inscripcionesExistentes.FirstOrDefault(it => it.IdTemplate == entrada.IdTemplate);
					if (inscripcionExistente != null && inscripcionExistente.Vigencia) {
						LambdaLogger.Log(
							$"[POST] - [InscripcionTemplate] - [Activar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"La inscripción al template ya se encuentra activa.");

						return Results.BadRequest($"La inscripción al template ya se encuentra activa.");
					}

					// Se crea lista con todos los templates a los que se va a inscribir el cliente...
					List<Template> templatesAInscribir = [];
					Template? templateAuxiliar = templateExistente;
					while (templateAuxiliar != null) {
						templatesAInscribir.Add(templateAuxiliar);
						if (entrada.ActivarPadres && templateAuxiliar.IdTemplatePadre != null) {
							templateAuxiliar = templatesVigentes.FirstOrDefault(t => t.Id == templateAuxiliar.IdTemplatePadre);
						} else {
							templateAuxiliar = null;
						}
					}

					// Se obtienen todas las normas suscritas...
					List<NormaSuscrita> normasSuscritas = await normaSuscritaDao.ObtenerPorSub(sub, entrada.IdNegocio, null);

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

					try {
						foreach(Template templateAInscribir in templatesAInscribir) {
							inscripcionExistente = inscripcionesExistentes.FirstOrDefault(it => it.IdTemplate == templateAInscribir.Id);

							// Si nunca se ha registrado una inscripción, se crea el registro...
							if (inscripcionExistente == null) {
								await inscripcionTemplateDao.Insertar(new InscripcionTemplate {
									Sub = sub,
									IdNegocio = entrada.IdNegocio,
									IdTemplate = templateAInscribir.Id,
									FechaActivacion = DateTime.UtcNow,
									Vigencia = true
								}, transaction);
							// Si no, se actualiza la existente...
							} else {
								if (inscripcionExistente.Vigencia) {
									continue;
								}

								inscripcionExistente.FechaActivacion = DateTime.UtcNow;
								inscripcionExistente.FechaDesactivacion = null;
								inscripcionExistente.Vigencia = true;
								await inscripcionTemplateDao.Actualizar(inscripcionExistente, transaction);
							}

							List<TemplateNorma> templateNormas = await templateNormaDao.ObtenerPorTemplate(templateAInscribir.Id);

							// Se actualizan las normas suscritas correspondientes al template...
							foreach (NormaSuscrita normaSuscrita in normasSuscritas.Where(ns => ns.IdTemplate == templateAInscribir.Id && !ns.Vigencia)) {
								if (templateNormas.Any(tn => tn.IdTemplate == normaSuscrita.IdTemplate && tn.IdNorma == normaSuscrita.IdNorma)) {
									normaSuscrita.FechaEliminacion = null;
									normaSuscrita.Vigencia = true;
									await normaSuscritaDao.Actualizar(normaSuscrita, transaction);
								}
							}

							// Se insertan las normas suscritas que no estaban registradas...
							foreach (TemplateNorma templateNorma in templateNormas) {
								if (!normasSuscritas.Any(ns => ns.IdTemplate == templateNorma.IdTemplate && ns.IdNorma == templateNorma.IdNorma)) {
									await normaSuscritaDao.Insertar(new NormaSuscrita {
										Id = 0,
										Sub = sub,
										IdNegocio = entrada.IdNegocio,
										IdTemplate = templateNorma.IdTemplate,
										IdNorma = templateNorma.IdNorma,
										Editable = false,
										Activado = false,
										FechaCreacion = DateTime.UtcNow,
										Vigencia = true
									}, transaction);
								}
							}
						}

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					LambdaLogger.Log(
						$"[POST] - [InscripcionTemplate] - [Activar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Activación exitosa de la inscripción a template ID: {entrada.IdTemplate}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [InscripcionTemplate] - [Activar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al activar la inscripción a template ID: {entrada.IdTemplate}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Obligaciones.Write.Self").WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapDesactivarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/Desactivar", async (EntInscripcionTemplateDesactivar entrada, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, ClaimsPrincipal user, InscripcionTemplateDao inscripcionTemplateDao, NormaSuscritaDao normaSuscritaDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, TemplateDao templateDao, TemplateNormaDao templateNormaDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					// Se valida que el cliente cuente con esa inscripción ya activa...
					InscripcionTemplate? inscripcionExistente = (await inscripcionTemplateDao.ObtenerPorSub(sub, entrada.IdNegocio, true)).FirstOrDefault(it => it.IdTemplate == entrada.IdTemplate);
					if (inscripcionExistente == null) {
						LambdaLogger.Log(
							$"[POST] - [InscripcionTemplate] - [Desactivar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"La inscripción al template no se encuentra activa.");

						return Results.BadRequest($"La inscripción al template no se encuentra activa.");
					}

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

					try {
						inscripcionExistente.FechaDesactivacion = DateTime.UtcNow;
						inscripcionExistente.Vigencia = false;
						await inscripcionTemplateDao.Actualizar(inscripcionExistente, transaction);

						// Se actualizan las normas suscritas correspondientes al template...
						List<NormaSuscrita> normasSuscritas = await normaSuscritaDao.ObtenerPorSub(sub, entrada.IdNegocio, true);
						foreach (NormaSuscrita normaSuscrita in normasSuscritas.Where(ns => ns.IdTemplate == entrada.IdTemplate)) {
							normaSuscrita.FechaEliminacion = DateTime.UtcNow;
							normaSuscrita.Vigencia = false;

							if (normaSuscrita.Activado) {
								normaSuscrita.FechaDesactivacion = DateTime.UtcNow;
								normaSuscrita.Activado = false;

								// Si la norma suscrita estaba activada, se elimina su próximo vencimiento existente...
								HistorialNormaSuscrita? proximoVencimientoExistente = (await historialNormaSuscritaDao.ObtenerPorNormaSuscritaYFechaCompletitud(normaSuscrita.Id, null, true))
									.OrderBy(hns => hns.FechaVencimiento)
									.FirstOrDefault();
								
								if (proximoVencimientoExistente != null) {
									proximoVencimientoExistente.FechaEliminacion = DateTime.UtcNow;
									proximoVencimientoExistente.Vigencia = false;
									await historialNormaSuscritaDao.Actualizar(proximoVencimientoExistente, transaction);
								}
							}

							await normaSuscritaDao.Actualizar(normaSuscrita, transaction);
						}

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					LambdaLogger.Log(
						$"[POST] - [InscripcionTemplate] - [Desactivar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Desactivación exitosa de la inscripción a template ID: {entrada.IdTemplate}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [InscripcionTemplate] - [Desactivar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al desactivar la inscripción a template ID: {entrada.IdTemplate}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Obligaciones.Write.Self", "Vencimientos.Write.Self").WithOpenApi();

			return routes;
		}
	}
}
