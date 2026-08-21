using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IDocumentoAdjuntoBcp {
		public bool TamannoValido(long tamanno);
		public bool MimeValido(string mime);
		public bool EstaVigente(DocumentoAdjunto? documentoAdjunto);
		public bool FueRecepcionado(DocumentoAdjunto documentoAdjunto);
		public bool Pertenece(DocumentoAdjunto documentoAdjunto, long idHistorialNormaSuscrita);
		public List<DocumentoAdjunto> FiltrarVigentes(List<DocumentoAdjunto> documentos);
		public List<DocumentoAdjunto> FiltrarRecepcionados(List<DocumentoAdjunto> documentos);
		public Task<DocumentoAdjunto?> Obtener(long idDocumentoAdjunto, NpgsqlTransaction? transaction = null);
		public Task<List<DocumentoAdjunto>> ObtenerPorVencimiento(long idHistorialNormaSuscrita, bool filtrarVigentes = false, bool filtrarRecepcionados = false, NpgsqlTransaction? transaction = null);
		public Task<(string preSignedUrl, Dictionary<string, string> fields, DocumentoAdjunto documentoAdjunto)> GenerarUrlSubida(string sub, long idNegocio, long idNormaSuscrita, long idHistorialNormaSuscrita, string nombreArchivo, string mimeArchivo, long tamannoArchivo);
		public Task ConfirmarSubida(DocumentoAdjunto documentoAdjunto);
		public Task<string> GenerarUrlBajada(DocumentoAdjunto documentoAdjunto, bool paraVisualizacion = false);
		public Task Eliminar(DocumentoAdjunto documentoAdjunto, NpgsqlTransaction? transaction = null);
		public Task EliminarPorHistorialNormaSuscrita(long idHistorialNormaSuscrita, NpgsqlTransaction? transaction = null);
	}
}
