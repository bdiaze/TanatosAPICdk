using Microsoft.AspNetCore.SignalR;
using Npgsql;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Repositories;

namespace TanatosAPI.UseCases {
    public class DocumentoAdjuntoUseCase(ISuscripcionBcp suscripcionBcp, IDocumentoAdjuntoBcp documentoAdjuntoBcp, INegocioBcp negocioBcp, INormaSuscritaBcp normaSuscritaBcp, IHistorialNormaSuscritaBcp historialNormaSuscritaBcp, IHistorialNotificacionBcp historialNotificacionBcp, ICuentaFinancieraBcp cuentaFinancieraBcp, IMovimientoFinancieroBcp movimientoFinancieroBcp) {
        public async Task<List<DocumentoAdjunto>> ObtenerVigentes(string sub, long idHistorialNormaSuscrita) {
            HistorialNormaSuscrita historialNormaSuscrita = (await historialNormaSuscritaBcp.Obtener(idHistorialNormaSuscrita, validarVigencia: true))!;

            NormaSuscrita normaSuscrita = await normaSuscritaBcp.Obtener(historialNormaSuscrita!.IdNormaSuscrita, validarSub: sub) ?? throw new InvalidOperationException("No existe la obligación.");
			if (!normaSuscritaBcp.EstaVigente(normaSuscrita) && !historialNormaSuscritaBcp.EstaCompletada(historialNormaSuscrita)) {
                throw new ErrorValidacion(TipoErrorValidacion.EstadoNoValido, "La obligación no está vigente ni el vencimiento completado", "El vencimiento es inválido.");
            }

            _ = await negocioBcp.Obtener(normaSuscrita.IdNegocio, validarVigencia: true, validarSub: sub);

            return await documentoAdjuntoBcp.ObtenerPorVencimiento(historialNormaSuscrita!.Id, filtrarVigentes: true, filtrarRecepcionados: true);
        }

        public async Task<List<DocumentoAdjunto>> ObtenerVigentesPorMovimiento(string sub, long idMovimientoFinanciero) {
            MovimientoFinanciero movimientoFinanciero = (await movimientoFinancieroBcp.Obtener(idMovimientoFinanciero, validarVigencia: true))!;
            CuentaFinanciera cuentaFinanciera = (await cuentaFinancieraBcp.Obtener(movimientoFinanciero.IdCuentaFinanciera, validarVigencia: true, validarSub: sub))!;
			_ = await negocioBcp.Obtener(cuentaFinanciera.IdNegocio, validarVigencia: true, validarSub: sub);

            return await documentoAdjuntoBcp.ObtenerPorMovimiento(idMovimientoFinanciero, filtrarVigentes: true, filtrarRecepcionados: true);
		}
        
        public async Task<(string preSignedUrl, Dictionary<string, string> fields, DocumentoAdjunto documentoAdjunto)> GenerarUrlSubida(string? sub, string nombreArchivo, string mime, long tamanno, long? idHistorialNormaSuscrita = null, long? idMovimientoFinanciero = null) {
            nombreArchivo = nombreArchivo.Trim();
            mime = mime.Trim();

            if (idHistorialNormaSuscrita != null) {
				HistorialNormaSuscrita historialNormaSuscrita = (await historialNormaSuscritaBcp.Obtener(idHistorialNormaSuscrita.Value, validarVigencia: true, validarSinCompletitud: true))!;
				NormaSuscrita normaSuscrita = (await normaSuscritaBcp.Obtener(historialNormaSuscrita!.IdNormaSuscrita, validarVigencia: true, validarSub: sub))!;
                _ = await negocioBcp.Obtener(normaSuscrita.IdNegocio, validarVigencia: true, validarSub: sub);
                await suscripcionBcp.ValidarTienePlanEmpresa(normaSuscrita.Sub);

				return await documentoAdjuntoBcp.GenerarUrlSubida(
				    normaSuscrita.Sub,
				    normaSuscrita.IdNegocio,
					nombreArchivo,
					mime,
					tamanno,
					idNormaSuscrita: normaSuscrita.Id,
				    idHistorialNormaSuscrita: historialNormaSuscrita.Id
			    );
			} else if (idMovimientoFinanciero != null) {
                MovimientoFinanciero movimientoFinanciero = (await movimientoFinancieroBcp.Obtener(idMovimientoFinanciero.Value, validarVigencia: true))!;
                CuentaFinanciera cuentaFinanciera = (await cuentaFinancieraBcp.Obtener(movimientoFinanciero.IdCuentaFinanciera, validarVigencia: true, validarSub: sub))!;
				_ = await negocioBcp.Obtener(cuentaFinanciera.IdNegocio, validarVigencia: true, validarSub: sub);
				await suscripcionBcp.ValidarTienePlanEmpresa(cuentaFinanciera.Sub);

				return await documentoAdjuntoBcp.GenerarUrlSubida(
					cuentaFinanciera.Sub,
					cuentaFinanciera.IdNegocio,
					nombreArchivo,
					mime,
					tamanno,
					idCuentaFinanciera: cuentaFinanciera.Id,
					idMovimientoFinanciero: movimientoFinanciero.Id
				);
			} else {
				throw new InvalidOperationException("Se debe incluir el ID del vencimiento o el ID del movimiento.");
			}
        }

