using Npgsql;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;
using TanatosAPI.Repositories;

namespace TanatosAPI.UseCases {
    public class DestinatarioNotificacionUseCase(IDatabaseConnectionHelper connectionHelper, DestinatarioNotificacionBcp destinatarioNotificacionBcp, INegocioBcp negocioBcp, UsuarioBcp usuarioBcp) {
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
                        Negocio negocio = await negocioBcp.ObtenerVigentePorSubYNegocio(destinatarioNotificacion.Sub, destinatarioNotificacion.IdNegocio, transaction) ?? throw new InvalidOperationException("ID de negocio no válido");
                        Usuario usuario = await usuarioBcp.ObtenerInformacionUsuario(destinatarioNotificacion.Sub, transaction);

                        string idMensaje = await destinatarioNotificacionBcp.EnviarCorreoValidacionDestinatario(
                            destinatarioNotificacion.Destino, 
                            usuario.Nombre ?? "",
                            negocio.Nombre,
                            codigoValidacion
                        );

                        await destinatarioNotificacionBcp.RegistrarHermesIdMensaje(destinatarioNotificacion, idMensaje, transaction);

                    } else if (destinatarioNotificacion.IdTipoReceptor == 2 /* Whatsapp */) {
                        Negocio negocio = await negocioBcp.ObtenerVigentePorSubYNegocio(destinatarioNotificacion.Sub, destinatarioNotificacion.IdNegocio, transaction) ?? throw new InvalidOperationException("ID de negocio no válido");
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
    }
}
