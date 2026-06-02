using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Diagnostics;
using System.Security.Claims;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
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

		private static IEndpointRouteBuilder MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes/{idHistorialNormaSuscrita}", async (long idHistorialNormaSuscrita, IHostEnvironment environment, ClaimsPrincipal user, NormaSuscritaDao normaSuscritaDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, DocumentoAdjuntoDao documentoAdjuntoDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					HistorialNormaSuscrita? historialNormaSuscrita = await historialNormaSuscritaDao.ObtenerPorId(idHistorialNormaSuscrita);
					if (historialNormaSuscrita == null || !historialNormaSuscrita.Vigencia) {
						LambdaLogger.Log(
							$"[GET] - [DocumentoAdjunto] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El ID de historial norma suscrita es inválido.");

						return Results.BadRequest($"El ID de historial norma suscrita es inválido.");
					}

					NormaSuscrita? normaSuscrita = await normaSuscritaDao.ObtenerPorId(historialNormaSuscrita.IdNormaSuscrita);
					if (normaSuscrita == null || !normaSuscrita.Vigencia || normaSuscrita.Sub != sub) {
						LambdaLogger.Log(
							$"[GET] - [DocumentoAdjunto] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El ID de historial norma suscrita es inválido.");

						return Results.BadRequest($"El ID de historial norma suscrita es inválido.");
					}

					List<DocumentoAdjunto> documentoAdjuntos = [.. (await documentoAdjuntoDao.ObtenerPorHistorial(historialNormaSuscrita.Id, true)).Where(da => da.EstadoSubida == 1)];

					List<SalDocumentoAdjunto> retorno = [.. documentoAdjuntos.Select(da => new SalDocumentoAdjunto {
						Id = da.Id,
						NombreArchivo = da.NombreArchivo,
						FechaSubida = da.FechaConfirmacionSubida
					})];

					LambdaLogger.Log(
						$"[GET] - [DocumentoAdjunto] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los tipos de unidad de tiempo vigentes - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [DocumentoAdjunto] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los tipos de unidad de tiempo vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Vencimientos.Read.Self");

			return routes;
		}

		private static void MapGenerarUrlSubidaEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/GenerarUrlSubida", async (EntDocumentoAdjuntoGenerarUrlSubida entrada, IHostEnvironment environment, ClaimsPrincipal user, DocumentoAdjuntoUseCase documentoAdjuntoUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					entrada.NombreArchivo = entrada.NombreArchivo.Trim();
					entrada.Mime = entrada.Mime.Trim();

					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					(string preSignedUrl, Dictionary<string, string> fields, DocumentoAdjunto documentoAdjunto) = await documentoAdjuntoUseCase.GenerarUrlSubida(
						sub,
						entrada.IdHistorialNormaSuscrita,
						entrada.NombreArchivo,
						entrada.Mime,
						entrada.Tamanno
					);

					LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [GenerarUrlSubida] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa de URL prefirmada para subida de documento - ID: {documentoAdjunto.Id}.");

					return Results.Ok(new SalDocumentoAdjuntoGenerarUrlSubida {
						IdDocumentoAdjunto = documentoAdjunto.Id,
						PreSignedUrl = preSignedUrl,
						PreSignedFields = fields
					});
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

		private static IEndpointRouteBuilder MapConfirmarSubidaEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/ConfirmarSubida", async (EntDocumentoAdjuntoConfirmarSubida entrada, IHostEnvironment environment, ClaimsPrincipal user, DocumentoAdjuntoHelper documentoAdjuntoHelper, NormaSuscritaDao normaSuscritaDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, DocumentoAdjuntoDao documentoAdjuntoDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					DocumentoAdjunto? documentoAdjunto = await documentoAdjuntoDao.ObtenerPorId(entrada.IdDocumentoAdjunto);
					if (documentoAdjunto == null || !documentoAdjunto.Vigencia) {
						LambdaLogger.Log(
							$"[POST] - [DocumentoAdjunto] - [ConfirmarSubida] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El ID de documento adjunto es inválido.");

						return Results.BadRequest($"El ID de documento adjunto es inválido.");
					}

					HistorialNormaSuscrita? historialNormaSuscrita = await historialNormaSuscritaDao.ObtenerPorId(documentoAdjunto.IdHistorialNormaSuscrita);
					if (historialNormaSuscrita == null || !historialNormaSuscrita.Vigencia || historialNormaSuscrita.FechaCompletitud != null) {
						LambdaLogger.Log(
							$"[POST] - [DocumentoAdjunto] - [ConfirmarSubida] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El ID de documento adjunto es inválido.");

						return Results.BadRequest($"El ID de documento adjunto es inválido.");
					}

					NormaSuscrita? normaSuscrita = await normaSuscritaDao.ObtenerPorId(historialNormaSuscrita.IdNormaSuscrita);
					if (normaSuscrita == null || !normaSuscrita.Vigencia || normaSuscrita.Sub != sub) {
						LambdaLogger.Log(
							$"[POST] - [DocumentoAdjunto] - [ConfirmarSubida] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El ID de documento adjunto es inválido.");

						return Results.BadRequest($"El ID de documento adjunto es inválido.");
					}

					if (documentoAdjunto.EstadoSubida != 1 /* Documento recepcionado */) {
						(long contentLength, string contentType) metadata = await documentoAdjuntoHelper.ObtenerMetadata(documentoAdjunto.BucketKey);

						documentoAdjunto.MimeReal = metadata.contentType;
						documentoAdjunto.TamannoReal = metadata.contentLength;
						documentoAdjunto.EstadoSubida = 1 /* Documento recepcionado */;
						documentoAdjunto.FechaConfirmacionSubida = DateTime.UtcNow;

						await documentoAdjuntoDao.Actualizar(documentoAdjunto);
					}

					LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [ConfirmarSubida] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Confirmación exitosa de la subida del documento - ID: {documentoAdjunto.Id}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [ConfirmarSubida] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la confirmación de la subida del documento - ID: {entrada.IdDocumentoAdjunto}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Vencimientos.Write.Self");

			return routes;
		}

		private static IEndpointRouteBuilder MapGenerarUrlBajadaEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/GenerarUrlBajada", async (EntDocumentoAdjuntoGenerarUrlBajada entrada, IHostEnvironment environment, ClaimsPrincipal user, DocumentoAdjuntoHelper documentoAdjuntoHelper, NormaSuscritaDao normaSuscritaDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, DocumentoAdjuntoDao documentoAdjuntoDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					DocumentoAdjunto? documentoAdjunto = await documentoAdjuntoDao.ObtenerPorId(entrada.IdDocumentoAdjunto);
					if (documentoAdjunto == null || !documentoAdjunto.Vigencia) {
						LambdaLogger.Log(
							$"[POST] - [DocumentoAdjunto] - [GenerarUrlBajada] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El ID de documento adjunto es inválido.");

						return Results.BadRequest($"El ID de documento adjunto es inválido.");
					}

					HistorialNormaSuscrita? historialNormaSuscrita = await historialNormaSuscritaDao.ObtenerPorId(documentoAdjunto.IdHistorialNormaSuscrita);
					// Solo no se deja descargar el documento si el vencimiento no existe, no está vigente ni completado...
					if (historialNormaSuscrita == null || (!historialNormaSuscrita.Vigencia && historialNormaSuscrita.FechaCompletitud == null)) {
						LambdaLogger.Log(
							$"[POST] - [DocumentoAdjunto] - [GenerarUrlBajada] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El ID de documento adjunto es inválido.");

						return Results.BadRequest($"El ID de documento adjunto es inválido.");
					}

					NormaSuscrita? normaSuscrita = await normaSuscritaDao.ObtenerPorId(historialNormaSuscrita.IdNormaSuscrita);
					// Solo no se deja descargar el documento si la norma suscrita no existe, no pertenece al usuario, o si no está vigente (con el vencimiento sin completar)...
					if (normaSuscrita == null || (!normaSuscrita.Vigencia && historialNormaSuscrita.FechaCompletitud == null) || normaSuscrita.Sub != sub) {
						LambdaLogger.Log(
							$"[POST] - [DocumentoAdjunto] - [GenerarUrlBajada] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El ID de documento adjunto es inválido.");

						return Results.BadRequest($"El ID de documento adjunto es inválido.");
					}

					string presignedUrl = await documentoAdjuntoHelper.ObtenerGetPreSignedUrl(documentoAdjunto.BucketKey, documentoAdjunto.NombreArchivo);

					LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [GenerarUrlBajada] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Generación exitosa de la URL prefirmada para bajada de documento - ID: {documentoAdjunto.Id}.");

					return Results.Ok(new SalDocumentoAdjuntoGenerarUrlBajada {
						PreSignedUrl = presignedUrl
					});
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [GenerarUrlBajada] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la generación de la URL prefirmada para bajada de documento - ID: {entrada.IdDocumentoAdjunto}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Vencimientos.Read.Self");

			return routes;
		}

		private static IEndpointRouteBuilder MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, ClaimsPrincipal user, DocumentoAdjuntoHelper documentoAdjuntoHelper, DocumentoAdjuntoBcp documentoAdjuntoBcp, NormaSuscritaDao normaSuscritaDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, DocumentoAdjuntoDao documentoAdjuntoDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new InvalidOperationException(Constant.CONST_SIN_INFO_USUARIO);

					DocumentoAdjunto? documentoAdjunto = await documentoAdjuntoDao.ObtenerPorId(id);
					if (documentoAdjunto == null || !documentoAdjunto.Vigencia) {
						LambdaLogger.Log(
							$"[DELETE] - [DocumentoAdjunto] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El ID de documento adjunto es inválido.");

						return Results.BadRequest($"El ID de documento adjunto es inválido.");
					}

					HistorialNormaSuscrita? historialNormaSuscrita = await historialNormaSuscritaDao.ObtenerPorId(documentoAdjunto.IdHistorialNormaSuscrita);
					if (historialNormaSuscrita == null || !historialNormaSuscrita.Vigencia || historialNormaSuscrita.FechaCompletitud != null) {
						LambdaLogger.Log(
							$"[DELETE] - [DocumentoAdjunto] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El ID de documento adjunto es inválido.");

						return Results.BadRequest($"El ID de documento adjunto es inválido.");
					}

					NormaSuscrita? normaSuscrita = await normaSuscritaDao.ObtenerPorId(historialNormaSuscrita.IdNormaSuscrita);
					if (normaSuscrita == null || !normaSuscrita.Vigencia || normaSuscrita.Sub != sub) {
						LambdaLogger.Log(
							$"[DELETE] - [DocumentoAdjunto] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El ID de documento adjunto es inválido.");

						return Results.BadRequest($"El ID de documento adjunto es inválido.");
					}

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();
					
					try {
						await documentoAdjuntoBcp.Eliminar(documentoAdjunto);

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					LambdaLogger.Log(
						$"[DELETE] - [DocumentoAdjunto] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del documento adjunto - ID: {id}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [DocumentoAdjunto] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del documento adjunto - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization("Vencimientos.Write.Self");

			return routes;
		}

		private static void MapGenerarUrlSubidaPorCodigoAccesoEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/GenerarUrlSubidaPorCodigoAcceso", async (EntDocumentoAdjuntoGenerarUrlSubidaPorCodigoAcceso entrada, IHostEnvironment environment, ClaimsPrincipal user, DocumentoAdjuntoUseCase documentoAdjuntoUseCase) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					entrada.NombreArchivo = entrada.NombreArchivo.Trim();
					entrada.Mime = entrada.Mime.Trim();

                    (string preSignedUrl, Dictionary<string, string> fields, DocumentoAdjunto documentoAdjunto) = await documentoAdjuntoUseCase.GenerarUrlSubidaPorCodigoAcceso(
                        entrada.CodigoAcceso,
                        entrada.NombreArchivo,
                        entrada.Mime,
                        entrada.Tamanno
                    );

					LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [GenerarUrlSubidaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa de URL prefirmada para subida de documento - ID: {documentoAdjunto.Id}.");

					return Results.Ok(new SalDocumentoAdjuntoGenerarUrlSubida {
						IdDocumentoAdjunto = documentoAdjunto.Id,
						PreSignedUrl = preSignedUrl,
						PreSignedFields = fields
					});
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

		private static IEndpointRouteBuilder MapConfirmarSubidaPorCodigoAccesoEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/ConfirmarSubidaPorCodigoAcceso", async (EntDocumentoAdjuntoConfirmarSubidaPorCodigoAcceso entrada, IHostEnvironment environment, ClaimsPrincipal user, CryptoHelper cryptoHelper, DocumentoAdjuntoHelper documentoAdjuntoHelper, NormaSuscritaDao normaSuscritaDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, HistorialNotificacionDao historialNotificacionDao, DocumentoAdjuntoDao documentoAdjuntoDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					// Se valida que el código de acceso exista, esté vigente y no haya caducado...
					HistorialNotificacion? historialNotificacion = await historialNotificacionDao.ObtenerPorCodigoAcceso(cryptoHelper.HashSHA256(entrada.CodigoAcceso), true);
					if (historialNotificacion == null || !historialNotificacion.Vigencia || historialNotificacion.FechaCaducidadCodigoAcceso < DateTime.UtcNow) {
						LambdaLogger.Log(
							$"[POST] - [DocumentoAdjunto] - [ConfirmarSubidaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El código de acceso es inválido.");

						return Results.BadRequest($"El código de acceso es inválido.");
					}

					// Se valida que el documento este vigente...
					DocumentoAdjunto? documentoAdjunto = await documentoAdjuntoDao.ObtenerPorId(entrada.IdDocumentoAdjunto);
					if (documentoAdjunto == null || !documentoAdjunto.Vigencia || historialNotificacion.IdHistorialNormaSuscrita != documentoAdjunto.IdHistorialNormaSuscrita) {
						LambdaLogger.Log(
							$"[POST] - [DocumentoAdjunto] - [ConfirmarSubidaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El documento adjunto indicado es inválido.");

						return Results.BadRequest($"El documento adjunto indicado es inválido.");
					}

					// Se valida que el vencimiento este vigente y sin completar...
					HistorialNormaSuscrita? historialNormaSuscrita = await historialNormaSuscritaDao.ObtenerPorId(documentoAdjunto.IdHistorialNormaSuscrita);
					if (historialNormaSuscrita == null || !historialNormaSuscrita.Vigencia || historialNormaSuscrita.FechaCompletitud != null) {
						LambdaLogger.Log(
							$"[POST] - [DocumentoAdjunto] - [ConfirmarSubidaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"La obligación no permite la subida de documentos adjuntos.");

						return Results.BadRequest($"La obligación no permite la subida de documentos adjuntos.");
					}

					// Se valida que la obligación este vigente...
					NormaSuscrita? normaSuscrita = await normaSuscritaDao.ObtenerPorId(historialNormaSuscrita.IdNormaSuscrita);
					if (normaSuscrita == null || !normaSuscrita.Vigencia) {
						LambdaLogger.Log(
							$"[POST] - [DocumentoAdjunto] - [ConfirmarSubidaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"La obligación no permite la subida de documentos adjuntos.");

						return Results.BadRequest($"La obligación no permite la subida de documentos adjuntos.");
					}

					if (documentoAdjunto.EstadoSubida != 1 /* Documento recepcionado */) {
						(long contentLength, string contentType) metadata = await documentoAdjuntoHelper.ObtenerMetadata(documentoAdjunto.BucketKey);

						documentoAdjunto.MimeReal = metadata.contentType;
						documentoAdjunto.TamannoReal = metadata.contentLength;
						documentoAdjunto.EstadoSubida = 1 /* Documento recepcionado */;
						documentoAdjunto.FechaConfirmacionSubida = DateTime.UtcNow;

						await documentoAdjuntoDao.Actualizar(documentoAdjunto);
					}

					LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [ConfirmarSubidaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Confirmación exitosa de la subida del documento - ID: {documentoAdjunto.Id}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [ConfirmarSubidaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la confirmación de la subida del documento - ID: {entrada.IdDocumentoAdjunto}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).AllowAnonymous();

			return routes;
		}

		private static IEndpointRouteBuilder MapGenerarUrlBajadaPorCodigoAccesoEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/GenerarUrlBajadaPorCodigoAcceso", async (EntDocumentoAdjuntoGenerarUrlBajadaPorCodigoAcceso entrada, IHostEnvironment environment, ClaimsPrincipal user, CryptoHelper cryptoHelper, DocumentoAdjuntoHelper documentoAdjuntoHelper, NormaSuscritaDao normaSuscritaDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, HistorialNotificacionDao historialNotificacionDao, DocumentoAdjuntoDao documentoAdjuntoDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					// Se valida que el código de acceso exista, esté vigente y no haya caducado...
					HistorialNotificacion? historialNotificacion = await historialNotificacionDao.ObtenerPorCodigoAcceso(cryptoHelper.HashSHA256(entrada.CodigoAcceso), true);
					if (historialNotificacion == null || !historialNotificacion.Vigencia || historialNotificacion.FechaCaducidadCodigoAcceso < DateTime.UtcNow) {
						LambdaLogger.Log(
							$"[POST] - [DocumentoAdjunto] - [GenerarUrlBajadaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El código de acceso es inválido.");

						return Results.BadRequest($"El código de acceso es inválido.");
					}

					// Se valida que el documento este vigente y este asociado al código de acceso...
					DocumentoAdjunto? documentoAdjunto = await documentoAdjuntoDao.ObtenerPorId(entrada.IdDocumentoAdjunto);
					if (documentoAdjunto == null || !documentoAdjunto.Vigencia || historialNotificacion.IdHistorialNormaSuscrita != documentoAdjunto.IdHistorialNormaSuscrita) {
						LambdaLogger.Log(
							$"[POST] - [DocumentoAdjunto] - [GenerarUrlBajadaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El documento adjunto indicado es inválido.");

						return Results.BadRequest($"El documento adjunto indicado es inválido.");
					}

					// Solo no se deja descargar el documento si el vencimiento no existe, no está vigente ni completado...
					HistorialNormaSuscrita? historialNormaSuscrita = await historialNormaSuscritaDao.ObtenerPorId(documentoAdjunto.IdHistorialNormaSuscrita);
					if (historialNormaSuscrita == null || (!historialNormaSuscrita.Vigencia && historialNormaSuscrita.FechaCompletitud == null)) {
						LambdaLogger.Log(
							$"[POST] - [DocumentoAdjunto] - [GenerarUrlBajadaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"La obligación no permite la descarga de documentos adjuntos.");

						return Results.BadRequest($"La obligación no permite la descarga de documentos adjuntos.");
					}

					NormaSuscrita? normaSuscrita = await normaSuscritaDao.ObtenerPorId(historialNormaSuscrita.IdNormaSuscrita);
					// Solo no se deja descargar el documento si la norma suscrita no existe, no pertenece al usuario, o si no está vigente (con el vencimiento sin completar)...
					if (normaSuscrita == null || (!normaSuscrita.Vigencia && historialNormaSuscrita.FechaCompletitud == null)) {
						LambdaLogger.Log(
							$"[POST] - [DocumentoAdjunto] - [GenerarUrlBajadaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"La obligación no permite la descarga de documentos adjuntos.");

						return Results.BadRequest($"La obligación no permite la descarga de documentos adjuntos.");
					}

					string presignedUrl = await documentoAdjuntoHelper.ObtenerGetPreSignedUrl(documentoAdjunto.BucketKey, documentoAdjunto.NombreArchivo);

					LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [GenerarUrlBajadaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Generación exitosa de la URL prefirmada para bajada de documento - ID: {documentoAdjunto.Id}.");

					return Results.Ok(new SalDocumentoAdjuntoGenerarUrlBajada {
						PreSignedUrl = presignedUrl
					});
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [DocumentoAdjunto] - [GenerarUrlBajadaPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la generación de la URL prefirmada para bajada de documento - ID: {entrada.IdDocumentoAdjunto}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).AllowAnonymous();

			return routes;
		}

		private static IEndpointRouteBuilder MapEliminarPorCodigoAccesoEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/PorCodigoAcceso/{id}", async (long id, [FromQuery] string codigoAcceso, IHostEnvironment environment, DatabaseConnectionHelper connectionHelper, ClaimsPrincipal user, CryptoHelper cryptoHelper, DocumentoAdjuntoHelper documentoAdjuntoHelper, DocumentoAdjuntoBcp documentoAdjuntoBcp, NormaSuscritaDao normaSuscritaDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, HistorialNotificacionDao historialNotificacionDao, DocumentoAdjuntoDao documentoAdjuntoDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					// Se valida que el código de acceso exista, esté vigente y no haya caducado...
					HistorialNotificacion? historialNotificacion = await historialNotificacionDao.ObtenerPorCodigoAcceso(cryptoHelper.HashSHA256(codigoAcceso), true);
					if (historialNotificacion == null || !historialNotificacion.Vigencia || historialNotificacion.FechaCaducidadCodigoAcceso < DateTime.UtcNow) {
						LambdaLogger.Log(
							$"[DELETE] - [DocumentoAdjunto] - [EliminarPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El código de acceso es inválido.");

						return Results.BadRequest($"El código de acceso es inválido.");
					}

					// Se valida que el documento este vigente...
					DocumentoAdjunto? documentoAdjunto = await documentoAdjuntoDao.ObtenerPorId(id);
					if (documentoAdjunto == null || !documentoAdjunto.Vigencia || historialNotificacion.IdHistorialNormaSuscrita != documentoAdjunto.IdHistorialNormaSuscrita) {
						LambdaLogger.Log(
							$"[DELETE] - [DocumentoAdjunto] - [EliminarPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El documento adjunto no está vigente.");

						return Results.BadRequest($"El documento adjunto no está vigente.");
					}

					// Se valida que el vencimiento este vigente y sin completar...
					HistorialNormaSuscrita? historialNormaSuscrita = await historialNormaSuscritaDao.ObtenerPorId(documentoAdjunto.IdHistorialNormaSuscrita);
					if (historialNormaSuscrita == null || !historialNormaSuscrita.Vigencia || historialNormaSuscrita.FechaCompletitud != null) {
						LambdaLogger.Log(
							$"[DELETE] - [DocumentoAdjunto] - [EliminarPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"La obligación no permite eliminar documentos adjuntos.");

						return Results.BadRequest($"La obligación no permite eliminar documentos adjuntos.");
					}

					// Se valida que la obligación este vigente...
					NormaSuscrita? normaSuscrita = await normaSuscritaDao.ObtenerPorId(historialNormaSuscrita.IdNormaSuscrita);
					if (normaSuscrita == null || !normaSuscrita.Vigencia) {
						LambdaLogger.Log(
							$"[DELETE] - [DocumentoAdjunto] - [EliminarPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"La obligación no permite eliminar documentos adjuntos.");

						return Results.BadRequest($"La obligación no permite eliminar documentos adjuntos.");
					}

					await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
					await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

					try {
						await documentoAdjuntoBcp.Eliminar(documentoAdjunto);

						await transaction.CommitAsync();
					} catch {
						await transaction.RollbackAsync();
						throw;
					}

					LambdaLogger.Log(
						$"[DELETE] - [DocumentoAdjunto] - [EliminarPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del documento adjunto - ID: {id}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [DocumentoAdjunto] - [EliminarPorCodigoAcceso] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del documento adjunto - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).AllowAnonymous();

			return routes;
		}

	}
}
