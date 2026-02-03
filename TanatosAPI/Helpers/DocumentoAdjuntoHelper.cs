namespace TanatosAPI.Helpers {
	public class DocumentoAdjuntoHelper(S3Helper s3Helper, VariableEntornoHelper variableEntorno) {
		public readonly string BUCKET_NAME = variableEntorno.Obtener("BUCKET_NAME_DOCUMENTOS_ADJUNTOS");

		public async Task<(string bucketName, string bucketKey, string preSignedUrl)> ObtenerPutPreSignedUrl(string sub, long idNegocio, long idNormaSuscrita, long idHistorialNormaSuscrita, string contentType) {
			string key = $"Sub-{sub}/Negocio-{idNegocio}/NormaSuscrita-{idNormaSuscrita}/HistorialNormaSuscrita-{idHistorialNormaSuscrita}/{Guid.NewGuid()}";
			return (BUCKET_NAME, key, await s3Helper.ObtenerPutPreSignedUrl(BUCKET_NAME, key, contentType));
		}

		public async Task<string> ObtenerGetPreSignedUrl(string bucketKey) {
			return await s3Helper.ObtenerGetPreSignedUrl(BUCKET_NAME, bucketKey);
		}

		public async Task<(long contentLength, string contentType)> ObtenerMetadata(string bucketKey) {
			return await s3Helper.ObtenerObjectMetadata(BUCKET_NAME, bucketKey);
		}
	}
}
