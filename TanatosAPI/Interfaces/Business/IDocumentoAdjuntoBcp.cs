using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IDocumentoAdjuntoBcp {
		public bool TamannoValido(long tamanno);
		public bool MimeValido(string mime);
		public bool EstaVigente(DocumentoAdjunto? documentoAdjunto);
		public bool PerteneceAVencimiento(DocumentoAdjunto documentoAdjunto, long idHistorialNormaSuscrita);
		public Task<DocumentoAdjunto?> ObtenerPorId(long idDocumentoAdjunto);
		public Task<List<DocumentoAdjunto>> ObtenerVigentesPorHistorialNormaSuscrita(long idHistorialNormaSuscrita);
		public Task<(string preSignedUrl, Dictionary<string, string> fields, DocumentoAdjunto documentoAdjunto)> GenerarUrlSubida(string sub, long idNegocio, long idNormaSuscrita, long idHistorialNormaSuscrita, string nombreArchivo, string mimeArchivo, long tamannoArchivo);
		public Task ConfirmarSubida(DocumentoAdjunto documentoAdjunto);
		public Task<string> GenerarUrlBajada(DocumentoAdjunto documentoAdjunto);
		public Task Eliminar(DocumentoAdjunto documentoAdjunto, NpgsqlTransaction? transaction = null);
		public Task EliminarPorHistorialNormaSuscrita(long idHistorialNormaSuscrita, NpgsqlTransaction? transaction = null);
	}
}
