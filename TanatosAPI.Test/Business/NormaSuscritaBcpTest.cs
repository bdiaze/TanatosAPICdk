using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using Npgsql;
using NSubstitute;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.UseCases;

namespace TanatosAPI.Test.Business {
    public class NormaSuscritaBcpTest {
        private readonly IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        private readonly IVariableEntornoHelper variableEntorno = Substitute.For<IVariableEntornoHelper>();
        private readonly IKairosHelper kairosHelper = Substitute.For<IKairosHelper>();
        private readonly INormaSuscritaDao normaSuscritaDao = Substitute.For<INormaSuscritaDao>();
        private readonly NormaSuscritaBcp normaSuscritaBcp;

        private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

        public NormaSuscritaBcpTest() {
            dateTimeProvider.UtcNow.Returns(FECHA_DUMMY);

            normaSuscritaBcp = new(dateTimeProvider, variableEntorno, kairosHelper, normaSuscritaDao);
        }

        public static NormaSuscrita NormaSuscritaDummy(
            long id = 100,
            string sub = "sub-test",
            long idNegocio = 1000,
            long? idTemplate = null,
            long? idNorma = null,
            string? nombre = "nombre-test",
            string? descripcion = "descripcion-test",
            string? multa = "multa-test",
            long? idTipoPeriodicidad = 10,
            long? idCategoriaNorma = 50,
            long? idCargo = 10_000,
            long? ordenVisual = null,
            bool editable = true,
            DateTime? fechaActivacion = null,
            DateTime? fechaDesactivacion = null,
            bool activado = false,
            List<ProcesoNotificacion>? procesosNotificacion = null,
            DateTime? fechaCreacion = null,
            DateTime? fechaEliminacion = null,
            bool vigencia = true
        ) => new() { 
            Id = id,
            Sub = sub,
            IdNegocio = idNegocio,
            IdTemplate = idTemplate,
            IdNorma = idNorma,
            Nombre = nombre,
            Descripcion = descripcion,
            Multa = multa,
            IdTipoPeriodicidad = idTipoPeriodicidad,
            IdCategoriaNorma = idCategoriaNorma,
            IdCargo = idCargo,
            OrdenVisual = ordenVisual,
            Editable = editable,
            FechaActivacion = fechaActivacion,
            FechaDesactivacion = fechaDesactivacion,
            Activado = activado,
            ProcesosNotificaciones = procesosNotificacion ?? [],
            FechaCreacion = fechaCreacion ?? FECHA_DUMMY,
            FechaEliminacion = fechaEliminacion,
            Vigencia = vigencia
        };

        public static TheoryData<NormaSuscrita?, bool> EstaVigenteCases => new() {
            { NormaSuscritaDummy(vigencia: true), true },
            { NormaSuscritaDummy(vigencia: false), false },
            { null, false },
        };
        [Theory]
        [MemberData(nameof(EstaVigenteCases))]
        public void EstaVigenteTest(NormaSuscrita? normaSuscrita, bool expectedResult) {
            Assert.Equal(expectedResult, normaSuscritaBcp.EstaVigente(normaSuscrita));
        }

        public static TheoryData<NormaSuscrita, string, bool> PerteneceCases => new() {
            { NormaSuscritaDummy(sub: "sub-test-1"), "sub-test-1", true },
            { NormaSuscritaDummy(sub: "sub-test-2"), "sub-test-1", false },
        };
        [Theory]
        [MemberData(nameof(PerteneceCases))]
        public void PerteneceTest(NormaSuscrita normaSuscrita, string sub, bool expectedResult) {
            Assert.Equal(expectedResult, normaSuscritaBcp.Pertenece(normaSuscrita, sub));
        }

        public static TheoryData<NormaSuscrita, long, bool> PerteneceNegocioCases => new() {
            { NormaSuscritaDummy(idNegocio: 100), 100, true },
            { NormaSuscritaDummy(idNegocio: 200), 100, false },
        };
        [Theory]
        [MemberData(nameof(PerteneceNegocioCases))]
        public void PerteneceNegocioTest(NormaSuscrita normaSuscrita, long idNegocio, bool expectedResult) {
            Assert.Equal(expectedResult, normaSuscritaBcp.PerteneceNegocio(normaSuscrita, idNegocio));
        }

        public static TheoryData<NormaSuscrita, bool> EstaActivaCases => new() {
            { NormaSuscritaDummy(vigencia: true, activado: true), true },
            { NormaSuscritaDummy(vigencia: true, activado: false), false },
            { NormaSuscritaDummy(vigencia: false, activado: true), false },
            { NormaSuscritaDummy(vigencia: false, activado: false), false },
        };
        [Theory]
        [MemberData(nameof(EstaActivaCases))]
        public void EstaActivaTest(NormaSuscrita normaSuscrita, bool expectedResult) {
            Assert.Equal(expectedResult, normaSuscritaBcp.EstaActiva(normaSuscrita));
        }

