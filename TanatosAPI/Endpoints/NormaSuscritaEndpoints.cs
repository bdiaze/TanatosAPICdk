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
using TanatosAPI.Interfaces.Helpers;
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

					List<NormaSuscrita> normas = await normaSuscritaUseCase.ObtenerPorSubYNegocio(sub, idNegocio, filtrarVigentes: true, incluirTemplates: true, incluirPeriodicidades: true, incluirCategorias: true, incluirCargos: true);

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

					NormaSuscrita obligacion = await normaSuscritaUseCase.ObtenerIncluyendoProximoVencimiento(idNormaSuscrita, sub);

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
					List<NormaSuscrita> normas = await normaSuscritaUseCase.ObtenerPorSubYNegocio(sub, idNegocio, filtrarVigentes: true, incluirTemplates: true, incluirPeriodicidades: true, incluirCategorias: true, incluirCargos: true, incluirHistorialVencimientos: true);

					List<SalNormaSuscritaObtenerConVencimiento> retorno = [.. normas.SelectMany(normaSuscrita => {
						TipoPeriodicidad? periodicidad = normaSuscrita.TipoPeriodicidad ?? normaSuscrita.TemplateNorma?.TipoPeriodicidad;
						CategoriaNorma? categoriaNorma = normaSuscrita.CategoriaNorma ?? normaSuscrita.TemplateNorma?.CategoriaNorma;

						return (normaSuscrita.HistorialesNormaSuscrita ?? []).Select(historialNormaSuscrita => new SalNormaSuscritaObtenerConVencimiento {
							FechaVencimiento = historialNormaSuscrita.FechaVencimiento,
							FechaCompletitud = historialNormaSuscrita.FechaCompletitud,
							IdTemplate = normaSuscrita.TemplateNorma?.IdTemplate,
							IdNorma = normaSuscrita.TemplateNorma?.IdNorma,
							IdNormaSuscrita = normaSuscrita.Id,
							IdHistorialNormaSuscrita = historialNormaSuscrita.Id,
							NombreTemplate = normaSuscrita.TemplateNorma?.Template?.Nombre,
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
			routes.MapGet("/ObtenerPorIdConVencimiento/{idNormaSuscrita}/{idHistorialNormaSuscrita}", async (long idNormaSuscrita, long idHistorialNormaSuscrita, IHostEnvironment environment, ClaimsPrincipal user, NormaSuscritaUseCase normaSuscritaUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					(HistorialNormaSuscrita vencimiento, bool tienePlanEmpresa) = await normaSuscritaUseCase.ObtenerVencimientoConDocumentosYPlan(idNormaSuscrita, idHistorialNormaSuscrita, sub);

                    SalNormaSuscritaObtenerPorIdConVencimiento retorno = new() {
						TienePlanEmpresa = tienePlanEmpresa,
						IdNegocio = vencimiento.NormaSuscrita!.Negocio?.Id,
						NombreNegocio = vencimiento.NormaSuscrita!.Negocio?.Nombre,
						Id = vencimiento.NormaSuscrita!.Id,
						Nombre = vencimiento.NormaSuscrita!.Nombre,
						Descripcion = vencimiento.NormaSuscrita!.Descripcion,
						Multa = vencimiento.NormaSuscrita!.Multa,
						IdTipoPeriodicidad = vencimiento.NormaSuscrita!.TipoPeriodicidad?.Id,
						NombreTipoPeriodicidad = vencimiento.NormaSuscrita!.TipoPeriodicidad?.Nombre,
						IdCategoriaNorma = vencimiento.NormaSuscrita!.CategoriaNorma?.Id,
						NombreCategoriaNorma = vencimiento.NormaSuscrita!.CategoriaNorma?.Nombre,
						IdCargo = vencimiento.NormaSuscrita!.Cargo?.Id,
						NombreCargo = vencimiento.NormaSuscrita!.Cargo?.Nombre,
						Fiscalizadores = [.. (vencimiento.NormaSuscrita.FiscalizadoresNormaSuscrita ?? []).Select(fns => {
							return new SalFiscalizadorNormaSuscrita() {
								Id = fns.Id,
								IdTipoFiscalizador = fns.TipoFiscalizador!.Id,
								NombreTipoFiscalizador = fns.TipoFiscalizador!.Nombre
							};
						})],
						TemplateNorma = (vencimiento.NormaSuscrita!.TemplateNorma == null) ? null : new SalTemplateNormaObtenerPorIdConVencimiento() {
							IdTemplate = vencimiento.NormaSuscrita!.TemplateNorma!.Template!.Id,
							NombreTemplate = vencimiento.NormaSuscrita!.TemplateNorma!.Template!.Nombre,
							Nombre = vencimiento.NormaSuscrita!.TemplateNorma!.Nombre,
							Descripcion = vencimiento.NormaSuscrita!.TemplateNorma!.Descripcion,
							Multa = vencimiento.NormaSuscrita!.TemplateNorma!.Multa,
							IdTipoPeriodicidad = vencimiento.NormaSuscrita!.TemplateNorma!.TipoPeriodicidad?.Id,
							NombreTipoPeriodicidad = vencimiento.NormaSuscrita!.TemplateNorma!.TipoPeriodicidad?.Nombre,
							IdCategoriaNorma = vencimiento.NormaSuscrita!.TemplateNorma!.CategoriaNorma?.Id,
							NombreCategoriaNorma = vencimiento.NormaSuscrita!.TemplateNorma!.CategoriaNorma?.Nombre,
							Fiscalizadores = [.. (vencimiento.NormaSuscrita!.TemplateNorma!.TemplateNormaFiscalizadores ?? []).Select(fns => {
                                return new SalFiscalizadorNormaSuscrita() {
									Id = 0,
									IdTipoFiscalizador = fns.TipoFiscalizador!.Id,
									NombreTipoFiscalizador = fns.TipoFiscalizador!.Nombre
								};
							})]
						},
						FechaVencimiento = vencimiento.FechaVencimiento,
						FechaCompletitud = vencimiento.FechaCompletitud,
						DocumentosAdjuntos = [.. (vencimiento.DocumentosAdjuntos ?? []).Select(da => new SalDocumentoAdjunto() {
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
                    
					(NormaSuscrita obligacion, _, _) = await normaSuscritaUseCase.CrearNormaSuscrita(
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
			routes.MapPut("/", async(EntNormaSuscritaActualizar entrada, IHostEnvironment environment, ClaimsPrincipal user, NormaSuscritaUseCase normaSuscritaUseCase) => {
				
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					(NormaSuscrita obligacion, _, _) = await normaSuscritaUseCase.ActualizarNormaSuscrita(
						sub, 
						entrada.Id, 
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
						TemplateNorma = obligacion.TemplateNorma == null ? null : new SalTemplateNorma() {
							IdTemplate = obligacion.TemplateNorma!.Template!.Id,
							NombreTemplate = obligacion.TemplateNorma!.Template!.Nombre,
							Nombre = obligacion.TemplateNorma!.Nombre,
							Descripcion = obligacion.TemplateNorma!.Descripcion,
							Multa = obligacion.TemplateNorma!.Multa,
							IdTipoPeriodicidad = obligacion.TemplateNorma!.TipoPeriodicidad?.Id,
							NombreTipoPeriodicidad = obligacion.TemplateNorma!.TipoPeriodicidad?.Nombre,
							IdCategoriaNorma = obligacion.TemplateNorma!.CategoriaNorma?.Id,
							NombreCategoriaNorma = obligacion.TemplateNorma!.CategoriaNorma?.Nombre,
							Fiscalizadores = [.. (obligacion.TemplateNorma!.TemplateNormaFiscalizadores ?? []).Select(f => new SalFiscalizadorNormaSuscrita {
								Id = 0,
								IdTipoFiscalizador = f.TipoFiscalizador!.Id,
								NombreTipoFiscalizador = f.TipoFiscalizador!.Nombre
							})],
							Notificaciones = [.. (obligacion.TemplateNorma!.TemplateNormaNotificaciones ?? []).Select(n => new SalNotificacionNormaSuscrita {
								Id = 0,
								IdTipoUnidadTiempoAntelacion = n.TipoUnidadTiempoAntelacion!.Id,
								NombreTipoUnidadTiempoAntelacion = n.TipoUnidadTiempoAntelacion!.Nombre,
								CantAntelacion = n.CantAntelacion
							})]
						},
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
                        await notificacionUseCase.ProcesarNotificacion(entrada.IdNormaSuscrita, entrada.Cron, entrada.FrecuenciaDias, entrada.InicioEjecucionUtc, entrada.IdTipoUnidadTiempoAntelacion, entrada.CantAntelacion, entrada.EsVencimiento, entrada.ProgramarSiguienteEjecucion, transaction);
						
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
			routes.MapGet("/ObtenerPorCodigoAccesoConVencimiento", async ([FromQuery] string codigoAcceso, IHostEnvironment environment, ClaimsPrincipal user, NormaSuscritaUseCase normaSuscritaUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					(HistorialNormaSuscrita vencimiento, bool tienePlanEmpresa) = await normaSuscritaUseCase.ObtenerVencimientoConDocumentosYPlan(codigoAcceso);

					SalNormaSuscritaObtenerPorIdConVencimiento retorno = new() {
						TienePlanEmpresa = tienePlanEmpresa,
						IdNegocio = vencimiento.NormaSuscrita!.Negocio?.Id,
						NombreNegocio = vencimiento.NormaSuscrita!.Negocio?.Nombre,
						Id = vencimiento.NormaSuscrita!.Id,
						Nombre = vencimiento.NormaSuscrita!.Nombre,
						Descripcion = vencimiento.NormaSuscrita!.Descripcion,
						Multa = vencimiento.NormaSuscrita!.Multa,
						IdTipoPeriodicidad = vencimiento.NormaSuscrita!.TipoPeriodicidad?.Id,
						NombreTipoPeriodicidad = vencimiento.NormaSuscrita!.TipoPeriodicidad?.Nombre,
						IdCategoriaNorma = vencimiento.NormaSuscrita!.CategoriaNorma?.Id,
						NombreCategoriaNorma = vencimiento.NormaSuscrita!.CategoriaNorma?.Nombre,
						IdCargo = vencimiento.NormaSuscrita!.Cargo?.Id,
						NombreCargo = vencimiento.NormaSuscrita!.Cargo?.Nombre,
						Fiscalizadores = [.. (vencimiento.NormaSuscrita.FiscalizadoresNormaSuscrita ?? []).Select(fns => {
							return new SalFiscalizadorNormaSuscrita() {
								Id = fns.Id,
								IdTipoFiscalizador = fns.TipoFiscalizador!.Id,
								NombreTipoFiscalizador = fns.TipoFiscalizador!.Nombre
							};
						})],
						TemplateNorma = (vencimiento.NormaSuscrita!.TemplateNorma == null) ? null : new SalTemplateNormaObtenerPorIdConVencimiento() {
							IdTemplate = vencimiento.NormaSuscrita!.TemplateNorma!.Template!.Id,
							NombreTemplate = vencimiento.NormaSuscrita!.TemplateNorma!.Template!.Nombre,
							Nombre = vencimiento.NormaSuscrita!.TemplateNorma!.Nombre,
							Descripcion = vencimiento.NormaSuscrita!.TemplateNorma!.Descripcion,
							Multa = vencimiento.NormaSuscrita!.TemplateNorma!.Multa,
							IdTipoPeriodicidad = vencimiento.NormaSuscrita!.TemplateNorma!.TipoPeriodicidad?.Id,
							NombreTipoPeriodicidad = vencimiento.NormaSuscrita!.TemplateNorma!.TipoPeriodicidad?.Nombre,
							IdCategoriaNorma = vencimiento.NormaSuscrita!.TemplateNorma!.CategoriaNorma?.Id,
							NombreCategoriaNorma = vencimiento.NormaSuscrita!.TemplateNorma!.CategoriaNorma?.Nombre,
							Fiscalizadores = [.. (vencimiento.NormaSuscrita!.TemplateNorma!.TemplateNormaFiscalizadores ?? []).Select(fns => {
								return new SalFiscalizadorNormaSuscrita() {
									Id = 0,
									IdTipoFiscalizador = fns.TipoFiscalizador!.Id,
									NombreTipoFiscalizador = fns.TipoFiscalizador!.Nombre
								};
							})]
						},
						FechaVencimiento = vencimiento.FechaVencimiento,
						FechaCompletitud = vencimiento.FechaCompletitud,
						DocumentosAdjuntos = [.. (vencimiento.DocumentosAdjuntos ?? []).Select(da => new SalDocumentoAdjunto() {
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
