using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecretsManager;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.Test.Helpers {
	public class S3HelperTest {
		private readonly IAmazonS3 amazonS3 = Substitute.For<IAmazonS3>();
		private readonly IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
		private readonly S3Helper s3Helper;

		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

		public S3HelperTest() {
			dateTimeProvider.UtcNow.Returns(FECHA_DUMMY);

			s3Helper = new(amazonS3, dateTimeProvider);
		}

		[Fact]
		public async Task ObtenerPutPreSignedUrlTest() {
			amazonS3.GetPreSignedURLAsync(Arg.Any<GetPreSignedUrlRequest>()).Returns("https://presigned-url.test");

			string retorno = await s3Helper.ObtenerPutPreSignedUrl("bucket-name-test", "bucket-key-test", "content-type-test");
			Assert.Equal("https://presigned-url.test", retorno);
			await amazonS3.Received(1).GetPreSignedURLAsync(Arg.Any<GetPreSignedUrlRequest>());
		}

		[Fact]
		public async Task ObtenerGetPreSignedUrlTest() {
			amazonS3.GetPreSignedURLAsync(Arg.Any<GetPreSignedUrlRequest>()).Returns("https://presigned-url.test");

			string retorno = await s3Helper.ObtenerGetPreSignedUrl("bucket-name-test", "bucket-key-test", "nombre-archivo-test");
			Assert.Equal("https://presigned-url.test", retorno);
			await amazonS3.Received(1).GetPreSignedURLAsync(Arg.Any<GetPreSignedUrlRequest>());
		}

		[Fact]
		public async Task ObtenerPostPreSignedUrlTest() {
			amazonS3.CreatePresignedPostAsync(Arg.Any<CreatePresignedPostRequest>()).Returns(new CreatePresignedPostResponse {
				Url = "https://presigned-url.test",
				Fields = new Dictionary<string, string>() {
					["field-test"] = "value-test"
				}
			});

			(string url, Dictionary<string, string> fields) = await s3Helper.ObtenerPostPreSignedUrl("bucket-name-test", "bucket-key-test", "content-type-test");
			Assert.Equal("https://presigned-url.test", url);
			Assert.Single(fields.Keys);
			Assert.Equal("value-test", fields["field-test"]);
			await amazonS3.Received(1).CreatePresignedPostAsync(Arg.Any<CreatePresignedPostRequest>());
		}

		[Fact]
		public async Task ObtenerObjectMetadataTest() {
			amazonS3.GetObjectMetadataAsync(Arg.Any<GetObjectMetadataRequest>()).Returns(new GetObjectMetadataResponse {
				ContentLength = 1024,
				ContentType = "content-type-test"
			});

			(long contentLength, string contentType) = await s3Helper.ObtenerObjectMetadata("bucket-name-test", "bucket-key-test");
			Assert.Equal(1024, contentLength);
			Assert.Equal("content-type-test", contentType);
			await amazonS3.Received(1).GetObjectMetadataAsync(Arg.Any<GetObjectMetadataRequest>());
		}

		[Fact]
		public async Task AgregarTagTest() {
			amazonS3.GetObjectTaggingAsync(Arg.Any<GetObjectTaggingRequest>()).Returns(new GetObjectTaggingResponse {
				Tagging = []
			});

			await s3Helper.AgregarTag("bucket-name-test", "bucket-key-test", "tag-key-test", "tag-value-test");
			await amazonS3.Received(1).GetObjectTaggingAsync(Arg.Any<GetObjectTaggingRequest>());
			await amazonS3.Received(1).PutObjectTaggingAsync(Arg.Any<PutObjectTaggingRequest>());
		}
	}
}