        public static TheoryData<NormaSuscrita, bool> EsEditableCases => new() {
            { NormaSuscritaDummy(editable: true), true },
            { NormaSuscritaDummy(editable: false), false }
        };
        [Theory]
        [MemberData(nameof(EsEditableCases))]
        public void EsEditableTest(NormaSuscrita normaSuscrita, bool expectedResult) {
            Assert.Equal(expectedResult, normaSuscritaBcp.EsEditable(normaSuscrita));
        }

        [Fact]
        public void FiltrarVigentesTest() {
            List<NormaSuscrita> normas = [
                NormaSuscritaDummy(id: 1, vigencia: true),
                NormaSuscritaDummy(id: 2, vigencia: false),
                NormaSuscritaDummy(id: 3, vigencia: true),
            ];

            List<NormaSuscrita> retorno = normaSuscritaBcp.FiltrarVigentes(normas);
            Assert.Equal(2, retorno.Count);
            Assert.All(retorno, r => {
                Assert.True(r.Vigencia);
                Assert.NotEqual(2, r.Id);
            });
        }

        [Fact]
        public async Task ObtenerTest_SinParametros() {
            normaSuscritaDao.ObtenerPorId(10).Returns(NormaSuscritaDummy(id: 10));

            NormaSuscrita? retorno = await normaSuscritaBcp.Obtener(10);
            Assert.NotNull(retorno);
            Assert.Equal(10, retorno.Id);
            await normaSuscritaDao.Received(1).ObtenerPorId(10);
        }

        [Fact]
        public async Task ObtenerTest_TodosParametros() {
            normaSuscritaDao.ObtenerPorId(10).Returns(NormaSuscritaDummy(id: 10, vigencia: true, sub: "sub-test", idNegocio: 100, editable: true));

            NormaSuscrita? retorno = await normaSuscritaBcp.Obtener(10, filtrarVigente: true, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 100, validarEditable: true);
            Assert.NotNull(retorno);
            Assert.Equal(10, retorno.Id);
            await normaSuscritaDao.Received(1).ObtenerPorId(10);
        }

        [Fact]
        public async Task ObtenerTest_FiltrandoNoVigentes() {
            normaSuscritaDao.ObtenerPorId(10).Returns(NormaSuscritaDummy(id: 10, vigencia: false));

            NormaSuscrita? retorno = await normaSuscritaBcp.Obtener(10, filtrarVigente: true);
            Assert.Null(retorno);
            await normaSuscritaDao.Received(1).ObtenerPorId(10);
        }

        [Fact]
        public async Task ObtenerTest_ValidandoNoVigente() {
            normaSuscritaDao.ObtenerPorId(10).Returns(NormaSuscritaDummy(id: 10, vigencia: false));

            ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => normaSuscritaBcp.Obtener(10, validarVigencia: true));
            Assert.Equal(TipoErrorValidacion.NoVigente, ex.TipoErrorValidacion);
            await normaSuscritaDao.Received(1).ObtenerPorId(10);
        }

