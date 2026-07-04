namespace TanatosAPI.Interfaces.Helpers {
    public interface IS3Helper {
        public Task<string> ObtenerPutPreSignedUrl(string bucketName, string bucketKey, string contentType);
        public Task<string> ObtenerGetPreSignedUrl(string bucketName, string bucketKey, string nombreArchivo);
        public Task<(string url, Dictionary<string, string> fields)> ObtenerPostPreSignedUrl(string bucketName, string bucketKey, string contentType, long maxSize);
        public Task<(long contentLength, string contentType)> ObtenerObjectMetadata(string bucketName, string bucketKey);
        public Task AgregarTag(string bucketName, string bucketKey, string tagKey, string tagValue);
    }
}
