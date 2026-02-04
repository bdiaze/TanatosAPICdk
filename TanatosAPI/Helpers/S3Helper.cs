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

		public async Task AgregarTag(string bucketName, string bucketKey, string tagKey, string tagValue) {
			// Obtenemos los tags actuales del objeto...
			GetObjectTaggingRequest requestGet = new() {
				BucketName = bucketName,
				Key = bucketKey,
			};

			GetObjectTaggingResponse responseGet = await amazonS3.GetObjectTaggingAsync(requestGet);

			Dictionary<string, string> tags = (responseGet.Tagging ?? Enumerable.Empty<Tag>()).ToDictionary(tag => tag.Key, tag => tag.Value);

			// Se agrega o modifica el tag indicado...
			tags[tagKey] = tagValue;

			// Se sube la nueva lista de tags...
			PutObjectTaggingRequest request = new() {
				BucketName = bucketName,
				Key = bucketKey,
				Tagging = new Tagging {
					TagSet = [.. tags.Select(kv => new Tag { Key = kv.Key, Value = kv.Value })]
				}
			};

			await amazonS3.PutObjectTaggingAsync(request);
		}
	}
}
