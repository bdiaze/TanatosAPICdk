using Microsoft.AspNetCore.SignalR;
using Npgsql;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Repositories;

namespace TanatosAPI.UseCases {
    public class DocumentoAdjuntoUseCase(ISuscripcionBcp suscripcionBcp, IDocumentoAdjuntoBcp documentoAdjuntoBcp, INormaSuscritaBcp normaSuscritaBcp, IHistorialNormaSuscritaBcp historialNormaSuscritaBcp, IHistorialNotificacionBcp historialNotificacionBcp) {
        public async Task<List<DocumentoAdjunto>> ObtenerVigentes(string sub, long idHistorialNormaSuscrita) {
            HistorialNormaSuscrita? historialNormaSuscrita = await historialNormaSuscritaBcp.Obtener(idHistorialNormaSuscrita);
            if (!historialNormaSuscritaBcp.EstaVigente(historialNormaSuscrita)) {
                throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El vencimiento no está vigente", "El vencimiento es inválido.");
            }

            NormaSuscrita? normaSuscrita = await normaSuscritaBcp.Obtener(historialNormaSuscrita!.IdNormaSuscrita);
            if (!normaSuscritaBcp.EstaVigente(normaSuscrita) && !historialNormaSuscritaBcp.EstaCompletada(historialNormaSuscrita)) {
                throw new ErrorValidacion(TipoErrorValidacion.EstadoNoValido, "La obligación no está vigente ni el vencimiento completado", "El vencimiento es inválido.");
            }

            if (sub != null && !normaSuscritaBcp.Pertenece(normaSuscrita!, sub)) {
                throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "La obligación no pertenece al usuario", "El vencimiento es inválido.");
            }

            return await documentoAdjuntoBcp.ObtenerPorVencimiento(historialNormaSuscrita!.Id, filtrarVigentes: true, filtrarRecepcionados: true);
        }
        
        public async Task<(string preSignedUrl, Dictionary<string, string> fields, DocumentoAdjunto documentoAdjunto)> GenerarUrlSubida(string? sub, long idHistorialNormaSuscrita, string nombreArchivo, string mime, long tamanno) {
            nombreArchivo = nombreArchivo.Trim();
            mime = mime.Trim();

            if (!documentoAdjuntoBcp.TamannoValido(tamanno)) {
                throw new ErrorValidacion(TipoErrorValidacion.TamannoNoValido, $"El tamaño del archivo es inválido.");
            }

            if (!documentoAdjuntoBcp.MimeValido(mime)) {
                throw new ErrorValidacion(TipoErrorValidacion.TipoNoValido, $"El MIME del archivo es inválido.");
            }

            HistorialNormaSuscrita? historialNormaSuscrita = await historialNormaSuscritaBcp.Obtener(idHistorialNormaSuscrita);
            if (!historialNormaSuscritaBcp.EstaVigente(historialNormaSuscrita)) {
                throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El vencimiento no existe o no está vigente", "El vencimiento es inválido.");
            }

            if (historialNormaSuscritaBcp.EstaCompletada(historialNormaSuscrita!)) {
                throw new ErrorValidacion(TipoErrorValidacion.EstadoNoValido, "El vencimiento ya está completado", "El vencimiento es inválido.");
            }

            NormaSuscrita? normaSuscrita = await normaSuscritaBcp.Obtener(historialNormaSuscrita!.IdNormaSuscrita);
            if (!normaSuscritaBcp.EstaVigente(normaSuscrita)) {
                throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "La obligación no existe o no está vigente", "El vencimiento es inválido.");
            }

