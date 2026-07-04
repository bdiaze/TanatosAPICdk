using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.Helpers {
	public class DocumentoAdjuntoHelper(IS3Helper s3Helper, IVariableEntornoHelper variableEntorno) : IDocumentoAdjuntoHelper {
		public readonly string BUCKET_NAME = variableEntorno.Obtener("BUCKET_NAME_DOCUMENTOS_ADJUNTOS");

		public async Task<(string bucketName, string bucketKey, string preSignedUrl)> ObtenerPutPreSignedUrl(string sub, long idNegocio, long idNormaSuscrita, long idHistorialNormaSuscrita, string contentType) {
			string key = $"Sub-{sub}/Negocio-{idNegocio}/NormaSuscrita-{idNormaSuscrita}/HistorialNormaSuscrita-{idHistorialNormaSuscrita}/{Guid.NewGuid()}";
			return (BUCKET_NAME, key, await s3Helper.ObtenerPutPreSignedUrl(BUCKET_NAME, key, contentType));
		}

		public async Task<string> ObtenerGetPreSignedUrl(string bucketKey, string nombreDocumento) {
			return await s3Helper.ObtenerGetPreSignedUrl(BUCKET_NAME, bucketKey, nombreDocumento);
		}

		public async Task<(string bucketName, string bucketKey, string preSignedUrl, Dictionary<string, string> fields)> ObtenerPostPreSignedUrl(string sub, long idNegocio, long idNormaSuscrita, long idHistorialNormaSuscrita, string contentType, long maxSize = 10 * 1024 * 1024) {
			string key = $"Sub-{sub}/Negocio-{idNegocio}/NormaSuscrita-{idNormaSuscrita}/HistorialNormaSuscrita-{idHistorialNormaSuscrita}/{Guid.NewGuid()}";
			(string url, Dictionary<string, string> fields) = await s3Helper.ObtenerPostPreSignedUrl(BUCKET_NAME, key, contentType, maxSize);
			return (BUCKET_NAME, key, url, fields);
		}

		public async Task<(long contentLength, string contentType)> ObtenerMetadata(string bucketKey) {
			return await s3Helper.ObtenerObjectMetadata(BUCKET_NAME, bucketKey);
		}

		public async Task AgregarTagEstadoEliminado(string bucketKey) {
			await s3Helper.AgregarTag(BUCKET_NAME, bucketKey, "Estado", "Eliminado");
		}
	}
}
