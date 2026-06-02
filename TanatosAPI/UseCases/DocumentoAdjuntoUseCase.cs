using Microsoft.AspNetCore.SignalR;
using Npgsql;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;

namespace TanatosAPI.UseCases {
    public class DocumentoAdjuntoUseCase(DatabaseConnectionHelper connectionHelper, SuscripcionBcp suscripcionBcp, DocumentoAdjuntoBcp documentoAdjuntoBcp, NormaSuscritaBcp normaSuscritaBcp, HistorialNormaSuscritaBcp historialNormaSuscritaBcp, HistorialNotificacionBcp historialNotificacionBcp) {
        public async Task<(string preSignedUrl, Dictionary<string, string> fields, DocumentoAdjunto documentoAdjunto)> GenerarUrlSubida(string? sub, long idHistorialNormaSuscrita, string nombreArchivo, string mime, long tamanno) {
            if (!documentoAdjuntoBcp.TamannoValido(tamanno)) {
                throw new ErrorValidacion($"El tamaño del archivo es inválido.");
            }

            if (!documentoAdjuntoBcp.MimeValido(mime)) {
                throw new ErrorValidacion($"El MIME del archivo es inválido.");
            }

            HistorialNormaSuscrita? historialNormaSuscrita = await historialNormaSuscritaBcp.ObtenerPorId(idHistorialNormaSuscrita);
            if (!historialNormaSuscritaBcp.EstaVigente(historialNormaSuscrita)) {
                throw new ErrorValidacion("El vencimiento no existe o no está vigente", "El vencimiento es inválido.");
            }

            if (historialNormaSuscritaBcp.EstaCompletada(historialNormaSuscrita!)) {
                throw new ErrorValidacion("El vencimiento ya está completado", "El vencimiento es inválido.");
            }

            NormaSuscrita? normaSuscrita = await normaSuscritaBcp.ObtenerPorId(historialNormaSuscrita!.IdNormaSuscrita);
            if (!normaSuscritaBcp.EstaVigente(normaSuscrita)) {
                throw new ErrorValidacion("La obligación no existe o no está vigente", "El vencimiento es inválido.");
            }

            // Si no se especifica el sub, se omite validación de pertenencia...
            if (sub != null && !normaSuscritaBcp.Pertenece(normaSuscrita!, sub)) {
                throw new ErrorValidacion("La obligación no pertenece al usuario", "El vencimiento es inválido.");
            }

            if (!await suscripcionBcp.TienePlanEmpresa(normaSuscrita!.Sub)) {
                throw new ErrorValidacion($"Tu plan no permite adjuntar documentos.");
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
            HistorialNotificacion? historialNotificacion = await historialNotificacionBcp.ObtenerPorCodigoAcceso(codigoAcceso);
            if (!historialNotificacionBcp.EstaVigente(historialNotificacion)) {
                throw new ErrorValidacion("La notificación no está vigente", "El código de acceso es inválido.");
            }

            if (!historialNotificacionBcp.CodigoAccesoVigente(historialNotificacion!)) {
                throw new ErrorValidacion("El código de acceso ha caducado", "El código de acceso es inválido.");
            }

            return await GenerarUrlSubida(null, historialNotificacion!.IdHistorialNormaSuscrita, nombreArchivo, mime, tamanno);
        }
    }
}
