using Amazon.Lambda.Core;
using Cronos;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;
using System.Diagnostics;
using System.Security.Claims;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;
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
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					List<InscripcionTemplate> inscripciones = await inscripcionTemplateDao.ObtenerPorSub(sub, idNegocio,  true);

					List<Template> templates = [];
					if (inscripciones.Count > 0) {
						templates = await templateDao.ObtenerPorVigencia(null);
					}

					List<SalInscripcionTemplate> retorno = [.. inscripciones.Select(i => new SalInscripcionTemplate { 
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
			}).RequireAuthorization("Obligaciones.Read.Self", "Templates.Read.Public");

			return routes;
		}

		private static IEndpointRouteBuilder MapActivarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/Activar", async (EntInscripcionTemplateActivar entrada, IHostEnvironment environment, IDatabaseConnectionHelper connectionHelper, ClaimsPrincipal user, IDateTimeProvider dateTimeProvider, ProcesoNotificacionBcp procesoNotificacionBcp, HistorialNormaSuscritaBcp historialNormaSuscritaBcp, SuscripcionBcp suscripcionBcp, InscripcionTemplateDao inscripcionTemplateDao, NormaSuscritaDao normaSuscritaDao, TemplateDao templateDao, TemplateNormaDao templateNormaDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					// Se valida que el template esté vigente...
					List<Template> templatesVigentes = await templateDao.ObtenerPorVigencia(true);
					Template? templateExistente = templatesVigentes.FirstOrDefault(t => t.Id == entrada.IdTemplate);
					if (templateExistente == null || !templateExistente.Vigencia) {
						LambdaLogger.Log(
							$"[POST] - [InscripcionTemplate] - [Activar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El template es inválido.");

						return Results.BadRequest($"El template es inválido.");
					}
										
					// Se valida que el usuario tenga plan empresa si este template requiere de dicho plan...
					bool tienePlanEmpresa = await suscripcionBcp.TienePlanEmpresa(sub);
					if (templateExistente.RequierePlanEmpresa && !tienePlanEmpresa) {
						LambdaLogger.Log(
							$"[POST] - [InscripcionTemplate] - [Activar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Tu plan no permite inscribirte a esta plantilla.");

						return Results.BadRequest($"Tu plan no permite inscribirte a esta plantilla.");
					}

					// Se crea lista con todos los templates a los que se va a inscribir el cliente...
					List<Template> templatesAInscribir = [];
					Template? templateAuxiliar = templateExistente;
					while (templateAuxiliar != null) {
						// Solo se agregan las plantillas que no requieren plan empresa o todas si el cliente tiene plan empresa...
						if (!templateAuxiliar.RequierePlanEmpresa || tienePlanEmpresa) {
							templatesAInscribir.Add(templateAuxiliar);
						}
						if (entrada.ActivarPadres && templateAuxiliar.IdTemplatePadre != null) {
							templateAuxiliar = templatesVigentes.FirstOrDefault(t => t.Id == templateAuxiliar.IdTemplatePadre);
						} else {
							templateAuxiliar = null;
						}
					}


					List<InscripcionTemplate> inscripcionesExistentes = await inscripcionTemplateDao.ObtenerPorSub(sub, entrada.IdNegocio, null);

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

					try {
						foreach(Template templateAInscribir in templatesAInscribir) {
							InscripcionTemplate? inscripcionExistente = inscripcionesExistentes.FirstOrDefault(it => it.IdTemplate == templateAInscribir.Id);
							// Si la inscripción ya está vigente, se omite...
							if (inscripcionExistente != null && inscripcionExistente.Vigencia) {
								continue;
							}

							// Si nunca se ha registrado una inscripción, se crea el registro...
							if (inscripcionExistente == null) {
								await inscripcionTemplateDao.Insertar(new InscripcionTemplate {
									Sub = sub,
									IdNegocio = entrada.IdNegocio,
									IdTemplate = templateAInscribir.Id,
									FechaActivacion = dateTimeProvider.UtcNow,
									Vigencia = true
								}, transaction);
							// Si no, se actualiza la existente...
							} else {
								inscripcionExistente.FechaActivacion = dateTimeProvider.UtcNow;
								inscripcionExistente.FechaDesactivacion = null;
								inscripcionExistente.Vigencia = true;
								await inscripcionTemplateDao.Actualizar(inscripcionExistente, transaction);
							}

							List<TemplateNorma> templateNormas = await templateNormaDao.ObtenerPorTemplate(templateAInscribir.Id, transaction);
							
							// Se insertan las normas suscritas que no estaban registradas...
							foreach (TemplateNorma templateNorma in templateNormas) {
								NormaSuscrita normaSuscrita = new() {
									Id = 0,
									Sub = sub,
									IdNegocio = entrada.IdNegocio,
									IdTemplate = templateNorma.IdTemplate,
									IdNorma = templateNorma.IdNorma,
									Editable = false,
									Activado = false,
									FechaCreacion = dateTimeProvider.UtcNow,
									Vigencia = true
								};
								normaSuscrita.Id = await normaSuscritaDao.Insertar(normaSuscrita, transaction);

								if (!string.IsNullOrWhiteSpace(templateNorma.CronActivacionAutomatica)) {
									CronExpression cron = CronExpression.Parse(templateNorma.CronActivacionAutomatica);

									TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("America/Santiago");

									DateTime proximoVencimiento = cron.GetNextOccurrence(dateTimeProvider.UtcNow, timeZoneInfo) ?? throw new InvalidOperationException("No se pudo calcular el próximo vencimiento para obligación con activación automática");
									HistorialNormaSuscrita historialNormaSuscrita = new() {
										Id = 0,
										IdNormaSuscrita = normaSuscrita.Id,
										FechaVencimiento = proximoVencimiento,
										FechaCreacion = dateTimeProvider.UtcNow,
										Vigencia = true
									};
									await historialNormaSuscritaBcp.Crear(historialNormaSuscrita, transaction);

									normaSuscrita.FechaActivacion = dateTimeProvider.UtcNow;
									normaSuscrita.Activado = true;
									await normaSuscritaDao.Actualizar(normaSuscrita, transaction);
								}

								await procesoNotificacionBcp.ActualizarProgramacionProcesosNormaSuscrita(normaSuscrita.Id, transaction);
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
			}).RequireAuthorization("Obligaciones.Write.Self");

			return routes;
		}

		private static IEndpointRouteBuilder MapDesactivarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/Desactivar", async (EntInscripcionTemplateDesactivar entrada, IHostEnvironment environment, IDatabaseConnectionHelper connectionHelper, ClaimsPrincipal user, IDateTimeProvider dateTimeProvider, NormaSuscritaBcp normaSuscritaBcp, InscripcionTemplateDao inscripcionTemplateDao, NormaSuscritaDao normaSuscritaDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					// Se valida que el cliente cuente con esa inscripción ya activa...
					InscripcionTemplate? inscripcionExistente = (await inscripcionTemplateDao.ObtenerPorSub(sub, entrada.IdNegocio, null)).FirstOrDefault(it => it.IdTemplate == entrada.IdTemplate);
					if (inscripcionExistente == null) {
						LambdaLogger.Log(
							$"[POST] - [InscripcionTemplate] - [Desactivar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"La inscripción al template no se encuentra activa.");

						return Results.BadRequest($"La inscripción al template no se encuentra activa.");
					}

					if (inscripcionExistente.Vigencia) {
						await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
						await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

						try {
							inscripcionExistente.FechaDesactivacion = dateTimeProvider.UtcNow;
							inscripcionExistente.Vigencia = false;
							await inscripcionTemplateDao.Actualizar(inscripcionExistente, transaction);

							// Se actualizan las normas suscritas correspondientes al template...
							List<NormaSuscrita> normasSuscritas = [.. (await normaSuscritaDao.ObtenerPorSub(sub, entrada.IdNegocio, true, transaction)).Where(ns => ns.IdTemplate == entrada.IdTemplate)];
							foreach (NormaSuscrita normaSuscrita in normasSuscritas) {
								await normaSuscritaBcp.EliminarNormaSuscrita(normaSuscrita, transaction);
							}

							await transaction.CommitAsync();
						} catch {
							await transaction.RollbackAsync();
							throw;
						}
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
			}).RequireAuthorization("Obligaciones.Write.Self", "Vencimientos.Write.Self");

			return routes;
		}
	}
}
