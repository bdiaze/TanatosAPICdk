using Amazon.Lambda.Core;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;

namespace TanatosAPI.Endpoints {
	public static class DestinatarioNotificacionEndpoints {
		public const short HORAS_CADUCIDAD_CODIGO_VALIDACION = 24;

		public static IEndpointRouteBuilder MapDestinatarioNotificacionEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/DestinatarioNotificacion");
			group.MapObtenerVigentes();
			group.MapCrearEndpoint();
			group.MapEliminarEndpoint();

			RouteGroupBuilder publicGroup = routes.MapGroup("/public/DestinatarioNotificacion");
			publicGroup.MapValidarEndpoint();

			return routes;
		}

		private static IEndpointRouteBuilder MapObtenerVigentes(this IEndpointRouteBuilder routes) {
			routes.MapGet("/Vigentes", async (IHostEnvironment environment, ClaimsPrincipal user, DestinatarioNotificacionDao destinatarioNotificacionDao, TipoReceptorNotificacionDao tipoReceptorNotificacionDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					List<TipoReceptorNotificacion> receptores = await tipoReceptorNotificacionDao.ObtenerPorVigencia(null);

					List<SalDestinatarioNotificacion> retorno = [.. (await destinatarioNotificacionDao.ObtenerPorSub(sub, true))
						.Select(d => new SalDestinatarioNotificacion() {
							Id = d.Id,
							IdTipoReceptor = d.IdTipoReceptor,
							NombreTipoReceptor = receptores.FirstOrDefault(r => r.Id == d.IdTipoReceptor)?.Nombre,
							Destino = d.Destino,
							Validado = d.Validado
						})
					];

					LambdaLogger.Log(
						$"[GET] - [DestinatarioNotificacion] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Obtención exitosa de los destinatarios de notificaciones vigentes - Cant. Registros: {retorno.Count}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[GET] - [DestinatarioNotificacion] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al obtener los destinatarios de notificaciones vigentes. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization().WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/", async (EntDestinatarioNotificacionCrear entrada, IHostEnvironment environment, ClaimsPrincipal user, CrytoHelper cryptoHelper, VariableEntornoHelper variableEntorno, DestinatarioNotificacionDao destinatarioNotificacionDao, TipoReceptorNotificacionDao tipoReceptorNotificacionDao, HermesHelper hermesHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					// Se valida que el usuario no tenga otro destinatario igual...
					List<DestinatarioNotificacion> destVigentes = await destinatarioNotificacionDao.ObtenerPorSub(sub, true);
					if (destVigentes.Any(d => d.Sub == sub && d.IdTipoReceptor == entrada.IdTipoReceptor && d.Destino == entrada.Destino)) {
						LambdaLogger.Log(
							$"[POST] - [DestinatarioNotificacion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"Ya tienes registrado dicho destinatario.");

						return Results.BadRequest($"Ya tienes registrado dicho destinatario.");
					}

					// Se valida que el tipo de receptor sea válido...
					TipoReceptorNotificacion? tipoReceptor = await tipoReceptorNotificacionDao.ObtenerPorId(entrada.IdTipoReceptor);
					if (tipoReceptor == null || !tipoReceptor.Vigencia) {
						LambdaLogger.Log(
							$"[POST] - [DestinatarioNotificacion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El tipo de receptor de notificación es inválido.");

						return Results.BadRequest($"El tipo de receptor de notificación es inválido.");
					}

					// Se valida regex del tipo de receptor...
					if (!string.IsNullOrEmpty(tipoReceptor.RegexValidacion) && !Regex.IsMatch(entrada.Destino, tipoReceptor.RegexValidacion)) {
						LambdaLogger.Log(
							$"[POST] - [DestinatarioNotificacion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El formato del destino no es válido.");

						return Results.BadRequest($"El formato del destino no es válido.");
					}

					// Se crea un código de validación...
					string codigoValidacion = cryptoHelper.GenerarToken(12);
					DestinatarioNotificacion? mismoCodigo = await destinatarioNotificacionDao.ObtenerPorCodigoValidacion(cryptoHelper.HashSHA256(codigoValidacion));
					while (mismoCodigo != null) {
						codigoValidacion = cryptoHelper.GenerarToken(12);
						mismoCodigo = await destinatarioNotificacionDao.ObtenerPorCodigoValidacion(cryptoHelper.HashSHA256(codigoValidacion));
					}

					DestinatarioNotificacion nuevoDestinatario = new() { 
						Id = 0,
						Sub = sub,
						IdTipoReceptor = entrada.IdTipoReceptor,
						Destino = entrada.Destino,
						CodigoValidacion = cryptoHelper.HashSHA256(codigoValidacion),
						FechaCaducidadCodigoValidacion = DateTime.UtcNow.AddHours(HORAS_CADUCIDAD_CODIGO_VALIDACION),
						Validado = false,
						FechaCreacion = DateTime.UtcNow,
						Vigencia = true
					};
					nuevoDestinatario.Id = await destinatarioNotificacionDao.Insertar(nuevoDestinatario);

					// Se envía mensaje con el código de validación...
					if (nuevoDestinatario.IdTipoReceptor == 1) {
						await hermesHelper.EnviarCorreo(new EntHermesCorreoEnviar() {
							De = new DireccionCorreo() { 
								Nombre = variableEntorno.Obtener("HERMES_DE_NOMBRE"),
								Correo = variableEntorno.Obtener("HERMES_DE_CORREO"),
							},
							Para = [
								new DireccionCorreo() { 
									Correo = nuevoDestinatario.Destino
								}
							],
							Asunto = variableEntorno.Obtener("HERMES_ASUNTO_CREAR_DESTINATARIO"),
							Cuerpo = variableEntorno.Obtener("HERMES_CUERPO_CREAR_DESTINATARIO").Replace("[CODIGO_VALIDACION]", WebUtility.UrlEncode(codigoValidacion)),
						});
					}

					SalDestinatarioNotificacion retorno = new() {
						Id = nuevoDestinatario.Id,
						IdTipoReceptor = nuevoDestinatario.IdTipoReceptor,
						Destino = nuevoDestinatario.Destino,
						Validado = nuevoDestinatario.Validado
					};

					LambdaLogger.Log(
						$"[POST] - [DestinatarioNotificacion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Creación exitosa del destinatario de notificación - ID: {retorno.Id}.");

					return Results.Ok(retorno);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [DestinatarioNotificacion] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la creación del destinatario de notificación. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization().WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapDelete("/{id}", async (long id, IHostEnvironment environment, ClaimsPrincipal user, DestinatarioNotificacionDao destinatarioNotificacionDao) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					string sub = user.Identity?.Name ?? throw new Exception("No se incluye la información del usuario.");

					DestinatarioNotificacion? existente = (await destinatarioNotificacionDao.ObtenerPorSub(sub)).FirstOrDefault(d => d.Id == id);

					if (existente == null) {
						LambdaLogger.Log(
							$"[DELETE] - [DestinatarioNotificacion] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
							$"El usuario no posee un destinatario de notificación con ID {id}.");

						return Results.BadRequest($"El usuario no posee un destinatario de notificación con ID {id}.");
					}

					existente.FechaEliminacion = DateTime.UtcNow;
					existente.Vigencia = false;
					await destinatarioNotificacionDao.Actualizar(existente);

					LambdaLogger.Log(
						$"[DELETE] - [DestinatarioNotificacion] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Eliminación exitosa del destinatario de notificación - ID: {id}.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[DELETE] - [DestinatarioNotificacion] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error en la eliminación del destinatario de notificación - ID: {id}. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization().WithOpenApi();

			return routes;
		}

		private static IEndpointRouteBuilder MapValidarEndpoint(this IEndpointRouteBuilder routes) {
			routes.MapPost("/Validar/", async (IHostEnvironment environment, EntDestinatarioNotificacionValidar entrada, DestinatarioNotificacionDao destinatarioNotificacionDao, CrytoHelper cryptoHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					DestinatarioNotificacion? destinatarioNotificacion = await destinatarioNotificacionDao.ObtenerPorCodigoValidacion(cryptoHelper.HashSHA256(entrada.CodigoValidacion));

					// Se valida que el código exista...
					if (destinatarioNotificacion == null) {
						LambdaLogger.Log(
							$"[POST] - [DestinatarioNotificacion] - [Validar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status404NotFound}] - " +
							$"Código ingresado no es válido");

						return Results.NotFound("Código ingresado no es válido");
					}

					// Si el código aun no ha sido validado, se verifica la fecha de caducidad y se valida...
					if (!destinatarioNotificacion.Validado) {
						if (destinatarioNotificacion.FechaCaducidadCodigoValidacion < DateTime.UtcNow) {
							LambdaLogger.Log(
								$"[POST] - [DestinatarioNotificacion] - [Validar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
								$"El código ingresado ya caducó");

							return Results.BadRequest("El código ingresado ya caducó");
						} else {
							destinatarioNotificacion.Validado = true;
							destinatarioNotificacion.FechaValidacion = DateTime.UtcNow;
							await destinatarioNotificacionDao.Actualizar(destinatarioNotificacion);
						}
					}

					LambdaLogger.Log(
						$"[POST] - [DestinatarioNotificacion] - [Validar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Se valida exitosamente el destinatario de notificación.");

					return Results.Ok();
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [DestinatarioNotificacion] - [Validar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrió un error al validar el destinatario de notificación. " +
						$"{ex}");
					return Results.Problem($"Ocurrió un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).AllowAnonymous().WithOpenApi();

			return routes;
		}
	}
}
