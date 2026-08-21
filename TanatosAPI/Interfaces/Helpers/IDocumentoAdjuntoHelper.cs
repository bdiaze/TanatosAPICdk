namespace TanatosAPI.Interfaces.Helpers {
	public interface IDocumentoAdjuntoHelper {
		public Task<(string bucketName, string bucketKey, string preSignedUrl)> ObtenerPutPreSignedUrl(string sub, long idNegocio, long idNormaSuscrita, long idHistorialNormaSuscrita, string contentType);
		public Task<string> ObtenerGetPreSignedUrl(string bucketKey, string nombreDocumento, bool inline = false);
		public Task<(string bucketName, string bucketKey, string preSignedUrl, Dictionary<string, string> fields)> ObtenerPostPreSignedUrl(string sub, long idNegocio, long idNormaSuscrita, long idHistorialNormaSuscrita, string contentType, long size);
		public Task<(long contentLength, string contentType)> ObtenerMetadata(string bucketKey);
		public Task AgregarTagEstadoEliminado(string bucketKey);
	}
}
