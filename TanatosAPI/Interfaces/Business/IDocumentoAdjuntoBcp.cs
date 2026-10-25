using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IDocumentoAdjuntoBcp {
		public bool TamannoValido(long tamanno);
		public bool MimeValido(string mime);
		public bool EstaVigente(DocumentoAdjunto? documentoAdjunto);
		public bool FueRecepcionado(DocumentoAdjunto documentoAdjunto);
		public bool Pertenece(DocumentoAdjunto documentoAdjunto, long idHistorialNormaSuscrita);
		public bool PerteneceMovimiento(DocumentoAdjunto documentoAdjunto, long idMovimientoFinanciero);
		public List<DocumentoAdjunto> FiltrarVigentes(List<DocumentoAdjunto> documentos);
		public List<DocumentoAdjunto> FiltrarRecepcionados(List<DocumentoAdjunto> documentos);
		public Task<DocumentoAdjunto?> Obtener(long idDocumentoAdjunto, bool filtrarVigente = false, bool validarVigencia = false, long? validarIdHistorialNormaSuscrita = null, long? validarIdMovimientoFinanciero = null, NpgsqlTransaction? transaction = null);
		public Task<List<DocumentoAdjunto>> ObtenerPorNegocio(long idNegocio, bool filtrarVigentes = false, bool filtrarRecepcionados = false, NpgsqlTransaction? transaction = null);
		public Task<List<DocumentoAdjunto>> ObtenerPorVencimiento(long idHistorialNormaSuscrita, bool filtrarVigentes = false, bool filtrarRecepcionados = false, NpgsqlTransaction? transaction = null);
		public Task<List<DocumentoAdjunto>> ObtenerPorMovimiento(long idMovimientoFinanciero, bool filtrarVigentes = false, bool filtrarRecepcionados = false, NpgsqlTransaction? transaction = null);
		public Task<(string preSignedUrl, Dictionary<string, string> fields, DocumentoAdjunto documentoAdjunto)> GenerarUrlSubida(string sub, long idNegocio, string nombreArchivo, string mimeArchivo, long tamannoArchivo, long? idNormaSuscrita = null, long? idHistorialNormaSuscrita = null, long? idCuentaFinanciera = null, long? idMovimientoFinanciero = null);
		public Task ConfirmarSubida(DocumentoAdjunto documentoAdjunto);
		public Task<string> GenerarUrlBajada(DocumentoAdjunto documentoAdjunto, bool paraVisualizacion = false);
		public Task Eliminar(DocumentoAdjunto documentoAdjunto, NpgsqlTransaction? transaction = null);
		public Task EliminarPorNegocio(long idNegocio, NpgsqlTransaction? transaction = null);
		public Task EliminarPorHistorialNormaSuscrita(long idHistorialNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task EliminarPorMovimientoFinanciero(long idMovimientoFinanciero, NpgsqlTransaction? transaction = null);
	}
}
