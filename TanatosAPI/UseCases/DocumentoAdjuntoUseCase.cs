using Npgsql;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;

namespace TanatosAPI.UseCases {
    public class DocumentoAdjuntoUseCase(DatabaseConnectionHelper connectionHelper, SuscripcionBcp suscripcionBcp, DocumentoAdjuntoBcp documentoAdjuntoBcp, HistorialNormaSuscritaBcp historialNormaSuscritaBcp, NormaSuscritaBcp normaSuscritaBcp) {
        public async Task<(string preSignedUrl, Dictionary<string, string> fields, DocumentoAdjunto documentoAdjunto)> GenerarUrlSubida(string sub, long idHistorialNormaSuscrita, string nombreArchivo, string mime, long tamanno) {
            if (!await suscripcionBcp.TienePlanEmpresa(sub)) {
                throw new ErrorValidacion($"Tu plan no permite adjuntar documentos.");
            }

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

            if (!normaSuscritaBcp.Pertenece(normaSuscrita!, sub)) {
                throw new ErrorValidacion("La obligación no pertenece al usuario", "El vencimiento es inválido.");
            }

            return await documentoAdjuntoBcp.GenerarUrlSubida(
                sub,
                normaSuscrita!.IdNegocio,
                normaSuscrita!.Id,
                historialNormaSuscrita.Id,
                nombreArchivo,
                mime,
                tamanno
            );
        }
    }
}
