using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Test.Business {
    public class DocumentoAdjuntoBcpTest {
        private readonly IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
		private readonly IDocumentoAdjuntoDao documentoAdjuntoDao = Substitute.For<IDocumentoAdjuntoDao>();
		private readonly IDocumentoAdjuntoHelper documentoAdjuntoHelper = Substitute.For<IDocumentoAdjuntoHelper>();
        private readonly DocumentoAdjuntoBcp documentoAdjuntoBcp;

		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

		public DocumentoAdjuntoBcpTest() {
			dateTimeProvider.UtcNow.Returns(FECHA_DUMMY);

			documentoAdjuntoBcp = new(dateTimeProvider, documentoAdjuntoDao, documentoAdjuntoHelper);
		}

		public static DocumentoAdjunto DocumentoAdjuntoDummy(
            long id = 1,
            long idHistorialNormaSuscrita = 1,
            string bucketName = "BucketNameTest",
            string bucketKey = "BucketKeyTest",
            string nombreArchivo = "NombreArchivoTest",
            string mimeEsperado = "MimeEsperadoTest",
            long tamannoEsperado = 1024,
            string? mimeReal = "MimeRealTest",
            long? tamannoReal = 1024,
            short estadoSubida = 1 /* Documento recepcionado */,
            DateTime? fechaEmisionUrlPrefirmadaPut = null,
            DateTime? fechaConfirmacionSubida = null,
            DateTime? fechaCreacion = null,
            DateTime? fechaEliminacion = null,
            bool vigencia = true
        ) => new() { 
            Id = id,
            IdHistorialNormaSuscrita = idHistorialNormaSuscrita,
            BucketName = bucketName,
            BucketKey = bucketKey,
            NombreArchivo = nombreArchivo,
            MimeEsperado = mimeEsperado,
            TamannoEsperado = tamannoEsperado,
            MimeReal = mimeReal,
            TamannoReal = tamannoReal,
            EstadoSubida = estadoSubida,
            FechaEmisionUrlPrefirmadaPut = fechaEmisionUrlPrefirmadaPut ?? DateTime.UtcNow,
            FechaConfirmacionSubida = fechaConfirmacionSubida,
            FechaCreacion = fechaCreacion ?? DateTime.UtcNow,
            FechaEliminacion = fechaEliminacion,
            Vigencia = vigencia
        };
        
        [Theory]
        [InlineData(DocumentoAdjuntoBcp.MAX_FILE_SIZE + 1, false)]
        [InlineData(DocumentoAdjuntoBcp.MAX_FILE_SIZE, true)]
        [InlineData(DocumentoAdjuntoBcp.MAX_FILE_SIZE - 1, true)]
        public void TamannoValidTest(long fileSize, bool expectedResult) {
            Assert.Equal(expectedResult, documentoAdjuntoBcp.TamannoValido(fileSize));
        }

        public static TheoryData<string, bool> MimeValidoCases => new() {
            { DocumentoAdjuntoBcp.ALLOWED_FILES_TYPES.First(), true },
            { DocumentoAdjuntoBcp.ALLOWED_FILES_TYPES.Last(), true },
            { "mime/invalido", false },
        };

        [Theory]
        [MemberData(nameof(MimeValidoCases))]
        public void MimeValidoTest(string mime, bool expectedResult) {
            Assert.Equal(expectedResult, documentoAdjuntoBcp.MimeValido(mime));
        }

        public static TheoryData<DocumentoAdjunto?, bool> EstaVigenteCases => new() {
            { DocumentoAdjuntoDummy(vigencia: true), true },
            { DocumentoAdjuntoDummy(vigencia: false), false },
            { null, false },
        };
        [Theory]
        [MemberData(nameof(EstaVigenteCases))]
        public void EstaVigenteTest(DocumentoAdjunto? documentoAdjunto, bool expectedResult) {
            Assert.Equal(expectedResult, documentoAdjuntoBcp.EstaVigente(documentoAdjunto));
        }

		public static TheoryData<DocumentoAdjunto, bool> FueRecepcionadoCases => new() {
			{ DocumentoAdjuntoDummy(estadoSubida: 1), true },
			{ DocumentoAdjuntoDummy(estadoSubida: 0), false }
		};
		[Theory]
		[MemberData(nameof(FueRecepcionadoCases))]
		public void FueRecepcionadoTest(DocumentoAdjunto documentoAdjunto, bool expectedResult) {
			Assert.Equal(expectedResult, documentoAdjuntoBcp.FueRecepcionado(documentoAdjunto));
		}

		public static TheoryData<(DocumentoAdjunto documentoAdjunto, long idHistorialNormaSuscrita), bool> PerteneceAVencimientoCases => new() {
            { (DocumentoAdjuntoDummy(idHistorialNormaSuscrita: 1), 1), true },
            { (DocumentoAdjuntoDummy(idHistorialNormaSuscrita: 1), 2), false },
        };
        [Theory]
        [MemberData(nameof(PerteneceAVencimientoCases))]
        public void PerteneceAVencimientoTest((DocumentoAdjunto documentoAdjunto, long idHistorialNormaSuscrita) entrada, bool expectedResult) {
            Assert.Equal(expectedResult, documentoAdjuntoBcp.Pertenece(entrada.documentoAdjunto, entrada.idHistorialNormaSuscrita));
        }

        [Theory]
        [InlineData(1L, 1L)]
        [InlineData(2L, 2L)]
        [InlineData(3L, null)]
        public async Task ObtenerTest(long idDocumentoAdjunto, long? expectedIdResult) {
            documentoAdjuntoDao.ObtenerPorId(1).Returns(DocumentoAdjuntoDummy(id: 1));
            documentoAdjuntoDao.ObtenerPorId(2).Returns(DocumentoAdjuntoDummy(id: 2));
            documentoAdjuntoDao.ObtenerPorId(3).Returns((DocumentoAdjunto?)null);

            DocumentoAdjunto? documento = await documentoAdjuntoBcp.Obtener(idDocumentoAdjunto);
            Assert.Equal(expectedIdResult, documento?.Id);
        }

        [Theory]
        [InlineData(1L, 2)]
        [InlineData(2L, 1)]
        [InlineData(3L, 0)]
        public async Task ObtenerPorVencimientoTest(long idHistorialNormaSuscrita, int expectedCount) {
            documentoAdjuntoDao.ObtenerPorHistorial(1, null).Returns([
                DocumentoAdjuntoDummy(id: 1, idHistorialNormaSuscrita: 1, estadoSubida: 1),
                DocumentoAdjuntoDummy(id: 2, idHistorialNormaSuscrita: 1, estadoSubida: 1),
				DocumentoAdjuntoDummy(id: 20, idHistorialNormaSuscrita: 1, estadoSubida: 0)
			]);
            documentoAdjuntoDao.ObtenerPorHistorial(2, null).Returns([
                DocumentoAdjuntoDummy(id: 3, idHistorialNormaSuscrita: 2, estadoSubida: 1),
				DocumentoAdjuntoDummy(id: 30, idHistorialNormaSuscrita: 2, estadoSubida: 0),
			]);
            documentoAdjuntoDao.ObtenerPorHistorial(3, null).Returns([]);

            List<DocumentoAdjunto> documentos = await documentoAdjuntoBcp.ObtenerPorVencimiento(idHistorialNormaSuscrita, filtrarVigentes: true, filtrarRecepcionados: true);
			Assert.Equal(expectedCount, documentos.Count);
			Assert.All(documentos, documento => {
                Assert.Equal(idHistorialNormaSuscrita, documento.IdHistorialNormaSuscrita);
                Assert.True(documento.Vigencia);
                Assert.Equal(1, documento.EstadoSubida);
            });
        }

        [Fact]
        public async Task GenerarUrlSubidaTest() {
            documentoAdjuntoHelper.ObtenerPostPreSignedUrl(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<string>(), Arg.Any<long>())
                .Returns(("bucket-name-test", "bucket-key-test", "https://pre-signed-url.test", new Dictionary<string, string>() { { "field-name-test", "field-value-test" } }));
            documentoAdjuntoDao.Insertar(Arg.Any<DocumentoAdjunto>()).Returns(99L);


            (string preSignedUrl, Dictionary<string, string> fields, DocumentoAdjunto documentoAdjunto) = await documentoAdjuntoBcp.GenerarUrlSubida(
                "sub-test-123", 
                1, 
                2, 
                3, 
                "nombreArchivoTest", 
                "mime/test", 
                1024
            );

            Assert.Equal("https://pre-signed-url.test", preSignedUrl);
            Assert.Equal("field-value-test", fields["field-name-test"]);
            Assert.Equal(99L, documentoAdjunto.Id);
            Assert.Equal("bucket-name-test", documentoAdjunto.BucketName);
            Assert.Equal(0 /* Generada URL prefirmada para PUT */, documentoAdjunto.EstadoSubida);
            Assert.Equal(FECHA_DUMMY, documentoAdjunto.FechaCreacion);
            Assert.Null(documentoAdjunto.FechaConfirmacionSubida);
            Assert.True(documentoAdjunto.Vigencia);
            await documentoAdjuntoDao.Received(1).Insertar(Arg.Any<DocumentoAdjunto>());
        }

        [Fact]
        public async Task ConfirmarSubidaTest_NoConfirmado() {            
            DocumentoAdjunto documento = DocumentoAdjuntoDummy(estadoSubida: 0);

			documentoAdjuntoHelper.ObtenerMetadata(documento.BucketKey)
                .Returns((2048, "mime-real/test"));

            await documentoAdjuntoBcp.ConfirmarSubida(documento);

            Assert.Equal(1, documento.EstadoSubida);
            Assert.Equal("mime-real/test", documento.MimeReal);
            Assert.Equal(2048, documento.TamannoReal);
            Assert.Equal(FECHA_DUMMY, documento.FechaConfirmacionSubida);
            await documentoAdjuntoDao.Received(1).Actualizar(documento, null);
        }

        [Fact]
        public async Task ConfirmarSubidaTest_YaConfirmado() {
            DocumentoAdjunto documento = DocumentoAdjuntoDummy(estadoSubida: 1);
            
            await documentoAdjuntoBcp.ConfirmarSubida(documento);

            await documentoAdjuntoDao.DidNotReceive().Actualizar(Arg.Any<DocumentoAdjunto>());
            await documentoAdjuntoHelper.DidNotReceive().ObtenerMetadata(Arg.Any<string>());
        }

        [Fact]
        public async Task GenerarUrlBajadaTest() {
            DocumentoAdjunto documento = DocumentoAdjuntoDummy();
			documentoAdjuntoHelper.ObtenerGetPreSignedUrl(documento.BucketKey, documento.NombreArchivo)
                .Returns("https://url.download");

            string url = await documentoAdjuntoBcp.GenerarUrlBajada(documento);
            Assert.Equal("https://url.download", url);
        }

        [Fact]
        public async Task EliminarTest_CuandoVigente() {
            DocumentoAdjunto documento = DocumentoAdjuntoDummy(vigencia: true);

            await documentoAdjuntoBcp.Eliminar(documento);
            
            Assert.False(documento.Vigencia);
            Assert.Equal(FECHA_DUMMY, documento.FechaEliminacion);
            await documentoAdjuntoDao.Received(1).Actualizar(documento, null);
            await documentoAdjuntoHelper.Received(1).AgregarTagEstadoEliminado(documento.BucketKey);
        }

        [Fact]
        public async Task EliminarTest_NoVigente() {
            DocumentoAdjunto documento = DocumentoAdjuntoDummy(vigencia: false);

            await documentoAdjuntoBcp.Eliminar(documento);

            await documentoAdjuntoDao.DidNotReceive().Actualizar(Arg.Any<DocumentoAdjunto>());
            await documentoAdjuntoHelper.DidNotReceive().AgregarTagEstadoEliminado(Arg.Any<string>());
        }

        [Fact]
        public async Task EliminarPorHistorialTest_ConDocumentos() {
            List<DocumentoAdjunto> documentos = new() { 
                DocumentoAdjuntoDummy(id: 1, vigencia: true),
                DocumentoAdjuntoDummy(id: 2, vigencia: true)
            };
            documentoAdjuntoDao.ObtenerPorHistorial(10, true, null).Returns(documentos);

            await documentoAdjuntoBcp.EliminarPorHistorialNormaSuscrita(10);

            await documentoAdjuntoDao.Received(documentos.Count).Actualizar(Arg.Any<DocumentoAdjunto>(), null);
            await documentoAdjuntoHelper.Received(documentos.Count).AgregarTagEstadoEliminado(Arg.Any<string>());
        }

        [Fact]
        public async Task EliminarPorHistorialTest_SinDocumentos() {
            documentoAdjuntoDao.ObtenerPorHistorial(999, true, null).Returns([]);

            await documentoAdjuntoBcp.EliminarPorHistorialNormaSuscrita(999);

            await documentoAdjuntoDao.DidNotReceive().Actualizar(Arg.Any<DocumentoAdjunto>(), null);
            await documentoAdjuntoHelper.DidNotReceive().AgregarTagEstadoEliminado(Arg.Any<string>());

        }
    }
}
