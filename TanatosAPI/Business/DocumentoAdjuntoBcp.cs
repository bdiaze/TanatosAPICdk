using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class DocumentoAdjuntoBcp(DocumentoAdjuntoDao documentoAdjuntoDao, DocumentoAdjuntoHelper documentoAdjuntoHelper) {
		private const long MAX_FILE_SIZE = 10 * 1024 * 1024;
		private readonly string[] ALLOWED_FILES_TYPES = ["application/pdf", "image/jpeg", "image/png", "image/webp"];

        public bool TamannoValido(long tamanno) {
			return tamanno <= MAX_FILE_SIZE;
		}

		public bool MimeValido(string mime) {
			return ALLOWED_FILES_TYPES.Contains(mime, StringComparer.OrdinalIgnoreCase);
		}

		public async Task<(string preSignedUrl, Dictionary<string, string> fields, DocumentoAdjunto documentoAdjunto)> GenerarUrlSubida(string sub, long idNegocio, long idNormaSuscrita, long idHistorialNormaSuscrita, string nombreArchivo, string mimeArchivo, long tamannoArchivo) {
            (string bucketName, string bucketKey, string preSignedUrl, Dictionary<string, string> fields) presignedPost = await documentoAdjuntoHelper.ObtenerPostPreSignedUrl(
                sub,
                idNegocio,
                idNormaSuscrita,
                idHistorialNormaSuscrita,
                mimeArchivo,
                tamannoArchivo
            );

            DateTime utcNow = DateTime.UtcNow;

            DocumentoAdjunto nuevo = new() {
                Id = 0,
                IdHistorialNormaSuscrita = idHistorialNormaSuscrita,
                BucketName = presignedPost.bucketName,
                BucketKey = presignedPost.bucketKey,
                NombreArchivo = nombreArchivo,
                MimeEsperado = mimeArchivo,
                TamannoEsperado = tamannoArchivo,
                MimeReal = null,
                TamannoReal = null,
                EstadoSubida = 0 /* Generada URL prefirmada para PUT */,
                FechaEmisionUrlPrefirmadaPut = utcNow,
                FechaConfirmacionSubida = null,
                FechaCreacion = utcNow,
                FechaEliminacion = null,
                Vigencia = true
            };
            nuevo.Id = await documentoAdjuntoDao.Insertar(nuevo);
            
            return (presignedPost.preSignedUrl, presignedPost.fields, nuevo);
        }

		public async Task Eliminar(DocumentoAdjunto documentoAdjunto, NpgsqlTransaction? transaction = null) {
			if (documentoAdjunto.Vigencia) {
				documentoAdjunto.Vigencia = false;
				documentoAdjunto.FechaEliminacion = DateTime.UtcNow;

				await documentoAdjuntoDao.Actualizar(documentoAdjunto, transaction);
				await documentoAdjuntoHelper.AgregarTagEstadoEliminado(documentoAdjunto.BucketKey);
			}
		}

		public async Task EliminarPorHistorialNormaSuscrita(long idHistorialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			List<DocumentoAdjunto> documentosAdjuntosEliminar = await documentoAdjuntoDao.ObtenerPorHistorial(idHistorialNormaSuscrita, true, transaction);
			foreach (DocumentoAdjunto documentoAdjuntoEliminar in documentosAdjuntosEliminar) {
				await Eliminar(documentoAdjuntoEliminar, transaction);
			}
		}
	}
}
