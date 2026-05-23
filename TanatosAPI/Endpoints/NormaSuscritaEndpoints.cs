using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using System.Diagnostics;
using System.Security.Claims;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;
using static Google.Api.FieldInfo.Types;

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
			routes.MapGet("/Vigentes/{idNegocio}", async (long idNegocio, IHostEnvironment environment, ClaimsPrincipal user, NormaSuscritaDao normaSuscritaDao, TipoPeriodicidadDao tipoPeriodicidadDao, CategoriaNormaDao categoriaNormaDao, CargoDao cargoDao, TemplateDao templateDao, TemplateNormaDao templateNormaDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					List<NormaSuscrita> normas = await normaSuscritaDao.ObtenerPorSub(sub, idNegocio, true);

					Dictionary<long, TipoPeriodicidad> periodicidades = [];
					Dictionary<long, CategoriaNorma> categorias = [];
					Dictionary<long, Cargo> cargos = [];
					if (normas.Count > 0) {
						periodicidades = (await tipoPeriodicidadDao.ObtenerPorVigencia(true)).ToDictionary(p => p.Id, p => p);
						categorias = (await categoriaNormaDao.ObtenerPorVigencia(true)).ToDictionary(c => c.Id, c => c);
						cargos = (await cargoDao.ObtenerPorSub(sub, idNegocio, true)).ToDictionary(c => c.Id, c => c);
					}

					Dictionary<(long IdTemplate, long IdNorma), TemplateNorma> templateNormas = [];
					foreach (long? idTemplate in normas.Where(n => n.IdTemplate != null).Select(n => n.IdTemplate).Distinct()) {
						if (idTemplate != null) {
							foreach (TemplateNorma templateNorma in await templateNormaDao.ObtenerPorTemplate(idTemplate.Value)) {
								templateNormas[(templateNorma.IdTemplate, templateNorma.IdNorma)] = templateNorma;
							}
						}
					}

					Dictionary<long, Template> templates = [];
					if (normas.Any(n => n.IdTemplate != null)) {
						templates = (await templateDao.ObtenerPorVigencia(true)).ToDictionary(p => p.Id, p => p);
					}

					List<SalNormaSuscrita> retorno = [.. normas.Select(n => {
							TemplateNorma? templateNorma = null;
							if (n.IdTemplate != null && n.IdNorma != null) {
								templateNormas.TryGetValue((n.IdTemplate.Value, n.IdNorma.Value), out templateNorma);
							}

							TipoPeriodicidad? periodicidad = (n.IdTipoPeriodicidad != null && periodicidades.TryGetValue(n.IdTipoPeriodicidad.Value, out TipoPeriodicidad? p)) ? p : null;
							CategoriaNorma? categoria = (n.IdCategoriaNorma != null && categorias.TryGetValue(n.IdCategoriaNorma.Value, out CategoriaNorma? cat)) ? cat : null;
							Cargo? cargo = (n.IdCargo != null && cargos.TryGetValue(n.IdCargo.Value, out Cargo? c)) ? c : null;

							TipoPeriodicidad? periodicidadTemplateNorma = (templateNorma?.IdTipoPeriodicidad != null && periodicidades.TryGetValue(templateNorma.IdTipoPeriodicidad.Value, out TipoPeriodicidad? ptn)) ? ptn : null;
							CategoriaNorma? categoriaTemplateNorma = (templateNorma?.IdCategoriaNorma != null && categorias.TryGetValue(templateNorma.IdCategoriaNorma, out CategoriaNorma? ctn)) ? ctn : null;

							return new SalNormaSuscrita() {
								Id = n.Id,
								Nombre = n.Nombre,
								Descripcion = n.Descripcion,
								IdTipoPeriodicidad = periodicidad?.Id,
								NombreTipoPeriodicidad = periodicidad?.Nombre,
								Multa = n.Multa,
								IdCategoriaNorma = categoria?.Id,
								NombreCategoriaNorma = categoria?.Nombre,
								IdCargo = cargo?.Id,
								NombreCargo = cargo?.Nombre,
								OrdenVisual = n.OrdenVisual,
								Editable = n.Editable,
								Activado = n.Activado,
								TemplateNorma = (templateNorma == null) ? null : new SalTemplateNorma() {
									IdTemplate = templateNorma.IdTemplate,
									NombreTemplate = templates[templateNorma.IdTemplate].Nombre,
									Nombre = templateNorma.Nombre,
									Descripcion = templateNorma.Descripcion,
									IdTipoPeriodicidad = periodicidadTemplateNorma?.Id,
									NombreTipoPeriodicidad = periodicidadTemplateNorma?.Nombre,
									Multa = templateNorma.Multa,
									IdCategoriaNorma = categoriaTemplateNorma?.Id,
									NombreCategoriaNorma = categoriaTemplateNorma?.Nombre
								}
							};
						})
					];

					LambdaLogger.Log(
						$"[GET] - [NormaSuscrita] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de las normas suscritas vigentes - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
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
			routes.MapGet("/ObtenerPorId/{idNormaSuscrita}", async (long idNormaSuscrita, IHostEnvironment environment, ClaimsPrincipal user, NormaSuscritaDao normaSuscritaDao, TipoPeriodicidadDao tipoPeriodicidadDao, CategoriaNormaDao categoriaNormaDao, CargoDao cargoDao, FiscalizadorNormaSuscritaDao fiscalizadorNormaSuscritaDao, NotificacionNormaSuscritaDao notificacionNormaSuscritaDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, TipoFiscalizadorDao tipoFiscalizadorDao, TipoUnidadTiempoDao tipoUnidadTiempoDao, TemplateDao templateDao, TemplateNormaDao templateNormaDao, TemplateNormaFiscalizadorDao templateNormaFiscalizadorDao, TemplateNormaNotificacionDao templateNormaNotificacionDao) => {

				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					NormaSuscrita? existente = await normaSuscritaDao.ObtenerPorId(idNormaSuscrita);

					if (existente == null || !existente.Vigencia || existente.Sub != sub) {
						LambdaLogger.Log(
							$"[GET] - [NormaSuscrita] - [ObtenerPorId] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El ID de norma suscrita es inválido.");

						return Results.BadRequest($"El ID de norma suscrita es inválido.");
					}

					Dictionary<long, TipoPeriodicidad> periodicidades = (await tipoPeriodicidadDao.ObtenerPorVigencia(true)).ToDictionary(p => p.Id, p => p);
                    Dictionary<long, CategoriaNorma> categorias = (await categoriaNormaDao.ObtenerPorVigencia(true)).ToDictionary(c => c.Id, c => c);

					List<FiscalizadorNormaSuscrita> fiscalizadoresNormaSuscrita = await fiscalizadorNormaSuscritaDao.ObtenerPorNormaSuscrita(existente.Id, true);

					Dictionary<long, TipoFiscalizador> fiscalizadores = [];
					if (fiscalizadoresNormaSuscrita.Count > 0) {
						fiscalizadores = (await tipoFiscalizadorDao.ObtenerPorVigencia(true)).ToDictionary(f => f.Id, f => f);
					}

					List<NotificacionNormaSuscrita> notificacionesNormaSuscrita = await notificacionNormaSuscritaDao.ObtenerPorNormaSuscrita(existente.Id, true);

					Dictionary<long, TipoUnidadTiempo> unidadesTiempo = [];
					if (notificacionesNormaSuscrita.Count > 0) {
						unidadesTiempo = (await tipoUnidadTiempoDao.ObtenerPorVigencia(true)).ToDictionary(ut => ut.Id, ut => ut);
					}

					Template? template = null;
					TemplateNorma? templateNorma = null;
					List<TemplateNormaFiscalizador> templateNormaFiscalizadores = [];
					List<TemplateNormaNotificacion> templateNormaNotificaciones = [];
					if (existente.IdTemplate != null && existente.IdNorma != null) {
						template = await templateDao.ObtenerPorId(existente.IdTemplate.Value);

						// Obtengo la información del template norma...
						templateNorma = (await templateNormaDao.ObtenerPorTemplate(existente.IdTemplate!.Value)).FirstOrDefault(tn => tn.IdNorma == existente.IdNorma);

						// Obtengo la información de los fiscalizadores del template norma...
						templateNormaFiscalizadores = await templateNormaFiscalizadorDao.ObtenerPorTemplateNorma(templateNorma!.IdTemplate, templateNorma!.IdNorma);
						if (templateNormaFiscalizadores.Count > 0 && (fiscalizadores == null || fiscalizadores.Count == 0)) {
							fiscalizadores = (await tipoFiscalizadorDao.ObtenerPorVigencia(true)).ToDictionary(f => f.Id, f => f);
						}

						// Obtengo la información de las notificaciones del template norma...
						templateNormaNotificaciones = await templateNormaNotificacionDao.ObtenerPorTemplateNorma(templateNorma!.IdTemplate, templateNorma!.IdNorma);
						if (templateNormaNotificaciones.Count > 0 && (unidadesTiempo == null || unidadesTiempo.Count == 0)) {
							unidadesTiempo = (await tipoUnidadTiempoDao.ObtenerPorVigencia(true)).ToDictionary(ut => ut.Id, ut => ut);
						}
					}

					List<HistorialNormaSuscrita> vencimientos = await historialNormaSuscritaDao.ObtenerPorNormaSuscrita(existente.Id, true);
					HistorialNormaSuscrita? historialNormaSuscrita = vencimientos
							.OrderByDescending(hns => hns.FechaVencimiento)
							.FirstOrDefault();

                    Dictionary<long, Cargo> cargos = (await cargoDao.ObtenerPorSub(sub, existente.IdNegocio, true)).ToDictionary(c => c.Id, c => c);

                    TipoPeriodicidad? periodicidad = (existente.IdTipoPeriodicidad != null && periodicidades.TryGetValue(existente.IdTipoPeriodicidad.Value, out TipoPeriodicidad? pns)) ? pns : null;
                    CategoriaNorma? categoria = (existente.IdCategoriaNorma != null && categorias.TryGetValue(existente.IdCategoriaNorma.Value, out CategoriaNorma? cn)) ? cn : null;
                    Cargo? cargo = (existente.IdCargo != null && cargos.TryGetValue(existente.IdCargo.Value, out Cargo? c)) ? c : null;

                    TipoPeriodicidad? periodicidadTemplateNorma = (templateNorma?.IdTipoPeriodicidad != null && periodicidades.TryGetValue(templateNorma.IdTipoPeriodicidad.Value, out TipoPeriodicidad? ptn)) ? ptn : null;
                    CategoriaNorma? categoriaTemplateNorma = (templateNorma?.IdCategoriaNorma != null && categorias.TryGetValue(templateNorma.IdCategoriaNorma, out CategoriaNorma? ctn)) ? ctn : null;

                    SalNormaSuscrita retorno = new() {
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
						OrdenVisual = existente.OrdenVisual,
						Editable = existente.Editable,
						Activado = existente.Activado,
						TemplateNorma = (template == null || templateNorma == null) ? null : new SalTemplateNorma() {
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
								TipoFiscalizador? fiscalizador = fiscalizadores.TryGetValue(fns.IdTipoFiscalizador, out TipoFiscalizador? ftn) ? ftn : null;
								if (fiscalizador == null) return null;

								return new SalFiscalizadorNormaSuscrita() {
									Id = 0,
									IdTipoFiscalizador = fiscalizador.Id,
									NombreTipoFiscalizador = fiscalizador.Nombre
								};
							}).Where(fns => fns != null).Select(fns => fns!)],
							Notificaciones = [.. templateNormaNotificaciones.Select(nns => {
								TipoUnidadTiempo? unidadTiempo = unidadesTiempo.TryGetValue(nns.IdTipoUnidadTiempoAntelacion, out TipoUnidadTiempo? tut) ? tut : null;
								if (unidadTiempo == null) return null;

								return new SalNotificacionNormaSuscrita() {
									Id = 0,
									IdTipoUnidadTiempoAntelacion = unidadTiempo.Id,
									NombreTipoUnidadTiempoAntelacion = unidadTiempo.Nombre,
									CantAntelacion = nns.CantAntelacion
								};
							}).Where(nns => nns != null).Select(nns => nns!)],
						},
						Fiscalizadores = [.. fiscalizadoresNormaSuscrita.Select(fns => {
                            TipoFiscalizador? fiscalizador = fiscalizadores.TryGetValue(fns.IdTipoFiscalizador, out TipoFiscalizador? ftn) ? ftn : null;
							if (fiscalizador == null) return null;

                            return new SalFiscalizadorNormaSuscrita() {
								Id = fns.Id,
								IdTipoFiscalizador = fiscalizador.Id,
								NombreTipoFiscalizador = fiscalizador.Nombre
							};
						}).Where(fns => fns != null).Select(fns => fns!)],
						Notificaciones = [.. notificacionesNormaSuscrita.Select(nns => {
                            TipoUnidadTiempo? unidadTiempo = unidadesTiempo.TryGetValue(nns.IdTipoUnidadTiempoAntelacion, out TipoUnidadTiempo? tut) ? tut : null;
							if (unidadTiempo == null) return null;

                            return new SalNotificacionNormaSuscrita() {
								Id = nns.Id,
								IdTipoUnidadTiempoAntelacion = unidadTiempo.Id,
								NombreTipoUnidadTiempoAntelacion = unidadTiempo.Nombre,
								CantAntelacion = nns.CantAntelacion
							};
						}).Where(nns => nns != null).Select(nns => nns!)],
						ProximoVencimiento = historialNormaSuscrita?.FechaVencimiento,
					};

					LambdaLogger.Log(
						$"[GET] - [NormaSuscrita] - [ObtenerPorId] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de la norma suscrita por ID: {idNormaSuscrita}.");

					return Results.Ok(retorno);
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
			routes.MapGet("/ObtenerConVencimiento/{idNegocio}", async (long idNegocio, IHostEnvironment environment, ClaimsPrincipal user, NormaSuscritaDao normaSuscritaDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, TemplateDao templateDao, TemplateNormaDao templateNormaDao, TipoPeriodicidadDao tipoPeriodicidadDao, CategoriaNormaDao categoriaNormaDao, CargoDao cargoDao) => {

				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					List<NormaSuscrita> normas = [.. await normaSuscritaDao.ObtenerPorSub(sub, idNegocio, null)];

					Dictionary<long, TipoPeriodicidad> periodicidades = [];
                    Dictionary<long, CategoriaNorma> categorias = [];
					if (normas.Count > 0) {
						periodicidades = (await tipoPeriodicidadDao.ObtenerPorVigencia(true)).ToDictionary(p => p.Id, p => p);
						categorias = (await categoriaNormaDao.ObtenerPorVigencia(true)).ToDictionary(c => c.Id, c => c);
					}

					Dictionary<long, Template>? templates =  null;
					Dictionary<(long idTemplate, long idNorma), TemplateNorma> templateNormas = [];
					foreach (long? idTemplate in normas.Where(n => n.IdTemplate != null).Select(n => n.IdTemplate).Distinct()) {
						if (idTemplate != null) {
							templates ??= (await templateDao.ObtenerPorVigencia(true)).ToDictionary(t => t.Id, t => t);
							Template? template = templates.TryGetValue(idTemplate.Value, out Template? t) ? t : null;

							if (template == null) {
								continue;
							}

							foreach (TemplateNorma templateNorma in await templateNormaDao.ObtenerPorTemplate(idTemplate.Value)) {
								templateNormas[(templateNorma.IdTemplate, templateNorma.IdNorma)] = templateNorma;
							}
						}
					}

					Dictionary<long, Cargo> cargos = (await cargoDao.ObtenerPorSub(sub, idNegocio, true)).ToDictionary(c => c.Id, c => c);

					List<SalNormaSuscritaObtenerConVencimiento> retorno = [];
					foreach (NormaSuscrita normaSuscrita in normas) {
						TemplateNorma? templateNorma = null;
						if (normaSuscrita.IdTemplate != null && normaSuscrita.IdNorma != null) {
							if (templateNormas.TryGetValue((normaSuscrita.IdTemplate.Value, normaSuscrita.IdNorma.Value), out TemplateNorma? templateNormaAux)) {
								templateNorma = templateNormaAux;
							}
						}

						CategoriaNorma? categoriaNorma = (normaSuscrita.IdCategoriaNorma ?? templateNorma?.IdCategoriaNorma) != null && categorias.TryGetValue((normaSuscrita.IdCategoriaNorma ?? templateNorma?.IdCategoriaNorma)!.Value, out CategoriaNorma? c) ? c : null;
						TipoPeriodicidad? tipoPeriodicidad = (normaSuscrita.IdTipoPeriodicidad ?? templateNorma?.IdTipoPeriodicidad) != null && periodicidades.TryGetValue((normaSuscrita.IdTipoPeriodicidad ?? templateNorma?.IdTipoPeriodicidad)!.Value, out TipoPeriodicidad? p) ? p : null;
						Cargo? cargo = (normaSuscrita.IdCargo != null && cargos.TryGetValue(normaSuscrita.IdCargo.Value, out Cargo? car)) ? car : null;

						List<HistorialNormaSuscrita> historialesNormaSuscrita = await historialNormaSuscritaDao.ObtenerPorNormaSuscrita(normaSuscrita.Id, true);

						// Solo se consideran en la salida las normas vigentes y activas, o los vencimientos ya completados
						if ((normaSuscrita.Vigencia && normaSuscrita.Activado) || historialesNormaSuscrita.Any(h => h.FechaCompletitud != null)) {

							// Si la norma no esta vigente o activa, se filtran los vencimientos para mostrar solo los ya completados...
							if (!normaSuscrita.Vigencia || !normaSuscrita.Activado) {
								historialesNormaSuscrita = [.. historialesNormaSuscrita.Where(h => h.FechaCompletitud != null)];
							}

							foreach (HistorialNormaSuscrita historialNormaSuscrita in historialesNormaSuscrita) {
								retorno.Add(new SalNormaSuscritaObtenerConVencimiento {
									FechaVencimiento = historialNormaSuscrita.FechaVencimiento,
									FechaCompletitud = historialNormaSuscrita.FechaCompletitud,
									IdNormaSuscrita = normaSuscrita.Id,
									IdHistorialNormaSuscrita = historialNormaSuscrita.Id,
									NombreNorma = normaSuscrita.Nombre ?? templateNorma?.Nombre,
									DescripcionNorma = normaSuscrita.Descripcion ?? templateNorma?.Descripcion,
									MultaNorma = normaSuscrita.Multa ?? templateNorma?.Multa,
									IdCategoriaNorma = categoriaNorma?.Id,
									NombreCategoriaNorma = categoriaNorma?.NombreCorto ?? categoriaNorma?.Nombre,
									IdTipoPeriodicidad = tipoPeriodicidad?.Id,
									NombreTipoPeriodicidad = tipoPeriodicidad?.Nombre,
									IdCargo = cargo?.Id,
									NombreCargo = cargo?.Nombre,
								});
							}
						}
					}

					LambdaLogger.Log(
						$"[GET] - [NormaSuscrita] - [ObtenerConVencimiento] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de las normas suscritas con vencimiento por ID Negocio: {idNegocio} - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
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
			routes.MapGet("/ObtenerPorIdConVencimiento/{idNormaSuscrita}/{idHistorialNormaSuscrita}", async (long idNormaSuscrita, long idHistorialNormaSuscrita, IHostEnvironment environment, ClaimsPrincipal user, SuscripcionBcp suscripcionBcp, NegocioDao negocioDao, NormaSuscritaDao normaSuscritaDao, FiscalizadorNormaSuscritaDao fiscalizadorNormaSuscritaDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, DocumentoAdjuntoDao documentoAdjuntoDao, CategoriaNormaDao categoriaNormaDao, CargoDao cargoDao, TipoPeriodicidadDao tipoPeriodicidadDao, TipoFiscalizadorDao tipoFiscalizadorDao, TipoUnidadTiempoDao tipoUnidadTiempoDao, TemplateDao templateDao, TemplateNormaDao templateNormaDao, TemplateNormaFiscalizadorDao templateNormaFiscalizadorDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

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

					Dictionary<long, TipoPeriodicidad> periodicidades = (await tipoPeriodicidadDao.ObtenerPorVigencia(true)).ToDictionary(p => p.Id, p => p);
                    Dictionary<long, CategoriaNorma> categorias = (await categoriaNormaDao.ObtenerPorVigencia(true)).ToDictionary(c => c.Id, c => c);

					List<FiscalizadorNormaSuscrita> fiscalizadoresNormaSuscrita = await fiscalizadorNormaSuscritaDao.ObtenerPorNormaSuscrita(existente.Id, true);

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
						TienePlanEmpresa = await suscripcionBcp.TienePlanEmpresa(existente.Sub),
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
						$"[GET] - [NormaSuscrita] - [ObtenerPorIdConVencimiento] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de la norma suscrita por ID {idNormaSuscrita} con vencimiento.");

					return Results.Ok(retorno);
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
			routes.MapPost("/", async (EntNormaSuscritaCrear entrada, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, ClaimsPrincipal user, FiscalizadorNormaSuscritaBcp fiscalizadorNormaSuscritaBcp, NotificacionNormaSuscritaBcp notificacionNormaSuscritaBcp, HistorialNormaSuscritaBcp historialNormaSuscritaBcp, ProcesoNotificacionBcp procesoNotificacionBcp, NormaSuscritaDao normaSuscritaDao, FiscalizadorNormaSuscritaDao fiscalizadorNormaSuscritaDao, NotificacionNormaSuscritaDao notificacionNormaSuscritaDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, HistorialNotificacionDao historialNotificacionDao, CargoDao cargoDao, DestinatarioNotificacionDao destinatarioNotificacionDao, TipoPeriodicidadDao tipoPeriodicidadDao, CategoriaNormaDao categoriaNormaDao, NegocioDao negocioDao, TipoFiscalizadorDao tipoFiscalizadorDao, TipoUnidadTiempoDao tipoUnidadTiempoDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					entrada.Nombre = entrada.Nombre.Trim();
					entrada.Descripcion = string.IsNullOrWhiteSpace(entrada.Descripcion) ? null : entrada.Descripcion?.Trim();
					entrada.Multa = string.IsNullOrWhiteSpace(entrada.Multa) ? null : entrada.Multa?.Trim();

					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					// Se valida que el usuario no tenga otra norma igual...
					List<NormaSuscrita> normasVigentes = await normaSuscritaDao.ObtenerPorSub(sub, entrada.IdNegocio, true);
					if (normasVigentes.Any(d => d.Sub == sub && d.IdNegocio == entrada.IdNegocio && d.Nombre == entrada.Nombre)) {
						LambdaLogger.Log(
							$"[POST] - [NormaSuscrita] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Ya tienes registrado una norma con el mismo nombre.");

						return Results.BadRequest($"Ya tienes registrado una norma con el mismo nombre.");
					}

					// Se valida que el tipo de periodicidad sea válido...
					TipoPeriodicidad? tipoPeriodicidad = null;
                    if (entrada.IdTipoPeriodicidad != null) {
						tipoPeriodicidad = await tipoPeriodicidadDao.ObtenerPorId(entrada.IdTipoPeriodicidad.Value);
						if (tipoPeriodicidad == null || !tipoPeriodicidad.Vigencia) {
							LambdaLogger.Log(
								$"[POST] - [NormaSuscrita] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
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
								$"[POST] - [NormaSuscrita] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
								$"La categoría es inválida.");

							return Results.BadRequest($"La categoría es inválida.");
						}
					}
									
					// Se valida que el negocio sea válido...
					Negocio? negocio = (await negocioDao.ObtenerPorSub(sub)).FirstOrDefault(n => n.Id == entrada.IdNegocio);
					if (negocio == null || !negocio.Vigencia) {
						LambdaLogger.Log(
							$"[POST] - [NormaSuscrita] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El negocio es inválido.");

						return Results.BadRequest($"El negocio es inválido.");
					}

                    // Se valida que el cargo sea válido...
                    Cargo? cargo = null;
                    if (entrada.IdCargo != null) {
                        cargo = (await cargoDao.ObtenerPorSub(sub, entrada.IdNegocio, true)).FirstOrDefault(c => c.Id == entrada.IdCargo);
                        LambdaLogger.Log(
                            $"[POST] - [NormaSuscrita] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                            $"El cargo es inválido.");

                        return Results.BadRequest($"El cargo es inválido.");
                    }

                    List<TipoFiscalizador> tiposFiscalizadores = [];
					// Se valida que los fiscalizadores sean válidos...
					if (entrada.Fiscalizadores != null && entrada.Fiscalizadores.Count > 0) {
						if (entrada.Fiscalizadores.GroupBy(n => n.IdTipoFiscalizador).Any(g => g.Count() > 1)) {
							LambdaLogger.Log(
								$"[POST] - [NormaSuscrita] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
								$"Los fiscalizadores incluyen duplicados.");

							return Results.BadRequest($"Los fiscalizadores incluyen duplicados.");
						}

						tiposFiscalizadores = await tipoFiscalizadorDao.ObtenerPorVigencia(true);
						foreach (EntFiscalizadorNormaSuscritaCrear fiscalizador in entrada.Fiscalizadores) {
							if (!tiposFiscalizadores.Any(tf => tf.Id == fiscalizador.IdTipoFiscalizador)) {
								LambdaLogger.Log(
									$"[POST] - [NormaSuscrita] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
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
								$"[POST] - [NormaSuscrita] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
								$"Una notificación con cantidad antelación inválido.");

							return Results.BadRequest($"Una notificación con cantidad antelación inválido.");
						}

						if (entrada.Notificaciones.GroupBy(n => new { n.IdTipoUnidadTiempoAntelacion, n.CantAntelacion}).Any(g => g.Count() > 1)) {
							LambdaLogger.Log(
								$"[POST] - [NormaSuscrita] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
								$"Las notificaciones incluyen duplicados.");

							return Results.BadRequest($"Las notificaciones incluyen duplicados.");
						}

						tiposUnidadesTiempo = await tipoUnidadTiempoDao.ObtenerPorVigencia(true);
						foreach (EntNotificacionNormaSuscritaCrear notificacion in entrada.Notificaciones) {
							if (!tiposUnidadesTiempo.Any(tut => tut.Id == notificacion.IdTipoUnidadTiempoAntelacion)) {
								LambdaLogger.Log(
									$"[POST] - [NormaSuscrita] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
									$"Una notificación es inválida.");

								return Results.BadRequest($"Una notificación es inválida.");
							}
						}
					}

					// Se valida que si la norma suscrita está activa, incluya una fecha de próximo vencimiento...
					if (entrada.Activado && entrada.ProximoVencimiento == null) {
						LambdaLogger.Log(
							$"[POST] - [NormaSuscrita] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Debe incluir la fecha de próximo vencimiento.");

						return Results.BadRequest($"Debe incluir la fecha de próximo vencimiento.");
					}

					// Se valida que el próximo vencimiento sea una fecha futura...
					if (entrada.Activado && entrada.ProximoVencimiento != null && entrada.ProximoVencimiento <= DateTime.UtcNow) {
						LambdaLogger.Log(
							$"[POST] - [NormaSuscrita] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El próximo vencimiento debe ser una fecha futura.");

						return Results.BadRequest($"El próximo vencimiento debe ser una fecha futura.");
					}

					NormaSuscrita nuevo = new() {
						Id = 0,
						Sub = sub,
						IdNegocio = entrada.IdNegocio,
						IdTemplate = null,
						IdNorma = null,
						Nombre = entrada.Nombre,
						Descripcion = entrada.Descripcion,
						IdTipoPeriodicidad = entrada.IdTipoPeriodicidad,
						Multa = entrada.Multa,
						IdCategoriaNorma = entrada.IdCategoriaNorma,
						IdCargo = entrada.IdCargo,
						OrdenVisual = null,
						Editable = true,
						FechaActivacion = entrada.Activado ? DateTime.UtcNow : null,
						FechaDesactivacion = null,
						Activado = entrada.Activado,
						FechaCreacion = DateTime.UtcNow,
						FechaEliminacion = null,
						Vigencia = true
					};

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

					try {
						nuevo.Id = await normaSuscritaDao.Insertar(nuevo, transaction);

						await fiscalizadorNormaSuscritaBcp.ActualizarPorNormaSuscrita(nuevo, entrada.Fiscalizadores?.Select(f => f.IdTipoFiscalizador).ToHashSet() ?? [], transaction);

						await notificacionNormaSuscritaBcp.ActualizarPorNormaSuscrita(nuevo, entrada.Notificaciones?.Select(n => (
							n.IdTipoUnidadTiempoAntelacion,
							n.CantAntelacion
						)).ToHashSet() ?? [], transaction);

						if (entrada.Activado) {
							HistorialNormaSuscrita historialNormaSuscrita = new() {
								Id = 0,
								IdNormaSuscrita = nuevo.Id,
								FechaVencimiento = entrada.ProximoVencimiento!.Value,
								FechaCreacion = DateTime.UtcNow,
								Vigencia = true
							};
							await historialNormaSuscritaBcp.Crear(historialNormaSuscrita, transaction);							
						}

						await procesoNotificacionBcp.ActualizarProgramacionProcesosNormaSuscrita(nuevo.Id, transaction);

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					List<FiscalizadorNormaSuscrita>? fiscalizadoresExistentes = await fiscalizadorNormaSuscritaDao.ObtenerPorNormaSuscrita(nuevo.Id, true);
					if (fiscalizadoresExistentes.Count == 0) {
						fiscalizadoresExistentes = null;
					}
					List<NotificacionNormaSuscrita>? notificacionesExistentes = await notificacionNormaSuscritaDao.ObtenerPorNormaSuscrita(nuevo.Id, true);
					if (notificacionesExistentes.Count == 0) {
						notificacionesExistentes = null;
					}

					SalNormaSuscrita retorno = new() {
						Id = nuevo.Id,
						Nombre = nuevo.Nombre,
						Descripcion = nuevo.Descripcion,
						IdTipoPeriodicidad = nuevo.IdTipoPeriodicidad,
						NombreTipoPeriodicidad = tipoPeriodicidad?.Nombre,
						Multa = nuevo.Multa,
						IdCategoriaNorma = nuevo.IdCategoriaNorma,
						NombreCategoriaNorma = categoria?.Nombre,
						IdCargo = cargo?.Id,
						NombreCargo = cargo?.Nombre,
						OrdenVisual = nuevo.OrdenVisual,
						Editable = true,
						Activado = nuevo.Activado,
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
						$"[POST] - [NormaSuscrita] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa de la norma - ID: {retorno.Id}.");

					return Results.Ok(retorno);
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
			routes.MapPut("/", async(EntNormaSuscritaActualizar entrada, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, ClaimsPrincipal user, FiscalizadorNormaSuscritaBcp fiscalizadorNormaSuscritaBcp, NotificacionNormaSuscritaBcp notificacionNormaSuscritaBcp, HistorialNormaSuscritaBcp historialNormaSuscritaBcp, ProcesoNotificacionBcp procesoNotificacionBcp, NormaSuscritaDao normaSuscritaDao, FiscalizadorNormaSuscritaDao fiscalizadorNormaSuscritaDao, NotificacionNormaSuscritaDao notificacionNormaSuscritaDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, TipoPeriodicidadDao tipoPeriodicidadDao, CargoDao cargoDao, CategoriaNormaDao categoriaNormaDao, NegocioDao negocioDao, TipoFiscalizadorDao tipoFiscalizadorDao, TipoUnidadTiempoDao tipoUnidadTiempoDao, TemplateNormaDao templateNormaDao, TemplateNormaFiscalizadorDao templateNormaFiscalizadorDao, TemplateNormaNotificacionDao templateNormaNotificacionDao) => {
				
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					entrada.Nombre = entrada.Nombre.Trim();
					entrada.Descripcion = string.IsNullOrWhiteSpace(entrada.Descripcion) ? null : entrada.Descripcion?.Trim();
					entrada.Multa = string.IsNullOrWhiteSpace(entrada.Multa) ? null : entrada.Multa?.Trim();

					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

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
						tipoPeriodicidad = await tipoPeriodicidadDao.ObtenerPorId(entrada.IdTipoPeriodicidad.Value);
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

					// Se valida que el cargo sea válido...
                    Cargo? cargo = null;
                    if (entrada.IdCargo != null) {
                        cargo = (await cargoDao.ObtenerPorSub(sub, entrada.IdNegocio, true)).FirstOrDefault(c => c.Id == entrada.IdCargo);
                        LambdaLogger.Log(
                            $"[PUT] - [NormaSuscrita] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                            $"El cargo es inválido.");

                        return Results.BadRequest($"El cargo es inválido.");
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

						tiposUnidadesTiempo = await tipoUnidadTiempoDao.ObtenerPorVigencia(true);
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
					if (entrada.Activado && proximoVencimientoExistente?.FechaVencimiento != entrada.ProximoVencimiento && entrada.ProximoVencimiento <= DateTime.UtcNow) {
						LambdaLogger.Log(
							$"[PUT] - [NormaSuscrita] - [Actualizar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El próximo vencimiento debe ser una fecha futura.");

						return Results.BadRequest($"El próximo vencimiento debe ser una fecha futura.");
					}

					existente.Nombre = entrada.Nombre;
					existente.Descripcion = entrada.Descripcion;
					existente.IdTipoPeriodicidad = entrada.IdTipoPeriodicidad;
					existente.Multa = entrada.Multa;
					existente.IdCategoriaNorma = entrada.IdCategoriaNorma;
					existente.IdCargo = entrada.IdCargo;

                    if (existente.Activado && !entrada.Activado) {
						existente.FechaDesactivacion = DateTime.UtcNow;
						existente.Activado = false;
					} else if (!existente.Activado && entrada.Activado) {
						existente.FechaActivacion = DateTime.UtcNow;
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
						List<TemplateNormaNotificacion> templateNormaNotificaciones = await templateNormaNotificacionDao.ObtenerPorTemplateNorma(existente.IdTemplate.Value, existente.IdNorma);
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

						await fiscalizadorNormaSuscritaBcp.ActualizarPorNormaSuscrita(existente, entrada.Fiscalizadores?.Select(f => f.IdTipoFiscalizador).ToHashSet() ?? [], transaction);

						await notificacionNormaSuscritaBcp.ActualizarPorNormaSuscrita(existente, entrada.Notificaciones?.Select(n => (
							n.IdTipoUnidadTiempoAntelacion, 
							n.CantAntelacion
						)).ToHashSet() ?? [], transaction);

						// En caso de que norma suscrita esté activa, se agrega historial en caso de que proximo vencimiento sea distinto al existente...
						if (entrada.Activado) {
							if (proximoVencimientoExistente?.FechaVencimiento != entrada.ProximoVencimiento) {
								if (proximoVencimientoExistente != null) {
									await historialNormaSuscritaBcp.EliminarPorNormaSuscrita(existente, true, transaction);
								}

								HistorialNormaSuscrita historialNormaSuscrita = new() {
									Id = 0,
									IdNormaSuscrita = existente.Id,
									FechaVencimiento = entrada.ProximoVencimiento!.Value,
									FechaCreacion = DateTime.UtcNow,
									Vigencia = true
								};
								await historialNormaSuscritaBcp.Crear(historialNormaSuscrita, transaction);
							}
						// En caso de que norma suscrita esté inactiva, se elimina el próximo vencimiento existente...
						} else {
							if (proximoVencimientoExistente != null) {
								await historialNormaSuscritaBcp.EliminarPorNormaSuscrita(existente, false, transaction);
							}
						}

						await procesoNotificacionBcp.ActualizarProgramacionProcesosNormaSuscrita(existente.Id, transaction);

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					List<FiscalizadorNormaSuscrita>? fiscalizadoresExistentes = await fiscalizadorNormaSuscritaDao.ObtenerPorNormaSuscrita(existente.Id, true);
					if (fiscalizadoresExistentes.Count == 0) {
						fiscalizadoresExistentes = null;
					}
					List<NotificacionNormaSuscrita>? notificacionesExistentes = await notificacionNormaSuscritaDao.ObtenerPorNormaSuscrita(existente.Id, true);
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
			routes.MapPut("/CompletarNorma", async (EntNormaSuscritaCompletarNorma entrada, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, ClaimsPrincipal user, HistorialNormaSuscritaBcp historialNormaSuscritaBcp, NormaSuscritaDao normaSuscritaDao, HistorialNormaSuscritaDao historialNormaSuscritaDao) => {

				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					NormaSuscrita? existente = await normaSuscritaDao.ObtenerPorId(entrada.IdNormaSuscrita);
					if (existente == null || !existente.Vigencia || existente.Sub != sub) {
						LambdaLogger.Log(
							$"[PUT] - [NormaSuscrita] - [CompletarNorma] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El ID de norma suscrita es inválido.");

						return Results.BadRequest($"El ID de norma suscrita es inválido.");
					}

					HistorialNormaSuscrita? historialExistente = await historialNormaSuscritaDao.ObtenerPorId(entrada.IdHistorialNormaSuscrita);
					if (historialExistente == null || !historialExistente.Vigencia || historialExistente.IdNormaSuscrita != entrada.IdNormaSuscrita) {
						LambdaLogger.Log(
							$"[PUT] - [NormaSuscrita] - [CompletarNorma] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El ID de norma suscrita es inválido.");

						return Results.BadRequest($"El ID de norma suscrita es inválido.");
					}

					if (historialExistente.FechaCompletitud != null) {
						LambdaLogger.Log(
							$"[PUT] - [NormaSuscrita] - [CompletarNorma] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"La norma suscrita ya se encuentra completada.");

						return Results.BadRequest($"La norma suscrita ya se encuentra completada.");
					}

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

					SalNormaSuscritaCompletarNorma retorno = new();

					try {
						await historialNormaSuscritaBcp.CompletarHistorialNormaSuscrita(historialExistente, transaction);
						retorno.FechaCompletitud = historialExistente.FechaCompletitud;

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					LambdaLogger.Log(
						$"[PUT] - [NormaSuscrita] - [CompletarNorma] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Se da por completada exitosamente la norma suscrita - ID Norma Suscrita: {entrada.IdNormaSuscrita} - ID Historial Norma Suscrita: {entrada.IdHistorialNormaSuscrita}.");

					return Results.Ok(retorno);
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
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, ClaimsPrincipal user, NormaSuscritaDao normaSuscritaDao, NormaSuscritaBcp normaSuscritaBcp) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					NormaSuscrita? existente = (await normaSuscritaDao.ObtenerPorSub(sub)).FirstOrDefault(d => d.Id == id);

					if (existente == null) {
						LambdaLogger.Log(
							$"[DELETE] - [NormaSuscrita] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El usuario no posee una norma con ID {id}.");

						return Results.BadRequest($"El usuario no posee una norma con ID {id}.");
					}

					// Se valida que la norma suscrita puede ser eliminada según marca de editable (no se deben eliminar normas suscritas generadas desde inscripción a templates)...
					if (!existente.Editable) {
						LambdaLogger.Log(
							$"[DELETE] - [NormaSuscrita] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"La norma suscrita no es eliminable.");

						return Results.BadRequest($"La norma suscrita no es eliminable.");
					}

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

					try { 
						await normaSuscritaBcp.EliminarNormaSuscrita(existente, transaction);

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					LambdaLogger.Log(
						$"[DELETE] - [NormaSuscrita] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa de la norma - ID: {id}.");

					return Results.Ok();
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
            routes.MapPost("/ProcesarNotificacion", async (EntKairosParametrosProceso entrada, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, ProcesoNotificacionBcp procesoNotificacionBcp) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
                    await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

                    try {
                        await procesoNotificacionBcp.ProcesarNotificacion(entrada.IdNormaSuscrita, entrada.Cron, entrada.IdTipoUnidadTiempoAntelacion, entrada.CantAntelacion, entrada.EsVencimiento, entrada.ProgramarSiguienteEjecucion, transaction);
						
						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					LambdaLogger.Log(
                        $"[POST] - [NormaSuscrita] - [ProcesarNotificacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se procesó exitosamente la notificación - ID Norma Suscrita: {entrada.IdNormaSuscrita} - Cron: {entrada.Cron} - ID Tipo Unidad Tiempo Antelacion: {entrada.IdTipoUnidadTiempoAntelacion} - Cant. Antelación: {entrada.CantAntelacion} - Programar Siguiente Ejecución: {entrada.ProgramarSiguienteEjecucion}.");

                    return Results.Ok();
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
			routes.MapGet("/ObtenerPorCodigoAccesoConVencimiento", async ([FromQuery] string codigoAcceso, IHostEnvironment environment, ClaimsPrincipal user, CryptoHelper cryptoHelper, SuscripcionBcp suscripcionBcp, NegocioDao negocioDao, NormaSuscritaDao normaSuscritaDao, FiscalizadorNormaSuscritaDao fiscalizadorNormaSuscritaDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, HistorialNotificacionDao historialNotificacionDao, DocumentoAdjuntoDao documentoAdjuntoDao, CategoriaNormaDao categoriaNormaDao, CargoDao cargoDao, TipoPeriodicidadDao tipoPeriodicidadDao, TipoFiscalizadorDao tipoFiscalizadorDao, TipoUnidadTiempoDao tipoUnidadTiempoDao, TemplateDao templateDao, TemplateNormaDao templateNormaDao, TemplateNormaFiscalizadorDao templateNormaFiscalizadorDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					// Se valida que el código de acceso exista, esté vigente y no haya caducado...
					HistorialNotificacion? historialNotificacion = await historialNotificacionDao.ObtenerPorCodigoAcceso(cryptoHelper.HashSHA256(codigoAcceso), true);
					if (historialNotificacion == null || !historialNotificacion.Vigencia || historialNotificacion.FechaCaducidadCodigoAcceso < DateTime.UtcNow) {
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

                    Dictionary<long, TipoPeriodicidad> periodicidades = (await tipoPeriodicidadDao.ObtenerPorVigencia(true)).ToDictionary(p => p.Id, p => p);
                    Dictionary<long, CategoriaNorma> categorias = (await categoriaNormaDao.ObtenerPorVigencia(true)).ToDictionary(p => p.Id, p => p);

					List<FiscalizadorNormaSuscrita> fiscalizadoresNormaSuscrita = await fiscalizadorNormaSuscritaDao.ObtenerPorNormaSuscrita(existente.Id, true);

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
						TienePlanEmpresa = await suscripcionBcp.TienePlanEmpresa(existente.Sub),
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
			routes.MapPut("/CompletarNormaPorCodigoAcceso", async (EntNormaSuscritaCompletarNormaPorCodigoAcceso entrada, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, ClaimsPrincipal user, CryptoHelper cryptoHelper, HistorialNormaSuscritaBcp historialNormaSuscritaBcp, NormaSuscritaDao normaSuscritaDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, HistorialNotificacionDao historialNotificacionDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					// Se valida que el código de acceso exista, esté vigente y no haya caducado...
					HistorialNotificacion? historialNotificacion = await historialNotificacionDao.ObtenerPorCodigoAcceso(cryptoHelper.HashSHA256(entrada.CodigoAcceso), true);
					if (historialNotificacion == null || !historialNotificacion.Vigencia || historialNotificacion.FechaCaducidadCodigoAcceso < DateTime.UtcNow) {
						LambdaLogger.Log(
							$"[PUT] - [NormaSuscrita] - [CompletarNormaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El código de acceso es inválido.");

						return Results.BadRequest($"El código de acceso es inválido.");
					}

					HistorialNormaSuscrita? historialExistente = await historialNormaSuscritaDao.ObtenerPorId(historialNotificacion.IdHistorialNormaSuscrita);
					if (historialExistente == null || !historialExistente.Vigencia) {
						LambdaLogger.Log(
							$"[PUT] - [NormaSuscrita] - [CompletarNormaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"La obligación no puede darse por completada.");

						return Results.BadRequest($"La obligación no puede darse por completada.");
					}

					if (historialExistente.FechaCompletitud != null) {
						LambdaLogger.Log(
							$"[PUT] - [NormaSuscrita] - [CompletarNormaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"La obligación ya se encuentra completada.");

						return Results.BadRequest($"La obligación ya se encuentra completada.");
					}

					NormaSuscrita? existente = await normaSuscritaDao.ObtenerPorId(historialExistente.IdNormaSuscrita);
					if (existente == null || !existente.Vigencia) {
						LambdaLogger.Log(
							$"[PUT] - [NormaSuscrita] - [CompletarNormaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"La obligación no puede darse por completada.");

						return Results.BadRequest($"La obligación no puede darse por completada.");
					}

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

					SalNormaSuscritaCompletarNorma retorno = new();

					try {
						await historialNormaSuscritaBcp.CompletarHistorialNormaSuscrita(historialExistente, transaction);
						retorno.FechaCompletitud = historialExistente.FechaCompletitud;

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					LambdaLogger.Log(
						$"[PUT] - [NormaSuscrita] - [CompletarNormaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Se da por completada exitosamente la norma suscrita por código de acceso - ID Norma Suscrita: {historialExistente.IdNormaSuscrita} - ID Historial Norma Suscrita: {historialExistente.Id}.");

					return Results.Ok(retorno);
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
