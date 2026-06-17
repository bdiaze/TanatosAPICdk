using Amazon.Lambda.Core;
using Scriban.Runtime;
using System.Diagnostics;
using System.Net;
using TanatosAPI.Entities.Others;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;
using TanatosAPI.UseCases;

namespace TanatosAPI.Endpoints {
	public static class PerfilEndpoints {
		public static void MapPerfilEndpoints(this IEndpointRouteBuilder routes) {
			RouteGroupBuilder group = routes.MapGroup("/Perfil");
			group.MapEnviarCodigoVerificacion();

            RouteGroupBuilder publicGroup = routes.MapGroup("/public/Perfil");
            publicGroup.MapConfirmarRegistro();
            publicGroup.MapReenviarCodigoVerificacion();
        }

		private static void MapEnviarCodigoVerificacion(this IEndpointRouteBuilder routes) {
			routes.MapPost("/EnviarCodigoVerificacion", async (EntPerfilEnviarCodigoVerificacion entrada, IHostEnvironment environment, IVariableEntornoHelper variableEntorno, IKMSHelper kmsHelper, HtmlRenderer htmlRenderer, HermesHelper hermesHelper) => {
				Stopwatch stopwatch = Stopwatch.StartNew();

				try {
					(string asunto, string tituloCuerpo, string subtituloCuerpo) = entrada.TipoCodigo switch {
						TipoCodigoVerificacion.SignUp =>
							(
								entrada.Nombre != null ? $"¡Hola {entrada.Nombre}, aquí tu código de verificación!" : "¡Ha llegado tu código de verificación!",
								entrada.Nombre != null ? $"¡Bienvenido {entrada.Nombre} a Todo en Orden!" : "¡Bienvenido a Todo en Orden!",
								"A continuación, te dejamos tu código de verificación para completar el proceso de creación de cuenta:"
							),
						TipoCodigoVerificacion.ForgotPassword =>
							(
								entrada.Nombre != null ? $"¡{entrada.Nombre}, aquí tu código para recuperar tu contraseña!" : "¡Ha llegado tu código para recuperar tu contraseña!",
								"Recuperación de Contraseña",
								"A continuación, te dejamos tu código para que puedas recuperar tu contraseña:"
							),
						TipoCodigoVerificacion.ResendCode =>
							(
								entrada.Nombre != null ? $"¡{entrada.Nombre}, aquí te reenviamos un nuevo código!" : "¡Nuevo código de verificación!",
								"Nuevo Código de Verificación",
								"A continuación, te dejamos tu nuevo código de verificación para completar el proceso de creación de cuenta:"
							),
						TipoCodigoVerificacion.UpdateUserAttribute or
						TipoCodigoVerificacion.VerifyUserAttribute =>
							(
								entrada.Nombre != null ? $"¡{entrada.Nombre}, aquí tu código para verificar tu nuevo correo electrónico!" : "¡Código para verificar tu nuevo correo electrónico!",
								"Código de Verificación",
								"A continuación, te dejamos tu código para verificar tu nuevo correo electrónico:"
							),
						TipoCodigoVerificacion.Authentication =>
							(
								entrada.Nombre != null ? $"¡{entrada.Nombre}, aquí tu código para iniciar sesión!" : "¡Tu código para iniciar sesión!",
								"Código de Inicio de Sesión",
								"A continuación, te dejamos tu código para que puedas iniciar sesión de forma segura:"
							),
						TipoCodigoVerificacion.AdminCreateUser =>
							(
								entrada.Nombre != null ? $"¡{entrada.Nombre}, aquí tu contraseña temporal!" : "¡Ha llegado tu contraseña temporal!",
								"Contraseña Temporal",
								"A continuación, te dejamos tu contraseña temporal para que puedas iniciar sesión:"
							),
						TipoCodigoVerificacion.AccountTakeOverNotification or _ => throw new InvalidOperationException("Tipo de Código inválido")
					};

					SalHermesEnviar retorno = await hermesHelper.EnviarCorreo(new EntHermesCorreoEnviar() {
						De = new DireccionCorreo() {
							Nombre = variableEntorno.Obtener("HERMES_DE_NOMBRE"),
							Correo = variableEntorno.Obtener("HERMES_DE_CORREO"),
						},
						Para = [
							new DireccionCorreo() {
								Correo = entrada.CorreoElectronico
							}
						],
						Asunto = asunto,
						Cuerpo = await htmlRenderer.GenerarHtml("CodigoVerificacion.html", new ScriptObject() {
							["TITULO"] = WebUtility.HtmlEncode(tituloCuerpo),
							["SUBTITULO"] = WebUtility.HtmlEncode(subtituloCuerpo),
							["CODIGO"] = await kmsHelper.Desencriptar(entrada.CodigoEncriptado),
						})
					});

					LambdaLogger.Log(
						$"[POST] - [Perfil] - [EnviarCodigoVerificacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
						$"Envío exitoso del código de verificación - Hermes ID Mensaje: {retorno.IdMensaje}.");

					return Results.Ok();
				} catch (ErrorValidacion ex) {
					LambdaLogger.Log(
						$"[POST] - [Perfil] - [EnviarCodigoVerificacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
						$"Ocurrió un error de validación. " +
						$"{ex}");
					return Results.BadRequest(ex.MensajeGenerico);
				} catch (Exception ex) {
					LambdaLogger.Log(
						$"[POST] - [Perfil] - [EnviarCodigoVerificacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
						$"Ocurrio un error al enviar el código de verificación. " +
						$"{ex}");
					return Results.Problem($"Ocurrio un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
				}
			}).RequireAuthorization(
				"Perfil.Read.All",
				"Perfil.Write.All"
			);
		}

        private static void MapConfirmarRegistro(this IEndpointRouteBuilder routes) {
            routes.MapPost("/ConfirmarRegistro", async (EntPerfilConfirmarRegistro entrada, IHostEnvironment environment, ICognitoHelper cognitoHelper) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
					await cognitoHelper.ConfirmarRegistro(entrada.Username, entrada.Codigo);

                    LambdaLogger.Log(
                        $"[POST] - [Perfil] - [ConfirmarRegistro] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Confirmarción exitosa del registro del usuario.");

                    return Results.Ok();
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[POST] - [Perfil] - [ConfirmarRegistro] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[POST] - [Perfil] - [ConfirmarRegistro] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrio un error al confirmar registro del usuario. " +
                        $"{ex}");
                    return Results.Problem($"Ocurrio un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
                }
            }).AllowAnonymous();
        }

        private static void MapReenviarCodigoVerificacion(this IEndpointRouteBuilder routes) {
            routes.MapPost("/ReenviarCodigoVerificacion", async (EntPerfilReenviarCodigoVerificacion entrada, IHostEnvironment environment, ICognitoHelper cognitoHelper) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
					await cognitoHelper.ReenviarCodigoVerificacion(entrada.Username);

                    LambdaLogger.Log(
                        $"[POST] - [Perfil] - [ReenviarCodigoVerificacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Reenvío exitoso del código de verificación.");

                    return Results.Ok();
                } catch (ErrorValidacion ex) {
                    LambdaLogger.Log(
                        $"[POST] - [Perfil] - [ReenviarCodigoVerificacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status400BadRequest}] - " +
                        $"Ocurrió un error de validación. " +
                        $"{ex}");
                    return Results.BadRequest(ex.MensajeGenerico);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[POST] - [Perfil] - [ReenviarCodigoVerificacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrio un error al reenviar el código de verificación. " +
                        $"{ex}");
                    return Results.Problem($"Ocurrio un error al procesar su solicitud. {(!environment.IsProduction() ? ex : "")}");
                }
            }).AllowAnonymous();
        }
    }
}
