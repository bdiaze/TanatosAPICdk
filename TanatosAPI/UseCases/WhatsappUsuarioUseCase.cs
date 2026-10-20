using Npgsql;
using System.Text.RegularExpressions;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.UseCases;

namespace TanatosAPI.UseCases {
	public class WhatsappUsuarioUseCase(IDatabaseConnectionHelper connectionHelper, IAccesoUseCase accesoUseCase, IWhatsappUsuarioBcp whatsappUsuarioBcp, ISuscripcionBcp suscripcionBcp, ITipoReceptorNotificacionBcp tipoReceptorNotificacionBcp, IUsuarioBcp usuarioBcp) : IWhatsappUsuarioUseCase {
		public async Task<WhatsappUsuario?> ObtenerUltimoVigente(string sub, NpgsqlTransaction? transaction = null) {
			WhatsappUsuario? whatsappUsuario = (await whatsappUsuarioBcp.ObtenerPorSub(sub, filtrarVigentes: true, transaction: transaction))
				.OrderByDescending(wu => wu.FechaCreacion)
				.FirstOrDefault();

			return whatsappUsuario;
		}

		public async Task ActualizarWhatsapp(string sub, string? whatsapp, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				// Se valida el formato del número de Whatsapp ingresado...
				if (whatsapp != null) {
					whatsapp = Regex.Replace(whatsapp, @"\s+", "", RegexOptions.NonBacktracking);
					if (string.IsNullOrWhiteSpace(whatsapp)) whatsapp = null;
					else if (!whatsapp.StartsWith('+')) whatsapp = $"+{whatsapp}";
				}

				// Si viene un número de Whatsapp, se valida que cumpla con el formato y que el plan del usuario permita su uso...
				if (whatsapp != null) {
					TipoReceptorNotificacion tipoReceptor = (await tipoReceptorNotificacionBcp.Obtener(2 /*WhatsApp*/, validarVigencia: true, transaction: transaction!.NpgsqlTransaction()))!;
					if (tipoReceptorNotificacionBcp.RegexInvalido(tipoReceptor, whatsapp)) throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "El formato del número de WhatsApp es inválido.");

					bool tienePlanEmpresa = await suscripcionBcp.ConsultaTienePlanEmpresa(sub, transaction!.NpgsqlTransaction());
					if (!tienePlanEmpresa && tipoReceptor.RequierePlanEmpresa) throw new ErrorValidacion(TipoErrorValidacion.RestringidoPorPlan, $"Tu plan no permite el uso de WhatsApp.");
				}

				// Se obtiene el Whatsapp actual del usuario...
				WhatsappUsuario? whatsappUsuario = await ObtenerUltimoVigente(sub, transaction!.NpgsqlTransaction());

				// Si el Whatsapp recepcionado es distinto al Whatsapp almacenado, se eliminan los Whatsapps vigentes y se crea un nuevo Whatsapp a validar...
				if (whatsappUsuario?.Whatsapp != whatsapp) {
					List<WhatsappUsuario> whatsappVigentes = await whatsappUsuarioBcp.ObtenerPorSub(sub, filtrarVigentes: true, transaction: transaction!.NpgsqlTransaction());
					foreach (WhatsappUsuario whatsappVigente in whatsappVigentes) {
						await whatsappUsuarioBcp.Eliminar(whatsappVigente, transaction!.NpgsqlTransaction());
					}

					if (whatsapp != null) {
						Usuario usuario = await usuarioBcp.Obtener(sub, transaction!.NpgsqlTransaction()) ?? throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "No se encuentra registro del usuario.");

						// Se crea el nuevo Whatsapp del usuario, además se genera un acceso para su validación, y se manda mensaje para validación...
						whatsappUsuario = await whatsappUsuarioBcp.Crear(sub, whatsapp, transaction: transaction!.NpgsqlTransaction());

						(Acceso acceso, string codigoAcceso) = await accesoUseCase.HabilitarAcceso([
							new RecursoSolicitado {
								Tipo = "whatsapp_usuario",
								IdInterno = whatsappUsuario.Id.ToString(),
								Acciones = [ "validar" ]
							}],
							TimeSpan.FromDays(30),
							transaction
						);
						whatsappUsuario.IdAccesoValidacion = acceso.Id;
						whatsappUsuario.HermesIdMensaje = await whatsappUsuarioBcp.EnviarWhatsappValidacion(
							codigoAcceso,
							whatsapp,
							usuario.Nombre ?? "",
							whatsappUsuario.Id
						);
						await whatsappUsuarioBcp.Actualizar(whatsappUsuario, transaction!.NpgsqlTransaction());
					} else {
						whatsappUsuario = null;
					}
				}

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}
			} catch {
				if (ownsTransaction && transaction != null) {
					await transaction.RollbackAsync();
				}
				throw;
			} finally {
				if (ownsTransaction) {
					if (transaction != null) await transaction.DisposeAsync();
					if (connection != null) await connection.DisposeAsync();
				}
			}
		}

		public async Task ValidarWhatsapp(long idWhatsappUsuario, string codigoAcceso, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				// Se valida que el código de acceso tenga permisos para validar el número de Whatsapp...
				_ = await accesoUseCase.ValidarAcceso(codigoAcceso, "whatsapp_usuario", idWhatsappUsuario.ToString(), "validar", transaction!.NpgsqlTransaction());
				
				// Se valida que el número de Whatsapp esté vigente...
				WhatsappUsuario? whatsappUsuario = await whatsappUsuarioBcp.Obtener(idWhatsappUsuario, transaction: transaction!.NpgsqlTransaction());
				if (!whatsappUsuarioBcp.EstaVigente(whatsappUsuario)) {
					throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El número de Whatsapp no existe o no está vigente", "El código de acceso es inválido.");
				}

				// Se procede a validar el número de Whatsapp...
				await whatsappUsuarioBcp.Validar(whatsappUsuario!, transaction!.NpgsqlTransaction());

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}
			} catch {
				if (ownsTransaction && transaction != null) {
					await transaction.RollbackAsync();
				}
				throw;
			} finally {
				if (ownsTransaction) {
					if (transaction != null) await transaction.DisposeAsync();
					if (connection != null) await connection.DisposeAsync();
				}
			}
		}
	}
}
