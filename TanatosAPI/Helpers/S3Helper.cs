using Amazon.S3;
using Amazon.S3.Model;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Text.RegularExpressions;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Helpers {
    [ExcludeFromCodeCoverage]
    public class S3Helper(IAmazonS3 amazonS3, IDateTimeProvider dateTimeProvider): IS3Helper {
		private readonly int PRE_SIGNED_URL_EXPIRATION_MINUTES = 5;

		public async Task<string> ObtenerPutPreSignedUrl(string bucketName, string bucketKey, string contentType) {
			GetPreSignedUrlRequest request = new() {
				BucketName = bucketName,
				Key = bucketKey,
				Verb = HttpVerb.PUT,
				Expires = dateTimeProvider.UtcNow.AddMinutes(PRE_SIGNED_URL_EXPIRATION_MINUTES),
				ContentType = contentType,
			};

			return await amazonS3.GetPreSignedURLAsync(request);
		}

		public async Task<string> ObtenerGetPreSignedUrl(string bucketName, string bucketKey, string nombreArchivo) {
			GetPreSignedUrlRequest request = new() {
				BucketName = bucketName,
				Key = bucketKey,
				Verb = HttpVerb.GET,
				Expires = dateTimeProvider.UtcNow.AddMinutes(PRE_SIGNED_URL_EXPIRATION_MINUTES),
				ResponseHeaderOverrides = new ResponseHeaderOverrides {
					ContentDisposition = $"attachment; filename*=UTF-8''{Uri.EscapeDataString(nombreArchivo)}"
				}
			};

			return await amazonS3.GetPreSignedURLAsync(request);
		}

		public async Task<(string url, Dictionary<string, string> fields)> ObtenerPostPreSignedUrl(string bucketName, string bucketKey, string contentType, long maxSize = 10 * 1024 * 1024) {
			CreatePresignedPostRequest request = new() { 
				BucketName = bucketName,
				Key = bucketKey,
				Expires = dateTimeProvider.UtcNow.AddMinutes(PRE_SIGNED_URL_EXPIRATION_MINUTES),
				Conditions = [
					S3PostCondition.ExactMatch("Content-Type", contentType),
					S3PostCondition.ContentLengthRange(0, maxSize),
				]
			};


			CreatePresignedPostResponse response = await amazonS3.CreatePresignedPostAsync(request);
			return (response.Url, response.Fields);
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
