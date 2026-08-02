using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using Npgsql;
using NSubstitute;
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
			Habilitado = habilitado,
			FechaCreacion = fechaCreacion ?? FECHA_DUMMY,
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

        [Fact]
        public async Task ProgramarUnProcesoNotificacionTest() {
            await normaSuscritaBcp.ProgramarUnProcesoNotificacion(new EntKairosIngresarProceso {
				Nombre = "nombre-test",
				Cron = "cron-test",
				ArnRol = "arn-rol-test",
				ArnProceso = "arn-proceso-test",
				Parametros = "parametros-test",
				Habilitado = true
			});
            await kairosHelper.Received(1).IngresarProceso(Arg.Is<EntKairosIngresarProceso>(p => 
                p.Nombre == "nombre-test" &&
                p.Cron == "cron-test" &&
                p.ArnRol == "arn-rol-test" &&
                p.ArnProceso == "arn-proceso-test" &&
                p.Parametros == "parametros-test" &&
                p.Habilitado == true

            ));
        }

        [Fact]
        public async Task ProgramarVariosProcesosNotificacionTest() {
            await normaSuscritaBcp.ProgramarVariosProcesosNotificacion([
				new EntKairosIngresarProceso {
				    Nombre = "nombre-test",
				    Cron = "cron-test",
				    ArnRol = "arn-rol-test",
				    ArnProceso = "arn-proceso-test",
				    Parametros = "parametros-test",
				    Habilitado = true
			    },
				new EntKairosIngresarProceso {
					Nombre = "nombre-test-2",
					Cron = "cron-test",
					ArnRol = "arn-rol-test",
					ArnProceso = "arn-proceso-test",
					Parametros = "parametros-test",
					Habilitado = true
				},
			]);
			await kairosHelper.Received(2).IngresarProceso(Arg.Is<EntKairosIngresarProceso>(p =>
				(p.Nombre == "nombre-test" || p.Nombre == "nombre-test-2") &&
				p.Cron == "cron-test" &&
				p.ArnRol == "arn-rol-test" &&
				p.ArnProceso == "arn-proceso-test" &&
				p.Parametros == "parametros-test" &&
				p.Habilitado == true
			));
		}

        [Fact]
        public async Task DesprogramarUnProcesoNotificacion() {
            await normaSuscritaBcp.DesprogramarUnProcesoNotificacion("id-proceso-test");
            await kairosHelper.Received(1).EliminarProceso("id-proceso-test");
        }

		[Fact]
		public async Task DesprogramarVariosProcesosNotificacion() {
			await normaSuscritaBcp.DesprogramarVariosProcesosNotificacion([
				"id-proceso-test",
				"id-proceso-test-2"
			]);
			await kairosHelper.Received(2).EliminarProceso(Arg.Is<string>(s => s == "id-proceso-test" || s == "id-proceso-test-2"));
		}

        [Fact]
        public async Task ReversarProcesosTest() {
            await normaSuscritaBcp.ReversarProcesos([
                new ProcesoNotificacion() {
                    IdProceso = "id-proceso-test-1",
                    IdCalendarizacion = "id-calendarizacion-test",
                    Nombre = "nombre-test",
                    ArnRol = "arn-rol-test",
                    ArnProceso = "arn-proceso-test",
                    Parametros = "parametros-test",
                    Habilitado = true,
                    FechaCreacion = FECHA_DUMMY,
                    Cron = "cron-test"
				}
            ], [
				new ProcesoNotificacion() {
					IdProceso = "id-proceso-test-2",
					IdCalendarizacion = "id-calendarizacion-test-2",
					Nombre = "nombre-test-2",
					ArnRol = "arn-rol-test",
					ArnProceso = "arn-proceso-test",
					Parametros = "parametros-test",
					Habilitado = true,
					FechaCreacion = FECHA_DUMMY,
					Cron = "cron-test"
				}
			]);
			await kairosHelper.Received(1).IngresarProceso(Arg.Is<EntKairosIngresarProceso>(p =>
				p.Nombre == "nombre-test-2" &&
				p.Cron == "cron-test" &&
				p.ArnRol == "arn-rol-test" &&
				p.ArnProceso == "arn-proceso-test" &&
				p.Parametros == "parametros-test" &&
				p.Habilitado == true
			));
			await kairosHelper.Received(1).EliminarProceso(Arg.Is<string>(s => s == "id-proceso-test-1"));
		}

        [Fact]
        public async Task ExtraerCronsAEliminarTest() {
            NormaSuscrita normaSuscrita = NormaSuscritaDummy();
            normaSuscrita.ProcesosNotificaciones = [
				ProcesoNotificacionDummy(idProceso: "id-proceso-test-1", idCalendarizacion: "id-calendarizacion-test-1", nombre: "nombre-test-1", cron: "0 13 15 * ? *", parametros: new EntKairosParametrosProceso {
					IdNormaSuscrita = normaSuscrita.Id,
					Cron = "0 13 15 * ? *",
					IdTipoUnidadTiempoAntelacion = null,
					CantAntelacion = null,
					EsVencimiento = true,
					ProgramarSiguienteEjecucion = true
				}),
				ProcesoNotificacionDummy(idProceso: "id-proceso-test-2", idCalendarizacion: "id-calendarizacion-test-2", nombre: "nombre-test-2", cron: "0 12 15 * ? *", parametros: new EntKairosParametrosProceso {
					IdNormaSuscrita = normaSuscrita.Id,
					Cron = "0 12 15 * ? *",
					IdTipoUnidadTiempoAntelacion = 1,
					CantAntelacion = 1,
					EsVencimiento = false,
					ProgramarSiguienteEjecucion = false
				}),
			];
            List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> deseados = [
                ("0 13 15 * ? *", null, null, true),
				("0 11 15 * ? *", new TipoUnidadTiempo() { Id = 1, Nombre = "Hora", CantSegundos = 3600, CantMinutos = 60, CantHoras = 1, Vigencia = true }, 2, false),
			];

			List<ProcesoNotificacion> retorno = normaSuscritaBcp.ExtraerCronsAEliminar(normaSuscrita, deseados);
            Assert.Single(retorno);
            Assert.Equal("id-proceso-test-2", retorno.First().IdProceso);
        }

		[Fact]
		public async Task ExtraerCronsACrearTest() {
			NormaSuscrita normaSuscrita = NormaSuscritaDummy();
			normaSuscrita.ProcesosNotificaciones = [
				ProcesoNotificacionDummy(idProceso: "id-proceso-test-1", idCalendarizacion: "id-calendarizacion-test-1", nombre: "nombre-test-1", cron: "0 13 15 * ? *", parametros: new EntKairosParametrosProceso {
					IdNormaSuscrita = normaSuscrita.Id,
					Cron = "0 13 15 * ? *",
					IdTipoUnidadTiempoAntelacion = null,
					CantAntelacion = null,
					EsVencimiento = true,
					ProgramarSiguienteEjecucion = true
				}),
				ProcesoNotificacionDummy(idProceso: "id-proceso-test-2", idCalendarizacion: "id-calendarizacion-test-2", nombre: "nombre-test-2", cron: "0 12 15 * ? *", parametros: new EntKairosParametrosProceso {
					IdNormaSuscrita = normaSuscrita.Id,
					Cron = "0 12 15 * ? *",
					IdTipoUnidadTiempoAntelacion = 1,
					CantAntelacion = 1,
					EsVencimiento = false,
					ProgramarSiguienteEjecucion = false
				}),
			];
			List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> deseados = [
				("0 13 15 * ? *", null, null, true),
				("0 11 15 * ? *", new TipoUnidadTiempo() { Id = 1, Nombre = "Hora", CantSegundos = 3600, CantMinutos = 60, CantHoras = 1, Vigencia = true }, 2, false),
			];

			List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> retorno = normaSuscritaBcp.ExtraerCronsACrear(normaSuscrita, deseados);
			Assert.Single(retorno);
			Assert.Equal("0 11 15 * ? *", retorno.First().Cron);
		}

        [Fact]
        public async Task ActualizarProcesosCronProgramadosTest() {
            variableEntorno.Obtener("APP_NAME").Returns("app-name-test");
			variableEntorno.Obtener("NOTIFICACIONES_LAMBDA_ARN").Returns("arn-proceso-test");
			variableEntorno.Obtener("NOTIFICACIONES_EJECUCION_ROLE_ARN").Returns("arn-rol-test");
            kairosHelper.IngresarProceso(Arg.Any<EntKairosIngresarProceso>()).Returns(new SalKairosIngresarProceso() {
                IdProceso = "id-proceso-test-3",
                IdCalendarizacion = "id-calendarizacion-test-3",
                Nombre = "nombre-test-3",
                ArnProceso = "arn-proceso-test",
                ArnRol = "arn-rol-test",
                Parametros = JsonSerializer.Serialize(new EntKairosParametrosProceso {
                    IdNormaSuscrita = 100,
                    Cron = "0 11 15 * ? *",
                    IdTipoUnidadTiempoAntelacion = 1,
                    CantAntelacion = 2,
                    EsVencimiento = false,
                    ProgramarSiguienteEjecucion = false
                }),
                FechaCreacion = FECHA_DUMMY,
                Habilitado = true
			});

			NormaSuscrita normaSuscrita = NormaSuscritaDummy(id: 100);
			normaSuscrita.ProcesosNotificaciones = [
				ProcesoNotificacionDummy(idProceso: "id-proceso-test-1", idCalendarizacion: "id-calendarizacion-test-1", nombre: "nombre-test-1", cron: "0 13 15 * ? *", parametros: new EntKairosParametrosProceso {
					IdNormaSuscrita = normaSuscrita.Id,
					Cron = "0 13 15 * ? *",
					IdTipoUnidadTiempoAntelacion = null,
					CantAntelacion = null,
					EsVencimiento = true,
					ProgramarSiguienteEjecucion = true
				}),
				ProcesoNotificacionDummy(idProceso: "id-proceso-test-2", idCalendarizacion: "id-calendarizacion-test-2", nombre: "nombre-test-2", cron: "0 12 15 * ? *", parametros: new EntKairosParametrosProceso {
					IdNormaSuscrita = normaSuscrita.Id,
					Cron = "0 12 15 * ? *",
					IdTipoUnidadTiempoAntelacion = 1,
					CantAntelacion = 1,
					EsVencimiento = false,
					ProgramarSiguienteEjecucion = false
				}),
			];
			List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> deseados = [
				("0 13 15 * ? *", null, null, true),
				("0 11 15 * ? *", new TipoUnidadTiempo() { Id = 1, Nombre = "Hora", CantSegundos = 3600, CantMinutos = 60, CantHoras = 1, Vigencia = true }, 2, false),
			];

            (List<ProcesoNotificacion> programados, List<ProcesoNotificacion> desprogramados) = await normaSuscritaBcp.ActualizarProcesosCronProgramados(normaSuscrita, deseados);
            Assert.Single(programados);
            Assert.Equal("0 11 15 * ? *", programados.First().Cron);
            Assert.Single(desprogramados);
            Assert.Equal("id-proceso-test-2", desprogramados.First().IdProceso);
            Assert.Equal(2, normaSuscrita.ProcesosNotificaciones.Count);
            Assert.All(normaSuscrita.ProcesosNotificaciones, p => {
                Assert.True(p.IdProceso == "id-proceso-test-1" || p.IdProceso == "id-proceso-test-3");
				Assert.NotEqual("id-proceso-test-2", p.IdProceso);
			});
            await kairosHelper.Received(1).IngresarProceso(Arg.Any<EntKairosIngresarProceso>());
			await kairosHelper.Received(1).IngresarProceso(Arg.Is<EntKairosIngresarProceso>(p =>
			    p.Nombre.StartsWith("app-name-test - ") &&
				p.Nombre.Contains($"- NormaSuscrita {normaSuscrita.Id} - ") &&
				p.Nombre.EndsWith($"Cron 0 11 15 * ? *") &&
				p.Cron == "0 11 15 * ? *" &&
				p.ArnRol == "arn-rol-test" &&
				p.ArnProceso == "arn-proceso-test" &&
				p.Habilitado == true
			));
			await kairosHelper.Received(1).EliminarProceso(Arg.Any<string>());
			await kairosHelper.Received(1).EliminarProceso(Arg.Is<string>(s => s == "id-proceso-test-2"));
            await normaSuscritaDao.Received(1).Actualizar(Arg.Is<NormaSuscrita>(n =>
					n.ProcesosNotificaciones.Count == 2 &&
					n.ProcesosNotificaciones.Any(p => p.IdProceso == "id-proceso-test-1") &&
					n.ProcesosNotificaciones.Any(p => p.IdProceso == "id-proceso-test-3")
				),
				Arg.Any<NpgsqlTransaction?>()
			);
		}

		[Fact]
		public async Task ExtraerFrecuenciasDiasAEliminarTest() {
			NormaSuscrita normaSuscrita = NormaSuscritaDummy();
			normaSuscrita.ProcesosNotificaciones = [
				ProcesoNotificacionDummy(idProceso: "id-proceso-test-1", idCalendarizacion: "id-calendarizacion-test-1", nombre: "nombre-test-1", frecuenciaDias: 14, inicioEjecucionUtc: FECHA_DUMMY, parametros: new EntKairosParametrosProceso {
					IdNormaSuscrita = normaSuscrita.Id,
					FrecuenciaDias = 14,
					InicioEjecucionUtc = FECHA_DUMMY,
					IdTipoUnidadTiempoAntelacion = null,
					CantAntelacion = null,
					EsVencimiento = true,
					ProgramarSiguienteEjecucion = true
				}),
				ProcesoNotificacionDummy(idProceso: "id-proceso-test-2", idCalendarizacion: "id-calendarizacion-test-2", nombre: "nombre-test-2", frecuenciaDias: 14, inicioEjecucionUtc: FECHA_DUMMY.AddMinutes(-5), parametros: new EntKairosParametrosProceso {
					IdNormaSuscrita = normaSuscrita.Id,
					FrecuenciaDias = 14,
					InicioEjecucionUtc = FECHA_DUMMY.AddHours(-1),
					IdTipoUnidadTiempoAntelacion = 1,
					CantAntelacion = 1,
					EsVencimiento = false,
					ProgramarSiguienteEjecucion = false
				}),
			];
			List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> deseados = [
				(14, FECHA_DUMMY, null, null, true),
				(14, FECHA_DUMMY.AddHours(-2), new TipoUnidadTiempo() { Id = 1, Nombre = "Hora", CantSegundos = 3600, CantMinutos = 60, CantHoras = 1, Vigencia = true }, 2, false),
			];

			List<ProcesoNotificacion> retorno = normaSuscritaBcp.ExtraerFrecuenciasDiasAEliminar(normaSuscrita, deseados);
			Assert.Single(retorno);
			Assert.Equal("id-proceso-test-2", retorno.First().IdProceso);
		}

		[Fact]
		public async Task ExtraerFrecuenciasDiasACrearTest() {
			NormaSuscrita normaSuscrita = NormaSuscritaDummy();
			normaSuscrita.ProcesosNotificaciones = [
				ProcesoNotificacionDummy(idProceso: "id-proceso-test-1", idCalendarizacion: "id-calendarizacion-test-1", nombre: "nombre-test-1", frecuenciaDias: 14, inicioEjecucionUtc: FECHA_DUMMY, parametros: new EntKairosParametrosProceso {
					IdNormaSuscrita = normaSuscrita.Id,
					FrecuenciaDias = 14,
					InicioEjecucionUtc = FECHA_DUMMY,
					IdTipoUnidadTiempoAntelacion = null,
					CantAntelacion = null,
					EsVencimiento = true,
					ProgramarSiguienteEjecucion = true
				}),
				ProcesoNotificacionDummy(idProceso: "id-proceso-test-2", idCalendarizacion: "id-calendarizacion-test-2", nombre: "nombre-test-2", frecuenciaDias: 14, inicioEjecucionUtc: FECHA_DUMMY.AddMinutes(-5), parametros: new EntKairosParametrosProceso {
					IdNormaSuscrita = normaSuscrita.Id,
					FrecuenciaDias = 14,
					InicioEjecucionUtc = FECHA_DUMMY.AddHours(-1),
					IdTipoUnidadTiempoAntelacion = 1,
					CantAntelacion = 1,
					EsVencimiento = false,
					ProgramarSiguienteEjecucion = false
				}),
			];
			List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> deseados = [
				(14, FECHA_DUMMY, null, null, true),
				(14, FECHA_DUMMY.AddHours(-2), new TipoUnidadTiempo() { Id = 1, Nombre = "Hora", CantSegundos = 3600, CantMinutos = 60, CantHoras = 1, Vigencia = true }, 2, false),
			];

			List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> retorno = normaSuscritaBcp.ExtraerFrecuenciasDiasACrear(normaSuscrita, deseados);
			Assert.Single(retorno);
			Assert.Equal(14, retorno.First().FrecuenciaDias);
			Assert.Equal(FECHA_DUMMY.AddHours(-2), retorno.First().InicioEjecucionUtc);
		}

		[Fact]
		public async Task ActualizarProcesosFrecuenciaDiasProgramadosTest() {
			variableEntorno.Obtener("APP_NAME").Returns("app-name-test");
			variableEntorno.Obtener("NOTIFICACIONES_LAMBDA_ARN").Returns("arn-proceso-test");
			variableEntorno.Obtener("NOTIFICACIONES_EJECUCION_ROLE_ARN").Returns("arn-rol-test");
			kairosHelper.IngresarProceso(Arg.Any<EntKairosIngresarProceso>()).Returns(new SalKairosIngresarProceso() {
				IdProceso = "id-proceso-test-3",
				IdCalendarizacion = "id-calendarizacion-test-3",
				Nombre = "nombre-test-3",
				ArnProceso = "arn-proceso-test",
				ArnRol = "arn-rol-test",
				Parametros = JsonSerializer.Serialize(new EntKairosParametrosProceso {
					IdNormaSuscrita = 100,
					FrecuenciaDias = 14,
					InicioEjecucionUtc = FECHA_DUMMY.AddHours(-2),
					IdTipoUnidadTiempoAntelacion = 1,
					CantAntelacion = 2,
					EsVencimiento = false,
					ProgramarSiguienteEjecucion = false
				}),
				FechaCreacion = FECHA_DUMMY,
				Habilitado = true
			});

			NormaSuscrita normaSuscrita = NormaSuscritaDummy(id: 100);
			normaSuscrita.ProcesosNotificaciones = [
				ProcesoNotificacionDummy(idProceso: "id-proceso-test-1", idCalendarizacion: "id-calendarizacion-test-1", nombre: "nombre-test-1", frecuenciaDias: 14, inicioEjecucionUtc: FECHA_DUMMY, parametros: new EntKairosParametrosProceso {
					IdNormaSuscrita = normaSuscrita.Id,
					FrecuenciaDias = 14,
					InicioEjecucionUtc = FECHA_DUMMY,
					IdTipoUnidadTiempoAntelacion = null,
					CantAntelacion = null,
					EsVencimiento = true,
					ProgramarSiguienteEjecucion = true
				}),
				ProcesoNotificacionDummy(idProceso: "id-proceso-test-2", idCalendarizacion: "id-calendarizacion-test-2", nombre: "nombre-test-2", frecuenciaDias: 14, inicioEjecucionUtc: FECHA_DUMMY.AddMinutes(-5), parametros: new EntKairosParametrosProceso {
					IdNormaSuscrita = normaSuscrita.Id,
					FrecuenciaDias = 14,
					InicioEjecucionUtc = FECHA_DUMMY.AddHours(-1),
					IdTipoUnidadTiempoAntelacion = 1,
					CantAntelacion = 1,
					EsVencimiento = false,
					ProgramarSiguienteEjecucion = false
				}),
			];
			List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> deseados = [
				(14, FECHA_DUMMY, null, null, true),
				(14, FECHA_DUMMY.AddHours(-2), new TipoUnidadTiempo() { Id = 1, Nombre = "Hora", CantSegundos = 3600, CantMinutos = 60, CantHoras = 1, Vigencia = true }, 2, false),
			];

			(List<ProcesoNotificacion> programados, List<ProcesoNotificacion> desprogramados) = await normaSuscritaBcp.ActualizarProcesosFrecuenciaDiasProgramados(normaSuscrita, deseados);
			Assert.Single(programados);
			Assert.Equal(14, programados.First().FrecuenciaDias);
			Assert.Equal(FECHA_DUMMY.AddHours(-2), programados.First().InicioEjecucionUtc);
			Assert.Single(desprogramados);
			Assert.Equal("id-proceso-test-2", desprogramados.First().IdProceso);
			Assert.Equal(2, normaSuscrita.ProcesosNotificaciones.Count);
			Assert.All(normaSuscrita.ProcesosNotificaciones, p => {
				Assert.True(p.IdProceso == "id-proceso-test-1" || p.IdProceso == "id-proceso-test-3");
				Assert.NotEqual("id-proceso-test-2", p.IdProceso);
			});
			await kairosHelper.Received(1).IngresarProceso(Arg.Any<EntKairosIngresarProceso>());
			await kairosHelper.Received(1).IngresarProceso(Arg.Is<EntKairosIngresarProceso>(p =>
				p.Nombre.StartsWith("app-name-test - ") &&
				p.Nombre.Contains($"- NormaSuscrita {normaSuscrita.Id} - ") &&
				p.Nombre.Contains($"- Inicio {FECHA_DUMMY.AddHours(-2):dd-MM-yyyy HH:mm} -") &&
				p.Nombre.EndsWith($"Frecuencia 14 Días") &&
				p.FrecuenciaDias == 14 &&
				p.InicioEjecucionUtc == FECHA_DUMMY.AddHours(-2) &&
				p.ArnRol == "arn-rol-test" &&
				p.ArnProceso == "arn-proceso-test" &&
				p.Habilitado == true
			));
			await kairosHelper.Received(1).EliminarProceso(Arg.Any<string>());
			await kairosHelper.Received(1).EliminarProceso(Arg.Is<string>(s => s == "id-proceso-test-2"));
			await normaSuscritaDao.Received(1).Actualizar(Arg.Is<NormaSuscrita>(n =>
					n.ProcesosNotificaciones.Count == 2 &&
					n.ProcesosNotificaciones.Any(p => p.IdProceso == "id-proceso-test-1") &&
					n.ProcesosNotificaciones.Any(p => p.IdProceso == "id-proceso-test-3")
				),
				Arg.Any<NpgsqlTransaction?>()
			);
		}
	}
}
