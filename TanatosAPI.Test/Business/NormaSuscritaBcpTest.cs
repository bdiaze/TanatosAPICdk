using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using Npgsql;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Transactions;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Kairos;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.UseCases;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TanatosAPI.Test.Business {
    public class NormaSuscritaBcpTest {
        private readonly IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        private readonly IVariableEntornoHelper variableEntorno = Substitute.For<IVariableEntornoHelper>();
        private readonly IKairosHelper kairosHelper = Substitute.For<IKairosHelper>();
        private readonly INormaSuscritaDao normaSuscritaDao = Substitute.For<INormaSuscritaDao>();
        private readonly NormaSuscritaBcp normaSuscritaBcp;

        private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);
		private static readonly DateTime FECHA_DUMMY_CHILE = new(2026, 1, 15, 11, 0, 0, DateTimeKind.Unspecified);

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

		public static ProcesoNotificacion ProcesoNotificacionDummy(
			string idProceso = "id-proceso-test-1",
			string idCalendarizacion = "id-calendarizacion-test-1",
			string nombre = "nombre-test-1",
			string arnRol = "arn-rol-test",
			string arnProceso = "arn-proceso-test",
			EntKairosParametrosProceso? parametros = null,
			bool habilitado = true,
			DateTime? fechaCreacion = null,
			string? cron = null,
			int? frecuenciaDias = null,
			DateTime? inicioEjecucionUtc = null
		) => new() {
			IdProceso = idProceso,
			IdCalendarizacion = idCalendarizacion,
			Nombre = nombre,
			ArnRol = arnRol,
			ArnProceso = arnProceso,
			Parametros = parametros == null ? "parametros-test-1" : JsonSerializer.Serialize(parametros),
			Cron = cron,
			FrecuenciaDias = frecuenciaDias,
			InicioEjecucionUtc = inicioEjecucionUtc
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

		public static TheoryData<string, long, string, string?, string?, long?, long?, long?, bool, string?, string?> CrearObligacionUsuarioCases => new() {
			{ "sub-test", 100, "nombre-test-100", "descripcion-test-100", "multa-test-100", 10, 20, 30, true, "descripcion-test-100", "multa-test-100" },
			{ "sub-test", 100, "nombre-test-100", "descripcion-test-100   ", "multa-test-100   ", 10, 20, 30, true, "descripcion-test-100", "multa-test-100" },
			{ "sub-test", 100, "nombre-test-100", "   ", "   ", 10, 20, 30, true, null, null },
			{ "sub-test", 100, "nombre-test-100", null, null, 10, 20, 30, true, null, null },
		};
		[Theory]
		[MemberData(nameof(CrearObligacionUsuarioCases))]
		public async Task CrearObligacionUsuarioTest_Valido(string sub, long idNegocio, string nombre, string? descripcion, string? multa, long? idTipoPeriodicidad, long? idCategoriaNorma, long? idCargo, bool activado, string? expectedDescripcion, string? expectedMulta) {
            normaSuscritaDao.ObtenerPorSub(sub, idNegocio, null).Returns([]);
            normaSuscritaDao.Insertar(Arg.Any<NormaSuscrita>()).Returns(99);

            NormaSuscrita retorno = await normaSuscritaBcp.CrearObligacionUsuario(
				sub,
				idNegocio,
				nombre,
				descripcion,
				multa,
				idTipoPeriodicidad,
				idCategoriaNorma,
				idCargo,
				activado
			);
            Assert.Equal(99, retorno.Id);
            Assert.Equal(sub, retorno.Sub);
            Assert.Equal(idNegocio, retorno.IdNegocio);
            Assert.Equal(nombre, retorno.Nombre);
            Assert.Equal(expectedDescripcion, retorno.Descripcion);
            Assert.Equal(expectedMulta, retorno.Multa);
            Assert.Equal(idTipoPeriodicidad, retorno.IdTipoPeriodicidad);
            Assert.Equal(idCategoriaNorma, retorno.IdCategoriaNorma);
            Assert.Equal(idCargo, retorno.IdCargo);
            Assert.Equal(activado, retorno.Vigencia);

            await normaSuscritaDao.Received(1).ObtenerPorSub(sub, idNegocio, null);
            await normaSuscritaDao.Received(1).Insertar(
                Arg.Is<NormaSuscrita>(n => 
                    n.Sub == sub &&
                    n.IdNegocio == idNegocio &&
                    n.IdTemplate == null &&
                    n.IdNorma == null &&
                    n.Nombre == nombre &&
                    n.Descripcion == expectedDescripcion &&
                    n.Multa == expectedMulta && 
                    n.IdTipoPeriodicidad == idTipoPeriodicidad &&
                    n.IdCategoriaNorma == idCategoriaNorma && 
                    n.IdCargo == idCargo &&
                    n.OrdenVisual == null &&
                    n.Editable == true &&
                    n.FechaActivacion == FECHA_DUMMY &&
                    n.FechaDesactivacion == null &&
                    n.Activado == activado &&
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

        [Fact]
        public async Task ActualizarTest() {
            await normaSuscritaBcp.Actualizar(NormaSuscritaDummy(id: 100));
            await normaSuscritaDao.Received(1).Actualizar(Arg.Is<NormaSuscrita>(n => n.Id == 100), Arg.Any<NpgsqlTransaction?>());
        }

		[Fact]
		public async Task ActivarTest_Valido() {
			await normaSuscritaBcp.Activar(NormaSuscritaDummy(activado: false));
			await normaSuscritaDao.Received(1).Actualizar(
                Arg.Is<NormaSuscrita>(n => 
                    n.FechaActivacion == FECHA_DUMMY &&
                    n.FechaDesactivacion == null &&
                    n.Activado == true
                ), 
                Arg.Any<NpgsqlTransaction?>()
            );
		}

		[Fact]
		public async Task ActivarTest_YaActivado() {
			await normaSuscritaBcp.Activar(NormaSuscritaDummy(activado: true));
			await normaSuscritaDao.DidNotReceive().Actualizar(Arg.Any<NormaSuscrita>(),Arg.Any<NpgsqlTransaction?>());
		}

        [Fact]
		public async Task DesactivarTest_Valido() {
			await normaSuscritaBcp.Desactivar(NormaSuscritaDummy(activado: true));
			await normaSuscritaDao.Received(1).Actualizar(
                Arg.Is<NormaSuscrita>(n => 
                    n.FechaDesactivacion == FECHA_DUMMY &&
                    n.Activado == false
                ), 
                Arg.Any<NpgsqlTransaction?>()
            );
		}

		[Fact]
		public async Task DesactivarTest_YaDesactivado() {
			await normaSuscritaBcp.Desactivar(NormaSuscritaDummy(activado: false));
			await normaSuscritaDao.DidNotReceive().Actualizar(Arg.Any<NormaSuscrita>(),Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task EliminarTest_Valido() {
			await normaSuscritaBcp.Eliminar(NormaSuscritaDummy(vigencia: true));
			await normaSuscritaDao.Received(1).Actualizar(
				Arg.Is<NormaSuscrita>(n =>
					n.FechaEliminacion == FECHA_DUMMY &&
					n.Vigencia == false
				),
				Arg.Any<NpgsqlTransaction?>()
			);
		}

		[Fact]
		public async Task EliminarTest_YaEliminado() {
			await normaSuscritaBcp.Eliminar(NormaSuscritaDummy(vigencia: false));
			await normaSuscritaDao.DidNotReceive().Actualizar(Arg.Any<NormaSuscrita>(), Arg.Any<NpgsqlTransaction?>());
		}
	}
}
