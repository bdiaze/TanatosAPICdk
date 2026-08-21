using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Diagnostics;
using System.Security.Claims;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Entities.Others.DocumentoAdjunto;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;
using TanatosAPI.UseCases;

namespace TanatosAPI.Endpoints {
	public static class DocumentoAdjuntoEndpoints {
		public static IEndpointRouteBuilder MapDocumentoAdjuntoEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/DocumentoAdjunto");
			group.MapObtenerVigentes();
			group.MapGenerarUrlSubidaEndpoint();
			group.MapConfirmarSubidaEndpoint();
			group.MapGenerarUrlBajadaEndpoint();
			group.MapEliminarEndpoint();

			RouteGroupBuilder publicGroup = routes.MapGroup("/public/DocumentoAdjunto");
			publicGroup.MapGenerarUrlSubidaPorCodigoAccesoEndpoint();
			publicGroup.MapConfirmarSubidaPorCodigoAccesoEndpoint();
			publicGroup.MapGenerarUrlBajadaPorCodigoAccesoEndpoint();
			publicGroup.MapEliminarPorCodigoAccesoEndpoint();

			return routes;
		}

		private static void MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes/{idHistorialNormaSuscrita}", async (long idHistorialNormaSuscrita, IHostEnvironment environment, ClaimsPrincipal user, DocumentoAdjuntoUseCase documentoAdjuntoUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					List<DocumentoAdjunto> documentoAdjuntos = await documentoAdjuntoUseCase.ObtenerVigentes(sub, idHistorialNormaSuscrita);

					List<SalDocumentoAdjunto> retorno = [.. documentoAdjuntos.Select(da => new SalDocumentoAdjunto {
						Id = da.Id,
						NombreArchivo = da.NombreArchivo,
						Mime = da.MimeReal ?? da.MimeEsperado,
						Tamanno = da.TamannoReal ?? da.TamannoEsperado,
						FechaSubida = da.FechaConfirmacionSubida
					})];

