using Amazon.CognitoIdentityProvider;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Test.Helpers {
	public class DocumentoAdjuntoHelperTest {
		private readonly IS3Helper s3Helper = Substitute.For<IS3Helper>();
		private readonly IVariableEntornoHelper variableEntorno = Substitute.For<IVariableEntornoHelper>();
		private readonly DocumentoAdjuntoHelper documentoAdjuntoHelper;

		private const string BUCKET_NAME_TEST = "bucket-name-test";

		public DocumentoAdjuntoHelperTest() {
			variableEntorno.Obtener("BUCKET_NAME_DOCUMENTOS_ADJUNTOS").Returns(BUCKET_NAME_TEST);

			documentoAdjuntoHelper = new(s3Helper, variableEntorno);
		}

		[Fact]
		public async Task ObtenerPutPreSignedUrlTest() {
			s3Helper.ObtenerPutPreSignedUrl(BUCKET_NAME_TEST, Arg.Any<string>(), Arg.Any<string>()).Returns("https://pre-signed-url.test");

			(string bucketName, string bucketKey, string preSignedUrl) = await documentoAdjuntoHelper.ObtenerPutPreSignedUrl("sub-test-123", 1, 10, 100, "mime/test");
			Assert.Equal(BUCKET_NAME_TEST, bucketName);
			Assert.StartsWith($"Sub-sub-test-123/Negocio-1/NormaSuscrita-10/HistorialNormaSuscrita-100/", bucketKey);
			Assert.Equal("https://pre-signed-url.test", preSignedUrl);
			await s3Helper.Received(1).ObtenerPutPreSignedUrl(BUCKET_NAME_TEST, Arg.Any<string>(), "mime/test");
		}

		[Fact]
		public async Task ObtenerGetPreSignedUrlTest() {
			s3Helper.ObtenerGetPreSignedUrl(BUCKET_NAME_TEST, Arg.Any<string>(), Arg.Any<string>()).Returns("https://pre-signed-url.test");

			string preSignedUrl = await documentoAdjuntoHelper.ObtenerGetPreSignedUrl("bucket-key-test", "nombre-archivo.test");
			Assert.Equal("https://pre-signed-url.test", preSignedUrl);
			await s3Helper.Received(1).ObtenerGetPreSignedUrl(BUCKET_NAME_TEST, Arg.Any<string>(), Arg.Any<string>());
		}

		[Fact]
		public async Task ObtenerPostPreSignedUrlTest() {
			s3Helper.ObtenerPostPreSignedUrl(BUCKET_NAME_TEST, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>()).Returns(
				(
					"https://pre-signed-url.test",
					new Dictionary<string, string>() {
						{ "field-name-test", "field-value-test" }
					}
				)
			);

			(string bucketName, string bucketKey, string preSignedUrl, Dictionary<string, string> fields) = await documentoAdjuntoHelper.ObtenerPostPreSignedUrl("sub-test-123", 1, 10, 100, "mime/test", 1024);
			Assert.Equal(BUCKET_NAME_TEST, bucketName);
			Assert.StartsWith($"Sub-sub-test-123/Negocio-1/NormaSuscrita-10/HistorialNormaSuscrita-100/", bucketKey);
			Assert.Equal("https://pre-signed-url.test", preSignedUrl);
			Assert.Equal("field-value-test", fields["field-name-test"]);
			await s3Helper.Received(1).ObtenerPostPreSignedUrl(BUCKET_NAME_TEST, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>());
		}

		[Fact]
		public async Task ObtenerMetadataTest() {
			s3Helper.ObtenerObjectMetadata(BUCKET_NAME_TEST, Arg.Any<string>()).Returns((1024, "mime/test"));
			(long contentLength, string contentType) = await documentoAdjuntoHelper.ObtenerMetadata("bucket-key-test");
			Assert.Equal(1024, contentLength);
			Assert.Equal("mime/test", contentType);
			await s3Helper.Received(1).ObtenerObjectMetadata(BUCKET_NAME_TEST, Arg.Any<string>());
		}

		[Fact]
		public async Task AgregarTagEstadoEliminadoTest() {
			await documentoAdjuntoHelper.AgregarTagEstadoEliminado("bucket-key-test");
			await s3Helper.Received(1).AgregarTag(BUCKET_NAME_TEST, Arg.Any<string>(), "Estado", "Eliminado");
		}
	}
}
