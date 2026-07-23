using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Diagnostics;
using System.Security.Claims;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.DocumentoAdjunto;
using TanatosAPI.Entities.Others.Kairos;
using TanatosAPI.Entities.Others.NormaSuscrita;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.UseCases;

namespace TanatosAPI.Endpoints {
	public static class NormaSuscritaEndpoints {
		public static IEndpointRouteBuilder MapNormaSuscritaEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/NormaSuscrita");
			group.MapObtenerVigentes();
			group.MapObtenerPorId();
			group.MapObtenerConVencimiento();
			group.MapObtenerPorIdConVencimiento();
			group.MapCrearEndpoint();
			group.MapActualizarEndpoint();
			group.MapCompletarNormaEndpoint();
			group.MapEliminarEndpoint();
			group.MapProcesarNotificacionEndpoint();

			RouteGroupBuilder publicGroup = routes.MapGroup("/public/NormaSuscrita");
			publicGroup.MapObtenerPorCodigoAccesoConVencimiento();
			publicGroup.MapCompletarNormaPorCodigoAccesoEndpoint();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes/{idNegocio}", async (long idNegocio, IHostEnvironment environment, ClaimsPrincipal user, NormaSuscritaUseCase normaSuscritaUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					List<NormaSuscrita> normas = await normaSuscritaUseCase.ObtenerVigentesPorSubConTemplatesYTipos(sub, idNegocio);

					List<SalNormaSuscrita> retorno = [.. normas.Select(n => {
							return new SalNormaSuscrita() {
								Id = n.Id,
								Nombre = n.Nombre,
								Descripcion = n.Descripcion,
								Multa = n.Multa,
								IdTipoPeriodicidad = n.TipoPeriodicidad?.Id,
								NombreTipoPeriodicidad = n.TipoPeriodicidad?.Nombre,
								IdCategoriaNorma = n.CategoriaNorma?.Id,
								NombreCategoriaNorma = n.CategoriaNorma?.Nombre,
								IdCargo = n.Cargo?.Id,
								NombreCargo = n.Cargo?.Nombre,
								OrdenVisual = n.OrdenVisual,
								Editable = n.Editable,
								Activado = n.Activado,
								TemplateNorma = (n.TemplateNorma == null) ? null : new SalTemplateNorma() {
									IdTemplate = n.TemplateNorma!.Template!.Id,
									NombreTemplate = n.TemplateNorma!.Template!.Nombre,
									Nombre = n.TemplateNorma!.Nombre,
									Descripcion = n.TemplateNorma!.Descripcion,
									Multa = n.TemplateNorma!.Multa,
									IdTipoPeriodicidad = n.TemplateNorma!.TipoPeriodicidad?.Id,
									NombreTipoPeriodicidad = n.TemplateNorma!.TipoPeriodicidad?.Nombre,
									IdCategoriaNorma = n.TemplateNorma!.CategoriaNorma?.Id,
									NombreCategoriaNorma = n.TemplateNorma!.CategoriaNorma?.Nombre
								}
							};
						})
					];