            // Si no se especifica el sub, se omite validación de pertenencia...
            if (sub != null && !normaSuscritaBcp.Pertenece(normaSuscrita!, sub)) {
                throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "La obligación no pertenece al usuario", "El vencimiento es inválido.");
            }

            if (!await suscripcionBcp.ConsultaTienePlanEmpresa(normaSuscrita!.Sub)) {
                throw new ErrorValidacion(TipoErrorValidacion.RestringidoPorPlan, $"Tu plan no permite adjuntar documentos.");
            }

            return await documentoAdjuntoBcp.GenerarUrlSubida(
                normaSuscrita!.Sub,
                normaSuscrita!.IdNegocio,
                normaSuscrita!.Id,
                historialNormaSuscrita.Id,
                nombreArchivo,
                mime,
                tamanno
            );
        }

        public async Task<(string preSignedUrl, Dictionary<string, string> fields, DocumentoAdjunto documentoAdjunto)> GenerarUrlSubidaPorCodigoAcceso(string codigoAcceso, string nombreArchivo, string mime, long tamanno) {
            nombreArchivo = nombreArchivo.Trim();
            mime = mime.Trim();

            HistorialNotificacion? historialNotificacion = await historialNotificacionBcp.ObtenerPorCodigoAccesoValidandoVigencia(codigoAcceso);

            return await GenerarUrlSubida(null, historialNotificacion!.IdHistorialNormaSuscrita, nombreArchivo, mime, tamanno);
        }

        public async Task ConfirmarSubida(string? sub, long idDocumentoAdjunto, long? idHistorialNormaSuscrita = null) {
            DocumentoAdjunto? documentoAdjunto = await documentoAdjuntoBcp.Obtener(idDocumentoAdjunto);
            if (!documentoAdjuntoBcp.EstaVigente(documentoAdjunto)) {
                throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El documento adjunto no existe o no está vigente", "El documento adjunto es inválido.");
            }

            // Si se incluye un ID de vencimiento, se valida que el documento pertenezca a dicho vencimiento...
            if (idHistorialNormaSuscrita != null && !documentoAdjuntoBcp.Pertenece(documentoAdjunto!, idHistorialNormaSuscrita.Value)) {
                throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El documento adjunto no pertenece al vencimiento indicado", "El documento adjunto es inválido.");
            }

            HistorialNormaSuscrita? historialNormaSuscrita = await historialNormaSuscritaBcp.Obtener(documentoAdjunto!.IdHistorialNormaSuscrita);
            if (!historialNormaSuscritaBcp.EstaVigente(historialNormaSuscrita)) {
                throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El vencimiento no existe o no está vigente", "El vencimiento es inválido.");
            }

            if (historialNormaSuscritaBcp.EstaCompletada(historialNormaSuscrita!)) {
                throw new ErrorValidacion(TipoErrorValidacion.TipoNoValido, "El vencimiento ya se encuentra completado", "El vencimiento es inválido.");
            }

            NormaSuscrita? normaSuscrita = await normaSuscritaBcp.Obtener(historialNormaSuscrita!.IdNormaSuscrita);
            if (!normaSuscritaBcp.EstaVigente(normaSuscrita)) {
                throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "La obligación no está vigente", "La obligación no está vigente.");
            }

            if (sub != null && !normaSuscritaBcp.Pertenece(normaSuscrita!, sub)) {
                throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "La obligación no pertenece al usuario", "La obligación no está vigente.");
            }

            await documentoAdjuntoBcp.ConfirmarSubida(documentoAdjunto);
        }

        public async Task ConfirmarSubidaPorCodigoAcceso(string codigoAcceso, long idDocumentoAdjunto) {
            HistorialNotificacion? historialNotificacion = await historialNotificacionBcp.ObtenerPorCodigoAccesoValidandoVigencia(codigoAcceso);

            await ConfirmarSubida(null, idDocumentoAdjunto, historialNotificacion!.IdHistorialNormaSuscrita);
        }

        public async Task<string> GenerarUrlBajada(string? sub, long idDocumentoAdjunto, long? idHistorialNormaSuscrita = null) {
            DocumentoAdjunto? documentoAdjunto = await documentoAdjuntoBcp.Obtener(idDocumentoAdjunto);
            if (!documentoAdjuntoBcp.EstaVigente(documentoAdjunto)) {
                throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El documento adjunto no existe o no está vigente", "El documento adjunto es inválido.");
            }

            // Si se incluye un ID de vencimiento, se valida que el documento pertenezca a dicho vencimiento...
            if (idHistorialNormaSuscrita != null && !documentoAdjuntoBcp.Pertenece(documentoAdjunto!, idHistorialNormaSuscrita.Value)) {
                throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El documento adjunto no pertenece al vencimiento indicado", "El documento adjunto es inválido.");
            }

            HistorialNormaSuscrita? historialNormaSuscrita = await historialNormaSuscritaBcp.Obtener(documentoAdjunto!.IdHistorialNormaSuscrita);
            if (!historialNormaSuscritaBcp.EstaVigente(historialNormaSuscrita)) {
                throw new ErrorValidacion(TipoErrorValidacion.EstadoNoValido, "El vencimiento no está vigente", "El documento adjunto es inválido.");
            }

            NormaSuscrita? normaSuscrita = await normaSuscritaBcp.Obtener(historialNormaSuscrita!.IdNormaSuscrita);
            if (!normaSuscritaBcp.EstaVigente(normaSuscrita) && !historialNormaSuscritaBcp.EstaCompletada(historialNormaSuscrita!)) {
                throw new ErrorValidacion(TipoErrorValidacion.EstadoNoValido, "La obligación no está vigente ni el vencimiento completado", "El documento adjunto es inválido.");
            }

            if (sub != null && !normaSuscritaBcp.Pertenece(normaSuscrita!, sub)) {
                throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "La obligación no pertenece al usuario", "El documento adjunto es inválido.");
            }

            return await documentoAdjuntoBcp.GenerarUrlBajada(documentoAdjunto);
        }

        public async Task<string> GenerarUrlBajadaPorCodigoAcceso(string codigoAcceso, long idDocumentoAdjunto) {
            HistorialNotificacion? historialNotificacion = await historialNotificacionBcp.ObtenerPorCodigoAccesoValidandoVigencia(codigoAcceso);

            return await GenerarUrlBajada(null, idDocumentoAdjunto, historialNotificacion!.IdHistorialNormaSuscrita);
        }

        public async Task Eliminar(string? sub, long idDocumentoAdjunto, long? idHistorialNormaSuscrita = null) {
            DocumentoAdjunto? documentoAdjunto = await documentoAdjuntoBcp.Obtener(idDocumentoAdjunto);
            
            // Si el documento no está vigente, se asume que ya fue eliminado...
            if (!documentoAdjuntoBcp.EstaVigente(documentoAdjunto)) {
                return;
            }

            // Si se incluye un ID de vencimiento, se valida que el documento pertenezca a dicho vencimiento...
            if (idHistorialNormaSuscrita != null && !documentoAdjuntoBcp.Pertenece(documentoAdjunto!, idHistorialNormaSuscrita.Value)) {
                throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El documento adjunto no pertenece al vencimiento indicado", "El documento adjunto es inválido.");
            }

            HistorialNormaSuscrita? historialNormaSuscrita = await historialNormaSuscritaBcp.Obtener(documentoAdjunto!.IdHistorialNormaSuscrita);
            if (!historialNormaSuscritaBcp.EstaVigente(historialNormaSuscrita)) {
                throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El vencimiento no está vigente", "El documento adjunto es inválido.");
            }

            if (historialNormaSuscritaBcp.EstaCompletada(historialNormaSuscrita!)) {
                throw new ErrorValidacion(TipoErrorValidacion.EstadoNoValido, "El vencimiento ya está completado", "El documento adjunto es inválido.");
            }

            NormaSuscrita? normaSuscrita = await normaSuscritaBcp.Obtener(historialNormaSuscrita!.IdNormaSuscrita);
            if (!normaSuscritaBcp.EstaVigente(normaSuscrita)) {
                throw new ErrorValidacion(TipoErrorValidacion.EstadoNoValido, "La obligación no está vigente", "El documento adjunto es inválido.");
            }

            if (sub != null && !normaSuscritaBcp.Pertenece(normaSuscrita!, sub)) {
                throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "La obligación no pertenece al usuario", "El documento adjunto es inválido.");
            }

            await documentoAdjuntoBcp.Eliminar(documentoAdjunto!);
        }

        public async Task EliminarPorCodigoAcceso(string codigoAcceso, long idDocumentoAdjunto) {
            HistorialNotificacion? historialNotificacion = await historialNotificacionBcp.ObtenerPorCodigoAccesoValidandoVigencia(codigoAcceso);

            await Eliminar(null, idDocumentoAdjunto, historialNotificacion!.IdHistorialNormaSuscrita);
        }
    }
}
