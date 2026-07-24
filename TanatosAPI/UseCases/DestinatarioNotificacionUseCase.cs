using Npgsql;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.UseCases {
    public class DestinatarioNotificacionUseCase(IDatabaseConnectionHelper connectionHelper, NegocioUseCase negocioUseCase, IDestinatarioNotificacionBcp destinatarioNotificacionBcp, INegocioBcp negocioBcp, IUsuarioBcp usuarioBcp, ISuscripcionBcp suscripcionBcp, ITipoReceptorNotificacionDao tipoReceptorNotificacionDao) {
        public const short HORAS_CADUCIDAD_CODIGO_VALIDACION = 24;

        public async Task<DestinatarioNotificacion> RegistrarDestinatario(string sub, long idNegocio, long? idEmpleado, long idTipoReceptor, string? alias, string destino, bool yaValidado = false, NpgsqlTransaction? transaction = null) {
            bool ownsTransaction = transaction == null;
            NpgsqlConnection? connection = null;
            try {
                if (ownsTransaction) {
                    connection = await connectionHelper.ObtenerConexion();
                    transaction = await connection.BeginTransactionAsync();
                }

                (DestinatarioNotificacion destinatarioNotificacion, string codigoValidacion) = await destinatarioNotificacionBcp.Insertar(
                    sub,
                    idNegocio,
                    idEmpleado,
                    idTipoReceptor,
                    alias,
                    destino,
                    yaValidado,
                    transaction
                );

                if (!destinatarioNotificacion.Validado) {
                    if (destinatarioNotificacion.IdTipoReceptor == 1 /* Correo electrónico */) {
                        Negocio negocio = await negocioBcp.Obtener(destinatarioNotificacion.IdNegocio, filtrarVigente: true, validarSub: destinatarioNotificacion.Sub, transaction: transaction) ?? throw new InvalidOperationException("ID de negocio no válido");
                        Usuario usuario = await usuarioBcp.ObtenerInformacionUsuario(destinatarioNotificacion.Sub, transaction);

                        string idMensaje = await destinatarioNotificacionBcp.EnviarCorreoValidacionDestinatario(
                            destinatarioNotificacion.Destino, 
                            usuario.Nombre ?? "",
                            negocio.Nombre,
                            codigoValidacion
                        );

                        await destinatarioNotificacionBcp.RegistrarHermesIdMensaje(destinatarioNotificacion, idMensaje, transaction);

                    } else if (destinatarioNotificacion.IdTipoReceptor == 2 /* Whatsapp */) {
                        Negocio negocio = await negocioBcp.Obtener(destinatarioNotificacion.IdNegocio, filtrarVigente: true, validarSub: destinatarioNotificacion.Sub, transaction: transaction) ?? throw new InvalidOperationException("ID de negocio no válido");
                        Usuario usuario = await usuarioBcp.ObtenerInformacionUsuario(destinatarioNotificacion.Sub, transaction);

                        string idMensaje = await destinatarioNotificacionBcp.EnviarWhatsappValidacionDestinatario(
                            destinatarioNotificacion.Destino,
                            usuario.Nombre ?? "",
                            negocio.Nombre,
                            codigoValidacion
                        );

                        await destinatarioNotificacionBcp.RegistrarHermesIdMensaje(destinatarioNotificacion, idMensaje, transaction);
                    }
                }

                if (ownsTransaction) {
                    await transaction!.CommitAsync();
                }

                return destinatarioNotificacion;
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
    
        public async Task ValidarDestinatario(string codigoValidacion) {
            DestinatarioNotificacion? destinatarioNotificacion = await destinatarioNotificacionBcp.ObtenerPorCodigoValidacion(codigoValidacion);
            if (!destinatarioNotificacionBcp.EstaVigente(destinatarioNotificacion)) {
                throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El destinatario no existe o no está vigente", "Código ingresado no es válido");
            }

            if (!destinatarioNotificacionBcp.EstaValidado(destinatarioNotificacion!)) {
                if (!destinatarioNotificacionBcp.CodigoValidacionVigente(destinatarioNotificacion!)) {
                    throw new ErrorValidacion(TipoErrorValidacion.AccesoCaducado, "El código de validación no está vigente", "Código ingresado no es válido");
                }

                await destinatarioNotificacionBcp.Validar(destinatarioNotificacion!);
            }
        }

		public async Task<bool> DestinatarioHabilitado(string sub, long idNegocio, long idDestinatario, NpgsqlTransaction? transaction = null) {
			// Se valida si el negocio es accesible...
			bool negocioAccesible = await negocioUseCase.NegocioAccesible(sub, idNegocio, transaction);
			if (!negocioAccesible) return false;

			// Se valida que el destinatario sea del negocio y este validado...
			List<DestinatarioNotificacion> destinatarios = await destinatarioNotificacionBcp.ObtenerVigentesPorSubYNegocio(sub, idNegocio, transaction);
			DestinatarioNotificacion? destinatarioSeleccionado = destinatarios.FirstOrDefault(d => d.Id == idDestinatario);
			if (destinatarioSeleccionado == null || !destinatarioSeleccionado.Validado) return false;

			// Se valida que el tipo de receptor esté vigente...
			TipoReceptorNotificacion? tipoReceptorDestinatario = await tipoReceptorNotificacionDao.ObtenerPorId(destinatarioSeleccionado.IdTipoReceptor, transaction);
			if (tipoReceptorDestinatario == null || !tipoReceptorDestinatario.Vigencia) {
				return false;
			}

			// Se valida si el usuario tiene plan Empresa...
			bool tienePlanEmpresa = await suscripcionBcp.ConsultaTienePlanEmpresa(sub, transaction);
			if (!tienePlanEmpresa) {
				// Dado que no tiene plan Empresa, se valida si el tipo de receptor requiere plan Empresa...
				if (tipoReceptorDestinatario.RequierePlanEmpresa) {
					return false;
				}

				// Dado que no tiene plan Empresa, se valida si el destinatario es de un empleado...
				if (destinatarioSeleccionado.IdEmpleado != null) {
					return false;
				}
			}

			return true;
		}
	}
}
