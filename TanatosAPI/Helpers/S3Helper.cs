using Amazon.S3;
using Amazon.S3.Model;
using System.Net.Mime;
using System.Text.RegularExpressions;

namespace TanatosAPI.Helpers {
	public class S3Helper(IAmazonS3 amazonS3) {
		private readonly int PRE_SIGNED_URL_EXPIRATION_MINUTES = 5;

		public async Task<string> ObtenerPutPreSignedUrl(string bucketName, string bucketKey, string contentType) {
			GetPreSignedUrlRequest request = new() {
				BucketName = bucketName,
				Key = bucketKey,
				Verb = HttpVerb.PUT,
				Expires = DateTime.UtcNow.AddMinutes(PRE_SIGNED_URL_EXPIRATION_MINUTES),
				ContentType = contentType,
			};

			return await amazonS3.GetPreSignedURLAsync(request);
		}

		public async Task<string> ObtenerGetPreSignedUrl(string bucketName, string bucketKey, string nombreArchivo) {
			GetPreSignedUrlRequest request = new() {
				BucketName = bucketName,
				Key = bucketKey,
				Verb = HttpVerb.GET,
				Expires = DateTime.UtcNow.AddMinutes(PRE_SIGNED_URL_EXPIRATION_MINUTES),
				ResponseHeaderOverrides = new ResponseHeaderOverrides {
					ContentDisposition = $"attachment; filename*=UTF-8''{Uri.EscapeDataString(nombreArchivo)}"
				}
			};

			return await amazonS3.GetPreSignedURLAsync(request);
		}

		public async Task<(long contentLength, string contentType)> ObtenerObjectMetadata(string bucketName, string bucketKey) {
			GetObjectMetadataRequest request = new() { 
				BucketName = bucketName,
				Key = bucketKey,
			};

			GetObjectMetadataResponse response = await amazonS3.GetObjectMetadataAsync(request);
			return (response.ContentLength, response.ContentType);
		}
	}
}