        public async Task<(string preSignedUrl, Dictionary<string, string> fields, DocumentoAdjunto documentoAdjunto)> GenerarUrlSubidaPorCodigoAcceso(string codigoAcceso, string nombreArchivo, string mime, long tamanno) {
            nombreArchivo = nombreArchivo.Trim();
            mime = mime.Trim();

            HistorialNotificacion historialNotificacion = await historialNotificacionBcp.ObtenerPorCodigoAccesoValidandoVigencia(codigoAcceso);

            return await GenerarUrlSubida(null, nombreArchivo, mime, tamanno, idHistorialNormaSuscrita: historialNotificacion.IdHistorialNormaSuscrita);
        }

        public async Task ConfirmarSubida(string? sub, long idDocumentoAdjunto, long? idHistorialNormaSuscrita = null, long? idMovimientoFinanciero = null) {
            DocumentoAdjunto documentoAdjunto = (await documentoAdjuntoBcp.Obtener(idDocumentoAdjunto, validarVigencia: true, validarIdHistorialNormaSuscrita: idHistorialNormaSuscrita, validarIdMovimientoFinanciero: idMovimientoFinanciero))!;
           
            if (documentoAdjunto.IdHistorialNormaSuscrita != null) {
				HistorialNormaSuscrita historialNormaSuscrita = (await historialNormaSuscritaBcp.Obtener(documentoAdjunto.IdHistorialNormaSuscrita.Value, validarVigencia: true, validarSinCompletitud: true))!;
				NormaSuscrita normaSuscrita = (await normaSuscritaBcp.Obtener(historialNormaSuscrita.IdNormaSuscrita, validarVigencia: true, validarSub: sub))!;
				_ = await negocioBcp.Obtener(normaSuscrita.IdNegocio, validarVigencia: true, validarSub: sub);
			} else if (documentoAdjunto.IdMovimientoFinanciero != null) {
                MovimientoFinanciero movimientoFinanciero = (await movimientoFinancieroBcp.Obtener(documentoAdjunto.IdMovimientoFinanciero.Value, validarVigencia: true))!;
				CuentaFinanciera cuentaFinanciera = (await cuentaFinancieraBcp.Obtener(movimientoFinanciero.IdCuentaFinanciera, validarVigencia: true, validarSub: sub))!;
				_ = await negocioBcp.Obtener(cuentaFinanciera.IdNegocio, validarVigencia: true, validarSub: sub);
			}
            
            await documentoAdjuntoBcp.ConfirmarSubida(documentoAdjunto);
        }

        public async Task ConfirmarSubidaPorCodigoAcceso(string codigoAcceso, long idDocumentoAdjunto) {
            HistorialNotificacion? historialNotificacion = await historialNotificacionBcp.ObtenerPorCodigoAccesoValidandoVigencia(codigoAcceso);

            await ConfirmarSubida(null, idDocumentoAdjunto, idHistorialNormaSuscrita: historialNotificacion!.IdHistorialNormaSuscrita);
        }

