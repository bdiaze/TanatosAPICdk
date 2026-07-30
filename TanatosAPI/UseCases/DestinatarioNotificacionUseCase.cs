using Actions_Compile;
using Amazon.DynamoDBv2;
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
    public class DestinatarioNotificacionUseCase(IDatabaseConnectionHelper connectionHelper, NegocioUseCase negocioUseCase, IDestinatarioNotificacionBcp destinatarioNotificacionBcp, INegocioBcp negocioBcp, IUsuarioBcp usuarioBcp, ISuscripcionBcp suscripcionBcp, ICargoBcp cargoBcp, IEmpleadoBcp empleadoBcp, ITipoReceptorNotificacionDao tipoReceptorNotificacionDao) {
        public const short HORAS_CADUCIDAD_CODIGO_VALIDACION = 24;

        public async Task<List<DestinatarioNotificacion>> ObtenerPorSubYNegocio(string sub, long idNegocio, bool filtrarVigente = false, bool filtrarValidado = false, bool crearDestinoUsuario = false, NpgsqlTransaction? transaction = null) {
            List<DestinatarioNotificacion> destinatarios = await destinatarioNotificacionBcp.ObtenerPorSubYNegocio(sub, idNegocio, filtrarVigente: filtrarVigente, filtrarValidado: filtrarValidado, transaction: transaction);

            if (crearDestinoUsuario) {
                Usuario usuario = await usuarioBcp.ObtenerInformacionUsuario(sub, transaction);
                if (usuario.CorreoElectronico != null) {
					bool destinoUsuarioYaCreado = destinatarios.Any(d => d.IdEmpleado == null && d.IdTipoReceptor == 1 /* Correo electrónico */ && d.Destino == usuario.CorreoElectronico);
                    if (!destinoUsuarioYaCreado) {
						(DestinatarioNotificacion nuevoDestinatario, _) = await destinatarioNotificacionBcp.Insertar(
						    sub,
						    idNegocio,
						    null,
						    1, // Correo electrónico
						    "Mi correo electrónico",
						    usuario.CorreoElectronico,
						    true,
						    transaction
					    );
						destinatarios.Add(nuevoDestinatario);
					}
				}
            }

            return destinatarios;
		}

        public async Task<List<DestinatarioNotificacion>> ObtenerDestinatariosNormaSuscrita(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
            List<DestinatarioNotificacion> destinatariosValidados = await ObtenerPorSubYNegocio(normaSuscrita.Sub, normaSuscrita.IdNegocio, filtrarVigente: true, filtrarValidado: true, crearDestinoUsuario: true, transaction: transaction);

			// No se usa el cargo responsable si el usuario no tiene plan empresa...
			long? idCargoResponsable = normaSuscrita.IdCargo;
            if (idCargoResponsable != null) {
                bool tienePlanEmpresa = await suscripcionBcp.ConsultaTienePlanEmpresa(normaSuscrita.Sub, transaction);
                if (!tienePlanEmpresa) idCargoResponsable = null;
            }

            // No se usa el cargo responsable si dicho cargo no está vigente, o no pertenece al usuario y negocio de la obligación...
            if (idCargoResponsable != null) {
                Cargo? cargo = await cargoBcp.Obtener(idCargoResponsable.Value, filtrarVigente: true, filtrarSub: normaSuscrita.Sub, filtrarIdNegocio: normaSuscrita.IdNegocio, transaction: transaction);
                idCargoResponsable = cargo?.Id;
            }

            if (idCargoResponsable == null) {
                // Si no tiene un cargo responsable, solo se dejan los destinatarios que no son de un empleado...
                return destinatarioNotificacionBcp.FiltrarPorEmpleado(destinatariosValidados, (long?)null);
            } else {
                // Si tiene un cargo responsable, solo se dejan los destinatarios que posean dicho cargo...
                HashSet<long?> idsEmpleados = [.. (await empleadoBcp.ObtenerPorSubYNegocio(normaSuscrita.Sub, normaSuscrita.IdNegocio, filtrarVigente: true, filtrarIdCargo: idCargoResponsable, transaction: transaction)).Select(e => e.Id)];
                List<DestinatarioNotificacion> destinatariosEmpleadosResponsables = destinatarioNotificacionBcp.FiltrarPorEmpleado(destinatariosValidados, idsEmpleados);
                if (destinatariosEmpleadosResponsables.Count == 0) {
                    // Si no tengo empleados responsables, se dejan los destinatarios que no son de un empleado...
                    return destinatarioNotificacionBcp.FiltrarPorEmpleado(destinatariosValidados, (long?)null);
                } else {
                    return destinatariosEmpleadosResponsables;
                }
            }
        }
    	
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
    
        public async Task ValidarDestinatario(string codigoValidacion, NpgsqlTransaction? transaction = null) {
            DestinatarioNotificacion destinatarioNotificacion = (await destinatarioNotificacionBcp.ObtenerPorCodigoValidacion(codigoValidacion, validarVigencia: true, validarCodigoValidacionVigente: true, transaction: transaction))!;
            await destinatarioNotificacionBcp.Validar(destinatarioNotificacion, transaction);
        }

		public async Task<bool> DestinatarioHabilitado(string sub, long idNegocio, long idDestinatario, NpgsqlTransaction? transaction = null) {
			// Se valida si el negocio es accesible...
			bool negocioAccesible = await negocioUseCase.NegocioAccesible(sub, idNegocio, transaction);
			if (!negocioAccesible) return false;

            // Se valida que el destinatario sea del negocio y este validado...
            DestinatarioNotificacion? destinatarioSeleccionado = await destinatarioNotificacionBcp.Obtener(idDestinatario, filtrarVigente: true, filtrarValidado: true, filtrarSub: sub, filtrarIdNegocio: idNegocio, transaction: transaction);
			if (destinatarioSeleccionado == null) return false;

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