					LambdaLogger.Log(
						$"[GET] - [DocumentoAdjunto] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los tipos de unidad de tiempo vigentes - Cant. Registros: {retorno.Count}.");
					return Results.Ok(retorno);
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[GET] - [DocumentoAdjunto] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [DocumentoAdjunto] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los tipos de unidad de tiempo vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Vencimientos.Read.Self");
		}

		private static void MapGenerarUrlSubidaEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/GenerarUrlSubida", async (EntDocumentoAdjuntoGenerarUrlSubida entrada, IHostEnvironment environment, ClaimsPrincipal user, DocumentoAdjuntoUseCase documentoAdjuntoUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					(string preSignedUrl, Dictionary<string, string> fields, DocumentoAdjunto documentoAdjunto) = await documentoAdjuntoUseCase.GenerarUrlSubida(
						sub,
						entrada.IdHistorialNormaSuscrita,
						entrada.NombreArchivo,
						entrada.Mime,
						entrada.Tamanno
					);

                    SalDocumentoAdjuntoGenerarUrlSubida retorno = new() {
						IdDocumentoAdjunto = documentoAdjunto.Id,
						PreSignedUrl = preSignedUrl,
						PreSignedFields = fields
					};

                    LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [GenerarUrlSubida] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa de URL prefirmada para subida de documento - ID: {documentoAdjunto.Id}.");
					return Results.Ok(retorno);
				} catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[POST] - [DocumentoAdjunto] - [GenerarUrlSubida] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
						$"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [GenerarUrlSubida] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación de URL prefirmada para subida de documento. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Vencimientos.Write.Self");
		}

		private static void MapConfirmarSubidaEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/ConfirmarSubida", async (EntDocumentoAdjuntoConfirmarSubida entrada, IHostEnvironment environment, ClaimsPrincipal user, DocumentoAdjuntoUseCase documentoAdjuntoUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					await documentoAdjuntoUseCase.ConfirmarSubida(sub, entrada.IdDocumentoAdjunto);

					LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [ConfirmarSubida] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Confirmación exitosa de la subida del documento - ID: {entrada.IdDocumentoAdjunto}.");

					return Results.Ok();
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[POST] - [DocumentoAdjunto] - [ConfirmarSubida] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [ConfirmarSubida] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la confirmación de la subida del documento - ID: {entrada.IdDocumentoAdjunto}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Vencimientos.Write.Self");
		}

		private static void MapGenerarUrlBajadaEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/GenerarUrlBajada", async (EntDocumentoAdjuntoGenerarUrlBajada entrada, IHostEnvironment environment, ClaimsPrincipal user, DocumentoAdjuntoUseCase documentoAdjuntoUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					string presignedUrl = await documentoAdjuntoUseCase.GenerarUrlBajada(sub, entrada.IdDocumentoAdjunto, paraVisualizacion: entrada.ParaVisualizacion);

                    SalDocumentoAdjuntoGenerarUrlBajada retorno = new() {
						PreSignedUrl = presignedUrl
					};

                    LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [GenerarUrlBajada] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Generación exitosa de la URL prefirmada para bajada de documento - ID: {entrada.IdDocumentoAdjunto}.");
					return Results.Ok(retorno);
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[POST] - [DocumentoAdjunto] - [GenerarUrlBajada] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [GenerarUrlBajada] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la generación de la URL prefirmada para bajada de documento - ID: {entrada.IdDocumentoAdjunto}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Vencimientos.Read.Self");
		}

		private static void MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, ClaimsPrincipal user, DocumentoAdjuntoUseCase documentoAdjuntoUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					await documentoAdjuntoUseCase.Eliminar(sub, id);
						
					LambdaLogger.Log(
						$"[DELETE] - [DocumentoAdjunto] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del documento adjunto - ID: {id}.");
					return Results.Ok();
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[DELETE] - [DocumentoAdjunto] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [DocumentoAdjunto] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del documento adjunto - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Vencimientos.Write.Self");
		}

		private static void MapGenerarUrlSubidaPorCodigoAccesoEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/GenerarUrlSubidaPorCodigoAcceso", async (EntDocumentoAdjuntoGenerarUrlSubidaPorCodigoAcceso entrada, IHostEnvironment environment, ClaimsPrincipal user, DocumentoAdjuntoUseCase documentoAdjuntoUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
                    (string preSignedUrl, Dictionary<string, string> fields, DocumentoAdjunto documentoAdjunto) = await documentoAdjuntoUseCase.GenerarUrlSubidaPorCodigoAcceso(
                        entrada.CodigoAcceso,
                        entrada.NombreArchivo,
                        entrada.Mime,
                        entrada.Tamanno
                    );

                    SalDocumentoAdjuntoGenerarUrlSubida retorno = new() {
						IdDocumentoAdjunto = documentoAdjunto.Id,
						PreSignedUrl = preSignedUrl,
						PreSignedFields = fields
					};

                    LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [GenerarUrlSubidaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa de URL prefirmada para subida de documento - ID: {documentoAdjunto.Id}.");
					return Results.Ok(retorno);
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[POST] - [DocumentoAdjunto] - [GenerarUrlSubidaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [GenerarUrlSubidaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación de URL prefirmada para subida de documento. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).AllowAnonymous();
		}

		private static void MapConfirmarSubidaPorCodigoAccesoEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/ConfirmarSubidaPorCodigoAcceso", async (EntDocumentoAdjuntoConfirmarSubidaPorCodigoAcceso entrada, IHostEnvironment environment, ClaimsPrincipal user, DocumentoAdjuntoUseCase documentoAdjuntoUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					await documentoAdjuntoUseCase.ConfirmarSubidaPorCodigoAcceso(entrada.CodigoAcceso, entrada.IdDocumentoAdjunto);
					
					LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [ConfirmarSubidaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Confirmación exitosa de la subida del documento - ID: {entrada.IdDocumentoAdjunto}.");
					return Results.Ok();
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[POST] - [DocumentoAdjunto] - [ConfirmarSubidaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [ConfirmarSubidaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la confirmación de la subida del documento - ID: {entrada.IdDocumentoAdjunto}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).AllowAnonymous();
		}

		private static void MapGenerarUrlBajadaPorCodigoAccesoEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/GenerarUrlBajadaPorCodigoAcceso", async (EntDocumentoAdjuntoGenerarUrlBajadaPorCodigoAcceso entrada, IHostEnvironment environment, ClaimsPrincipal user, DocumentoAdjuntoUseCase documentoAdjuntoUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string presignedUrl = await documentoAdjuntoUseCase.GenerarUrlBajadaPorCodigoAcceso(entrada.CodigoAcceso, entrada.IdDocumentoAdjunto, entrada.ParaVisualizacion);

                    SalDocumentoAdjuntoGenerarUrlBajada retorno = new() {
						PreSignedUrl = presignedUrl
					};

                    LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [GenerarUrlBajadaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Generación exitosa de la URL prefirmada para bajada de documento - ID: {entrada.IdDocumentoAdjunto}.");
					return Results.Ok(retorno);
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[POST] - [DocumentoAdjunto] - [GenerarUrlBajadaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [GenerarUrlBajadaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la generación de la URL prefirmada para bajada de documento - ID: {entrada.IdDocumentoAdjunto}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).AllowAnonymous();
		}

		private static void MapEliminarPorCodigoAccesoEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/PorCodigoAcceso/{id}", async (long id, [FromQuery] string codigoAcceso, IHostEnvironment environment, ClaimsPrincipal user, DocumentoAdjuntoUseCase documentoAdjuntoUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					await documentoAdjuntoUseCase.EliminarPorCodigoAcceso(codigoAcceso, id);

					LambdaLogger.Log(
						$"[DELETE] - [DocumentoAdjunto] - [EliminarPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del documento adjunto - ID: {id}.");
					return Results.Ok();
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[DELETE] - [DocumentoAdjunto] - [EliminarPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [DocumentoAdjunto] - [EliminarPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del documento adjunto - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).AllowAnonymous();
		}

	}
}