        public async Task<string> GenerarUrlBajada(string? sub, long idDocumentoAdjunto, long? idHistorialNormaSuscrita = null, long? idMovimientoFinanciero = null, bool paraVisualizacion = false) {
            DocumentoAdjunto documentoAdjunto = (await documentoAdjuntoBcp.Obtener(idDocumentoAdjunto, validarVigencia: true, validarIdHistorialNormaSuscrita: idHistorialNormaSuscrita, validarIdMovimientoFinanciero: idMovimientoFinanciero))!;

            if (documentoAdjunto.IdHistorialNormaSuscrita != null) {
                HistorialNormaSuscrita historialNormaSuscrita = (await historialNormaSuscritaBcp.Obtener(documentoAdjunto.IdHistorialNormaSuscrita.Value, validarVigencia: true))!;
                
                NormaSuscrita normaSuscrita = await normaSuscritaBcp.Obtener(historialNormaSuscrita!.IdNormaSuscrita, validarSub: sub) ?? throw new InvalidOperationException("No existe la obligación."); ;
                if (!normaSuscritaBcp.EstaVigente(normaSuscrita) && !historialNormaSuscritaBcp.EstaCompletada(historialNormaSuscrita!)) {
                    throw new ErrorValidacion(TipoErrorValidacion.EstadoNoValido, "La obligación no está vigente ni el vencimiento completado", "El documento adjunto es inválido.");
                }

				_ = await negocioBcp.Obtener(normaSuscrita.IdNegocio, validarVigencia: true, validarSub: sub);
			} else if (documentoAdjunto.IdMovimientoFinanciero != null) {
				MovimientoFinanciero movimientoFinanciero = (await movimientoFinancieroBcp.Obtener(documentoAdjunto.IdMovimientoFinanciero.Value, validarVigencia: true))!;
				CuentaFinanciera cuentaFinanciera = (await cuentaFinancieraBcp.Obtener(movimientoFinanciero.IdCuentaFinanciera, validarVigencia: true, validarSub: sub))!;
				_ = await negocioBcp.Obtener(cuentaFinanciera.IdNegocio, validarVigencia: true, validarSub: sub);
			}

            return await documentoAdjuntoBcp.GenerarUrlBajada(documentoAdjunto, paraVisualizacion);
        }

        public async Task<string> GenerarUrlBajadaPorCodigoAcceso(string codigoAcceso, long idDocumentoAdjunto, bool paraVisualizacion = false) {
            HistorialNotificacion? historialNotificacion = await historialNotificacionBcp.ObtenerPorCodigoAccesoValidandoVigencia(codigoAcceso);

            return await GenerarUrlBajada(null, idDocumentoAdjunto, idHistorialNormaSuscrita: historialNotificacion!.IdHistorialNormaSuscrita, paraVisualizacion: paraVisualizacion);
        }

        public async Task Eliminar(string? sub, long idDocumentoAdjunto, long? idHistorialNormaSuscrita = null, long? idMovimientoFinanciero = null) {
            DocumentoAdjunto? documentoAdjunto = await documentoAdjuntoBcp.Obtener(idDocumentoAdjunto, filtrarVigente: true, validarIdHistorialNormaSuscrita: idHistorialNormaSuscrita, validarIdMovimientoFinanciero: idMovimientoFinanciero);
            
            if (documentoAdjunto != null) {
                if (documentoAdjunto.IdHistorialNormaSuscrita != null) {
                    HistorialNormaSuscrita historialNormaSuscrita = (await historialNormaSuscritaBcp.Obtener(documentoAdjunto.IdHistorialNormaSuscrita.Value, validarVigencia: true, validarSinCompletitud: true))!;
					NormaSuscrita normaSuscrita = (await normaSuscritaBcp.Obtener(historialNormaSuscrita!.IdNormaSuscrita, validarVigencia: true, validarSub: sub))!;
					_ = await negocioBcp.Obtener(normaSuscrita.IdNegocio, validarVigencia: true, validarSub: sub);
				} else if (documentoAdjunto.IdMovimientoFinanciero != null) {
					MovimientoFinanciero movimientoFinanciero = (await movimientoFinancieroBcp.Obtener(documentoAdjunto.IdMovimientoFinanciero.Value, validarVigencia: true))!;
					CuentaFinanciera cuentaFinanciera = (await cuentaFinancieraBcp.Obtener(movimientoFinanciero.IdCuentaFinanciera, validarVigencia: true, validarSub: sub))!;
					_ = await negocioBcp.Obtener(cuentaFinanciera.IdNegocio, validarVigencia: true, validarSub: sub);
				}

                await documentoAdjuntoBcp.Eliminar(documentoAdjunto!);
			}
		}

        public async Task EliminarPorCodigoAcceso(string codigoAcceso, long idDocumentoAdjunto) {
            HistorialNotificacion? historialNotificacion = await historialNotificacionBcp.ObtenerPorCodigoAccesoValidandoVigencia(codigoAcceso);

            await Eliminar(null, idDocumentoAdjunto, idHistorialNormaSuscrita: historialNotificacion!.IdHistorialNormaSuscrita);
        }
    }
}
