using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Test.Business {
    public class DocumentoAdjuntoBcpTest {
        private readonly IVariableEntornoHelper variableEntorno = Substitute.For<IVariableEntornoHelper>();
        private readonly IS3Helper s3Helper = Substitute.For<IS3Helper>();
        private readonly IDocumentoAdjuntoDao documentoAdjuntoDao = Substitute.For<IDocumentoAdjuntoDao>();
        private readonly IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        private readonly DocumentoAdjuntoHelper documentoAdjuntoHelper;
        private readonly DocumentoAdjuntoBcp documentoAdjuntoBcp;

        public static DocumentoAdjunto DocumentoAdjuntoTest(
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

        private const string FAKE_BUCKET_NAME = "bucket-name-test";
        private static readonly DateTime FAKE_FECHA = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

        public DocumentoAdjuntoBcpTest() {
            variableEntorno.Obtener("BUCKET_NAME_DOCUMENTOS_ADJUNTOS").Returns(FAKE_BUCKET_NAME);
            dateTimeProvider.UtcNow.Returns(FAKE_FECHA);
            documentoAdjuntoHelper = new(s3Helper, variableEntorno);
            documentoAdjuntoBcp = new(dateTimeProvider, documentoAdjuntoDao, documentoAdjuntoHelper);
        }

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
            { DocumentoAdjuntoTest(vigencia: true), true },
            { DocumentoAdjuntoTest(vigencia: false), false },
            { null, false },
        };
        [Theory]
        [MemberData(nameof(EstaVigenteCases))]
        public void EstaVigenteTest(DocumentoAdjunto? documentoAdjunto, bool expectedResult) {
            Assert.Equal(expectedResult, documentoAdjuntoBcp.EstaVigente(documentoAdjunto));
        }

        public static TheoryData<(DocumentoAdjunto documentoAdjunto, long idHistorialNormaSuscrita), bool> PerteneceAVencimientoCases => new() {
            { (DocumentoAdjuntoTest(idHistorialNormaSuscrita: 1), 1), true },
            { (DocumentoAdjuntoTest(idHistorialNormaSuscrita: 1), 2), false },
        };
        [Theory]
        [MemberData(nameof(PerteneceAVencimientoCases))]
        public void PerteneceAVencimientoTest((DocumentoAdjunto documentoAdjunto, long idHistorialNormaSuscrita) entrada, bool expectedResult) {
            Assert.Equal(expectedResult, documentoAdjuntoBcp.PerteneceAVencimiento(entrada.documentoAdjunto, entrada.idHistorialNormaSuscrita));
        }

        [Theory]
        [InlineData(1L, 1L)]
        [InlineData(2L, 2L)]
        [InlineData(3L, null)]
        public async Task ObtenerPorIdTest(long idDocumentoAdjunto, long? expectedIdResult) {
            documentoAdjuntoDao.ObtenerPorId(1).Returns(DocumentoAdjuntoTest(id: 1));
            documentoAdjuntoDao.ObtenerPorId(2).Returns(DocumentoAdjuntoTest(id: 2));
            documentoAdjuntoDao.ObtenerPorId(3).Returns((DocumentoAdjunto?)null);

            DocumentoAdjunto? documento = await documentoAdjuntoBcp.ObtenerPorId(idDocumentoAdjunto);
            Assert.Equal(expectedIdResult, documento?.Id);
        }

        [Theory]
        [InlineData(1L, 2)]
        [InlineData(2L, 1)]
        [InlineData(3L, 0)]
        public async Task ObtenerVigentesPorHistorialNormaSuscritaTest(long idHistorialNormaSuscrita, int expectedCount) {
            documentoAdjuntoDao.ObtenerPorHistorial(1).Returns([
                DocumentoAdjuntoTest(id: 1, idHistorialNormaSuscrita: 1),
                DocumentoAdjuntoTest(id: 2, idHistorialNormaSuscrita: 1)
            ]);
            documentoAdjuntoDao.ObtenerPorHistorial(2).Returns([
                DocumentoAdjuntoTest(id: 3, idHistorialNormaSuscrita: 2)
            ]);
            documentoAdjuntoDao.ObtenerPorHistorial(3).Returns([]);

            List<DocumentoAdjunto> documentos = await documentoAdjuntoBcp.ObtenerVigentesPorHistorialNormaSuscrita(idHistorialNormaSuscrita);
            Assert.All(documentos, documento => Assert.Equal(idHistorialNormaSuscrita, documento.IdHistorialNormaSuscrita));
            Assert.Equal(expectedCount, documentos.Count);
        }

        [Fact]
        public async Task GenerarUrlSubidaTest() {
            const string FAKE_URL_RETURN = "https://url.test";
            Dictionary<string, string> FAKE_FIELDS_RETURN = new() {
                { "fake-field", "fake-field-value" }    
            };

            s3Helper.ObtenerPostPreSignedUrl(FAKE_BUCKET_NAME, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>())
                .Returns((FAKE_URL_RETURN, FAKE_FIELDS_RETURN));
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

            Assert.Equal(FAKE_URL_RETURN, preSignedUrl);
            Assert.Equal(FAKE_FIELDS_RETURN.Count, fields.Count);
            Assert.All(FAKE_FIELDS_RETURN, ffr => {
                Assert.True(fields.ContainsKey(ffr.Key));
                Assert.Equal(ffr.Value, fields[ffr.Key]);
            });
            Assert.Equal(99L, documentoAdjunto.Id);
            Assert.Equal(FAKE_BUCKET_NAME, documentoAdjunto.BucketName);
            Assert.Equal(0 /* Generada URL prefirmada para PUT */, documentoAdjunto.EstadoSubida);
            Assert.Equal(FAKE_FECHA, documentoAdjunto.FechaCreacion);
            Assert.Null(documentoAdjunto.FechaConfirmacionSubida);
            Assert.True(documentoAdjunto.Vigencia);
            await documentoAdjuntoDao.Received(1).Insertar(Arg.Any<DocumentoAdjunto>());
        }

        [Fact]
        public async Task ConfirmarSubidaTest_NoConfirmado() {
            const long FAKE_CONTENT_LENGTH = 2048;
            const string FAKE_CONTENT_TYPE = "mime-real/test";
            
            DocumentoAdjunto documento = DocumentoAdjuntoTest(estadoSubida: 0);

            s3Helper.ObtenerObjectMetadata(FAKE_BUCKET_NAME, documento.BucketKey)
                .Returns((FAKE_CONTENT_LENGTH, FAKE_CONTENT_TYPE));

            await documentoAdjuntoBcp.ConfirmarSubida(documento);

            Assert.Equal(1, documento.EstadoSubida);
            Assert.Equal(FAKE_CONTENT_TYPE, documento.MimeReal);
            Assert.Equal(FAKE_CONTENT_LENGTH, documento.TamannoReal);
            Assert.Equal(FAKE_FECHA, documento.FechaConfirmacionSubida);
            await documentoAdjuntoDao.Received(1).Actualizar(documento, null);
        }

        [Fact]
        public async Task ConfirmarSubidaTest_YaConfirmado() {
            DocumentoAdjunto documento = DocumentoAdjuntoTest(estadoSubida: 1);
            
            await documentoAdjuntoBcp.ConfirmarSubida(documento);

            await documentoAdjuntoDao.DidNotReceive().Actualizar(Arg.Any<DocumentoAdjunto>());
            await s3Helper.DidNotReceive().ObtenerObjectMetadata(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GenerarUrlBajadaTest() {
            const string FAKE_URL_DOWNLOAD = "https://url.download";
            DocumentoAdjunto documento = DocumentoAdjuntoTest();
            s3Helper.ObtenerGetPreSignedUrl(FAKE_BUCKET_NAME, documento.BucketKey, documento.NombreArchivo)
                .Returns(FAKE_URL_DOWNLOAD);

            string url = await documentoAdjuntoBcp.GenerarUrlBajada(documento);
            Assert.Equal(FAKE_URL_DOWNLOAD, url);
        }

        [Fact]
        public async Task EliminarTest_CuandoVigente() {
            DocumentoAdjunto documento = DocumentoAdjuntoTest(vigencia: true);

            await documentoAdjuntoBcp.Eliminar(documento);
            
            Assert.False(documento.Vigencia);
            Assert.Equal(FAKE_FECHA, documento.FechaEliminacion);
            await documentoAdjuntoDao.Received(1).Actualizar(documento, null);
            await s3Helper.Received(1).AgregarTag(FAKE_BUCKET_NAME, documento.BucketKey, "Estado", "Eliminado");
        }

        [Fact]
        public async Task EliminarTest_NoVigente() {
            DocumentoAdjunto documento = DocumentoAdjuntoTest(vigencia: false);

            await documentoAdjuntoBcp.Eliminar(documento);

            await documentoAdjuntoDao.DidNotReceive().Actualizar(Arg.Any<DocumentoAdjunto>());
            await s3Helper.DidNotReceive().AgregarTag(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task EliminarPorHistorialTest_ConDocumentos() {
            List<DocumentoAdjunto> documentos = new() { 
                DocumentoAdjuntoTest(id: 1, vigencia: true),
                DocumentoAdjuntoTest(id: 2, vigencia: true)
            };
            documentoAdjuntoDao.ObtenerPorHistorial(10, true, null).Returns(documentos);

            await documentoAdjuntoBcp.EliminarPorHistorialNormaSuscrita(10);

            await documentoAdjuntoDao.Received(documentos.Count).Actualizar(Arg.Any<DocumentoAdjunto>(), null);
            await s3Helper.Received(documentos.Count).AgregarTag(FAKE_BUCKET_NAME, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task EliminarPorHistorialTest_SinDocumentos() {
            documentoAdjuntoDao.ObtenerPorHistorial(999, true, null).Returns([]);

            await documentoAdjuntoBcp.EliminarPorHistorialNormaSuscrita(999);

            await documentoAdjuntoDao.DidNotReceive().Actualizar(Arg.Any<DocumentoAdjunto>(), null);
            await s3Helper.DidNotReceive().AgregarTag(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());

        }
    }
}