					LambdaLogger.Log(
						$"[GET] - [NormaSuscrita] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de las normas suscritas vigentes - Cant. Registros: {retorno.Count}.");
					return Results.Ok(retorno);
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[GET] - [NormaSuscrita] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [NormaSuscrita] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener las normas suscritas vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Obligaciones.Read.Self", "Sistema.Read.Public", "Templates.Read.Public");

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerPorId(this IEndpointRouteBuilder routes) {
			routes.MapGet("/ObtenerPorId/{idNormaSuscrita}", async (long idNormaSuscrita, IHostEnvironment environment, ClaimsPrincipal user, NormaSuscritaUseCase normaSuscritaUseCase) => {

				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					NormaSuscrita obligacion = await normaSuscritaUseCase.ObtenerConTemplateTiposFiscalizadoresNotificacionesYVencimientoValidandoVigenciaYPertenencia(
						idNormaSuscrita, 
						sub
					);

					SalNormaSuscrita retorno = new() {
						Id = obligacion.Id,
						Nombre = obligacion.Nombre,
						Descripcion = obligacion.Descripcion,
						Multa = obligacion.Multa,
						IdTipoPeriodicidad = obligacion.TipoPeriodicidad?.Id,
						NombreTipoPeriodicidad = obligacion.TipoPeriodicidad?.Nombre,
						IdCategoriaNorma = obligacion.CategoriaNorma?.Id,
						NombreCategoriaNorma = obligacion.CategoriaNorma?.Nombre,
						IdCargo = obligacion.Cargo?.Id,
						NombreCargo = obligacion.Cargo?.Nombre,
						OrdenVisual = obligacion.OrdenVisual,
						Editable = obligacion.Editable,
						Activado = obligacion.Activado,
						Fiscalizadores = [.. (obligacion.FiscalizadoresNormaSuscrita ?? []).Select(fns => {
							return new SalFiscalizadorNormaSuscrita() {
								Id = fns.Id,
								IdTipoFiscalizador = fns.TipoFiscalizador!.Id,
								NombreTipoFiscalizador = fns.TipoFiscalizador!.Nombre
							};
						})],
						Notificaciones = [.. (obligacion.NotificacionesNormaSuscrita ?? []).Select(nns => {
							return new SalNotificacionNormaSuscrita() {
								Id = nns.Id,
								IdTipoUnidadTiempoAntelacion = nns.TipoUnidadTiempo!.Id,
								NombreTipoUnidadTiempoAntelacion = nns.TipoUnidadTiempo!.Nombre,
								CantAntelacion = nns.CantAntelacion
							};
						})],
						ProximoVencimiento = obligacion.HistorialesNormaSuscrita?.FirstOrDefault()?.FechaVencimiento,
						TemplateNorma = (obligacion.TemplateNorma == null) ? null : new SalTemplateNorma() {
							IdTemplate = obligacion.TemplateNorma!.Template!.Id,
							NombreTemplate = obligacion.TemplateNorma!.Template!.Nombre,
							Nombre = obligacion.TemplateNorma!.Nombre,
							Descripcion = obligacion.TemplateNorma!.Descripcion,
							Multa = obligacion.TemplateNorma!.Multa,
							IdTipoPeriodicidad = obligacion.TemplateNorma!.TipoPeriodicidad?.Id,
							NombreTipoPeriodicidad = obligacion.TemplateNorma!.TipoPeriodicidad?.Nombre,
							IdCategoriaNorma = obligacion.TemplateNorma!.CategoriaNorma?.Id,
							NombreCategoriaNorma = obligacion.TemplateNorma!.CategoriaNorma?.Nombre,
							Fiscalizadores = [.. (obligacion.TemplateNorma!.TemplateNormaFiscalizadores ?? []).Select(fns => {
								return new SalFiscalizadorNormaSuscrita() {
									Id = 0,
									IdTipoFiscalizador = fns.TipoFiscalizador!.Id,
									NombreTipoFiscalizador = fns.TipoFiscalizador!.Nombre
								};
							})],
							Notificaciones = [.. (obligacion.TemplateNorma!.TemplateNormaNotificaciones ?? []).Select(nns => {
								return new SalNotificacionNormaSuscrita() {
									Id = 0,
									IdTipoUnidadTiempoAntelacion = nns.TipoUnidadTiempoAntelacion!.Id,
									NombreTipoUnidadTiempoAntelacion = nns.TipoUnidadTiempoAntelacion!.Nombre,
									CantAntelacion = nns.CantAntelacion
								};
							})],
						},
					};

					LambdaLogger.Log(
						$"[GET] - [NormaSuscrita] - [ObtenerPorId] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de la norma suscrita por ID: {idNormaSuscrita}.");
					return Results.Ok(retorno);
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[GET] - [NormaSuscrita] - [ObtenerPorId] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [NormaSuscrita] - [ObtenerPorId] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener la norma suscrita por ID: {idNormaSuscrita}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Obligaciones.Read.Self", "Vencimientos.Read.Self", "Sistema.Read.Public", "Templates.Read.Public");

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerConVencimiento(this IEndpointRouteBuilder routes) {
			routes.MapGet("/ObtenerConVencimiento/{idNegocio}", async (long idNegocio, IHostEnvironment environment, ClaimsPrincipal user, NormaSuscritaUseCase normaSuscritaUseCase) => {

				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					List<NormaSuscrita> normas = await normaSuscritaUseCase.ObtenerVigentesPorSubConTemplatesTiposEHistorialVencimientos(sub, idNegocio);

					List<SalNormaSuscritaObtenerConVencimiento> retorno = [.. normas.SelectMany(normaSuscrita => {
						TipoPeriodicidad? periodicidad = normaSuscrita.TipoPeriodicidad ?? normaSuscrita.TemplateNorma?.TipoPeriodicidad;
						CategoriaNorma? categoriaNorma = normaSuscrita.CategoriaNorma ?? normaSuscrita.TemplateNorma?.CategoriaNorma;

						return (normaSuscrita.HistorialesNormaSuscrita ?? []).Select(historialNormaSuscrita => new SalNormaSuscritaObtenerConVencimiento {
							FechaVencimiento = historialNormaSuscrita.FechaVencimiento,
							FechaCompletitud = historialNormaSuscrita.FechaCompletitud,
							IdNormaSuscrita = normaSuscrita.Id,
							IdHistorialNormaSuscrita = historialNormaSuscrita.Id,
							NombreNorma = normaSuscrita.Nombre ?? normaSuscrita.TemplateNorma?.Nombre,
							DescripcionNorma = normaSuscrita.Descripcion ?? normaSuscrita.TemplateNorma?.Descripcion,
							MultaNorma = normaSuscrita.Multa ?? normaSuscrita.TemplateNorma?.Multa,
							IdTipoPeriodicidad = periodicidad?.Id,
							NombreTipoPeriodicidad = periodicidad?.Nombre,
							IdCategoriaNorma = categoriaNorma?.Id,
							NombreCategoriaNorma = categoriaNorma?.NombreCorto ?? categoriaNorma?.Nombre,
							IdCargo = normaSuscrita.Cargo?.Id,
							NombreCargo = normaSuscrita.Cargo?.Nombre,
						});
					})];

					LambdaLogger.Log(
						$"[GET] - [NormaSuscrita] - [ObtenerConVencimiento] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de las normas suscritas con vencimiento por ID Negocio: {idNegocio} - Cant. Registros: {retorno.Count}.");
					return Results.Ok(retorno);
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[GET] - [NormaSuscrita] - [ObtenerConVencimiento] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [NormaSuscrita] - [ObtenerConVencimiento] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener las normas suscritas con vencimiento por ID Negocio: {idNegocio}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Obligaciones.Read.Self", "Vencimientos.Read.Self", "Sistema.Read.Public", "Templates.Read.Public");

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerPorIdConVencimiento(this IEndpointRouteBuilder routes) {
			routes.MapGet("/ObtenerPorIdConVencimiento/{idNormaSuscrita}/{idHistorialNormaSuscrita}", async (long idNormaSuscrita, long idHistorialNormaSuscrita, IHostEnvironment environment, ClaimsPrincipal user, ISuscripcionBcp suscripcionBcp, INegocioDao negocioDao, INormaSuscritaDao normaSuscritaDao, IFiscalizadorNormaSuscritaBcp fiscalizadorNormaSuscritaBcp, IHistorialNormaSuscritaDao historialNormaSuscritaDao, IDocumentoAdjuntoDao documentoAdjuntoDao, ICategoriaNormaDao categoriaNormaDao, ICargoDao cargoDao, ITipoPeriodicidadBcp tipoPeriodicidadBcp, ITipoFiscalizadorDao tipoFiscalizadorDao, ITemplateDao templateDao, ITemplateNormaDao templateNormaDao, ITemplateNormaFiscalizadorDao templateNormaFiscalizadorDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					NormaSuscrita? existente = await normaSuscritaDao.ObtenerPorId(idNormaSuscrita);
					if (existente == null || existente.Sub != sub) {
						LambdaLogger.Log(
							$"[GET] - [NormaSuscrita] - [ObtenerPorIdConVencimiento] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El ID de norma suscrita es inválido.");

						return Results.BadRequest($"El ID de norma suscrita es inválido.");
					}

					// Se valida que el negocio este vigente...
                    Negocio? negocio = (await negocioDao.ObtenerPorSub(sub)).FirstOrDefault(n => n.Id == existente.IdNegocio);
                    if (negocio == null || !negocio.Vigencia) {
                        LambdaLogger.Log(
                            $"[GET] - [NormaSuscrita] - [ObtenerPorIdConVencimiento] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                            $"El ID de norma suscrita es inválido.");

                        return Results.BadRequest($"El ID de norma suscrita es inválido.");
                    }

                    HistorialNormaSuscrita? historialExistente = await historialNormaSuscritaDao.ObtenerPorId(idHistorialNormaSuscrita);
					if (historialExistente == null || (!historialExistente.Vigencia && historialExistente.FechaCompletitud == null) || historialExistente.IdNormaSuscrita != idNormaSuscrita) {
						LambdaLogger.Log(
							$"[GET] - [NormaSuscrita] - [ObtenerPorIdConVencimiento] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El ID de norma suscrita es inválido.");

						return Results.BadRequest($"El ID de norma suscrita es inválido.");
					}

					// Solo se permite obtener el detalle de un vencimiento no completado si la norma suscrita esta vigente...
					if (!existente.Vigencia && historialExistente.FechaCompletitud == null) {
						LambdaLogger.Log(
							$"[GET] - [NormaSuscrita] - [ObtenerPorIdConVencimiento] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El ID de norma suscrita es inválido.");

						return Results.BadRequest($"El ID de norma suscrita es inválido.");
					}

					Dictionary<long, TipoPeriodicidad> periodicidades = (await tipoPeriodicidadBcp.ObtenerPorVigencia(true)).ToDictionary(p => p.Id, p => p);
                    Dictionary<long, CategoriaNorma> categorias = (await categoriaNormaDao.ObtenerPorVigencia(true)).ToDictionary(c => c.Id, c => c);

					List<FiscalizadorNormaSuscrita> fiscalizadoresNormaSuscrita = await fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(existente.Id);

                    Dictionary<long, TipoFiscalizador> fiscalizadores = [];
					if (fiscalizadoresNormaSuscrita.Count > 0) {
						fiscalizadores = (await tipoFiscalizadorDao.ObtenerPorVigencia(true)).ToDictionary(p => p.Id, p => p);
					}

					Template? template = null;
					TemplateNorma? templateNorma = null;
					List<TemplateNormaFiscalizador> templateNormaFiscalizadores = [];
					if (existente.IdTemplate != null && existente.IdNorma != null) {
						template = await templateDao.ObtenerPorId(existente.IdTemplate.Value);
						if (template != null && !template.Vigencia) template = null;

						if (template != null) {
                            // Obtengo la información del template norma...
                            templateNorma = (await templateNormaDao.ObtenerPorTemplate(existente.IdTemplate!.Value)).FirstOrDefault(tn => tn.IdNorma == existente.IdNorma);

                            // Obtengo la información de los fiscalizadores del template norma...
                            templateNormaFiscalizadores = await templateNormaFiscalizadorDao.ObtenerPorTemplateNorma(templateNorma!.IdTemplate, templateNorma!.IdNorma);
                            if (templateNormaFiscalizadores.Count > 0 && (fiscalizadores == null || fiscalizadores.Count == 0)) {
                                fiscalizadores = (await tipoFiscalizadorDao.ObtenerPorVigencia(true)).ToDictionary(p => p.Id, p => p);
                            }
                        }
					}

					List<DocumentoAdjunto> documentosAdjuntos = [.. (await documentoAdjuntoDao.ObtenerPorHistorial(historialExistente.Id, true)).Where(da => da.EstadoSubida == 1)];
                    
					Dictionary<long, Cargo> cargos = (await cargoDao.ObtenerPorSub(sub, existente.IdNegocio, true)).ToDictionary(c => c.Id, c => c);

                    TipoPeriodicidad? periodicidad = (existente.IdTipoPeriodicidad != null && periodicidades.TryGetValue(existente.IdTipoPeriodicidad.Value, out TipoPeriodicidad? pns)) ? pns : null;
                    CategoriaNorma? categoria = (existente.IdCategoriaNorma != null && categorias.TryGetValue(existente.IdCategoriaNorma.Value, out CategoriaNorma? cn)) ? cn : null;
                    Cargo? cargo = (existente.IdCargo != null && cargos.TryGetValue(existente.IdCargo.Value, out Cargo? c)) ? c : null;

                    TipoPeriodicidad? periodicidadTemplateNorma = (templateNorma?.IdTipoPeriodicidad != null && periodicidades.TryGetValue(templateNorma.IdTipoPeriodicidad.Value, out TipoPeriodicidad? ptn)) ? ptn : null;
                    CategoriaNorma? categoriaTemplateNorma = (templateNorma?.IdCategoriaNorma != null && categorias.TryGetValue(templateNorma.IdCategoriaNorma, out CategoriaNorma? ctn)) ? ctn : null;

                    SalNormaSuscritaObtenerPorIdConVencimiento retorno = new() {
						TienePlanEmpresa = await suscripcionBcp.ConsultaTienePlanEmpresa(existente.Sub),
						IdNegocio = negocio?.Id,
						NombreNegocio = negocio?.Nombre,
						Id = existente.Id,
						Nombre = existente.Nombre,
						Descripcion = existente.Descripcion,
						IdTipoPeriodicidad = periodicidad?.Id,
						NombreTipoPeriodicidad = periodicidad?.Nombre,
						Multa = existente.Multa,
						IdCategoriaNorma = categoria?.Id,
						NombreCategoriaNorma = categoria?.Nombre,
						IdCargo = cargo?.Id,
						NombreCargo = cargo?.Nombre,
						Fiscalizadores = [.. fiscalizadoresNormaSuscrita
							.Where(fns => fiscalizadores.ContainsKey(fns.IdTipoFiscalizador))
							.Select(fns => {
								TipoFiscalizador fiscalizador = fiscalizadores[fns.IdTipoFiscalizador];
								return new SalFiscalizadorNormaSuscrita() {
									Id = fns.Id,
									IdTipoFiscalizador = fiscalizador.Id,
									NombreTipoFiscalizador = fiscalizador.Nombre
								};
							})
						],
						TemplateNorma = (template == null || templateNorma == null) ? null : new SalTemplateNormaObtenerPorIdConVencimiento() {
							IdTemplate = template.Id,
							NombreTemplate = template.Nombre,
							Nombre = templateNorma.Nombre,
							Descripcion = templateNorma.Descripcion,
							IdTipoPeriodicidad = periodicidadTemplateNorma?.Id,
							NombreTipoPeriodicidad = periodicidadTemplateNorma?.Nombre,
							Multa = templateNorma.Multa,
							IdCategoriaNorma = categoriaTemplateNorma?.Id,
							NombreCategoriaNorma = categoriaTemplateNorma?.Nombre,
							Fiscalizadores = [.. templateNormaFiscalizadores.Select(fns => {
                                TipoFiscalizador? fiscalizador = fiscalizadores.TryGetValue(fns.IdTipoFiscalizador, out TipoFiscalizador? f) ? f : null;
								if (fiscalizador == null) return null;

                                return new SalFiscalizadorNormaSuscrita() {
									Id = 0,
									IdTipoFiscalizador = fiscalizador.Id,
									NombreTipoFiscalizador = fiscalizador.Nombre
								};
							}).Where(fns => fns != null).Select(fns => fns!)],
						},
						FechaVencimiento = historialExistente.FechaVencimiento,
						FechaCompletitud = historialExistente.FechaCompletitud,
						DocumentosAdjuntos = [.. documentosAdjuntos.Select(da => new SalDocumentoAdjunto() {
							Id = da.Id,
							NombreArchivo = da.NombreArchivo,
							FechaSubida = da.FechaConfirmacionSubida
						})]
					};

					LambdaLogger.Log(
						$"[GET] - [NormaSuscrita] - [ObtenerPorIdConVencimiento] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de la norma suscrita por ID {idNormaSuscrita} con vencimiento.");
					return Results.Ok(retorno);
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[GET] - [NormaSuscrita] - [ObtenerPorIdConVencimiento] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [NormaSuscrita] - [ObtenerPorIdConVencimiento] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener la norma suscrita por ID {idNormaSuscrita} con vencimiento. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Obligaciones.Read.Self", "Vencimientos.Read.Self", "Sistema.Read.Public", "Templates.Read.Public");

			return routes;
		}

		private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (EntNormaSuscritaCrear entrada, IHostEnvironment environment, ClaimsPrincipal user, NormaSuscritaUseCase normaSuscritaUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);
                    
					NormaSuscrita obligacion = await normaSuscritaUseCase.CrearNormaSuscrita(
						sub,
						entrada.IdNegocio,
						entrada.Nombre,
						entrada.Descripcion,
						entrada.Multa,
						entrada.IdTipoPeriodicidad,
						entrada.IdCategoriaNorma,
						entrada.IdCargo,
						entrada.Activado,
						entrada.ProximoVencimiento,
						[.. (entrada.Fiscalizadores ?? []).Select(f => f.IdTipoFiscalizador)],
						[.. (entrada.Notificaciones ?? []).Select(n => (n.IdTipoUnidadTiempoAntelacion, n.CantAntelacion))]
					);

                    SalNormaSuscrita retorno = new() {
                        Id = obligacion.Id,
                        Nombre = obligacion.Nombre,
                        Descripcion = obligacion.Descripcion,
                        Multa = obligacion.Multa,
                        IdTipoPeriodicidad = obligacion.TipoPeriodicidad?.Id,
                        NombreTipoPeriodicidad = obligacion.TipoPeriodicidad?.Nombre,
                        IdCategoriaNorma = obligacion.CategoriaNorma?.Id,
                        NombreCategoriaNorma = obligacion.CategoriaNorma?.Nombre,
                        IdCargo = obligacion.Cargo?.Id,
                        NombreCargo = obligacion.Cargo?.Nombre,
                        OrdenVisual = obligacion.OrdenVisual,
                        Editable = obligacion.Editable,
                        Activado = obligacion.Activado,
                        TemplateNorma = null,
                        Fiscalizadores = [.. obligacion.FiscalizadoresNormaSuscrita?.Select(fns => new SalFiscalizadorNormaSuscrita {
                            Id = fns.Id,
                            IdTipoFiscalizador = fns.IdTipoFiscalizador,
                            NombreTipoFiscalizador = fns.TipoFiscalizador?.Nombre
                        }) ?? []],
                        Notificaciones = [.. obligacion.NotificacionesNormaSuscrita?.Select(nns => new SalNotificacionNormaSuscrita {
                            Id = nns.Id,
                            IdTipoUnidadTiempoAntelacion = nns.IdTipoUnidadTiempoAntelacion,
                            NombreTipoUnidadTiempoAntelacion = nns.TipoUnidadTiempo?.Nombre,
                            CantAntelacion = nns.CantAntelacion
                        }) ?? []],
                        ProximoVencimiento = obligacion.HistorialesNormaSuscrita?.FirstOrDefault()?.FechaVencimiento
                    };

					LambdaLogger.Log(
						$"[POST] - [NormaSuscrita] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa de la norma - ID: {retorno.Id}.");
					return Results.Ok(retorno);
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[POST] - [NormaSuscrita] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [NormaSuscrita] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación de la norma. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Obligaciones.Write.Self", "Vencimientos.Write.Self", "Sistema.Read.Public");

			return routes;
		}

		private static IEndpointRouteBuilder MapActualizarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/", async(EntNormaSuscritaActualizar entrada, IHostEnvironment environment, IDatabaseConnectionHelper connectionHelper, ClaimsPrincipal user, IDateTimeProvider dateTimeProvider, HistorialNormaSuscritaUseCase historialNormaSuscritaUseCase, ISuscripcionBcp suscripcionBcp, IFiscalizadorNormaSuscritaBcp fiscalizadorNormaSuscritaBcp, INotificacionNormaSuscritaBcp notificacionNormaSuscritaBcp, IHistorialNormaSuscritaBcp historialNormaSuscritaBcp, NormaSuscritaUseCase normaSuscritaUseCase, INormaSuscritaDao normaSuscritaDao, IHistorialNormaSuscritaDao historialNormaSuscritaDao, ITipoPeriodicidadBcp tipoPeriodicidadBcp, ICargoDao cargoDao, ICategoriaNormaDao categoriaNormaDao, INegocioDao negocioDao, ITipoFiscalizadorDao tipoFiscalizadorDao, ITipoUnidadTiempoBcp tipoUnidadTiempoBcp, ITemplateNormaDao templateNormaDao, ITemplateNormaFiscalizadorDao templateNormaFiscalizadorDao, ITemplateNormaNotificacionBcp templateNormaNotificacionBcp) => {
				
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					entrada.Nombre = entrada.Nombre.Trim();
					entrada.Descripcion = string.IsNullOrWhiteSpace(entrada.Descripcion) ? null : entrada.Descripcion?.Trim();
					entrada.Multa = string.IsNullOrWhiteSpace(entrada.Multa) ? null : entrada.Multa?.Trim();

					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					// Se valida que la norma suscrita exista...
					NormaSuscrita? existente = (await normaSuscritaDao.ObtenerPorSub(sub, entrada.IdNegocio)).FirstOrDefault(n => n.Id == entrada.Id);
					if (existente == null) {
						LambdaLogger.Log(
							$"[PUT] - [NormaSuscrita] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"No existe la norma con ID {entrada.Id}.");

						return Results.BadRequest($"No existe la norma con ID {entrada.Id}.");
					}

					// Se valida que el tipo de periodicidad sea válido...
					TipoPeriodicidad? tipoPeriodicidad = null;
                    if (entrada.IdTipoPeriodicidad != null) {
						tipoPeriodicidad = await tipoPeriodicidadBcp.ObtenerPorId(entrada.IdTipoPeriodicidad.Value);
						if (tipoPeriodicidad == null || !tipoPeriodicidad.Vigencia) {
							LambdaLogger.Log(
								$"[PUT] - [NormaSuscrita] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
								$"La periodicidad es inválida.");

							return Results.BadRequest($"La periodicidad es inválida.");
						}
					}

					CategoriaNorma? categoria = null;
					// Se valida que la categoría sea válida...
					if (entrada.IdCategoriaNorma != null) {
						categoria = await categoriaNormaDao.ObtenerPorId(entrada.IdCategoriaNorma.Value);
						if (categoria == null || !categoria.Vigencia) {
							LambdaLogger.Log(
								$"[PUT] - [NormaSuscrita] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
								$"La categoría es inválida.");

							return Results.BadRequest($"La categoría es inválida.");
						}
					}

					// Se valida que el negocio sea válido...
					Negocio? negocio = (await negocioDao.ObtenerPorSub(sub)).FirstOrDefault(n => n.Id == entrada.IdNegocio);
					if (negocio == null || !negocio.Vigencia) {
						LambdaLogger.Log(
							$"[PUT] - [NormaSuscrita] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El negocio es inválido.");

						return Results.BadRequest($"El negocio es inválido.");
					}

					// Se valida que si no tiene plan empresa, no se incluya un cargo...
					bool tienePlanEmpresa = await suscripcionBcp.ConsultaTienePlanEmpresa(sub);
					if (!tienePlanEmpresa && entrada.IdCargo != null) {
						LambdaLogger.Log(
							$"[POST] - [NormaSuscrita] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Tu plan no permite asignar un cargo responsable a la obligación.");

						return Results.BadRequest($"Tu plan no permite asignar un cargo responsable a la obligación.");
					}

					// Se valida que el cargo sea válido...
					Cargo? cargo = null;
                    if (entrada.IdCargo != null) {
                        cargo = (await cargoDao.ObtenerPorSub(sub, entrada.IdNegocio, true)).FirstOrDefault(c => c.Id == entrada.IdCargo);
						if (cargo == null || !cargo.Vigencia) {
							LambdaLogger.Log(
							$"[PUT] - [NormaSuscrita] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El cargo es inválido.");

							return Results.BadRequest($"El cargo es inválido.");
						}
                    }

                    List<TipoFiscalizador> tiposFiscalizadores = [];
					// Se valida que los fiscalizadores sean válidos...
					if (entrada.Fiscalizadores != null && entrada.Fiscalizadores.Count > 0) {
						if (entrada.Fiscalizadores.GroupBy(n => n.IdTipoFiscalizador).Any(g => g.Count() > 1)) {
							LambdaLogger.Log(
								$"[PUT] - [NormaSuscrita] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
								$"Los fiscalizadores incluyen duplicados.");

							return Results.BadRequest($"Los fiscalizadores incluyen duplicados.");
						}

						tiposFiscalizadores = await tipoFiscalizadorDao.ObtenerPorVigencia(true);
						foreach (EntFiscalizadorNormaSuscritaActualizar fiscalizador in entrada.Fiscalizadores) {
							if (!tiposFiscalizadores.Any(tf => tf.Id == fiscalizador.IdTipoFiscalizador)) {
								LambdaLogger.Log(
									$"[PUT] - [NormaSuscrita] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
									$"Un fiscalizador es inválido.");

								return Results.BadRequest($"Un fiscalizador es inválido.");
							}
						}
					}

					List<TipoUnidadTiempo> tiposUnidadesTiempo = [];
					// Se valida que las notificaciones sean válidas...
					if (entrada.Notificaciones != null && entrada.Notificaciones.Count > 0) {
						if (entrada.Notificaciones.Any(n => n.CantAntelacion <= 0)) {
							LambdaLogger.Log(
								$"[PUT] - [NormaSuscrita] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
								$"Una notificación con cantidad antelación inválido.");

							return Results.BadRequest($"Una notificación con cantidad antelación inválido.");
						}

						if (entrada.Notificaciones.GroupBy(n => new { n.IdTipoUnidadTiempoAntelacion, n.CantAntelacion }).Any(g => g.Count() > 1)) {
							LambdaLogger.Log(
								$"[PUT] - [NormaSuscrita] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
								$"Las notificaciones incluyen duplicados.");

							return Results.BadRequest($"Las notificaciones incluyen duplicados.");
						}

						tiposUnidadesTiempo = await tipoUnidadTiempoBcp.ObtenerPorVigencia(true);
						foreach (EntNotificacionNormaSuscritaActualizar notificacion in entrada.Notificaciones) {
							if (!tiposUnidadesTiempo.Any(tut => tut.Id == notificacion.IdTipoUnidadTiempoAntelacion)) {
								LambdaLogger.Log(
									$"[PUT] - [NormaSuscrita] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
									$"Una notificación es inválido.");

								return Results.BadRequest($"Una notificación es inválido.");
							}
						}
					}

					// Se valida que si la norma suscrita está activa, incluya una fecha de próximo vencimiento...
					if (entrada.Activado && entrada.ProximoVencimiento == null) {
						LambdaLogger.Log(
							$"[PUT] - [NormaSuscrita] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Debe incluir la fecha de próximo vencimiento.");

						return Results.BadRequest($"Debe incluir la fecha de próximo vencimiento.");
					}

					List<HistorialNormaSuscrita> vencimientos = await historialNormaSuscritaDao.ObtenerPorNormaSuscrita(existente.Id, true);
					HistorialNormaSuscrita? proximoVencimientoExistente = vencimientos
							.OrderByDescending(hns => hns.FechaVencimiento)
							.FirstOrDefault();

					// En caso de estar modificando la fecha del próximo vencimiento, se valida que el próximo vencimiento sea una fecha futura...
					if (entrada.Activado && proximoVencimientoExistente?.FechaVencimiento != entrada.ProximoVencimiento && entrada.ProximoVencimiento <= dateTimeProvider.UtcNow) {
						LambdaLogger.Log(
							$"[PUT] - [NormaSuscrita] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El próximo vencimiento debe ser una fecha futura.");

						return Results.BadRequest($"El próximo vencimiento debe ser una fecha futura.");
					}

					// Se modifica próximo vencimiento si es una fecha pasada y según periodicidad es posible calcular un próximo vencimiento...
					if (entrada.Activado && entrada.ProximoVencimiento <= dateTimeProvider.UtcNow && tipoPeriodicidad != null && 
					   (tipoPeriodicidad.DeltaDias != null || tipoPeriodicidad.DeltaMeses != null || tipoPeriodicidad.DeltaAnnos != null)) {
						entrada.ProximoVencimiento = historialNormaSuscritaUseCase.CalcularVencimientoFuturo(
							DateTime.SpecifyKind(entrada.ProximoVencimiento!.Value, DateTimeKind.Utc),
							tipoPeriodicidad
						);
					}

					existente.Nombre = entrada.Nombre;
					existente.Descripcion = entrada.Descripcion;
					existente.IdTipoPeriodicidad = entrada.IdTipoPeriodicidad;
					existente.Multa = entrada.Multa;
					existente.IdCategoriaNorma = entrada.IdCategoriaNorma;
					existente.IdCargo = entrada.IdCargo;

                    if (existente.Activado && !entrada.Activado) {
						existente.FechaDesactivacion = dateTimeProvider.UtcNow;
						existente.Activado = false;
					} else if (!existente.Activado && entrada.Activado) {
						existente.FechaActivacion = dateTimeProvider.UtcNow;
						existente.FechaDesactivacion = null;
						existente.Activado = true;
					}

					// Se setean en null atributos que sean igual a template norma...
					if (existente.IdTemplate != null && existente.IdNorma != null) {
						TemplateNorma? templateNorma = (await templateNormaDao.ObtenerPorTemplate(existente.IdTemplate.Value)).FirstOrDefault(tn => tn.IdNorma == existente.IdNorma);

						if (templateNorma?.Nombre == existente.Nombre) {
							existente.Nombre = null;
						}

						if (templateNorma?.Descripcion == existente.Descripcion) {
							existente.Descripcion = null;
						}

						if (templateNorma?.IdTipoPeriodicidad == existente.IdTipoPeriodicidad) {
							existente.IdTipoPeriodicidad = null;
						}

						if (templateNorma?.Multa == existente.Multa) {
							existente.Multa = null;
						}

						if (templateNorma?.IdCategoriaNorma == existente.IdCategoriaNorma) {
							existente.IdCategoriaNorma = null;
						}

						// Se compararn los fiscalizadores...
						List<TemplateNormaFiscalizador> templateNormaFiscalizadores = await templateNormaFiscalizadorDao.ObtenerPorTemplateNorma(existente.IdTemplate.Value, existente.IdNorma);
						HashSet<long> setFiscalizadoresTemplate = [.. templateNormaFiscalizadores.Select(tf => tf.IdTipoFiscalizador)];
						HashSet<long> setFiscalizadoresEntrada = [.. entrada.Fiscalizadores?.Select(f => f.IdTipoFiscalizador) ?? []];

						if (setFiscalizadoresEntrada.SetEquals(setFiscalizadoresTemplate)) {
							entrada.Fiscalizadores = null;
						}

						// Se comparan las notificaciones...
						List<TemplateNormaNotificacion> templateNormaNotificaciones = await templateNormaNotificacionBcp.ObtenerPorTemplateNorma(existente.IdTemplate.Value, existente.IdNorma);
						HashSet<(long, int)> setNotificacionesTemplate = [.. templateNormaNotificaciones.Select(tn => (tn.IdTipoUnidadTiempoAntelacion, tn.CantAntelacion))];
						HashSet<(long, int)> setNotificacionesEntrada = [.. entrada.Notificaciones?.Select(n => (n.IdTipoUnidadTiempoAntelacion, n.CantAntelacion)) ?? []];

						if (setNotificacionesEntrada.SetEquals(setNotificacionesTemplate)) {
							entrada.Notificaciones = null;
						}
					}

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

					try {
						await normaSuscritaDao.Actualizar(existente, transaction);

						await fiscalizadorNormaSuscritaBcp.ActualizarPorNormaSuscrita(existente.Id, entrada.Fiscalizadores?.Select(f => f.IdTipoFiscalizador).ToHashSet() ?? [], transaction);

						await notificacionNormaSuscritaBcp.ActualizarPorNormaSuscrita(existente.Id, entrada.Notificaciones?.Select(n => (
							n.IdTipoUnidadTiempoAntelacion, 
							n.CantAntelacion
						)).ToHashSet() ?? [], transaction);

						// En caso de que norma suscrita esté activa, se agrega historial en caso de que proximo vencimiento sea distinto al existente...
						if (entrada.Activado) {
							if (proximoVencimientoExistente?.FechaVencimiento != entrada.ProximoVencimiento) {
								if (proximoVencimientoExistente != null) {
									await historialNormaSuscritaUseCase.EliminarPorNormaSuscrita(existente.Id, true, transaction);
								}

								_ = await historialNormaSuscritaBcp.Crear(existente.Id, entrada.ProximoVencimiento!.Value, transaction);
							}
						// En caso de que norma suscrita esté inactiva, se elimina el próximo vencimiento existente...
						} else {
							if (proximoVencimientoExistente != null) {
								await historialNormaSuscritaUseCase.EliminarPorNormaSuscrita(existente.Id, false, transaction);
							}
						}

						await normaSuscritaUseCase.ActualizarProgramacionProcesosNormaSuscrita(existente.Id, transaction);

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					List<FiscalizadorNormaSuscrita>? fiscalizadoresExistentes = await fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(existente.Id);
					if (fiscalizadoresExistentes.Count == 0) {
						fiscalizadoresExistentes = null;
					}
					List<NotificacionNormaSuscrita>? notificacionesExistentes = await notificacionNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(existente.Id);
					if (notificacionesExistentes.Count == 0) {
						notificacionesExistentes = null;
					}

					SalNormaSuscrita retorno = new() {
						Id = existente.Id,
						Nombre = existente.Nombre,
						Descripcion = existente.Descripcion,
						IdTipoPeriodicidad = existente.IdTipoPeriodicidad,
						NombreTipoPeriodicidad = existente.IdTipoPeriodicidad == null ? null : tipoPeriodicidad?.Nombre,
						Multa = existente.Multa,
						IdCategoriaNorma = existente.IdCategoriaNorma,
						NombreCategoriaNorma = existente.IdCategoriaNorma == null ? null : categoria?.Nombre,
						IdCargo = cargo?.Id,
						NombreCargo = cargo?.Nombre,
						OrdenVisual = existente.OrdenVisual,
						Editable = existente.Editable,
						Activado = existente.Activado,
						TemplateNorma = null,
						Fiscalizadores = fiscalizadoresExistentes == null ? null : [.. fiscalizadoresExistentes.Select(fns => new SalFiscalizadorNormaSuscrita {
							Id = fns.Id,
							IdTipoFiscalizador = fns.IdTipoFiscalizador,
							NombreTipoFiscalizador = tiposFiscalizadores.FirstOrDefault(tp => tp.Id == fns.IdTipoFiscalizador)?.Nombre
						})],
						Notificaciones = notificacionesExistentes == null ? null : [.. notificacionesExistentes.Select(nns => new SalNotificacionNormaSuscrita {
							Id = nns.Id,
							IdTipoUnidadTiempoAntelacion = nns.IdTipoUnidadTiempoAntelacion,
							NombreTipoUnidadTiempoAntelacion = tiposUnidadesTiempo.FirstOrDefault(tut => tut.Id == nns.IdTipoUnidadTiempoAntelacion)?.Nombre,
							CantAntelacion = nns.CantAntelacion
						})],
						ProximoVencimiento = entrada.Activado ? entrada.ProximoVencimiento : null
					};

					LambdaLogger.Log(
						$"[PUT] - [NormaSuscrita] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Actualización exitosa de la norma suscrita - ID: {entrada.Id}.");
					return Results.Ok(retorno);
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[PUT] - [NormaSuscrita] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [NormaSuscrita] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la actualización de la norma suscrita - ID: {entrada.Id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Obligaciones.Write.Self", "Vencimientos.Write.Self", "Sistema.Read.Public");

			return routes;
		}

		private static IEndpointRouteBuilder MapCompletarNormaEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/CompletarNorma", async (EntNormaSuscritaCompletarNorma entrada, IHostEnvironment environment, ClaimsPrincipal user, NormaSuscritaUseCase normaSuscritaUseCase) => {

				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

                    HistorialNormaSuscrita vencimiento = await normaSuscritaUseCase.CompletarNormaValidandoPertenencia(sub, entrada.IdNormaSuscrita, entrada.IdHistorialNormaSuscrita);

					SalNormaSuscritaCompletarNorma retorno = new() {
						FechaCompletitud = vencimiento.FechaCompletitud
					};

					LambdaLogger.Log(
						$"[PUT] - [NormaSuscrita] - [CompletarNorma] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Se da por completada exitosamente la norma suscrita - ID Norma Suscrita: {vencimiento.IdNormaSuscrita} - ID Historial Norma Suscrita: {vencimiento.Id}.");
					return Results.Ok(retorno);
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[PUT] - [NormaSuscrita] - [CompletarNorma] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [NormaSuscrita] - [CompletarNorma] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al dar por completada la norma suscrita - ID Norma Suscrita: {entrada.IdNormaSuscrita} - ID Historial Norma Suscrita: {entrada.IdHistorialNormaSuscrita}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Obligaciones.Write.Self", "Vencimientos.Write.Self");

			return routes;
		}

		private static IEndpointRouteBuilder MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, ClaimsPrincipal user, NormaSuscritaUseCase normaSuscritaUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					await normaSuscritaUseCase.EliminarNormaValidandoPertenencia(sub, id);

					LambdaLogger.Log(
						$"[DELETE] - [NormaSuscrita] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa de la norma - ID: {id}.");
					return Results.Ok();
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[DELETE] - [NormaSuscrita] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [NormaSuscrita] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación de la norma - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Obligaciones.Write.Self", "Vencimientos.Write.Self");

			return routes;
		}

		private static IEndpointRouteBuilder MapProcesarNotificacionEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPost("/ProcesarNotificacion", async (EntKairosParametrosProceso entrada, IHostEnvironment environment, IDatabaseConnectionHelper connectionHelper, NotificacionUseCase notificacionUseCase) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
                    await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

                    try {
                        await notificacionUseCase.ProcesarNotificacion(entrada.IdNormaSuscrita, entrada.Cron, entrada.IdTipoUnidadTiempoAntelacion, entrada.CantAntelacion, entrada.EsVencimiento, entrada.ProgramarSiguienteEjecucion, transaction);
						
						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					LambdaLogger.Log(
                        $"[POST] - [NormaSuscrita] - [ProcesarNotificacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se procesó exitosamente la notificación - ID Norma Suscrita: {entrada.IdNormaSuscrita} - Cron: {entrada.Cron} - ID Tipo Unidad Tiempo Antelacion: {entrada.IdTipoUnidadTiempoAntelacion} - Cant. Antelación: {entrada.CantAntelacion} - Programar Siguiente Ejecución: {entrada.ProgramarSiguienteEjecucion}.");
                    return Results.Ok();
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[POST] - [NormaSuscrita] - [ProcesarNotificacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[POST] - [NormaSuscrita] - [ProcesarNotificacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al procesar notificación - ID Norma Suscrita: {entrada.IdNormaSuscrita} - Cron: {entrada.Cron} - ID Tipo Unidad Tiempo Antelacion: {entrada.IdTipoUnidadTiempoAntelacion} - Cant. Antelación: {entrada.CantAntelacion} - Programar Siguiente Ejecución: {entrada.ProgramarSiguienteEjecucion}. " +
                        $"{ex}");
                    return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
                }
            }).RequireAuthorization(
                "Obligaciones.Read.All",
                "Negocios.Read.All",
                "Vencimientos.Read.All",
                "Vencimientos.Write.All",
                "Templates.Read.Public",
                "Sistema.Read.Public"
            );

            return routes;
        }

		private static IEndpointRouteBuilder MapObtenerPorCodigoAccesoConVencimiento(this IEndpointRouteBuilder routes) {
			routes.MapGet("/ObtenerPorCodigoAccesoConVencimiento", async ([FromQuery] string codigoAcceso, IHostEnvironment environment, ClaimsPrincipal user, IDateTimeProvider dateTimeProvider, ISuscripcionBcp suscripcionBcp, INegocioDao negocioDao, INormaSuscritaDao normaSuscritaDao, IFiscalizadorNormaSuscritaBcp fiscalizadorNormaSuscritaBcp, IHistorialNormaSuscritaDao historialNormaSuscritaDao, IHistorialNotificacionDao historialNotificacionDao, IDocumentoAdjuntoDao documentoAdjuntoDao, ICategoriaNormaDao categoriaNormaDao, ICargoDao cargoDao, ITipoPeriodicidadBcp tipoPeriodicidadBcp, ITipoFiscalizadorDao tipoFiscalizadorDao, ITipoUnidadTiempoBcp tipoUnidadTiempoBcp, ITemplateDao templateDao, ITemplateNormaDao templateNormaDao, ITemplateNormaFiscalizadorDao templateNormaFiscalizadorDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					// Se valida que el código de acceso exista, esté vigente y no haya caducado...
					HistorialNotificacion? historialNotificacion = await historialNotificacionDao.ObtenerPorCodigoAcceso(CryptoHelper.HashSHA256(codigoAcceso), true);
					if (historialNotificacion == null || !historialNotificacion.Vigencia || historialNotificacion.FechaCaducidadCodigoAcceso < dateTimeProvider.UtcNow) {
						LambdaLogger.Log(
							$"[GET] - [NormaSuscrita] - [ObtenerPorCodigoAccesoConVencimiento] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El código de acceso es inválido.");

						return Results.BadRequest($"El código de acceso es inválido.");
					}

					// Se valida que el vencimiento asociada a la notificación exista, esté vigente o completada...
					HistorialNormaSuscrita? historialExistente = await historialNormaSuscritaDao.ObtenerPorId(historialNotificacion.IdHistorialNormaSuscrita);
					if (historialExistente == null || (!historialExistente.Vigencia && historialExistente.FechaCompletitud == null)) {
						LambdaLogger.Log(
							$"[GET] - [NormaSuscrita] - [ObtenerPorCodigoAccesoConVencimiento] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"La obligación no permite el uso de códigos de acceso.");

						return Results.BadRequest($"La obligación no permite el uso de códigos de acceso.");
					}

					// Se valida que exista la obligación...
					NormaSuscrita? existente = await normaSuscritaDao.ObtenerPorId(historialExistente.IdNormaSuscrita);
					if (existente == null) {
						LambdaLogger.Log(
							$"[GET] - [NormaSuscrita] - [ObtenerPorCodigoAccesoConVencimiento] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"La obligación no permite el uso de códigos de acceso.");

						return Results.BadRequest($"La obligación no permite el uso de códigos de acceso.");
					}

                    // Se valida que el negocio este vigente...
                    Negocio? negocio = (await negocioDao.ObtenerPorSub(existente.Sub)).FirstOrDefault(n => n.Id == existente.IdNegocio);
                    if (negocio == null || !negocio.Vigencia) {
                        LambdaLogger.Log(
                            $"[GET] - [NormaSuscrita] - [ObtenerPorCodigoAccesoConVencimiento] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                            $"La obligación no permite el uso de códigos de acceso.");

                        return Results.BadRequest($"La obligación no permite el uso de códigos de acceso.");
                    }

                    // Solo se permite obtener el detalle de un vencimiento no completado si la norma suscrita esta vigente...
                    if (!existente.Vigencia && historialExistente.FechaCompletitud == null) {
						LambdaLogger.Log(
							$"[GET] - [NormaSuscrita] - [ObtenerPorCodigoAccesoConVencimiento] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"La obligación no permite el uso de códigos de acceso.");

						return Results.BadRequest($"La obligación no permite el uso de códigos de acceso.");
					}

                    Dictionary<long, TipoPeriodicidad> periodicidades = (await tipoPeriodicidadBcp.ObtenerPorVigencia(true)).ToDictionary(p => p.Id, p => p);
                    Dictionary<long, CategoriaNorma> categorias = (await categoriaNormaDao.ObtenerPorVigencia(true)).ToDictionary(p => p.Id, p => p);

					List<FiscalizadorNormaSuscrita> fiscalizadoresNormaSuscrita = await fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(existente.Id);

                    Dictionary<long, TipoFiscalizador> fiscalizadores = [];
					if (fiscalizadoresNormaSuscrita.Count > 0) {
						fiscalizadores = (await tipoFiscalizadorDao.ObtenerPorVigencia(true)).ToDictionary(p => p.Id, p => p);
					}

					Template? template = null;
					TemplateNorma? templateNorma = null;
					List<TemplateNormaFiscalizador> templateNormaFiscalizadores = [];
					if (existente.IdTemplate != null && existente.IdNorma != null) {
						template = await templateDao.ObtenerPorId(existente.IdTemplate.Value);
                        if (template != null && !template.Vigencia) template = null;

						if (template != null) {
							// Obtengo la información del template norma...
							templateNorma = (await templateNormaDao.ObtenerPorTemplate(existente.IdTemplate!.Value)).FirstOrDefault(tn => tn.IdNorma == existente.IdNorma);

							// Obtengo la información de los fiscalizadores del template norma...
							templateNormaFiscalizadores = await templateNormaFiscalizadorDao.ObtenerPorTemplateNorma(templateNorma!.IdTemplate, templateNorma!.IdNorma);
							if (templateNormaFiscalizadores.Count > 0 && (fiscalizadores == null || fiscalizadores.Count == 0)) {
								fiscalizadores = (await tipoFiscalizadorDao.ObtenerPorVigencia(true)).ToDictionary(p => p.Id, p => p);
							}
						}
					}

					List<DocumentoAdjunto> documentosAdjuntos = [.. (await documentoAdjuntoDao.ObtenerPorHistorial(historialExistente.Id, true)).Where(da => da.EstadoSubida == 1)];

                    Dictionary<long, Cargo> cargos = (await cargoDao.ObtenerPorSub(existente.Sub, existente.IdNegocio, true)).ToDictionary(c => c.Id, c => c);

                    TipoPeriodicidad? periodicidad = (existente.IdTipoPeriodicidad != null && periodicidades.TryGetValue(existente.IdTipoPeriodicidad.Value, out TipoPeriodicidad? pns)) ? pns : null;
                    CategoriaNorma? categoria = (existente.IdCategoriaNorma != null && categorias.TryGetValue(existente.IdCategoriaNorma.Value, out CategoriaNorma? cn)) ? cn : null;
                    Cargo? cargo = (existente.IdCargo != null && cargos.TryGetValue(existente.IdCargo.Value, out Cargo? c)) ? c : null;

                    TipoPeriodicidad? periodicidadTemplateNorma = (templateNorma?.IdTipoPeriodicidad != null && periodicidades.TryGetValue(templateNorma.IdTipoPeriodicidad.Value, out TipoPeriodicidad? ptn)) ? ptn : null;
                    CategoriaNorma? categoriaTemplateNorma = (templateNorma?.IdCategoriaNorma != null && categorias.TryGetValue(templateNorma.IdCategoriaNorma, out CategoriaNorma? ctn)) ? ctn : null;


                    SalNormaSuscritaObtenerPorIdConVencimiento retorno = new() {
						TienePlanEmpresa = await suscripcionBcp.ConsultaTienePlanEmpresa(existente.Sub),
						IdNegocio = negocio?.Id,
						NombreNegocio = negocio?.Nombre,
						Id = existente.Id,
						Nombre = existente.Nombre,
						Descripcion = existente.Descripcion,
						IdTipoPeriodicidad = periodicidad?.Id,
						NombreTipoPeriodicidad = periodicidad?.Nombre,
						Multa = existente.Multa,
						IdCategoriaNorma = categoria?.Id,
						NombreCategoriaNorma = categoria?.Nombre,
                        IdCargo = cargo?.Id,
                        NombreCargo = cargo?.Nombre,
                        Fiscalizadores = [.. fiscalizadoresNormaSuscrita.Select(fns => {
                            TipoFiscalizador? fiscalizador = fiscalizadores.TryGetValue(fns.IdTipoFiscalizador, out TipoFiscalizador? f) ? f : null;
                            if (fiscalizador == null) return null;

                            return new SalFiscalizadorNormaSuscrita() {
                                Id = fns.Id,
                                IdTipoFiscalizador = fiscalizador.Id,
                                NombreTipoFiscalizador = fiscalizador.Nombre
                            };
                        }).Where(fns => fns != null).Select(fns => fns!)],
                        TemplateNorma = (template == null || templateNorma == null) ? null : new SalTemplateNormaObtenerPorIdConVencimiento() {
							IdTemplate = template.Id,
							NombreTemplate = template.Nombre,
							Nombre = templateNorma.Nombre,
							Descripcion = templateNorma.Descripcion,
							IdTipoPeriodicidad = periodicidadTemplateNorma?.Id,
							NombreTipoPeriodicidad = periodicidadTemplateNorma?.Nombre,
							Multa = templateNorma.Multa,
							IdCategoriaNorma = categoriaTemplateNorma?.Id,
							NombreCategoriaNorma = categoriaTemplateNorma?.Nombre,
							Fiscalizadores = [.. templateNormaFiscalizadores.Select(fns => {
                                TipoFiscalizador? fiscalizador = fiscalizadores.TryGetValue(fns.IdTipoFiscalizador, out TipoFiscalizador? f) ? f : null;
                                if (fiscalizador == null) return null;

                                return new SalFiscalizadorNormaSuscrita() {
                                    Id = 0,
                                    IdTipoFiscalizador = fiscalizador.Id,
                                    NombreTipoFiscalizador = fiscalizador.Nombre
                                };
                            }).Where(fns => fns != null).Select(fns => fns!)],
						},
						FechaVencimiento = historialExistente.FechaVencimiento,
						FechaCompletitud = historialExistente.FechaCompletitud,
						DocumentosAdjuntos = [.. documentosAdjuntos.Select(da => new SalDocumentoAdjunto() {
							Id = da.Id,
							NombreArchivo = da.NombreArchivo,
							FechaSubida = da.FechaConfirmacionSubida
						})]
					};

					LambdaLogger.Log(
						$"[GET] - [NormaSuscrita] - [ObtenerPorCodigoAccesoConVencimiento] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de la norma suscrita por código de acceso con vencimiento.");
					return Results.Ok(retorno);
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[GET] - [NormaSuscrita] - [ObtenerPorCodigoAccesoConVencimiento] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [NormaSuscrita] - [ObtenerPorCodigoAccesoConVencimiento] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener la norma suscrita por código de acceso con vencimiento. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).AllowAnonymous();

			return routes;
		}

		private static IEndpointRouteBuilder MapCompletarNormaPorCodigoAccesoEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPut("/CompletarNormaPorCodigoAcceso", async (EntNormaSuscritaCompletarNormaPorCodigoAcceso entrada, IHostEnvironment environment, ClaimsPrincipal user, NormaSuscritaUseCase normaSuscritaUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					HistorialNormaSuscrita vencimiento = await normaSuscritaUseCase.CompletarNormaPorCodigoAcceso(entrada.CodigoAcceso);

                    SalNormaSuscritaCompletarNorma retorno = new() {
						FechaCompletitud = vencimiento.FechaCompletitud
					};

					LambdaLogger.Log(
						$"[PUT] - [NormaSuscrita] - [CompletarNormaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Se da por completada exitosamente la norma suscrita por código de acceso - ID Norma Suscrita: {vencimiento.IdNormaSuscrita} - ID Historial Norma Suscrita: {vencimiento.Id}.");
					return Results.Ok(retorno);
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[PUT] - [NormaSuscrita] - [CompletarNormaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[PUT] - [NormaSuscrita] - [CompletarNormaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al dar por completada la norma suscrita por código de acceso. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).AllowAnonymous();

			return routes;
		}
	}
}