        [Fact]
        public async Task ObtenerTest_ValidandoPertenencia() {
            normaSuscritaDao.ObtenerPorId(10).Returns(NormaSuscritaDummy(id: 10, sub: "sub-test", vigencia: true));

            ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => normaSuscritaBcp.Obtener(10, validarSub: "otro-sub-test"));
            Assert.Equal(TipoErrorValidacion.NoPertenece, ex.TipoErrorValidacion);
            await normaSuscritaDao.Received(1).ObtenerPorId(10);
        }

        [Fact]
        public async Task ObtenerTest_ValidandoPertenenciaNegocio() {
            normaSuscritaDao.ObtenerPorId(10).Returns(NormaSuscritaDummy(id: 10, idNegocio: 100, vigencia: true));

            ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => normaSuscritaBcp.Obtener(10, validarIdNegocio: 200));
            Assert.Equal(TipoErrorValidacion.NoPertenece, ex.TipoErrorValidacion);
            await normaSuscritaDao.Received(1).ObtenerPorId(10);
        }

        [Fact]
        public async Task ObtenerTest_ValidandoEditable() {
            normaSuscritaDao.ObtenerPorId(10).Returns(NormaSuscritaDummy(id: 10, editable: false, vigencia: true));

            ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => normaSuscritaBcp.Obtener(10, validarEditable: true));
            Assert.Equal(TipoErrorValidacion.EstadoNoValido, ex.TipoErrorValidacion);
            await normaSuscritaDao.Received(1).ObtenerPorId(10);
        }

        [Fact]
        public async Task ObtenerPorSubYNegocioTest_SinParametros() {
            normaSuscritaDao.ObtenerPorSub("sub-test", 100, null).Returns([
                NormaSuscritaDummy(id: 1, sub: "sub-test", idNegocio: 100, vigencia: true),
                NormaSuscritaDummy(id: 2, sub: "sub-test", idNegocio: 100, vigencia: false),
                NormaSuscritaDummy(id: 3, sub: "sub-test", idNegocio: 100, vigencia: true),
            ]);

            List<NormaSuscrita> retorno = await normaSuscritaBcp.ObtenerPorSubYNegocio("sub-test", 100);
            Assert.Equal(3, retorno.Count);
            Assert.Contains(1, retorno.Select(n => n.Id));
            Assert.Contains(2, retorno.Select(n => n.Id));
            Assert.Contains(3, retorno.Select(n => n.Id));
            await normaSuscritaDao.Received(1).ObtenerPorSub("sub-test", 100, null);
        }

        [Fact]
        public async Task ObtenerPorSubYNegocioTest_FiltrandoVigentes() {
            normaSuscritaDao.ObtenerPorSub("sub-test", 100, null).Returns([
                NormaSuscritaDummy(id: 1, sub: "sub-test", idNegocio: 100, vigencia: true),
                NormaSuscritaDummy(id: 2, sub: "sub-test", idNegocio: 100, vigencia: false),
                NormaSuscritaDummy(id: 3, sub: "sub-test", idNegocio: 100, vigencia: true),
            ]);

            List<NormaSuscrita> retorno = await normaSuscritaBcp.ObtenerPorSubYNegocio("sub-test", 100, filtrarVigentes: true);
            Assert.Equal(2, retorno.Count);
            Assert.Contains(1, retorno.Select(n => n.Id));
            Assert.DoesNotContain(2, retorno.Select(n => n.Id));
            Assert.Contains(3, retorno.Select(n => n.Id));
            await normaSuscritaDao.Received(1).ObtenerPorSub("sub-test", 100, null);
        }

        [Fact]
        public async Task CrearObligacionUsuarioTest_Valido() {
            normaSuscritaDao.ObtenerPorSub("sub-test", 100, null).Returns([
                NormaSuscritaDummy(id: 1, sub: "sub-test", idNegocio: 100, nombre: "nombre-test-1", vigencia: true),
                NormaSuscritaDummy(id: 2, sub: "sub-test", idNegocio: 100, nombre: "nombre-test-2", vigencia: false),
            ]);
            normaSuscritaDao.Insertar(Arg.Any<NormaSuscrita>()).Returns(99);

            NormaSuscrita retorno = await normaSuscritaBcp.CrearObligacionUsuario(
                "sub-test",
                100,
                "otro-nombre-test",
                "descripcion-test",
                "multa-test",
                10,
                20,
                30,
                true
            );
            Assert.Equal(99, retorno.Id);
            Assert.Equal("sub-test", retorno.Sub);
            Assert.Equal(100, retorno.IdNegocio);
            Assert.Equal("otro-nombre-test", retorno.Nombre);
            Assert.Equal("descripcion-test", retorno.Descripcion);
            Assert.Equal("multa-test", retorno.Multa);
            Assert.Equal(10, retorno.IdTipoPeriodicidad);
            Assert.Equal(20, retorno.IdCategoriaNorma);
            Assert.Equal(30, retorno.IdCargo);
            Assert.True(retorno.Vigencia);

            await normaSuscritaDao.Received(1).ObtenerPorSub("sub-test", 100, null);
            await normaSuscritaDao.Received(1).Insertar(
                Arg.Is<NormaSuscrita>(n => 
                    n.Sub == "sub-test" &&
                    n.IdNegocio == 100 &&
                    n.IdTemplate == null &&
                    n.IdNorma == null &&
                    n.Nombre == "otro-nombre-test" &&
                    n.Descripcion == "descripcion-test" &&
                    n.Multa == "multa-test" && 
                    n.IdTipoPeriodicidad == 10 &&
                    n.IdCategoriaNorma == 20 && 
                    n.IdCargo == 30 &&
                    n.OrdenVisual == null &&
                    n.Editable == true &&
                    n.FechaActivacion == FECHA_DUMMY &&
                    n.FechaDesactivacion == null &&
                    n.Activado == true &&
                    n.FechaCreacion == FECHA_DUMMY &&
                    n.FechaEliminacion == null &&
                    n.Vigencia == true
                ), 
                Arg.Any<NpgsqlTransaction?>()
            );
        }

        [Fact]
        public async Task CrearObligacionUsuarioTest_NombreRepetido() {
            normaSuscritaDao.ObtenerPorSub("sub-test", 100, null).Returns([
                NormaSuscritaDummy(id: 1, sub: "sub-test", idNegocio: 100, nombre: "nombre-test-1", vigencia: true),
                NormaSuscritaDummy(id: 2, sub: "sub-test", idNegocio: 100, nombre: "nombre-test-2", vigencia: false),
            ]);

            ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => normaSuscritaBcp.CrearObligacionUsuario(
                "sub-test",
                100,
                "nombre-test-1",
                "descripcion-test",
                "multa-test",
                10,
                20,
                30,
                true
            ));
            Assert.Equal(TipoErrorValidacion.YaExiste, ex.TipoErrorValidacion);

            await normaSuscritaDao.Received(1).ObtenerPorSub("sub-test", 100, null);
            await normaSuscritaDao.DidNotReceive().Insertar(Arg.Any<NormaSuscrita>(), Arg.Any<NpgsqlTransaction?>());
        }
    }
}
