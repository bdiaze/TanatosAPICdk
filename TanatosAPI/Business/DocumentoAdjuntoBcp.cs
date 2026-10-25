using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
	public class DocumentoAdjuntoBcp(IDateTimeProvider dateTimeProvider, IVariableEntornoHelper variableEntorno, IDocumentoAdjuntoDao documentoAdjuntoDao, IS3Helper s3Helper) : IDocumentoAdjuntoBcp {
		public readonly string BUCKET_NAME = variableEntorno.Obtener("BUCKET_NAME_DOCUMENTOS_ADJUNTOS");
		public const long MAX_FILE_SIZE = 10 * 1024 * 1024;
		public static readonly string[] ALLOWED_FILES_TYPES = ["application/pdf", "image/jpeg", "image/png", "image/webp"];
		
		public bool TamannoValido(long tamanno) {
			return tamanno <= MAX_FILE_SIZE;
		}

		public bool MimeValido(string mime) {
			return ALLOWED_FILES_TYPES.Contains(mime, StringComparer.OrdinalIgnoreCase);
		}

        public bool EstaVigente(DocumentoAdjunto? documentoAdjunto) {
            return documentoAdjunto != null && documentoAdjunto.Vigencia;
        }

        public bool FueRecepcionado(DocumentoAdjunto documentoAdjunto) {
            return documentoAdjunto.EstadoSubida == 1 /* Documento recepcionado */;
        }

        public bool Pertenece(DocumentoAdjunto documentoAdjunto, long idHistorialNormaSuscrita) {
            return documentoAdjunto.IdHistorialNormaSuscrita == idHistorialNormaSuscrita;
        }

		public bool PerteneceMovimiento(DocumentoAdjunto documentoAdjunto, long idMovimientoFinanciero) {
			return documentoAdjunto.IdMovimientoFinanciero == idMovimientoFinanciero;
		}

		public List<DocumentoAdjunto> FiltrarVigentes(List<DocumentoAdjunto> documentos) {
			return [.. documentos.Where(d => EstaVigente(d))];
		}

		public List<DocumentoAdjunto> FiltrarRecepcionados(List<DocumentoAdjunto> documentos) {
			return [.. documentos.Where(FueRecepcionado)];
		}

		public async Task<DocumentoAdjunto?> Obtener(long idDocumentoAdjunto, bool filtrarVigente = false, bool validarVigencia = false, long? validarIdHistorialNormaSuscrita = null, long? validarIdMovimientoFinanciero = null, NpgsqlTransaction? transaction = null) {
            DocumentoAdjunto? item = await documentoAdjuntoDao.ObtenerPorId(idDocumentoAdjunto, transaction);
			if (validarVigencia && !EstaVigente(item)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El documento adjunto no existe o no está vigente", "El documento adjunto es inválido.");
            if (item != null) {
                if (validarIdHistorialNormaSuscrita != null && !Pertenece(item, validarIdHistorialNormaSuscrita.Value)) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El documento adjunto no pertenece al vencimiento indicado", "El documento adjunto es inválido.");
				if (validarIdMovimientoFinanciero != null && !PerteneceMovimiento(item, validarIdMovimientoFinanciero.Value)) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El documento adjunto no pertenece al movimiento indicado", "El documento adjunto es inválido.");
			}

			if (filtrarVigente && !EstaVigente(item)) return null;

			return item;
        }

		public async Task<List<DocumentoAdjunto>> ObtenerPorNegocio(long idNegocio, bool filtrarVigentes = false, bool filtrarRecepcionados = false, NpgsqlTransaction? transaction = null) {
			List<DocumentoAdjunto> documentos = await documentoAdjuntoDao.ObtenerPorNegocio(idNegocio, transaction);
			if (filtrarVigentes) documentos = FiltrarVigentes(documentos);
			if (filtrarRecepcionados) documentos = FiltrarRecepcionados(documentos);

			return documentos;
		}

		public async Task<List<DocumentoAdjunto>> ObtenerPorVencimiento(long idHistorialNormaSuscrita, bool filtrarVigentes = false, bool filtrarRecepcionados = false, NpgsqlTransaction? transaction = null) {
			List<DocumentoAdjunto> documentos = await documentoAdjuntoDao.ObtenerPorHistorial(idHistorialNormaSuscrita, null, transaction);
            if (filtrarVigentes) documentos = FiltrarVigentes(documentos);
            if (filtrarRecepcionados) documentos = FiltrarRecepcionados(documentos);

			return documentos;
		}

		public async Task<List<DocumentoAdjunto>> ObtenerPorMovimiento(long idMovimientoFinanciero, bool filtrarVigentes = false, bool filtrarRecepcionados = false, NpgsqlTransaction? transaction = null) {
			List<DocumentoAdjunto> documentos = await documentoAdjuntoDao.ObtenerPorMovimiento(idMovimientoFinanciero, transaction);
			if (filtrarVigentes) documentos = FiltrarVigentes(documentos);
			if (filtrarRecepcionados) documentos = FiltrarRecepcionados(documentos);

			return documentos;
		}

		public async Task<(string preSignedUrl, Dictionary<string, string> fields, DocumentoAdjunto documentoAdjunto)> GenerarUrlSubida(string sub, long idNegocio, string nombreArchivo, string mimeArchivo, long tamannoArchivo, long? idNormaSuscrita = null, long? idHistorialNormaSuscrita = null, long? idCuentaFinanciera = null, long? idMovimientoFinanciero = null) {
            nombreArchivo = nombreArchivo.Trim();
            mimeArchivo = mimeArchivo.Trim();

            if (!TamannoValido(tamannoArchivo)) throw new ErrorValidacion(TipoErrorValidacion.TamannoNoValido, $"El tamaño del archivo es inválido.");
            if (!MimeValido(mimeArchivo)) throw new ErrorValidacion(TipoErrorValidacion.TipoNoValido, $"El MIME del archivo es inválido.");

			string bucketKey;
			if (idNormaSuscrita != null && idHistorialNormaSuscrita != null) {
				bucketKey = $"Sub-{sub}/Negocio-{idNegocio}/NormaSuscrita-{idNormaSuscrita}/HistorialNormaSuscrita-{idHistorialNormaSuscrita}/{Guid.NewGuid()}";
			} else if (idCuentaFinanciera != null && idMovimientoFinanciero != null) {
				bucketKey = $"Sub-{sub}/Negocio-{idNegocio}/CuentaFinanciera-{idCuentaFinanciera}/MovimientoFinanciero-{idMovimientoFinanciero}/{Guid.NewGuid()}";
			} else { 
				throw new InvalidOperationException("Se debe incluir el ID del vencimiento o el ID del movimiento.");
            }
            
			(string preSignedUrl, Dictionary<string, string> fields) = await s3Helper.ObtenerPostPreSignedUrl(BUCKET_NAME, bucketKey, mimeArchivo, tamannoArchivo);
			
            DateTime utcNow = dateTimeProvider.UtcNow;

            DocumentoAdjunto nuevo = new() {
                Id = 0,
				IdNegocio = idNegocio,
                IdHistorialNormaSuscrita = idHistorialNormaSuscrita,
                BucketName = BUCKET_NAME,
                BucketKey = bucketKey,
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
            
            return (preSignedUrl, fields, nuevo);
        }

        public async Task ConfirmarSubida(DocumentoAdjunto documentoAdjunto) {
            if (documentoAdjunto.EstadoSubida != 1 /* Documento recepcionado */) {
                (long contentLength, string contentType) = await s3Helper.ObtenerObjectMetadata(BUCKET_NAME, documentoAdjunto.BucketKey);

				documentoAdjunto.MimeReal = contentType;
                documentoAdjunto.TamannoReal = contentLength;
                documentoAdjunto.EstadoSubida = 1 /* Documento recepcionado */;
                documentoAdjunto.FechaConfirmacionSubida = dateTimeProvider.UtcNow;

                await documentoAdjuntoDao.Actualizar(documentoAdjunto);
            }
        }

        public async Task<string> GenerarUrlBajada(DocumentoAdjunto documentoAdjunto, bool paraVisualizacion = false) {
            return await s3Helper.ObtenerGetPreSignedUrl(BUCKET_NAME, documentoAdjunto.BucketKey, documentoAdjunto.NombreArchivo, paraVisualizacion);

		}

		public async Task Eliminar(DocumentoAdjunto documentoAdjunto, NpgsqlTransaction? transaction = null) {
			if (documentoAdjunto.Vigencia) {
				documentoAdjunto.Vigencia = false;
				documentoAdjunto.FechaEliminacion = dateTimeProvider.UtcNow;

				await documentoAdjuntoDao.Actualizar(documentoAdjunto, transaction);
                await s3Helper.AgregarTag(BUCKET_NAME, documentoAdjunto.BucketKey, "Estado", "Eliminado");
			}
		}

		public async Task EliminarPorNegocio(long idNegocio, NpgsqlTransaction? transaction = null) {
			List<DocumentoAdjunto> documentosAdjuntosEliminar = await ObtenerPorNegocio(idNegocio, filtrarVigentes: true, transaction: transaction);
			foreach (DocumentoAdjunto documentoAdjuntoEliminar in documentosAdjuntosEliminar) {
				await Eliminar(documentoAdjuntoEliminar, transaction);
			}
		}

		public async Task EliminarPorHistorialNormaSuscrita(long idHistorialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			List<DocumentoAdjunto> documentosAdjuntosEliminar = await documentoAdjuntoDao.ObtenerPorHistorial(idHistorialNormaSuscrita, true, transaction);
			foreach (DocumentoAdjunto documentoAdjuntoEliminar in documentosAdjuntosEliminar) {
				await Eliminar(documentoAdjuntoEliminar, transaction);
			}
		}

		public async Task EliminarPorMovimientoFinanciero(long idMovimientoFinanciero, NpgsqlTransaction? transaction = null) {
			List<DocumentoAdjunto> documentosAdjuntosEliminar = await ObtenerPorMovimiento(idMovimientoFinanciero, filtrarVigentes: true, transaction: transaction);
			foreach (DocumentoAdjunto documentoAdjuntoEliminar in documentosAdjuntosEliminar) {
				await Eliminar(documentoAdjuntoEliminar, transaction);
			}
		}
	}
}
