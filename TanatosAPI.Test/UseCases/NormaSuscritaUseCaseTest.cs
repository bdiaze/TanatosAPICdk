using Npgsql;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Text;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Kairos;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.UseCases;
using TanatosAPI.Test.Business;
using TanatosAPI.UseCases;

namespace TanatosAPI.Test.UseCases {
    public class NormaSuscritaUseCaseTest {
        private readonly IDatabaseConnectionHelper connectionHelper = Substitute.For<IDatabaseConnectionHelper>();
        private readonly IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        private readonly IHistorialNormaSuscritaUseCase historialNormaSuscritaUseCase = Substitute.For<IHistorialNormaSuscritaUseCase>();
        private readonly INotificacionNormaSuscritaUseCase notificacionNormaSuscritaUseCase = Substitute.For<INotificacionNormaSuscritaUseCase>();
		private readonly INormaSuscritaProcesoNotificacionUseCase normaSuscritaProcesoNotificacionUseCase = Substitute.For<INormaSuscritaProcesoNotificacionUseCase>();
		private readonly INormaSuscritaBcp normaSuscritaBcp = Substitute.For<INormaSuscritaBcp>();
        private readonly IHistorialNormaSuscritaBcp historialNormaSuscritaBcp = Substitute.For<IHistorialNormaSuscritaBcp>();
        private readonly IHistorialNotificacionBcp historialNotificacionBcp = Substitute.For<IHistorialNotificacionBcp>();
        private readonly IFiscalizadorNormaSuscritaBcp fiscalizadorNormaSuscritaBcp = Substitute.For<IFiscalizadorNormaSuscritaBcp>();
        private readonly INotificacionNormaSuscritaBcp notificacionNormaSuscritaBcp = Substitute.For<INotificacionNormaSuscritaBcp>();
        private readonly ITemplateBcp templateBcp = Substitute.For<ITemplateBcp>();
        private readonly ITemplateNormaBcp templateNormaBcp = Substitute.For<ITemplateNormaBcp>();
        private readonly ITemplateNormaNotificacionBcp templateNormaNotificacionBcp = Substitute.For<ITemplateNormaNotificacionBcp>();
        private readonly ITemplateNormaFiscalizadorBcp templateNormaFiscalizadorBcp = Substitute.For<ITemplateNormaFiscalizadorBcp>();
        private readonly ITipoPeriodicidadBcp tipoPeriodicidadBcp = Substitute.For<ITipoPeriodicidadBcp>();
        private readonly ICategoriaNormaBcp categoriaNormaBcp = Substitute.For<ICategoriaNormaBcp>();
        private readonly ITipoFiscalizadorBcp tipoFiscalizadorBcp = Substitute.For<ITipoFiscalizadorBcp>();
        private readonly ITipoUnidadTiempoBcp tipoUnidadTiempoBcp = Substitute.For<ITipoUnidadTiempoBcp>();
        private readonly ICargoBcp cargoBcp = Substitute.For<ICargoBcp>();
        private readonly INegocioBcp negocioBcp = Substitute.For<INegocioBcp>();
        private readonly ISuscripcionBcp suscripcionBcp = Substitute.For<ISuscripcionBcp>();
        private readonly IDocumentoAdjuntoBcp documentoAdjuntoBcp = Substitute.For<IDocumentoAdjuntoBcp>();
        private readonly NormaSuscritaUseCase normaSuscritaUseCase;

        private readonly IDatabaseConnection connection = Substitute.For<IDatabaseConnection>();
        private readonly IDatabaseTransaction transaction = Substitute.For<IDatabaseTransaction>();

        private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc); // 15-01-2026 11:00 Chile

        public NormaSuscritaUseCaseTest() {
            dateTimeProvider.UtcNow.Returns(FECHA_DUMMY);

            connection.BeginTransactionAsync().Returns(transaction);
            connectionHelper.ObtenerConexionWrapper().Returns(connection);

            normaSuscritaUseCase = new(
                connectionHelper, dateTimeProvider, historialNormaSuscritaUseCase, notificacionNormaSuscritaUseCase, normaSuscritaProcesoNotificacionUseCase,
				normaSuscritaBcp, historialNormaSuscritaBcp, historialNotificacionBcp, fiscalizadorNormaSuscritaBcp,
                notificacionNormaSuscritaBcp, templateBcp, templateNormaBcp, templateNormaNotificacionBcp, 
                templateNormaFiscalizadorBcp, tipoPeriodicidadBcp, categoriaNormaBcp, tipoFiscalizadorBcp, 
                tipoUnidadTiempoBcp, cargoBcp, negocioBcp, suscripcionBcp, documentoAdjuntoBcp
            );
        }

        [Fact]
        public async Task IncluirTemplateTest() {
            templateBcp.ObtenerVariosSoloVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
                TemplateBcpTest.TemplateDummy(id: 10),
				TemplateBcpTest.TemplateDummy(id: 20),
			]);
            templateNormaBcp.ObtenerPorTemplate(10, Arg.Any<NpgsqlTransaction?>()).Returns([
                TemplateNormaBcpTest.TemplateNormaDummy(idTemplate: 10, idNorma: 100),
				TemplateNormaBcpTest.TemplateNormaDummy(idTemplate: 10, idNorma: 200),
			]);
			templateNormaBcp.ObtenerPorTemplate(20, Arg.Any<NpgsqlTransaction?>()).Returns([
				TemplateNormaBcpTest.TemplateNormaDummy(idTemplate: 20, idNorma: 100),
			]);

			List<NormaSuscrita> entrada = [
                NormaSuscritaBcpTest.NormaSuscritaDummy(id: 1, idTemplate: 10, idNorma: 100),
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 2, idTemplate: 10, idNorma: 200),
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 3, idTemplate: 20, idNorma: 100),
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 4, idTemplate: 30, idNorma: 100),
			];

            await normaSuscritaUseCase.IncluirTemplate(entrada);
            Assert.Equal(4, entrada.Count);
			Assert.Equal(10, entrada.First(t => t.Id == 1).IdTemplate);
			Assert.Equal(100, entrada.First(t => t.Id == 1).IdNorma);
			Assert.Equal(10, entrada.First(t => t.Id == 1).TemplateNorma?.IdTemplate);
			Assert.Equal(100, entrada.First(t => t.Id == 1).TemplateNorma?.IdNorma);
			Assert.Equal(10, entrada.First(t => t.Id == 1).TemplateNorma?.Template?.Id);
			Assert.Equal(10, entrada.First(t => t.Id == 2).IdTemplate);
			Assert.Equal(200, entrada.First(t => t.Id == 2).IdNorma);
			Assert.Equal(10, entrada.First(t => t.Id == 2).TemplateNorma?.IdTemplate);
			Assert.Equal(200, entrada.First(t => t.Id == 2).TemplateNorma?.IdNorma);
			Assert.Equal(10, entrada.First(t => t.Id == 2).TemplateNorma?.Template?.Id);
			Assert.Equal(20, entrada.First(t => t.Id == 3).IdTemplate);
			Assert.Equal(100, entrada.First(t => t.Id == 3).IdNorma);
			Assert.Equal(20, entrada.First(t => t.Id == 3).TemplateNorma?.IdTemplate);
			Assert.Equal(100, entrada.First(t => t.Id == 3).TemplateNorma?.IdNorma);
			Assert.Equal(20, entrada.First(t => t.Id == 3).TemplateNorma?.Template?.Id);
			Assert.Null(entrada.First(t => t.Id == 4).TemplateNorma);
			Assert.Null(entrada.First(t => t.Id == 4).IdTemplate);
			Assert.Null(entrada.First(t => t.Id == 4).IdNorma);
			await templateBcp.Received(1).ObtenerVariosSoloVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>());
			await templateNormaBcp.Received(1).ObtenerPorTemplate(10, Arg.Any<NpgsqlTransaction?>());
			await templateNormaBcp.Received(1).ObtenerPorTemplate(20, Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task IncluirTemplateTest_Individual() {
			templateBcp.ObtenerVariosSoloVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TemplateBcpTest.TemplateDummy(id: 10),
			]);
			templateNormaBcp.ObtenerPorTemplate(10, Arg.Any<NpgsqlTransaction?>()).Returns([
				TemplateNormaBcpTest.TemplateNormaDummy(idTemplate: 10, idNorma: 100),
			]);

			NormaSuscrita entrada = NormaSuscritaBcpTest.NormaSuscritaDummy(id: 1, idTemplate: 10, idNorma: 100);

			await normaSuscritaUseCase.IncluirTemplate(entrada);
			Assert.Equal(10, entrada.IdTemplate);
			Assert.Equal(100, entrada.IdNorma);
			Assert.Equal(10, entrada.TemplateNorma?.IdTemplate);
			Assert.Equal(100, entrada.TemplateNorma?.IdNorma);
			Assert.Equal(10, entrada.TemplateNorma?.Template?.Id);
			await templateBcp.Received(1).ObtenerVariosSoloVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>());
			await templateNormaBcp.Received(1).ObtenerPorTemplate(10, Arg.Any<NpgsqlTransaction?>());
		}

        [Fact]
        public async Task IncluirTipoPeriodicidadTest() {
            tipoPeriodicidadBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([
                TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 10),
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 20),
			]);

			List<NormaSuscrita> entrada = [
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 1, idTipoPeriodicidad: 10),
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 2, idTipoPeriodicidad: 20),
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 3, idTipoPeriodicidad: 30),
			];

			await normaSuscritaUseCase.IncluirTipoPeriodicidad(entrada);
			Assert.Equal(3, entrada.Count);
			Assert.Equal(10, entrada.First(t => t.Id == 1).IdTipoPeriodicidad);
			Assert.Equal(10, entrada.First(t => t.Id == 1).TipoPeriodicidad?.Id);
			Assert.Equal(20, entrada.First(t => t.Id == 2).IdTipoPeriodicidad);
			Assert.Equal(20, entrada.First(t => t.Id == 2).TipoPeriodicidad?.Id);
			Assert.Null(entrada.First(t => t.Id == 3).TipoPeriodicidad);
			Assert.Null(entrada.First(t => t.Id == 3).IdTipoPeriodicidad);
			await tipoPeriodicidadBcp.Received(1).ObtenerVigentes(Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task IncluirTipoPeriodicidadTest_Individual() {
			tipoPeriodicidadBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 10),
			]);

			NormaSuscrita entrada = NormaSuscritaBcpTest.NormaSuscritaDummy(id: 1, idTipoPeriodicidad: 10);
			entrada.IdTemplate = 10;
			entrada.IdNorma = 100;
			entrada.TemplateNorma = TemplateNormaBcpTest.TemplateNormaDummy(idTemplate: 10, idNorma: 100, idCategoriaNorma: 10);

			await normaSuscritaUseCase.IncluirTipoPeriodicidad(entrada);
			Assert.Equal(10, entrada.IdTipoPeriodicidad);
			Assert.Equal(10, entrada.TipoPeriodicidad?.Id);
			Assert.Equal(10, entrada.TemplateNorma.IdTipoPeriodicidad);
			Assert.Equal(10, entrada.TemplateNorma.TipoPeriodicidad?.Id);
			await tipoPeriodicidadBcp.Received(1).ObtenerVigentes(Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task IncluirCategoriaNormaTest() {
			categoriaNormaBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([
				CategoriaNormaBcpTest.CategoriaNormaDummy(id: 10),
				CategoriaNormaBcpTest.CategoriaNormaDummy(id: 20),
			]);

			List<NormaSuscrita> entrada = [
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 1, idCategoriaNorma: 10),
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 2, idCategoriaNorma: 20),
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 3, idCategoriaNorma: 30),
			];

			await normaSuscritaUseCase.IncluirCategoriaNorma(entrada);
			Assert.Equal(3, entrada.Count);
			Assert.Equal(10, entrada.First(t => t.Id == 1).IdCategoriaNorma);
			Assert.Equal(10, entrada.First(t => t.Id == 1).CategoriaNorma?.Id);
			Assert.Equal(20, entrada.First(t => t.Id == 2).IdCategoriaNorma);
			Assert.Equal(20, entrada.First(t => t.Id == 2).CategoriaNorma?.Id);
			Assert.Null(entrada.First(t => t.Id == 3).CategoriaNorma);
			Assert.Null(entrada.First(t => t.Id == 3).IdCategoriaNorma);
			await categoriaNormaBcp.Received(1).ObtenerVigentes(Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task IncluirCategoriaNormaTest_Individual() {
			categoriaNormaBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([
				CategoriaNormaBcpTest.CategoriaNormaDummy(id: 10)
			]);

			NormaSuscrita entrada = NormaSuscritaBcpTest.NormaSuscritaDummy(id: 1, idCategoriaNorma: 10);
			entrada.IdTemplate = 10;
			entrada.IdNorma = 100;
			entrada.TemplateNorma = TemplateNormaBcpTest.TemplateNormaDummy(idTemplate: 10, idNorma: 100, idCategoriaNorma: 10);

			await normaSuscritaUseCase.IncluirCategoriaNorma(entrada);
			Assert.Equal(10, entrada.IdCategoriaNorma);
			Assert.Equal(10, entrada.CategoriaNorma?.Id);
			Assert.Equal(10, entrada.TemplateNorma.IdCategoriaNorma);
			Assert.Equal(10, entrada.TemplateNorma.CategoriaNorma?.Id);
			await categoriaNormaBcp.Received(1).ObtenerVigentes(Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task IncluirCargoTest() {
			cargoBcp.ObtenerPorSubYNegocio("sub-test", 10, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([
				CargoBcpTest.CargoDummy(id: 10, sub: "sub-test", idNegocio: 10, vigencia: true),
				CargoBcpTest.CargoDummy(id: 20, sub: "sub-test", idNegocio: 10, vigencia: true),
			]);
			cargoBcp.ObtenerPorSubYNegocio("sub-test", 20, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([
				CargoBcpTest.CargoDummy(id: 30, sub: "sub-test", idNegocio: 20, vigencia: true),
			]);
			cargoBcp.ObtenerPorSubYNegocio("sub-test", 30, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([]);

			List<NormaSuscrita> entrada = [
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 1, sub: "sub-test", idNegocio: 10, idCargo: 10),
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 2, sub: "sub-test", idNegocio: 10, idCargo: 20),
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 3, sub: "sub-test", idNegocio: 20, idCargo: 30),
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 4, sub: "sub-test", idNegocio: 20, idCargo: 40),
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 5, sub: "sub-test", idNegocio: 30, idCargo: 30),
			];

			await normaSuscritaUseCase.IncluirCargo(entrada);
			Assert.Equal(5, entrada.Count);
			Assert.Equal(10, entrada.First(t => t.Id == 1).IdCargo);
			Assert.Equal(10, entrada.First(t => t.Id == 1).Cargo?.Id);
			Assert.Equal(20, entrada.First(t => t.Id == 2).IdCargo);
			Assert.Equal(20, entrada.First(t => t.Id == 2).Cargo?.Id);
			Assert.Equal(30, entrada.First(t => t.Id == 3).IdCargo);
			Assert.Equal(30, entrada.First(t => t.Id == 3).Cargo?.Id);
			Assert.Null(entrada.First(t => t.Id == 4).Cargo);
			Assert.Null(entrada.First(t => t.Id == 4).IdCargo);
			Assert.Null(entrada.First(t => t.Id == 5).Cargo);
			Assert.Null(entrada.First(t => t.Id == 5).IdCargo);
			await cargoBcp.Received(3).ObtenerPorSubYNegocio("sub-test", Arg.Is<long>(n => n == 10 || n == 20 || n == 30), filtrarVigente: true, Arg.Any<NpgsqlTransaction?>());
			await cargoBcp.Received(3).ObtenerPorSubYNegocio(Arg.Any<string>(), Arg.Any<long>(), filtrarVigente: Arg.Any<bool>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task IncluirCargoTest_Individual() {
			cargoBcp.ObtenerPorSubYNegocio("sub-test", 10, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([
				CargoBcpTest.CargoDummy(id: 10, sub: "sub-test", idNegocio: 10, vigencia: true),
			]);
			NormaSuscrita entrada = NormaSuscritaBcpTest.NormaSuscritaDummy(id: 1, sub: "sub-test", idNegocio: 10, idCargo: 10);

			await normaSuscritaUseCase.IncluirCargo(entrada);
			Assert.Equal(10, entrada.IdCargo);
			Assert.Equal(10, entrada.Cargo?.Id);
			await cargoBcp.Received(1).ObtenerPorSubYNegocio("sub-test", Arg.Is<long>(n => n == 10), filtrarVigente: true, Arg.Any<NpgsqlTransaction?>());
			await cargoBcp.Received(1).ObtenerPorSubYNegocio(Arg.Any<string>(), Arg.Any<long>(), filtrarVigente: Arg.Any<bool>(), Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task IncluirFiscalizadoresTest() {
			fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(10, Arg.Any<NpgsqlTransaction?>()).Returns([
				FiscalizadorNormaSuscritaBcpTest.FiscalizadorNormaSuscritaDummy(id: 1, idNormaSuscrita: 10, idTipoFiscalizador: 100),
				FiscalizadorNormaSuscritaBcpTest.FiscalizadorNormaSuscritaDummy(id: 2, idNormaSuscrita: 10, idTipoFiscalizador: 200),
				FiscalizadorNormaSuscritaBcpTest.FiscalizadorNormaSuscritaDummy(id: 3, idNormaSuscrita: 10, idTipoFiscalizador: 300),
			]);
			templateNormaFiscalizadorBcp.ObtenerPorTemplateNorma(10, 100, Arg.Any<NpgsqlTransaction?>()).Returns([
				TemplateNormaFiscalizadorBcpTest.TemplateNormaFiscalizadorDummy(idTemplate: 10, idNorma: 100, idTipoFiscalizador: 100),
				TemplateNormaFiscalizadorBcpTest.TemplateNormaFiscalizadorDummy(idTemplate: 10, idNorma: 100, idTipoFiscalizador: 300),
			]);
			tipoFiscalizadorBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 100),
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 200),
			]);

			NormaSuscrita entrada = NormaSuscritaBcpTest.NormaSuscritaDummy(id: 10);
			entrada.IdTemplate = 10;
			entrada.IdNorma = 100;
			entrada.TemplateNorma = TemplateNormaBcpTest.TemplateNormaDummy(idTemplate: 10, idNorma: 100);

			await normaSuscritaUseCase.IncluirFiscalizadores(entrada);
			Assert.NotNull(entrada.FiscalizadoresNormaSuscrita);
			Assert.Equal(2, entrada.FiscalizadoresNormaSuscrita.Count);
			Assert.Equal(100, entrada.FiscalizadoresNormaSuscrita.First(f => f.Id == 1).IdTipoFiscalizador);
			Assert.Equal(100, entrada.FiscalizadoresNormaSuscrita.First(f => f.Id == 1).TipoFiscalizador?.Id);
			Assert.Equal(200, entrada.FiscalizadoresNormaSuscrita.First(f => f.Id == 2).IdTipoFiscalizador);
			Assert.Equal(200, entrada.FiscalizadoresNormaSuscrita.First(f => f.Id == 2).TipoFiscalizador?.Id);
			Assert.DoesNotContain(3, entrada.FiscalizadoresNormaSuscrita.Select(f => f.Id));
			Assert.Equal(1, entrada.TemplateNorma.TemplateNormaFiscalizadores?.Count);
			Assert.Equal(100, entrada.TemplateNorma.TemplateNormaFiscalizadores?.First().IdTipoFiscalizador);
			Assert.Equal(100, entrada.TemplateNorma.TemplateNormaFiscalizadores?.First().TipoFiscalizador?.Id);
			await fiscalizadorNormaSuscritaBcp.Received(1).ObtenerVigentesPorNormaSuscrita(10, Arg.Any<NpgsqlTransaction?>());
			await fiscalizadorNormaSuscritaBcp.Received(1).ObtenerVigentesPorNormaSuscrita(Arg.Any<long>(), Arg.Any<NpgsqlTransaction?>());
			await templateNormaFiscalizadorBcp.Received(1).ObtenerPorTemplateNorma(10, 100, Arg.Any<NpgsqlTransaction?>());
			await templateNormaFiscalizadorBcp.Received(1).ObtenerPorTemplateNorma(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<NpgsqlTransaction?>());
			await tipoFiscalizadorBcp.Received(1).ObtenerVigentes(Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task IncluirNotificacionesTest() {
			notificacionNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(10, Arg.Any<NpgsqlTransaction?>()).Returns([
				NotificacionNormaSuscritaBcpTest.NotificacionNormaSuscritaDummy(id: 1, idNormaSuscrita: 10, idTipoUnidadTiempoAntelacion: 100),
				NotificacionNormaSuscritaBcpTest.NotificacionNormaSuscritaDummy(id: 2, idNormaSuscrita: 10, idTipoUnidadTiempoAntelacion: 200),
				NotificacionNormaSuscritaBcpTest.NotificacionNormaSuscritaDummy(id: 3, idNormaSuscrita: 10, idTipoUnidadTiempoAntelacion: 300),
			]);
			templateNormaNotificacionBcp.ObtenerPorTemplateNorma(10, 100, Arg.Any<NpgsqlTransaction?>()).Returns([
				TemplateNormaNotificacionBcpTest.TemplateNormaNotificacionDummy(idTemplate: 10, idNorma: 100, idTipoUnidadTiempoAntelacion: 100),
				TemplateNormaNotificacionBcpTest.TemplateNormaNotificacionDummy(idTemplate: 10, idNorma: 100, idTipoUnidadTiempoAntelacion: 300),
			]);
			tipoUnidadTiempoBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 100),
				TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 200),
			]);

			NormaSuscrita entrada = NormaSuscritaBcpTest.NormaSuscritaDummy(id: 10);
			entrada.IdTemplate = 10;
			entrada.IdNorma = 100;
			entrada.TemplateNorma = TemplateNormaBcpTest.TemplateNormaDummy(idTemplate: 10, idNorma: 100);

			await normaSuscritaUseCase.IncluirNotificaciones(entrada);
			Assert.NotNull(entrada.NotificacionesNormaSuscrita);
			Assert.Equal(2, entrada.NotificacionesNormaSuscrita.Count);
			Assert.Equal(100, entrada.NotificacionesNormaSuscrita.First(f => f.Id == 1).IdTipoUnidadTiempoAntelacion);
			Assert.Equal(100, entrada.NotificacionesNormaSuscrita.First(f => f.Id == 1).TipoUnidadTiempo?.Id);
			Assert.Equal(200, entrada.NotificacionesNormaSuscrita.First(f => f.Id == 2).IdTipoUnidadTiempoAntelacion);
			Assert.Equal(200, entrada.NotificacionesNormaSuscrita.First(f => f.Id == 2).TipoUnidadTiempo?.Id);
			Assert.DoesNotContain(3, entrada.NotificacionesNormaSuscrita.Select(f => f.Id));
			Assert.Equal(1, entrada.TemplateNorma.TemplateNormaNotificaciones?.Count);
			Assert.Equal(100, entrada.TemplateNorma.TemplateNormaNotificaciones?.First().IdTipoUnidadTiempoAntelacion);
			Assert.Equal(100, entrada.TemplateNorma.TemplateNormaNotificaciones?.First().TipoUnidadTiempoAntelacion?.Id);
			await notificacionNormaSuscritaBcp.Received(1).ObtenerVigentesPorNormaSuscrita(10, Arg.Any<NpgsqlTransaction?>());
			await notificacionNormaSuscritaBcp.Received(1).ObtenerVigentesPorNormaSuscrita(Arg.Any<long>(), Arg.Any<NpgsqlTransaction?>());
			await templateNormaNotificacionBcp.Received(1).ObtenerPorTemplateNorma(10, 100, Arg.Any<NpgsqlTransaction?>());
			await templateNormaNotificacionBcp.Received(1).ObtenerPorTemplateNorma(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<NpgsqlTransaction?>());
			await tipoUnidadTiempoBcp.Received(1).ObtenerVigentes(Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task IncluirHistorialVencimientosTest() {
			normaSuscritaBcp.EstaActiva(Arg.Is<NormaSuscrita>(n => n.Id == 1)).Returns(true);
			normaSuscritaBcp.EstaActiva(Arg.Is<NormaSuscrita>(n => n.Id == 2)).Returns(false);
			historialNormaSuscritaBcp.ObtenerPorNormaSuscrita(1, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 10, idNormaSuscrita: 1),
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 11, idNormaSuscrita: 1),
			]);
			historialNormaSuscritaBcp.ObtenerPorNormaSuscrita(2, filtrarVigente: true, filtrarCompletadas: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 12, idNormaSuscrita: 2, fechaCompletitud: FECHA_DUMMY),
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 13, idNormaSuscrita: 2, fechaCompletitud: FECHA_DUMMY),
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 14, idNormaSuscrita: 2, fechaCompletitud: FECHA_DUMMY),
			]);

			List<NormaSuscrita> entrada = [
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 1, activado: true),
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 2, activado: false),
			];

			await normaSuscritaUseCase.IncluirHistorialVencimientos(entrada);
			Assert.NotNull(entrada.FirstOrDefault(n => n.Id == 1));
			Assert.NotNull(entrada.FirstOrDefault(n => n.Id == 2));
			Assert.Equal(2, entrada.First(n => n.Id == 1).HistorialesNormaSuscrita?.Count);
			Assert.Contains(10, entrada.First(n => n.Id == 1).HistorialesNormaSuscrita?.Select(h => h.Id) ?? []);
			Assert.Contains(11, entrada.First(n => n.Id == 1).HistorialesNormaSuscrita?.Select(h => h.Id) ?? []);
			Assert.Equal(3, entrada.First(n => n.Id == 2).HistorialesNormaSuscrita?.Count);
			Assert.Contains(12, entrada.First(n => n.Id == 2).HistorialesNormaSuscrita?.Select(h => h.Id) ?? []);
			Assert.Contains(13, entrada.First(n => n.Id == 2).HistorialesNormaSuscrita?.Select(h => h.Id) ?? []);
			Assert.Contains(14, entrada.First(n => n.Id == 2).HistorialesNormaSuscrita?.Select(h => h.Id) ?? []);
			await historialNormaSuscritaBcp.Received(1).ObtenerPorNormaSuscrita(1, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaBcp.Received(1).ObtenerPorNormaSuscrita(2, filtrarVigente: true, filtrarCompletadas: true, transaction: Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaBcp.Received(2).ObtenerPorNormaSuscrita(Arg.Any<long>(), filtrarVigente: Arg.Any<bool>(), filtrarNoCompletadas: Arg.Any<bool>(), filtrarCompletadas: Arg.Any<bool>(), transaction: Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task IncluirHistorialVencimientosTest_Individual() {
			normaSuscritaBcp.EstaActiva(Arg.Is<NormaSuscrita>(n => n.Id == 1)).Returns(true);
			historialNormaSuscritaBcp.ObtenerPorNormaSuscrita(1, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 10, idNormaSuscrita: 1),
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 11, idNormaSuscrita: 1),
			]);

			NormaSuscrita entrada = NormaSuscritaBcpTest.NormaSuscritaDummy(id: 1, activado: true);

			await normaSuscritaUseCase.IncluirHistorialVencimientos(entrada);
			Assert.Equal(2, entrada.HistorialesNormaSuscrita?.Count);
			Assert.Contains(10, entrada.HistorialesNormaSuscrita?.Select(h => h.Id) ?? []);
			Assert.Contains(11, entrada.HistorialesNormaSuscrita?.Select(h => h.Id) ?? []);
			await historialNormaSuscritaBcp.Received(1).ObtenerPorNormaSuscrita(Arg.Any<long>(), filtrarVigente: Arg.Any<bool>(), filtrarNoCompletadas: Arg.Any<bool>(), filtrarCompletadas: Arg.Any<bool>(), transaction: Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]

		public async Task ObtenerTest_SinParametros() {
			normaSuscritaBcp.Obtener(1, validarVigencia: Arg.Any<bool>(), validarSub: Arg.Any<string?>(), validarIdNegocio: Arg.Any<long?>(), transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 1)
			);
			NormaSuscrita? normaSuscrita = await normaSuscritaUseCase.Obtener(1);
			Assert.NotNull(normaSuscrita);
			Assert.Null(normaSuscrita.TemplateNorma);
			Assert.Null(normaSuscrita.TipoPeriodicidad);
			Assert.Null(normaSuscrita.CategoriaNorma);
			Assert.Null(normaSuscrita.Cargo);
			Assert.Empty(normaSuscrita.FiscalizadoresNormaSuscrita ?? []);
			Assert.Empty(normaSuscrita.NotificacionesNormaSuscrita ?? []);
			Assert.Empty(normaSuscrita.HistorialesNormaSuscrita ?? []);
			await normaSuscritaBcp.Received(1).Obtener(1, validarVigencia: Arg.Any<bool>(), validarSub: Arg.Any<string?>(), validarIdNegocio: Arg.Any<long?>(), transaction: Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]

		public async Task ObtenerTest_IncluyendoTodo() {
			normaSuscritaBcp.Obtener(1, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 5, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 1, sub: "sub-test", idNegocio: 5, idTemplate: 10, idNorma: 100, idTipoPeriodicidad: 20, idCategoriaNorma: 30, idCargo: 40)
			);
			templateBcp.ObtenerVariosSoloVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TemplateBcpTest.TemplateDummy(id: 10),
			]);
			templateNormaBcp.ObtenerPorTemplate(10, Arg.Any<NpgsqlTransaction?>()).Returns([
				TemplateNormaBcpTest.TemplateNormaDummy(idTemplate: 10, idNorma: 100),
			]);
			tipoPeriodicidadBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 20),
			]);
			categoriaNormaBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([
				CategoriaNormaBcpTest.CategoriaNormaDummy(id: 30)
			]);
			cargoBcp.ObtenerPorSubYNegocio("sub-test", 5, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([
				CargoBcpTest.CargoDummy(id: 40, sub: "sub-test", idNegocio: 5, vigencia: true),
			]);
			fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(1, Arg.Any<NpgsqlTransaction?>()).Returns([
				FiscalizadorNormaSuscritaBcpTest.FiscalizadorNormaSuscritaDummy(id: 1000, idNormaSuscrita: 1, idTipoFiscalizador: 100),
				FiscalizadorNormaSuscritaBcpTest.FiscalizadorNormaSuscritaDummy(id: 2000, idNormaSuscrita: 1, idTipoFiscalizador: 200),
				FiscalizadorNormaSuscritaBcpTest.FiscalizadorNormaSuscritaDummy(id: 3000, idNormaSuscrita: 1, idTipoFiscalizador: 300),
			]);
			templateNormaFiscalizadorBcp.ObtenerPorTemplateNorma(10, 100, Arg.Any<NpgsqlTransaction?>()).Returns([
				TemplateNormaFiscalizadorBcpTest.TemplateNormaFiscalizadorDummy(idTemplate: 10, idNorma: 100, idTipoFiscalizador: 100),
				TemplateNormaFiscalizadorBcpTest.TemplateNormaFiscalizadorDummy(idTemplate: 10, idNorma: 100, idTipoFiscalizador: 300),
			]);
			tipoFiscalizadorBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 100),
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 200),
			]);
			notificacionNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(1, Arg.Any<NpgsqlTransaction?>()).Returns([
				NotificacionNormaSuscritaBcpTest.NotificacionNormaSuscritaDummy(id: 10_000, idNormaSuscrita: 1, idTipoUnidadTiempoAntelacion: 500),
				NotificacionNormaSuscritaBcpTest.NotificacionNormaSuscritaDummy(id: 20_000, idNormaSuscrita: 1, idTipoUnidadTiempoAntelacion: 600),
				NotificacionNormaSuscritaBcpTest.NotificacionNormaSuscritaDummy(id: 30_000, idNormaSuscrita: 1, idTipoUnidadTiempoAntelacion: 700),
			]);
			templateNormaNotificacionBcp.ObtenerPorTemplateNorma(10, 100, Arg.Any<NpgsqlTransaction?>()).Returns([
				TemplateNormaNotificacionBcpTest.TemplateNormaNotificacionDummy(idTemplate: 10, idNorma: 100, idTipoUnidadTiempoAntelacion: 600),
				TemplateNormaNotificacionBcpTest.TemplateNormaNotificacionDummy(idTemplate: 10, idNorma: 100, idTipoUnidadTiempoAntelacion: 700),
			]);
			tipoUnidadTiempoBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 500),
				TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 600),
			]);
			normaSuscritaBcp.EstaActiva(Arg.Is<NormaSuscrita>(n => n.Id == 1)).Returns(true);
			historialNormaSuscritaBcp.ObtenerPorNormaSuscrita(1, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 1),
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 110_000, idNormaSuscrita: 1),
			]);

			NormaSuscrita? normaSuscrita = await normaSuscritaUseCase.Obtener(1, 
				validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 5, 
				incluirTemplate: true, incluirPeriodicidad: true, incluirCategoria: true, incluirCargo: true,
				incluirFiscalizadores: true, incluirNotificaciones: true, incluirHistorialVencimientos: true
			);

			Assert.NotNull(normaSuscrita);
			Assert.NotNull(normaSuscrita.TemplateNorma);
			Assert.NotNull(normaSuscrita.TipoPeriodicidad);
			Assert.NotNull(normaSuscrita.CategoriaNorma);
			Assert.NotNull(normaSuscrita.Cargo);
			Assert.NotEmpty(normaSuscrita.FiscalizadoresNormaSuscrita ?? []);
			Assert.NotEmpty(normaSuscrita.NotificacionesNormaSuscrita ?? []);
			Assert.NotEmpty(normaSuscrita.HistorialesNormaSuscrita ?? []);
			await normaSuscritaBcp.Received(1).Obtener(1, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 5, transaction: Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerPorSubYNegocioTest_SinParametros() {
			normaSuscritaBcp.ObtenerPorSubYNegocio("sub-test", 5).Returns([
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 1),
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 2),
			]);

			List<NormaSuscrita> retorno = await normaSuscritaUseCase.ObtenerPorSubYNegocio("sub-test", 5);
			Assert.Equal(2, retorno.Count);
			Assert.All(retorno, n => {
				Assert.NotNull(n);
				Assert.Null(n.TemplateNorma);
				Assert.Null(n.TipoPeriodicidad);
				Assert.Null(n.CategoriaNorma);
				Assert.Null(n.Cargo);
				Assert.Empty(n.FiscalizadoresNormaSuscrita ?? []);
				Assert.Empty(n.NotificacionesNormaSuscrita ?? []);
				Assert.Empty(n.HistorialesNormaSuscrita ?? []);
			});

			await normaSuscritaBcp.Received(1).ObtenerPorSubYNegocio("sub-test", 5, filtrarVigentes: Arg.Any<bool>(), transaction: Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerPorSubYNegocioTest_IncluyendoTodo() {
			normaSuscritaBcp.ObtenerPorSubYNegocio("sub-test", 5, filtrarVigentes: true).Returns([
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 1, sub: "sub-test", idNegocio: 5, idTemplate: 10, idNorma: 100, idTipoPeriodicidad: 20, idCategoriaNorma: 30, idCargo: 40),
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 2, sub: "sub-test", idNegocio: 5, idTemplate: 10, idNorma: 200, idTipoPeriodicidad: 20, idCategoriaNorma: 30, idCargo: 40),
			]);
			templateBcp.ObtenerVariosSoloVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TemplateBcpTest.TemplateDummy(id: 10),
			]);
			templateNormaBcp.ObtenerPorTemplate(10, Arg.Any<NpgsqlTransaction?>()).Returns([
				TemplateNormaBcpTest.TemplateNormaDummy(idTemplate: 10, idNorma: 100),
				TemplateNormaBcpTest.TemplateNormaDummy(idTemplate: 10, idNorma: 200),
			]);
			tipoPeriodicidadBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 20),
			]);
			categoriaNormaBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([
				CategoriaNormaBcpTest.CategoriaNormaDummy(id: 30)
			]);
			cargoBcp.ObtenerPorSubYNegocio("sub-test", 5, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([
				CargoBcpTest.CargoDummy(id: 40, sub: "sub-test", idNegocio: 5, vigencia: true),
			]);
			normaSuscritaBcp.EstaActiva(Arg.Is<NormaSuscrita>(n => n.Id == 1 || n.Id == 2)).Returns(true);
			historialNormaSuscritaBcp.ObtenerPorNormaSuscrita(1, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 1),
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 110_000, idNormaSuscrita: 1),
			]);
			historialNormaSuscritaBcp.ObtenerPorNormaSuscrita(2, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 200_000, idNormaSuscrita: 2),
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 210_000, idNormaSuscrita: 2),
			]);


			List<NormaSuscrita> retorno = await normaSuscritaUseCase.ObtenerPorSubYNegocio("sub-test", 5, filtrarVigentes: true, 
				incluirTemplates: true, incluirPeriodicidades: true, incluirCategorias: true, incluirCargos: true, incluirHistorialVencimientos: true);
			Assert.Equal(2, retorno.Count);
			Assert.All(retorno, n => {
				Assert.NotNull(n);
				Assert.NotNull(n.TemplateNorma);
				Assert.NotNull(n.TipoPeriodicidad);
				Assert.NotNull(n.CategoriaNorma);
				Assert.NotNull(n.Cargo);
				Assert.NotEmpty(n.HistorialesNormaSuscrita ?? []);
			});

			await normaSuscritaBcp.Received(1).ObtenerPorSubYNegocio("sub-test", 5, filtrarVigentes: true, transaction: Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerIncluyendoProximoVencimientoTest() {
			NormaSuscrita retorno = NormaSuscritaBcpTest.NormaSuscritaDummy(id: 10, sub: "sub-test", idNegocio: 5, 
				idTemplate: null, idNorma: null, idTipoPeriodicidad: null, idCategoriaNorma: null, idCargo: null);
			templateBcp.ObtenerVariosSoloVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([]);
			normaSuscritaBcp.Obtener(10, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: Arg.Any<long?>(), transaction: Arg.Any<NpgsqlTransaction?>()).Returns(retorno);
			tipoPeriodicidadBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([]);
			categoriaNormaBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([]);
			cargoBcp.ObtenerPorSubYNegocio("sub-test", 5, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([]);
			fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(10, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			tipoFiscalizadorBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([]);
			notificacionNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(10, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			tipoUnidadTiempoBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([]);
			normaSuscritaBcp.EstaActiva(Arg.Is<NormaSuscrita>(n => n.Id == 1)).Returns(true);
			historialNormaSuscritaBcp.ObtenerPorNormaSuscrita(10, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 10, fechaVencimiento: FECHA_DUMMY),
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 110_000, idNormaSuscrita: 10, fechaVencimiento: FECHA_DUMMY.AddDays(14)),
			]);
			historialNormaSuscritaBcp.FiltrarUltimoVencimiento(Arg.Any<List<HistorialNormaSuscrita>>()).Returns(
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 110_000, idNormaSuscrita: 10, fechaVencimiento: FECHA_DUMMY.AddDays(14))
			);

			NormaSuscrita normaSuscrita = await normaSuscritaUseCase.ObtenerIncluyendoProximoVencimiento(10, "sub-test");
			Assert.NotNull(normaSuscrita.HistorialesNormaSuscrita);
			Assert.Single(normaSuscrita.HistorialesNormaSuscrita);
			Assert.Equal(FECHA_DUMMY.AddDays(14), normaSuscrita.HistorialesNormaSuscrita.First().FechaVencimiento);
			historialNormaSuscritaBcp.Received(1).FiltrarUltimoVencimiento(Arg.Any<List<HistorialNormaSuscrita>>());
		}

		[Fact]
		public async Task ObtenerIncluyendoProximoVencimientoTest_SinProximoVencimiento() {
			NormaSuscrita retorno = NormaSuscritaBcpTest.NormaSuscritaDummy(id: 10, sub: "sub-test", idNegocio: 5,
				idTemplate: null, idNorma: null, idTipoPeriodicidad: null, idCategoriaNorma: null, idCargo: null);
			templateBcp.ObtenerVariosSoloVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([]);
			normaSuscritaBcp.Obtener(10, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: Arg.Any<long?>(), transaction: Arg.Any<NpgsqlTransaction?>()).Returns(retorno);
			tipoPeriodicidadBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([]);
			categoriaNormaBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([]);
			cargoBcp.ObtenerPorSubYNegocio("sub-test", 5, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([]);
			fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(10, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			tipoFiscalizadorBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([]);
			notificacionNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(10, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			tipoUnidadTiempoBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([]);
			normaSuscritaBcp.EstaActiva(Arg.Is<NormaSuscrita>(n => n.Id == 1)).Returns(true);
			historialNormaSuscritaBcp.ObtenerPorNormaSuscrita(10, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([]);
			historialNormaSuscritaBcp.FiltrarUltimoVencimiento(Arg.Any<List<HistorialNormaSuscrita>>()).Returns((HistorialNormaSuscrita?)null);

			NormaSuscrita normaSuscrita = await normaSuscritaUseCase.ObtenerIncluyendoProximoVencimiento(10, "sub-test");
			Assert.NotNull(normaSuscrita.HistorialesNormaSuscrita);
			Assert.Empty(normaSuscrita.HistorialesNormaSuscrita);
			historialNormaSuscritaBcp.Received(1).FiltrarUltimoVencimiento(Arg.Any<List<HistorialNormaSuscrita>>());
		}

		[Fact]
		public async Task ObtenerVencimientoConDocumentosYPlanTest() {
			historialNormaSuscritaBcp.Obtener(100, validarVigencia: true, validarIdNormaSuscrita: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100, idNormaSuscrita: 10, fechaVencimiento: FECHA_DUMMY.AddDays(14))
			);
			NormaSuscrita retorno = NormaSuscritaBcpTest.NormaSuscritaDummy(id: 10, sub: "sub-test", idNegocio: 5,
				idTemplate: null, idNorma: null, idTipoPeriodicidad: null, idCategoriaNorma: null, idCargo: null);
			normaSuscritaBcp.Obtener(10, validarSub: "sub-test", transaction: Arg.Any<NpgsqlTransaction?>()).Returns(retorno);
			templateBcp.ObtenerVariosSoloVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([]);
			tipoPeriodicidadBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([]);
			categoriaNormaBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([]);
			cargoBcp.ObtenerPorSubYNegocio("sub-test", 5, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([]);
			fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(10, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			tipoFiscalizadorBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([]);
			historialNormaSuscritaBcp.EstaCompletada(Arg.Any<HistorialNormaSuscrita>()).Returns(true);
			normaSuscritaBcp.EstaVigente(Arg.Any<NormaSuscrita>()).Returns(true);
			negocioBcp.Obtener(5, validarVigencia: true, validarSub: "sub-test", transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NegocioBcpTest.NegocioDummy(id: 5, sub: "sub-test")
			);
			documentoAdjuntoBcp.ObtenerPorVencimiento(100, filtrarVigentes: true, filtrarRecepcionados: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([
				DocumentoAdjuntoBcpTest.DocumentoAdjuntoDummy(id: 10_000, idHistorialNormaSuscrita: 100),
				DocumentoAdjuntoBcpTest.DocumentoAdjuntoDummy(id: 20_000, idHistorialNormaSuscrita: 100)
			]);
			suscripcionBcp.ConsultaTienePlanEmpresa("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(true);

			(HistorialNormaSuscrita vencimiento, bool tienePlanEmpresa) = await normaSuscritaUseCase.ObtenerVencimientoConDocumentosYPlan(10, 100, "sub-test");
			Assert.NotNull(vencimiento.NormaSuscrita);
			Assert.NotNull(vencimiento.NormaSuscrita.Negocio);
			Assert.NotNull(vencimiento.DocumentosAdjuntos);
			Assert.True(tienePlanEmpresa);
			Assert.Equal(2, vencimiento.DocumentosAdjuntos.Count);
			await historialNormaSuscritaBcp.Received(1).Obtener(100, validarVigencia: true, validarIdNormaSuscrita: 10, transaction: Arg.Any<NpgsqlTransaction?>());
			await negocioBcp.Received(1).Obtener(5, validarVigencia: true, validarSub: "sub-test", transaction: Arg.Any<NpgsqlTransaction?>());
			await documentoAdjuntoBcp.Received(1).ObtenerPorVencimiento(100, filtrarVigentes: true, filtrarRecepcionados: true, transaction: Arg.Any<NpgsqlTransaction?>());
			await suscripcionBcp.Received(1).ConsultaTienePlanEmpresa("sub-test", Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerVencimientoConDocumentosYPlanTest_ObligacionNoExistente() {
			historialNormaSuscritaBcp.Obtener(100, validarVigencia: true, validarIdNormaSuscrita: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100, idNormaSuscrita: 10, fechaVencimiento: FECHA_DUMMY.AddDays(14))
			);
			normaSuscritaBcp.Obtener(10, validarSub: "sub-test", transaction: Arg.Any<NpgsqlTransaction?>()).Returns((NormaSuscrita?)null);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => normaSuscritaUseCase.ObtenerVencimientoConDocumentosYPlan(10, 100, "sub-test"));
			Assert.Equal(TipoErrorValidacion.NoExiste, ex.TipoErrorValidacion);
			await historialNormaSuscritaBcp.Received(1).Obtener(100, validarVigencia: true, validarIdNormaSuscrita: 10, transaction: Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerVencimientoConDocumentosYPlanTest_VencimientoNoCompletadoYObligacionNoVigente() {
			historialNormaSuscritaBcp.Obtener(100, validarVigencia: true, validarIdNormaSuscrita: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100, idNormaSuscrita: 10, fechaVencimiento: FECHA_DUMMY.AddDays(14))
			);
			NormaSuscrita retorno = NormaSuscritaBcpTest.NormaSuscritaDummy(id: 10, sub: "sub-test", idNegocio: 5,
				idTemplate: null, idNorma: null, idTipoPeriodicidad: null, idCategoriaNorma: null, idCargo: null);
			normaSuscritaBcp.Obtener(10, validarSub: "sub-test", transaction: Arg.Any<NpgsqlTransaction?>()).Returns(retorno);
			templateBcp.ObtenerVariosSoloVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([]);
			tipoPeriodicidadBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([]);
			categoriaNormaBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([]);
			cargoBcp.ObtenerPorSubYNegocio("sub-test", 5, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([]);
			fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(10, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			tipoFiscalizadorBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([]);
			historialNormaSuscritaBcp.EstaCompletada(Arg.Any<HistorialNormaSuscrita>()).Returns(false);
			normaSuscritaBcp.EstaVigente(Arg.Any<NormaSuscrita>()).Returns(false);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => normaSuscritaUseCase.ObtenerVencimientoConDocumentosYPlan(10, 100, "sub-test"));
			Assert.Equal(TipoErrorValidacion.EstadoNoValido, ex.TipoErrorValidacion);
			await historialNormaSuscritaBcp.Received(1).Obtener(100, validarVigencia: true, validarIdNormaSuscrita: 10, transaction: Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ObtenerVencimientoConDocumentosYPlanTest_ConCodigoAcceso() {
			historialNotificacionBcp.ObtenerPorCodigoAccesoValidandoVigencia("codigo-acceso-test", Arg.Any<NpgsqlTransaction?>()).Returns(
				HistorialNotificacionBcpTest.HistorialNotificacionDummy(id: 1000, idHistorialNormaSuscrita: 100, codigoAcceso: "hash-codigo-acceso-test")
			);
			historialNormaSuscritaBcp.Obtener(100, validarVigencia: true, validarIdNormaSuscrita: null, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100, idNormaSuscrita: 10, fechaVencimiento: FECHA_DUMMY.AddDays(14))
			);
			NormaSuscrita retorno = NormaSuscritaBcpTest.NormaSuscritaDummy(id: 10, sub: "sub-test", idNegocio: 5,
				idTemplate: null, idNorma: null, idTipoPeriodicidad: null, idCategoriaNorma: null, idCargo: null);
			normaSuscritaBcp.Obtener(10, validarSub: null, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(retorno);
			templateBcp.ObtenerVariosSoloVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([]);
			tipoPeriodicidadBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([]);
			categoriaNormaBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([]);
			cargoBcp.ObtenerPorSubYNegocio("sub-test", 5, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([]);
			fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(10, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			tipoFiscalizadorBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([]);
			historialNormaSuscritaBcp.EstaCompletada(Arg.Any<HistorialNormaSuscrita>()).Returns(true);
			normaSuscritaBcp.EstaVigente(Arg.Any<NormaSuscrita>()).Returns(true);
			negocioBcp.Obtener(5, validarVigencia: true, validarSub: null, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NegocioBcpTest.NegocioDummy(id: 5, sub: "sub-test")
			);
			documentoAdjuntoBcp.ObtenerPorVencimiento(100, filtrarVigentes: true, filtrarRecepcionados: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([
				DocumentoAdjuntoBcpTest.DocumentoAdjuntoDummy(id: 10_000, idHistorialNormaSuscrita: 100),
				DocumentoAdjuntoBcpTest.DocumentoAdjuntoDummy(id: 20_000, idHistorialNormaSuscrita: 100)
			]);
			suscripcionBcp.ConsultaTienePlanEmpresa("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(true);

			(HistorialNormaSuscrita vencimiento, bool tienePlanEmpresa) = await normaSuscritaUseCase.ObtenerVencimientoConDocumentosYPlan("codigo-acceso-test");
			Assert.NotNull(vencimiento.NormaSuscrita);
			Assert.NotNull(vencimiento.NormaSuscrita.Negocio);
			Assert.NotNull(vencimiento.DocumentosAdjuntos);
			Assert.True(tienePlanEmpresa);
			Assert.Equal(2, vencimiento.DocumentosAdjuntos.Count);
			await historialNotificacionBcp.Received(1).ObtenerPorCodigoAccesoValidandoVigencia("codigo-acceso-test", Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaBcp.Received(1).Obtener(100, validarVigencia: true, validarIdNormaSuscrita: null, transaction: Arg.Any<NpgsqlTransaction?>());
			await negocioBcp.Received(1).Obtener(5, validarVigencia: true, validarSub: null, transaction: Arg.Any<NpgsqlTransaction?>());
			await documentoAdjuntoBcp.Received(1).ObtenerPorVencimiento(100, filtrarVigentes: true, filtrarRecepcionados: true, transaction: Arg.Any<NpgsqlTransaction?>());
			await suscripcionBcp.Received(1).ConsultaTienePlanEmpresa("sub-test", Arg.Any<NpgsqlTransaction?>());
		}

		[Fact]
		public async Task ActualizarProgramacionProcesosNormaSuscritaTest_Cron() {
			normaSuscritaBcp.Obtener(10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(
					id: 10, sub: "sub-test", idNegocio: 5, 
					idTemplate: null, idNorma: null, idTipoPeriodicidad: 1, idCategoriaNorma: null, idCargo: null, activado: true
				)
			);
			templateBcp.ObtenerVariosSoloVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([]);
			tipoPeriodicidadBcp.ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>()).Returns(
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 1, vigencia: true, cron: "MI HO DM * ? *", deltaDias: null, deltaMeses: 1, deltaAnnos: null)
			);
			tipoPeriodicidadBcp.EstaVigente(Arg.Any<TipoPeriodicidad>()).Returns(true);
			notificacionNormaSuscritaUseCase.ObtenerAntelacionesConsiderandoTemplate(10, null, null, Arg.Any<NpgsqlTransaction?>()).Returns([
				(TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 1, cantSegundos: 3600, cantMinutos: 60, cantHoras: 1), 1)
			]);
			historialNormaSuscritaBcp.ObtenerProximoVencimiento(10, Arg.Any<NpgsqlTransaction?>()).Returns(FECHA_DUMMY.AddMonths(1)); // 15-02-2026 11:00 Chile
			notificacionNormaSuscritaUseCase.GenerarCrons(FECHA_DUMMY.AddMonths(1), "MI HO DM * ? *", Arg.Any<List<(TipoUnidadTiempo, int)>>()).Returns([
				("0 11 15 * ? *", TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 1, cantSegundos: 3600, cantMinutos: 60, cantHoras: 1), 1, false)
			]);
			normaSuscritaProcesoNotificacionUseCase.ActualizarProcesosNotificacionesNormaSuscrita(Arg.Any<NormaSuscrita>(), Arg.Any<List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)>>(), Arg.Any<List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)>>()).Returns(
				([], [])
			);

			await normaSuscritaUseCase.ActualizarProgramacionProcesosNormaSuscrita(10);
			await notificacionNormaSuscritaUseCase.Received(1).ObtenerAntelacionesConsiderandoTemplate(10, null, null, Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaBcp.Received(1).ObtenerProximoVencimiento(10, Arg.Any<NpgsqlTransaction?>());
			await notificacionNormaSuscritaUseCase.Received(1).GenerarCrons(FECHA_DUMMY.AddMonths(1), "MI HO DM * ? *", Arg.Any<List<(TipoUnidadTiempo, int)>>());
			await normaSuscritaProcesoNotificacionUseCase.Received(1).ActualizarProcesosNotificacionesNormaSuscrita(Arg.Any<NormaSuscrita>(), Arg.Any<List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)>>(), Arg.Any<List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)>>(), Arg.Any<IDatabaseTransaction>());
		}

		[Fact]
		public async Task ActualizarProgramacionProcesosNormaSuscritaTest_FrecuenciaDias() {
			normaSuscritaBcp.Obtener(10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(
					id: 10, sub: "sub-test", idNegocio: 5,
					idTemplate: null, idNorma: null, idTipoPeriodicidad: 1, idCategoriaNorma: null, idCargo: null, activado: true
				)
			);
			templateBcp.ObtenerVariosSoloVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([]);
			tipoPeriodicidadBcp.ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>()).Returns(
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 1, vigencia: true, cron: null, frecuenciaDias: 14, deltaDias: 14, deltaMeses: null, deltaAnnos: null)
			);
			tipoPeriodicidadBcp.EstaVigente(Arg.Any<TipoPeriodicidad>()).Returns(true);
			notificacionNormaSuscritaUseCase.ObtenerAntelacionesConsiderandoTemplate(10, null, null, Arg.Any<NpgsqlTransaction?>()).Returns([
				(TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 1, cantSegundos: 3600, cantMinutos: 60, cantHoras: 1), 1)
			]);
			historialNormaSuscritaBcp.ObtenerProximoVencimiento(10, Arg.Any<NpgsqlTransaction?>()).Returns(FECHA_DUMMY.AddMonths(1)); // 15-02-2026 11:00 Chile
			notificacionNormaSuscritaUseCase.GenerarFrecuenciasDias(FECHA_DUMMY.AddMonths(1), 14, Arg.Any<List<(TipoUnidadTiempo, int)>>()).Returns([
				(14, FECHA_DUMMY.AddMonths(1).AddHours(-1), TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 1, cantSegundos: 3600, cantMinutos: 60, cantHoras: 1), 1, false)
			]);
			normaSuscritaProcesoNotificacionUseCase.ActualizarProcesosNotificacionesNormaSuscrita(Arg.Any<NormaSuscrita>(), Arg.Any<List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)>>(), Arg.Any<List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)>>()).Returns(
				([], [])
			);

			await normaSuscritaUseCase.ActualizarProgramacionProcesosNormaSuscrita(10);
			await notificacionNormaSuscritaUseCase.Received(1).ObtenerAntelacionesConsiderandoTemplate(10, null, null, Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaBcp.Received(1).ObtenerProximoVencimiento(10, Arg.Any<NpgsqlTransaction?>());
			await notificacionNormaSuscritaUseCase.Received(1).GenerarFrecuenciasDias(FECHA_DUMMY.AddMonths(1), 14, Arg.Any<List<(TipoUnidadTiempo, int)>>());
			await normaSuscritaProcesoNotificacionUseCase.Received(1).ActualizarProcesosNotificacionesNormaSuscrita(Arg.Any<NormaSuscrita>(), Arg.Any<List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)>>(), Arg.Any<List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)>>(), Arg.Any<IDatabaseTransaction>());
		}

		[Fact]
		public async Task ActualizarProgramacionProcesosNormaSuscritaTest_Desactivado() {
			normaSuscritaBcp.Obtener(10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(
					id: 10, sub: "sub-test", idNegocio: 5,
					idTemplate: null, idNorma: null, idTipoPeriodicidad: 1, idCategoriaNorma: null, idCargo: null, activado: false
				)
			);
			templateBcp.ObtenerVariosSoloVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([]);
			tipoPeriodicidadBcp.ObtenerPorId(1, Arg.Any<NpgsqlTransaction?>()).Returns(
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 1, vigencia: true, cron: "MI HO DM * ? *", deltaDias: null, deltaMeses: 1, deltaAnnos: null)
			);
			normaSuscritaProcesoNotificacionUseCase.ActualizarProcesosNotificacionesNormaSuscrita(Arg.Any<NormaSuscrita>(), Arg.Any<List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)>>(), Arg.Any<List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)>>()).Returns(
				([], [])
			);

			await normaSuscritaUseCase.ActualizarProgramacionProcesosNormaSuscrita(10);
			await tipoPeriodicidadBcp.DidNotReceive().ObtenerPorId(Arg.Any<long>(), Arg.Any<NpgsqlTransaction?>());
			await notificacionNormaSuscritaUseCase.DidNotReceive().ObtenerAntelacionesConsiderandoTemplate(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<NpgsqlTransaction?>());
			await notificacionNormaSuscritaUseCase.DidNotReceive().GenerarCrons(Arg.Any<DateTime>(), Arg.Any<string>(), Arg.Any<List<(TipoUnidadTiempo, int)>>());
			await notificacionNormaSuscritaUseCase.DidNotReceive().GenerarFrecuenciasDias(Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<List<(TipoUnidadTiempo, int)>>());
			await normaSuscritaProcesoNotificacionUseCase.Received(1).ActualizarProcesosNotificacionesNormaSuscrita(
				Arg.Any<NormaSuscrita>(),
				Arg.Is<List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)>>(d => d.Count == 0),
				Arg.Is<List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)>>(d => d.Count == 0),
				Arg.Any<IDatabaseTransaction>()
			);
		}

		[Fact]
		public async Task ActualizarProgramacionProcesosNormaSuscritaTest_Rollback() {
			normaSuscritaBcp.Obtener(10, transaction: Arg.Any<NpgsqlTransaction?>()).ThrowsAsync<Exception>();

			await Assert.ThrowsAsync<Exception>(() => normaSuscritaUseCase.ActualizarProgramacionProcesosNormaSuscrita(10));
			await normaSuscritaProcesoNotificacionUseCase.Received(1).ReversarProcesosProgramadosDesprogramados(Arg.Any<List<SalKairosIngresarProceso>>(), Arg.Any<List<NormaSuscritaProcesoNotificacion>>());
		}


		[Fact]
		public async Task ReversarProcesosProgramadosDesprogramadosTest() {
			List<SalKairosIngresarProceso> programados = [];
			List<NormaSuscritaProcesoNotificacion> desprogramados = [];

			await normaSuscritaUseCase.ReversarProcesosProgramadosDesprogramados(programados, desprogramados);
			await normaSuscritaProcesoNotificacionUseCase.Received(1).ReversarProcesosProgramadosDesprogramados(programados, desprogramados);
		}

		[Fact]
		public async Task EliminarNormaSuscritaTest_Valido() {
			normaSuscritaBcp.Obtener(10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(
					id: 10, sub: "sub-test", idNegocio: 5,
					idTemplate: null, idNorma: null, idTipoPeriodicidad: null, idCategoriaNorma: null, idCargo: null, activado: false
				)
			);
			templateBcp.ObtenerVariosSoloVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([]);
			NormaSuscrita normaSuscrita = NormaSuscritaBcpTest.NormaSuscritaDummy(id: 10, vigencia: true);
			
			await normaSuscritaUseCase.EliminarNormaSuscrita(normaSuscrita, transaction);
			await normaSuscritaBcp.Received(1).Eliminar(Arg.Any<NormaSuscrita>(), Arg.Any<NpgsqlTransaction>());
			await fiscalizadorNormaSuscritaBcp.Received(1).EliminarPorNormaSuscrita(10, Arg.Any<NpgsqlTransaction>());
			await notificacionNormaSuscritaBcp.Received(1).EliminarPorNormaSuscrita(10, Arg.Any<NpgsqlTransaction>());
			await historialNormaSuscritaUseCase.Received(1).EliminarPorNormaSuscrita(10, false, Arg.Any<NpgsqlTransaction>());
		}

		[Fact]
		public async Task EliminarNormaSuscritaTest_NoVigente() {
			NormaSuscrita normaSuscrita = NormaSuscritaBcpTest.NormaSuscritaDummy(id: 10, vigencia: false);

			await normaSuscritaUseCase.EliminarNormaSuscrita(normaSuscrita, transaction);
			await normaSuscritaBcp.DidNotReceive().Eliminar(Arg.Any<NormaSuscrita>(), Arg.Any<NpgsqlTransaction>());
			await fiscalizadorNormaSuscritaBcp.DidNotReceive().EliminarPorNormaSuscrita(Arg.Any<long>(), Arg.Any<NpgsqlTransaction>());
			await notificacionNormaSuscritaBcp.DidNotReceive().EliminarPorNormaSuscrita(Arg.Any<long>(), Arg.Any<NpgsqlTransaction>());
			await historialNormaSuscritaUseCase.DidNotReceive().EliminarPorNormaSuscrita(Arg.Any<long>(), Arg.Any<bool>(), Arg.Any<NpgsqlTransaction>());
		}

		[Fact]
		public async Task EliminarNormaValidandoPertenenciaTest_Valido() {
			NormaSuscrita normaSuscrita = NormaSuscritaBcpTest.NormaSuscritaDummy(id: 10, sub: "sub-test", idNegocio: 5,
				idTemplate: null, idNorma: null, idTipoPeriodicidad: null, idCategoriaNorma: null, idCargo: null,
				vigencia: true, editable: true
			);

			normaSuscritaBcp.Obtener(10, filtrarVigente: true, validarSub: "sub-test", validarEditable: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(normaSuscrita);
			normaSuscritaBcp.Obtener(10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(normaSuscrita);
			templateBcp.ObtenerVariosSoloVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([]);
		
			await normaSuscritaUseCase.EliminarNormaValidandoPertenencia("sub-test", 10);
			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.Received(1).Obtener(10, filtrarVigente: true, validarSub: "sub-test", validarEditable: true, transaction: Arg.Any<NpgsqlTransaction?>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task EliminarNormaValidandoPertenenciaTest_NoVigente() {
			normaSuscritaBcp.Obtener(10, filtrarVigente: true, validarSub: "sub-test", validarEditable: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns((NormaSuscrita?)null);
			
			await normaSuscritaUseCase.EliminarNormaValidandoPertenencia("sub-test", 10);
			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.Received(1).Obtener(10, filtrarVigente: true, validarSub: "sub-test", validarEditable: true, transaction: Arg.Any<NpgsqlTransaction?>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task EliminarNormaValidandoPertenenciaTest_Rollback() {
			normaSuscritaBcp.Obtener(10, filtrarVigente: true, validarSub: "sub-test", validarEditable: true, transaction: Arg.Any<NpgsqlTransaction?>()).ThrowsAsync<Exception>();

			await Assert.ThrowsAsync<Exception>(() => normaSuscritaUseCase.EliminarNormaValidandoPertenencia("sub-test", 10));
			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.Received(1).Obtener(10, filtrarVigente: true, validarSub: "sub-test", validarEditable: true, transaction: Arg.Any<NpgsqlTransaction?>());
			await normaSuscritaProcesoNotificacionUseCase.Received(1).ReversarProcesosProgramadosDesprogramados(Arg.Any<List<SalKairosIngresarProceso>>(), Arg.Any<List<NormaSuscritaProcesoNotificacion>>());
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}


		[Fact]
		public async Task CrearNormaSuscritaTest_Valido() {
			NormaSuscrita obligacion = NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, idNegocio: 10, sub: "sub-test");

			suscripcionBcp.ConsultaTienePlanEmpresa("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(true);
			tipoFiscalizadorBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 1000),
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 2000)
			]);
			tipoUnidadTiempoBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000)
			]);
			tipoPeriodicidadBcp.ObtenerValidandoVigencia(100, Arg.Any<NpgsqlTransaction?>()).Returns(TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 100));
			categoriaNormaBcp.ObtenerValidandoVigencia(200, Arg.Any<NpgsqlTransaction?>()).Returns(CategoriaNormaBcpTest.CategoriaNormaDummy(id: 200));
			cargoBcp.Obtener(300, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				CargoBcpTest.CargoDummy(id: 300, sub: "sub-test", idNegocio: 10)
			);
			normaSuscritaBcp.CrearObligacionUsuario("sub-test", 10, "nombre-test", "descripcion-test", "multa-test", 100, 200, 300, true, Arg.Any<NpgsqlTransaction?>()).Returns(obligacion);
			fiscalizadorNormaSuscritaBcp.ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				FiscalizadorNormaSuscritaBcpTest.FiscalizadorNormaSuscritaDummy(id: 10_000, idNormaSuscrita: 999, idTipoFiscalizador: 1000),
				FiscalizadorNormaSuscritaBcpTest.FiscalizadorNormaSuscritaDummy(id: 10_001, idNormaSuscrita: 999, idTipoFiscalizador: 2000),
			]);
			notificacionNormaSuscritaBcp.ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				NotificacionNormaSuscritaBcpTest.NotificacionNormaSuscritaDummy(id: 20_000, idNormaSuscrita: 999, idTipoUnidadTiempoAntelacion: 5000, cantAntelacion: 1)
			]);
			historialNormaSuscritaBcp.Crear(999, FECHA_DUMMY.AddDays(1), Arg.Any<NpgsqlTransaction?>()).Returns(
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 999, fechaVencimiento: FECHA_DUMMY.AddDays(1))
			);

			// Para ActualizarProgramacionProcesosNormaSuscrita
			normaSuscritaBcp.Obtener(999, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(obligacion);
			templateBcp.ObtenerVariosSoloVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([]);
			tipoPeriodicidadBcp.ObtenerPorId(5000, Arg.Any<NpgsqlTransaction?>()).Returns(
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 5000, vigencia: true, cron: "MI HO DM * ? *", deltaDias: null, deltaMeses: 1, deltaAnnos: null)
			);
			tipoPeriodicidadBcp.EstaVigente(Arg.Any<TipoPeriodicidad>()).Returns(true);
			notificacionNormaSuscritaUseCase.ObtenerAntelacionesConsiderandoTemplate(999, null, null, Arg.Any<NpgsqlTransaction?>()).Returns([
				(TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000, cantSegundos: 3600, cantMinutos: 60, cantHoras: 1), 1)
			]);
			historialNormaSuscritaBcp.ObtenerProximoVencimiento(10, Arg.Any<NpgsqlTransaction?>()).Returns(FECHA_DUMMY);
			notificacionNormaSuscritaUseCase.GenerarCrons(FECHA_DUMMY.AddMonths(1), "MI HO DM * ? *", Arg.Any<List<(TipoUnidadTiempo, int)>>()).Returns([
				("0 11 15 * ? *", TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000, cantSegundos: 3600, cantMinutos: 60, cantHoras: 1), 1, false)
			]);
		
			(NormaSuscrita obligacion, List<SalKairosIngresarProceso> programados, List<NormaSuscritaProcesoNotificacion> desprogramados) retorno = await normaSuscritaUseCase.CrearNormaSuscrita(
				"sub-test",
				10,
				"nombre-test",
				"descripcion-test",
				"multa-test",
				100,
				200,
				300,
				true, 
				FECHA_DUMMY.AddDays(1),
				[1000, 2000],
				[(5000, 1)]
			);
			Assert.Equal(999, retorno.obligacion.Id);

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.Received(1).CrearObligacionUsuario("sub-test", 10, "nombre-test", "descripcion-test", "multa-test", 100, 200, 300, true, Arg.Any<NpgsqlTransaction?>());
			await fiscalizadorNormaSuscritaBcp.Received(1).ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>());
			await notificacionNormaSuscritaBcp.Received(1).ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaBcp.Received(1).Crear(999, FECHA_DUMMY.AddDays(1), Arg.Any<NpgsqlTransaction?>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task CrearNormaSuscritaTest_SinPlanEmpresaConCargo() {
			NormaSuscrita obligacion = NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, idNegocio: 10, sub: "sub-test");

			suscripcionBcp.ConsultaTienePlanEmpresa("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(false);
			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => normaSuscritaUseCase.CrearNormaSuscrita(
				"sub-test",
				10,
				"nombre-test",
				"descripcion-test",
				"multa-test",
				100,
				200,
				300,
				true,
				FECHA_DUMMY.AddDays(1),
				[1000, 2000],
				[(5000, 1)]
			));
			Assert.Equal(TipoErrorValidacion.RestringidoPorPlan, ex.TipoErrorValidacion);

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.DidNotReceive().CrearObligacionUsuario(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<bool>(), Arg.Any<NpgsqlTransaction?>());
			await fiscalizadorNormaSuscritaBcp.DidNotReceive().ActualizarPorNormaSuscrita(Arg.Any<long>(), Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>());
			await notificacionNormaSuscritaBcp.DidNotReceive().ActualizarPorNormaSuscrita(Arg.Any<long>(), Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaBcp.DidNotReceive().Crear(Arg.Any<long>(), Arg.Any<DateTime>(), Arg.Any<NpgsqlTransaction?>());
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task CrearNormaSuscritaTest_CantAntelacionNegativa() {
			NormaSuscrita obligacion = NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, idNegocio: 10, sub: "sub-test");

			suscripcionBcp.ConsultaTienePlanEmpresa("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(true);
			tipoFiscalizadorBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 1000),
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 2000)
			]);
			tipoUnidadTiempoBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000)
			]);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => normaSuscritaUseCase.CrearNormaSuscrita(
				"sub-test",
				10,
				"nombre-test",
				"descripcion-test",
				"multa-test",
				100,
				200,
				300,
				true,
				FECHA_DUMMY.AddDays(1),
				[1000, 2000],
				[(5000, -1)]
			));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.DidNotReceive().CrearObligacionUsuario(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<bool>(), Arg.Any<NpgsqlTransaction?>());
			await fiscalizadorNormaSuscritaBcp.DidNotReceive().ActualizarPorNormaSuscrita(Arg.Any<long>(), Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>());
			await notificacionNormaSuscritaBcp.DidNotReceive().ActualizarPorNormaSuscrita(Arg.Any<long>(), Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaBcp.DidNotReceive().Crear(Arg.Any<long>(), Arg.Any<DateTime>(), Arg.Any<NpgsqlTransaction?>());
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task CrearNormaSuscritaTest_ActivadoSinProximoVencimiento() {
			NormaSuscrita obligacion = NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, idNegocio: 10, sub: "sub-test");

			suscripcionBcp.ConsultaTienePlanEmpresa("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(true);
			tipoFiscalizadorBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 1000),
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 2000)
			]);
			tipoUnidadTiempoBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000)
			]);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => normaSuscritaUseCase.CrearNormaSuscrita(
				"sub-test",
				10,
				"nombre-test",
				"descripcion-test",
				"multa-test",
				100,
				200,
				300,
				true,
				null,
				[1000, 2000],
				[(5000, 1)]
			));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.DidNotReceive().CrearObligacionUsuario(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<bool>(), Arg.Any<NpgsqlTransaction?>());
			await fiscalizadorNormaSuscritaBcp.DidNotReceive().ActualizarPorNormaSuscrita(Arg.Any<long>(), Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>());
			await notificacionNormaSuscritaBcp.DidNotReceive().ActualizarPorNormaSuscrita(Arg.Any<long>(), Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaBcp.DidNotReceive().Crear(Arg.Any<long>(), Arg.Any<DateTime>(), Arg.Any<NpgsqlTransaction?>());
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task CrearNormaSuscritaTest_ActivadoProximoVencimientoPasado() {
			NormaSuscrita obligacion = NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, idNegocio: 10, sub: "sub-test");

			suscripcionBcp.ConsultaTienePlanEmpresa("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(true);
			tipoFiscalizadorBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 1000),
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 2000)
			]);
			tipoUnidadTiempoBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000)
			]);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => normaSuscritaUseCase.CrearNormaSuscrita(
				"sub-test",
				10,
				"nombre-test",
				"descripcion-test",
				"multa-test",
				100,
				200,
				300,
				true,
				FECHA_DUMMY.AddDays(-1),
				[1000, 2000],
				[(5000, 1)]
			));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.DidNotReceive().CrearObligacionUsuario(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<bool>(), Arg.Any<NpgsqlTransaction?>());
			await fiscalizadorNormaSuscritaBcp.DidNotReceive().ActualizarPorNormaSuscrita(Arg.Any<long>(), Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>());
			await notificacionNormaSuscritaBcp.DidNotReceive().ActualizarPorNormaSuscrita(Arg.Any<long>(), Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaBcp.DidNotReceive().Crear(Arg.Any<long>(), Arg.Any<DateTime>(), Arg.Any<NpgsqlTransaction?>());
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ActualizarNormaSuscritaTest_Valido() {
			normaSuscritaBcp.Obtener(999, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, sub: "sub-test", idNegocio: 10, idTemplate: null, idNorma: null, idTipoPeriodicidad: null, idCategoriaNorma: null, idCargo: null, activado: true)
			);
			fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(999, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			notificacionNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(999, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			suscripcionBcp.ConsultaTienePlanEmpresa("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(true);
			tipoFiscalizadorBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 1000),
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 2000)
			]);
			tipoUnidadTiempoBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000)
			]);
			historialNormaSuscritaBcp.ObtenerPorNormaSuscrita(999, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 999, fechaVencimiento: FECHA_DUMMY)
			]);
			historialNormaSuscritaBcp.FiltrarUltimoVencimiento(Arg.Any<List<HistorialNormaSuscrita>>()).Returns(
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 999, fechaVencimiento: FECHA_DUMMY)
			);
			tipoPeriodicidadBcp.ObtenerValidandoVigencia(id: 100, Arg.Any<NpgsqlTransaction?>()).Returns(TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 100));
			categoriaNormaBcp.ObtenerValidandoVigencia(id: 200, Arg.Any<NpgsqlTransaction?>()).Returns(CategoriaNormaBcpTest.CategoriaNormaDummy(id: 200));
			cargoBcp.Obtener(300, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				CargoBcpTest.CargoDummy(id: 300, sub: "sub-test", idNegocio: 10)
			);
			fiscalizadorNormaSuscritaBcp.ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				FiscalizadorNormaSuscritaBcpTest.FiscalizadorNormaSuscritaDummy(id: 10_000, idNormaSuscrita: 999, idTipoFiscalizador: 1000),
				FiscalizadorNormaSuscritaBcpTest.FiscalizadorNormaSuscritaDummy(id: 10_001, idNormaSuscrita: 999, idTipoFiscalizador: 2000),
			]);
			notificacionNormaSuscritaBcp.ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				NotificacionNormaSuscritaBcpTest.NotificacionNormaSuscritaDummy(id: 20_000, idNormaSuscrita: 999, idTipoUnidadTiempoAntelacion: 5000, cantAntelacion: 1)
			]);
			historialNormaSuscritaBcp.Crear(999, FECHA_DUMMY.AddDays(14), Arg.Any<NpgsqlTransaction?>()).Returns(
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 999, fechaVencimiento: FECHA_DUMMY.AddDays(14))
			);

			// Para ActualizarProgramacionProcesosNormaSuscrita
			normaSuscritaBcp.Obtener(999, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, sub: "sub-test", idNegocio: 10, idTemplate: null, idNorma: null, idTipoPeriodicidad: 100, idCategoriaNorma: 200, idCargo: 300, activado: true)
			);
			tipoPeriodicidadBcp.ObtenerPorId(100, Arg.Any<NpgsqlTransaction?>()).Returns(
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 100, vigencia: true, cron: "MI HO DM * ? *", deltaDias: null, deltaMeses: 1, deltaAnnos: null)
			);
			tipoPeriodicidadBcp.EstaVigente(Arg.Any<TipoPeriodicidad>()).Returns(true);
			notificacionNormaSuscritaUseCase.ObtenerAntelacionesConsiderandoTemplate(999, null, null, Arg.Any<NpgsqlTransaction?>()).Returns([
				(TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000, cantSegundos: 3600, cantMinutos: 60, cantHoras: 1), 1)
			]);
			historialNormaSuscritaBcp.ObtenerProximoVencimiento(10, Arg.Any<NpgsqlTransaction?>()).Returns(FECHA_DUMMY.AddDays(14));
			notificacionNormaSuscritaUseCase.GenerarCrons(FECHA_DUMMY.AddDays(14), "MI HO DM * ? *", Arg.Any<List<(TipoUnidadTiempo, int)>>()).Returns([
				("0 11 15 * ? *", TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000, cantSegundos: 3600, cantMinutos: 60, cantHoras: 1), 1, false)
			]);
			
			(NormaSuscrita obligacion, List<SalKairosIngresarProceso> programados, List<NormaSuscritaProcesoNotificacion> desprogramados) retorno = await normaSuscritaUseCase.ActualizarNormaSuscrita(
				"sub-test",	
				999,
				10,
				"otro-nombre-test",
				"otra-descripcion-test",
				"otra-multa-test",
				100,
				200,
				300,
				true, 
				FECHA_DUMMY.AddDays(14),
				[1000, 2000],
				[(5000, 1)]
			);

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.Received(1).Actualizar(Arg.Any<NormaSuscrita>(), Arg.Any<NpgsqlTransaction?>());
			await fiscalizadorNormaSuscritaBcp.Received(1).ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>());
			await notificacionNormaSuscritaBcp.Received(1).ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaUseCase.Received(1).EliminarPorNormaSuscrita(999, true, Arg.Any<NpgsqlTransaction>());
			await historialNormaSuscritaBcp.Received(1).Crear(999, FECHA_DUMMY.AddDays(14), Arg.Any<NpgsqlTransaction?>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ActualizarNormaSuscritaTest_ValidoProximoVencimientoModificado() {
			normaSuscritaBcp.Obtener(999, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, sub: "sub-test", idNegocio: 10, idTemplate: null, idNorma: null, idTipoPeriodicidad: null, idCategoriaNorma: null, idCargo: null, activado: true)
			);
			fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(999, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			notificacionNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(999, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			suscripcionBcp.ConsultaTienePlanEmpresa("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(true);
			tipoFiscalizadorBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 1000),
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 2000)
			]);
			tipoUnidadTiempoBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000)
			]);
			historialNormaSuscritaBcp.ObtenerPorNormaSuscrita(999, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 999, fechaVencimiento: FECHA_DUMMY)
			]);
			historialNormaSuscritaBcp.FiltrarUltimoVencimiento(Arg.Any<List<HistorialNormaSuscrita>>()).Returns(
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 999, fechaVencimiento: FECHA_DUMMY)
			);
			tipoPeriodicidadBcp.ObtenerValidandoVigencia(id: 100, Arg.Any<NpgsqlTransaction?>()).Returns(TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 100));
			categoriaNormaBcp.ObtenerValidandoVigencia(id: 200, Arg.Any<NpgsqlTransaction?>()).Returns(CategoriaNormaBcpTest.CategoriaNormaDummy(id: 200));
			cargoBcp.Obtener(300, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				CargoBcpTest.CargoDummy(id: 300, sub: "sub-test", idNegocio: 10)
			);
			historialNormaSuscritaUseCase.CalcularVencimientoFuturo(FECHA_DUMMY.AddDays(-1), Arg.Any<TipoPeriodicidad>()).Returns(FECHA_DUMMY.AddDays(14));
			fiscalizadorNormaSuscritaBcp.ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				FiscalizadorNormaSuscritaBcpTest.FiscalizadorNormaSuscritaDummy(id: 10_000, idNormaSuscrita: 999, idTipoFiscalizador: 1000),
				FiscalizadorNormaSuscritaBcpTest.FiscalizadorNormaSuscritaDummy(id: 10_001, idNormaSuscrita: 999, idTipoFiscalizador: 2000),
			]);
			notificacionNormaSuscritaBcp.ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				NotificacionNormaSuscritaBcpTest.NotificacionNormaSuscritaDummy(id: 20_000, idNormaSuscrita: 999, idTipoUnidadTiempoAntelacion: 5000, cantAntelacion: 1)
			]);
			historialNormaSuscritaBcp.Crear(999, FECHA_DUMMY.AddDays(14), Arg.Any<NpgsqlTransaction?>()).Returns(
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 999, fechaVencimiento: FECHA_DUMMY.AddDays(14))
			);

			// Para ActualizarProgramacionProcesosNormaSuscrita
			normaSuscritaBcp.Obtener(999, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, sub: "sub-test", idNegocio: 10, idTemplate: null, idNorma: null, idTipoPeriodicidad: 100, idCategoriaNorma: 200, idCargo: 300, activado: true)
			);
			tipoPeriodicidadBcp.ObtenerPorId(100, Arg.Any<NpgsqlTransaction?>()).Returns(
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 100, vigencia: true, cron: "MI HO DM * ? *", deltaDias: null, deltaMeses: 1, deltaAnnos: null)
			);
			tipoPeriodicidadBcp.EstaVigente(Arg.Any<TipoPeriodicidad>()).Returns(true);
			notificacionNormaSuscritaUseCase.ObtenerAntelacionesConsiderandoTemplate(999, null, null, Arg.Any<NpgsqlTransaction?>()).Returns([
				(TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000, cantSegundos: 3600, cantMinutos: 60, cantHoras: 1), 1)
			]);
			historialNormaSuscritaBcp.ObtenerProximoVencimiento(10, Arg.Any<NpgsqlTransaction?>()).Returns(FECHA_DUMMY.AddDays(14));
			notificacionNormaSuscritaUseCase.GenerarCrons(FECHA_DUMMY.AddDays(14), "MI HO DM * ? *", Arg.Any<List<(TipoUnidadTiempo, int)>>()).Returns([
				("0 11 15 * ? *", TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000, cantSegundos: 3600, cantMinutos: 60, cantHoras: 1), 1, false)
			]);

			(NormaSuscrita obligacion, List<SalKairosIngresarProceso> programados, List<NormaSuscritaProcesoNotificacion> desprogramados) retorno = await normaSuscritaUseCase.ActualizarNormaSuscrita(
				"sub-test",
				999,
				10,
				"otro-nombre-test",
				"otra-descripcion-test",
				"otra-multa-test",
				100,
				200,
				300,
				true,
				FECHA_DUMMY.AddDays(-1),
				[1000, 2000],
				[(5000, 1)]
			);

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			historialNormaSuscritaUseCase.Received(1).CalcularVencimientoFuturo(FECHA_DUMMY.AddDays(-1), Arg.Any<TipoPeriodicidad>());
			await normaSuscritaBcp.Received(1).Actualizar(Arg.Any<NormaSuscrita>(), Arg.Any<NpgsqlTransaction?>());
			await fiscalizadorNormaSuscritaBcp.Received(1).ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>());
			await notificacionNormaSuscritaBcp.Received(1).ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaUseCase.Received(1).EliminarPorNormaSuscrita(999, true, Arg.Any<NpgsqlTransaction>());
			await historialNormaSuscritaBcp.Received(1).Crear(999, FECHA_DUMMY.AddDays(14), Arg.Any<NpgsqlTransaction?>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ActualizarNormaSuscritaTest_Desactivando() {
			normaSuscritaBcp.Obtener(999, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, sub: "sub-test", idNegocio: 10, idTemplate: null, idNorma: null, idTipoPeriodicidad: null, idCategoriaNorma: null, idCargo: null, activado: true)
			);
			fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(999, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			notificacionNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(999, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			suscripcionBcp.ConsultaTienePlanEmpresa("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(true);
			tipoFiscalizadorBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 1000),
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 2000)
			]);
			tipoUnidadTiempoBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000)
			]);
			historialNormaSuscritaBcp.ObtenerPorNormaSuscrita(999, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 999, fechaVencimiento: FECHA_DUMMY)
			]);
			historialNormaSuscritaBcp.FiltrarUltimoVencimiento(Arg.Any<List<HistorialNormaSuscrita>>()).Returns(
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 999, fechaVencimiento: FECHA_DUMMY)
			);
			tipoPeriodicidadBcp.ObtenerValidandoVigencia(id: 100, Arg.Any<NpgsqlTransaction?>()).Returns(TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 100));
			categoriaNormaBcp.ObtenerValidandoVigencia(id: 200, Arg.Any<NpgsqlTransaction?>()).Returns(CategoriaNormaBcpTest.CategoriaNormaDummy(id: 200));
			cargoBcp.Obtener(300, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				CargoBcpTest.CargoDummy(id: 300, sub: "sub-test", idNegocio: 10)
			);
			fiscalizadorNormaSuscritaBcp.ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				FiscalizadorNormaSuscritaBcpTest.FiscalizadorNormaSuscritaDummy(id: 10_000, idNormaSuscrita: 999, idTipoFiscalizador: 1000),
				FiscalizadorNormaSuscritaBcpTest.FiscalizadorNormaSuscritaDummy(id: 10_001, idNormaSuscrita: 999, idTipoFiscalizador: 2000),
			]);
			notificacionNormaSuscritaBcp.ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				NotificacionNormaSuscritaBcpTest.NotificacionNormaSuscritaDummy(id: 20_000, idNormaSuscrita: 999, idTipoUnidadTiempoAntelacion: 5000, cantAntelacion: 1)
			]);

			// Para ActualizarProgramacionProcesosNormaSuscrita
			normaSuscritaBcp.Obtener(999, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, sub: "sub-test", idNegocio: 10, idTemplate: null, idNorma: null, idTipoPeriodicidad: 100, idCategoriaNorma: 200, idCargo: 300, activado: false)
			);
			
			(NormaSuscrita obligacion, List<SalKairosIngresarProceso> programados, List<NormaSuscritaProcesoNotificacion> desprogramados) retorno = await normaSuscritaUseCase.ActualizarNormaSuscrita(
				"sub-test",
				999,
				10,
				"otro-nombre-test",
				"otra-descripcion-test",
				"otra-multa-test",
				100,
				200,
				300,
				false,
				null,
				[1000, 2000],
				[(5000, 1)]
			);

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.Received(1).Actualizar(Arg.Any<NormaSuscrita>(), Arg.Any<NpgsqlTransaction?>());
			await normaSuscritaBcp.Received(1).Desactivar(Arg.Any<NormaSuscrita>(), Arg.Any<NpgsqlTransaction?>());
			await fiscalizadorNormaSuscritaBcp.Received(1).ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>());
			await notificacionNormaSuscritaBcp.Received(1).ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaUseCase.Received(1).EliminarPorNormaSuscrita(999, false, Arg.Any<NpgsqlTransaction>());
			await historialNormaSuscritaBcp.DidNotReceive().Crear(999, FECHA_DUMMY.AddDays(14), Arg.Any<NpgsqlTransaction?>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ActualizarNormaSuscritaTest_Activando() {
			normaSuscritaBcp.Obtener(999, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, sub: "sub-test", idNegocio: 10, idTemplate: null, idNorma: null, idTipoPeriodicidad: null, idCategoriaNorma: null, idCargo: null, activado: false)
			);
			fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(999, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			notificacionNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(999, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			suscripcionBcp.ConsultaTienePlanEmpresa("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(true);
			tipoFiscalizadorBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 1000),
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 2000)
			]);
			tipoUnidadTiempoBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000)
			]);
			historialNormaSuscritaBcp.ObtenerPorNormaSuscrita(999, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([]);
			historialNormaSuscritaBcp.FiltrarUltimoVencimiento(Arg.Any<List<HistorialNormaSuscrita>>()).Returns((HistorialNormaSuscrita?)null);
			tipoPeriodicidadBcp.ObtenerValidandoVigencia(id: 100, Arg.Any<NpgsqlTransaction?>()).Returns(TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 100));
			categoriaNormaBcp.ObtenerValidandoVigencia(id: 200, Arg.Any<NpgsqlTransaction?>()).Returns(CategoriaNormaBcpTest.CategoriaNormaDummy(id: 200));
			cargoBcp.Obtener(300, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				CargoBcpTest.CargoDummy(id: 300, sub: "sub-test", idNegocio: 10)
			);
			fiscalizadorNormaSuscritaBcp.ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				FiscalizadorNormaSuscritaBcpTest.FiscalizadorNormaSuscritaDummy(id: 10_000, idNormaSuscrita: 999, idTipoFiscalizador: 1000),
				FiscalizadorNormaSuscritaBcpTest.FiscalizadorNormaSuscritaDummy(id: 10_001, idNormaSuscrita: 999, idTipoFiscalizador: 2000),
			]);
			notificacionNormaSuscritaBcp.ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				NotificacionNormaSuscritaBcpTest.NotificacionNormaSuscritaDummy(id: 20_000, idNormaSuscrita: 999, idTipoUnidadTiempoAntelacion: 5000, cantAntelacion: 1)
			]);
			historialNormaSuscritaBcp.Crear(999, FECHA_DUMMY.AddDays(14), Arg.Any<NpgsqlTransaction?>()).Returns(
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 999, fechaVencimiento: FECHA_DUMMY.AddDays(14))
			);

			// Para ActualizarProgramacionProcesosNormaSuscrita
			normaSuscritaBcp.Obtener(999, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, sub: "sub-test", idNegocio: 10, idTemplate: null, idNorma: null, idTipoPeriodicidad: 100, idCategoriaNorma: 200, idCargo: 300, activado: true)
			);
			tipoPeriodicidadBcp.ObtenerPorId(100, Arg.Any<NpgsqlTransaction?>()).Returns(
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 100, vigencia: true, cron: "MI HO DM * ? *", deltaDias: null, deltaMeses: 1, deltaAnnos: null)
			);
			tipoPeriodicidadBcp.EstaVigente(Arg.Any<TipoPeriodicidad>()).Returns(true);
			notificacionNormaSuscritaUseCase.ObtenerAntelacionesConsiderandoTemplate(999, null, null, Arg.Any<NpgsqlTransaction?>()).Returns([
				(TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000, cantSegundos: 3600, cantMinutos: 60, cantHoras: 1), 1)
			]);
			historialNormaSuscritaBcp.ObtenerProximoVencimiento(10, Arg.Any<NpgsqlTransaction?>()).Returns(FECHA_DUMMY.AddDays(14));
			notificacionNormaSuscritaUseCase.GenerarCrons(FECHA_DUMMY.AddDays(14), "MI HO DM * ? *", Arg.Any<List<(TipoUnidadTiempo, int)>>()).Returns([
				("0 11 15 * ? *", TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000, cantSegundos: 3600, cantMinutos: 60, cantHoras: 1), 1, false)
			]);
			
			(NormaSuscrita obligacion, List<SalKairosIngresarProceso> programados, List<NormaSuscritaProcesoNotificacion> desprogramados) retorno = await normaSuscritaUseCase.ActualizarNormaSuscrita(
				"sub-test",
				999,
				10,
				"otro-nombre-test",
				"otra-descripcion-test",
				"otra-multa-test",
				100,
				200,
				300,
				true,
				FECHA_DUMMY.AddDays(14),
				[1000, 2000],
				[(5000, 1)]
			);

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.Received(1).Actualizar(Arg.Any<NormaSuscrita>(), Arg.Any<NpgsqlTransaction?>());
			await normaSuscritaBcp.Received(1).Activar(Arg.Any<NormaSuscrita>(), Arg.Any<NpgsqlTransaction?>());
			await fiscalizadorNormaSuscritaBcp.Received(1).ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>());
			await notificacionNormaSuscritaBcp.Received(1).ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaUseCase.Received(1).EliminarPorNormaSuscrita(999, true, Arg.Any<NpgsqlTransaction>());
			await historialNormaSuscritaBcp.Received(1).Crear(999, FECHA_DUMMY.AddDays(14), Arg.Any<NpgsqlTransaction?>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ActualizarNormaSuscritaTest_MismoProximoVencimiento() {
			normaSuscritaBcp.Obtener(999, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, sub: "sub-test", idNegocio: 10, idTemplate: null, idNorma: null, idTipoPeriodicidad: null, idCategoriaNorma: null, idCargo: null, activado: true)
			);
			fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(999, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			notificacionNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(999, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			suscripcionBcp.ConsultaTienePlanEmpresa("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(true);
			tipoFiscalizadorBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 1000),
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 2000)
			]);
			tipoUnidadTiempoBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000)
			]);
			historialNormaSuscritaBcp.ObtenerPorNormaSuscrita(999, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 999, fechaVencimiento: FECHA_DUMMY.AddDays(14))
			]);
			historialNormaSuscritaBcp.FiltrarUltimoVencimiento(Arg.Any<List<HistorialNormaSuscrita>>()).Returns(
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 999, fechaVencimiento: FECHA_DUMMY.AddDays(14))
			);
			tipoPeriodicidadBcp.ObtenerValidandoVigencia(id: 100, Arg.Any<NpgsqlTransaction?>()).Returns(TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 100));
			categoriaNormaBcp.ObtenerValidandoVigencia(id: 200, Arg.Any<NpgsqlTransaction?>()).Returns(CategoriaNormaBcpTest.CategoriaNormaDummy(id: 200));
			cargoBcp.Obtener(300, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				CargoBcpTest.CargoDummy(id: 300, sub: "sub-test", idNegocio: 10)
			);
			fiscalizadorNormaSuscritaBcp.ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				FiscalizadorNormaSuscritaBcpTest.FiscalizadorNormaSuscritaDummy(id: 10_000, idNormaSuscrita: 999, idTipoFiscalizador: 1000),
				FiscalizadorNormaSuscritaBcpTest.FiscalizadorNormaSuscritaDummy(id: 10_001, idNormaSuscrita: 999, idTipoFiscalizador: 2000),
			]);
			notificacionNormaSuscritaBcp.ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				NotificacionNormaSuscritaBcpTest.NotificacionNormaSuscritaDummy(id: 20_000, idNormaSuscrita: 999, idTipoUnidadTiempoAntelacion: 5000, cantAntelacion: 1)
			]);

			// Para ActualizarProgramacionProcesosNormaSuscrita
			normaSuscritaBcp.Obtener(999, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, sub: "sub-test", idNegocio: 10, idTemplate: null, idNorma: null, idTipoPeriodicidad: 100, idCategoriaNorma: 200, idCargo: 300, activado: true)
			);
			tipoPeriodicidadBcp.ObtenerPorId(100, Arg.Any<NpgsqlTransaction?>()).Returns(
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 100, vigencia: true, cron: "MI HO DM * ? *", deltaDias: null, deltaMeses: 1, deltaAnnos: null)
			);
			tipoPeriodicidadBcp.EstaVigente(Arg.Any<TipoPeriodicidad>()).Returns(true);
			notificacionNormaSuscritaUseCase.ObtenerAntelacionesConsiderandoTemplate(999, null, null, Arg.Any<NpgsqlTransaction?>()).Returns([
				(TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000, cantSegundos: 3600, cantMinutos: 60, cantHoras: 1), 1)
			]);
			historialNormaSuscritaBcp.ObtenerProximoVencimiento(10, Arg.Any<NpgsqlTransaction?>()).Returns(FECHA_DUMMY.AddDays(14));
			notificacionNormaSuscritaUseCase.GenerarCrons(FECHA_DUMMY.AddDays(14), "MI HO DM * ? *", Arg.Any<List<(TipoUnidadTiempo, int)>>()).Returns([
				("0 11 15 * ? *", TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000, cantSegundos: 3600, cantMinutos: 60, cantHoras: 1), 1, false)
			]);
			
			(NormaSuscrita obligacion, List<SalKairosIngresarProceso> programados, List<NormaSuscritaProcesoNotificacion> desprogramados) retorno = await normaSuscritaUseCase.ActualizarNormaSuscrita(
				"sub-test",
				999,
				10,
				"otro-nombre-test",
				"otra-descripcion-test",
				"otra-multa-test",
				100,
				200,
				300,
				true,
				FECHA_DUMMY.AddDays(14),
				[1000, 2000],
				[(5000, 1)]
			);

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.Received(1).Actualizar(Arg.Any<NormaSuscrita>(), Arg.Any<NpgsqlTransaction?>());
			await fiscalizadorNormaSuscritaBcp.Received(1).ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>());
			await notificacionNormaSuscritaBcp.Received(1).ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaUseCase.DidNotReceive().EliminarPorNormaSuscrita(Arg.Any<long>(), Arg.Any<bool>(), Arg.Any<NpgsqlTransaction>());
			await historialNormaSuscritaBcp.DidNotReceive().Crear(Arg.Any<long>(), Arg.Any<DateTime>(), Arg.Any<NpgsqlTransaction?>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ActualizarNormaSuscritaTest_IgualQueTemplate() {
			normaSuscritaBcp.Obtener(999, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, sub: "sub-test", idNegocio: 10, idTemplate: 1, idNorma: 1, idTipoPeriodicidad: null, idCategoriaNorma: null, idCargo: null, activado: true)
			);
			templateBcp.ObtenerVariosSoloVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TemplateBcpTest.TemplateDummy(id: 1)
			]);
			templateNormaBcp.ObtenerPorTemplate(1, Arg.Any<NpgsqlTransaction?>()).Returns([
				TemplateNormaBcpTest.TemplateNormaDummy(idTemplate: 1, idNorma: 1, nombre: "nombre-template-test", descripcion: "descripcion-template-test", 
				multa: "multa-template-test", idTipoPeriodicidad: 100, idCategoriaNorma: 200)
			]);
			tipoFiscalizadorBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 1000),
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 2000)
			]);
			tipoUnidadTiempoBcp.ObtenerVigentes(Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000)
			]);
			templateNormaFiscalizadorBcp.ObtenerPorTemplateNorma(1, 1, Arg.Any<NpgsqlTransaction?>()).Returns([
				TemplateNormaFiscalizadorBcpTest.TemplateNormaFiscalizadorDummy(idTemplate: 1, idNorma: 1, idTipoFiscalizador: 1000),
				TemplateNormaFiscalizadorBcpTest.TemplateNormaFiscalizadorDummy(idTemplate: 1, idNorma: 1, idTipoFiscalizador: 2000)
			]);
			templateNormaNotificacionBcp.ObtenerPorTemplateNorma(1, 1, Arg.Any<NpgsqlTransaction?>()).Returns([
				TemplateNormaNotificacionBcpTest.TemplateNormaNotificacionDummy(idTemplate: 1, idNorma: 1, idTipoUnidadTiempoAntelacion: 5000, cantAntelacion: 1)
			]);
			fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(999, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			notificacionNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(999, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			suscripcionBcp.ConsultaTienePlanEmpresa("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(true);
			tipoFiscalizadorBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 1000),
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 2000)
			]);
			tipoUnidadTiempoBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000)
			]);
			historialNormaSuscritaBcp.ObtenerPorNormaSuscrita(999, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 999, fechaVencimiento: FECHA_DUMMY)
			]);
			historialNormaSuscritaBcp.FiltrarUltimoVencimiento(Arg.Any<List<HistorialNormaSuscrita>>()).Returns(
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 999, fechaVencimiento: FECHA_DUMMY)
			);
			tipoPeriodicidadBcp.ObtenerValidandoVigencia(id: 100, Arg.Any<NpgsqlTransaction?>()).Returns(TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 100));
			categoriaNormaBcp.ObtenerValidandoVigencia(id: 200, Arg.Any<NpgsqlTransaction?>()).Returns(CategoriaNormaBcpTest.CategoriaNormaDummy(id: 200));
			cargoBcp.Obtener(300, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				CargoBcpTest.CargoDummy(id: 300, sub: "sub-test", idNegocio: 10)
			);
			fiscalizadorNormaSuscritaBcp.ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				FiscalizadorNormaSuscritaBcpTest.FiscalizadorNormaSuscritaDummy(id: 10_000, idNormaSuscrita: 999, idTipoFiscalizador: 1000),
				FiscalizadorNormaSuscritaBcpTest.FiscalizadorNormaSuscritaDummy(id: 10_001, idNormaSuscrita: 999, idTipoFiscalizador: 2000),
			]);
			notificacionNormaSuscritaBcp.ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				NotificacionNormaSuscritaBcpTest.NotificacionNormaSuscritaDummy(id: 20_000, idNormaSuscrita: 999, idTipoUnidadTiempoAntelacion: 5000, cantAntelacion: 1)
			]);
			historialNormaSuscritaBcp.Crear(999, FECHA_DUMMY.AddDays(14), Arg.Any<NpgsqlTransaction?>()).Returns(
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 999, fechaVencimiento: FECHA_DUMMY.AddDays(14))
			);

			// Para ActualizarProgramacionProcesosNormaSuscrita
			normaSuscritaBcp.Obtener(999, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, sub: "sub-test", idNegocio: 10, idTemplate: null, idNorma: null, idTipoPeriodicidad: 100, idCategoriaNorma: 200, idCargo: 300, activado: true)
			);
			tipoPeriodicidadBcp.ObtenerPorId(100, Arg.Any<NpgsqlTransaction?>()).Returns(
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 100, vigencia: true, cron: "MI HO DM * ? *", deltaDias: null, deltaMeses: 1, deltaAnnos: null)
			);
			tipoPeriodicidadBcp.EstaVigente(Arg.Any<TipoPeriodicidad>()).Returns(true);
			notificacionNormaSuscritaUseCase.ObtenerAntelacionesConsiderandoTemplate(999, null, null, Arg.Any<NpgsqlTransaction?>()).Returns([
				(TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000, cantSegundos: 3600, cantMinutos: 60, cantHoras: 1), 1)
			]);
			historialNormaSuscritaBcp.ObtenerProximoVencimiento(10, Arg.Any<NpgsqlTransaction?>()).Returns(FECHA_DUMMY.AddDays(14));
			notificacionNormaSuscritaUseCase.GenerarCrons(FECHA_DUMMY.AddDays(14), "MI HO DM * ? *", Arg.Any<List<(TipoUnidadTiempo, int)>>()).Returns([
				("0 11 15 * ? *", TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000, cantSegundos: 3600, cantMinutos: 60, cantHoras: 1), 1, false)
			]);
			
			(NormaSuscrita obligacion, List<SalKairosIngresarProceso> programados, List<NormaSuscritaProcesoNotificacion> desprogramados) retorno = await normaSuscritaUseCase.ActualizarNormaSuscrita(
				"sub-test",
				999,
				10,
				"nombre-template-test",
				"descripcion-template-test",
				"multa-template-test",
				100,
				200,
				300,
				true,
				FECHA_DUMMY.AddDays(14),
				[1000, 2000],
				[(5000, 1)]
			);

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.Received(1).Actualizar(Arg.Any<NormaSuscrita>(), Arg.Any<NpgsqlTransaction?>());
			await fiscalizadorNormaSuscritaBcp.Received(1).ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>());
			await notificacionNormaSuscritaBcp.Received(1).ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaUseCase.Received(1).EliminarPorNormaSuscrita(999, true, Arg.Any<NpgsqlTransaction>());
			await historialNormaSuscritaBcp.Received(1).Crear(999, FECHA_DUMMY.AddDays(14), Arg.Any<NpgsqlTransaction?>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ActualizarNormaSuscritaTest_SinPlanEmpresaConIdCargo() {
			normaSuscritaBcp.Obtener(999, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, sub: "sub-test", idNegocio: 10, idTemplate: null, idNorma: null, idTipoPeriodicidad: null, idCategoriaNorma: null, idCargo: null, activado: true)
			);
			fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(999, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			notificacionNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(999, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			suscripcionBcp.ConsultaTienePlanEmpresa("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(false);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => normaSuscritaUseCase.ActualizarNormaSuscrita(
				"sub-test",
				999,
				10,
				"otro-nombre-test",
				"otra-descripcion-test",
				"otra-multa-test",
				100,
				200,
				300,
				true,
				FECHA_DUMMY.AddDays(14),
				[1000, 2000],
				[(5000, 1)]
			));
			Assert.Equal(TipoErrorValidacion.RestringidoPorPlan, ex.TipoErrorValidacion);

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.DidNotReceive().Actualizar(Arg.Any<NormaSuscrita>(), Arg.Any<NpgsqlTransaction?>());
			await fiscalizadorNormaSuscritaBcp.DidNotReceive().ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>());
			await notificacionNormaSuscritaBcp.DidNotReceive().ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaUseCase.DidNotReceive().EliminarPorNormaSuscrita(999, true, Arg.Any<NpgsqlTransaction>());
			await historialNormaSuscritaBcp.DidNotReceive().Crear(999, FECHA_DUMMY.AddDays(14), Arg.Any<NpgsqlTransaction?>());
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ActualizarNormaSuscritaTest_CantAntelacionNegativa() {
			normaSuscritaBcp.Obtener(999, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, sub: "sub-test", idNegocio: 10, idTemplate: null, idNorma: null, idTipoPeriodicidad: null, idCategoriaNorma: null, idCargo: null, activado: true)
			);
			fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(999, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			notificacionNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(999, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			suscripcionBcp.ConsultaTienePlanEmpresa("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(true);
			tipoFiscalizadorBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 1000),
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 2000)
			]);
			tipoUnidadTiempoBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000)
			]);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => normaSuscritaUseCase.ActualizarNormaSuscrita(
				"sub-test",
				999,
				10,
				"otro-nombre-test",
				"otra-descripcion-test",
				"otra-multa-test",
				100,
				200,
				300,
				true,
				FECHA_DUMMY.AddDays(14),
				[1000, 2000],
				[(5000, -1)]
			));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.DidNotReceive().Actualizar(Arg.Any<NormaSuscrita>(), Arg.Any<NpgsqlTransaction?>());
			await fiscalizadorNormaSuscritaBcp.DidNotReceive().ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>());
			await notificacionNormaSuscritaBcp.DidNotReceive().ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaUseCase.DidNotReceive().EliminarPorNormaSuscrita(999, true, Arg.Any<NpgsqlTransaction>());
			await historialNormaSuscritaBcp.DidNotReceive().Crear(999, FECHA_DUMMY.AddDays(14), Arg.Any<NpgsqlTransaction?>());
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ActualizarNormaSuscritaTest_SinProximoVencimiento() {
			normaSuscritaBcp.Obtener(999, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, sub: "sub-test", idNegocio: 10, idTemplate: null, idNorma: null, idTipoPeriodicidad: null, idCategoriaNorma: null, idCargo: null, activado: true)
			);
			fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(999, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			notificacionNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(999, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			suscripcionBcp.ConsultaTienePlanEmpresa("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(true);
			tipoFiscalizadorBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 1000),
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 2000)
			]);
			tipoUnidadTiempoBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000)
			]);
			historialNormaSuscritaBcp.ObtenerPorNormaSuscrita(999, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 999, fechaVencimiento: FECHA_DUMMY)
			]);
			historialNormaSuscritaBcp.FiltrarUltimoVencimiento(Arg.Any<List<HistorialNormaSuscrita>>()).Returns(
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 999, fechaVencimiento: FECHA_DUMMY)
			);
			tipoPeriodicidadBcp.ObtenerValidandoVigencia(id: 100, Arg.Any<NpgsqlTransaction?>()).Returns(TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 100));
			categoriaNormaBcp.ObtenerValidandoVigencia(id: 200, Arg.Any<NpgsqlTransaction?>()).Returns(CategoriaNormaBcpTest.CategoriaNormaDummy(id: 200));
			cargoBcp.Obtener(300, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				CargoBcpTest.CargoDummy(id: 300, sub: "sub-test", idNegocio: 10)
			);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => normaSuscritaUseCase.ActualizarNormaSuscrita(
				"sub-test",
				999,
				10,
				"otro-nombre-test",
				"otra-descripcion-test",
				"otra-multa-test",
				100,
				200,
				300,
				true,
				null,
				[1000, 2000],
				[(5000, 1)]
			));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.DidNotReceive().Actualizar(Arg.Any<NormaSuscrita>(), Arg.Any<NpgsqlTransaction?>());
			await fiscalizadorNormaSuscritaBcp.DidNotReceive().ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>());
			await notificacionNormaSuscritaBcp.DidNotReceive().ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaUseCase.DidNotReceive().EliminarPorNormaSuscrita(999, true, Arg.Any<NpgsqlTransaction>());
			await historialNormaSuscritaBcp.DidNotReceive().Crear(999, FECHA_DUMMY.AddDays(14), Arg.Any<NpgsqlTransaction?>());
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ActualizarNormaSuscritaTest_ProximoVencimientoPasado() {
			normaSuscritaBcp.Obtener(999, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, sub: "sub-test", idNegocio: 10, idTemplate: null, idNorma: null, idTipoPeriodicidad: null, idCategoriaNorma: null, idCargo: null, activado: true)
			);
			fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(999, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			notificacionNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(999, Arg.Any<NpgsqlTransaction?>()).Returns([]);
			suscripcionBcp.ConsultaTienePlanEmpresa("sub-test", Arg.Any<NpgsqlTransaction?>()).Returns(true);
			tipoFiscalizadorBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 1000),
				TipoFiscalizadorBcpTest.TipoFiscalizadorDummy(id: 2000)
			]);
			tipoUnidadTiempoBcp.ValidarTodosVigentes(Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>()).Returns([
				TipoUnidadTiempoBcpTest.TipoUnidadTiempoDummy(id: 5000)
			]);
			historialNormaSuscritaBcp.ObtenerPorNormaSuscrita(999, filtrarVigente: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns([
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 999, fechaVencimiento: FECHA_DUMMY)
			]);
			historialNormaSuscritaBcp.FiltrarUltimoVencimiento(Arg.Any<List<HistorialNormaSuscrita>>()).Returns(
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100_000, idNormaSuscrita: 999, fechaVencimiento: FECHA_DUMMY)
			);
			tipoPeriodicidadBcp.ObtenerValidandoVigencia(id: 100, Arg.Any<NpgsqlTransaction?>()).Returns(TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 100));
			categoriaNormaBcp.ObtenerValidandoVigencia(id: 200, Arg.Any<NpgsqlTransaction?>()).Returns(CategoriaNormaBcpTest.CategoriaNormaDummy(id: 200));
			cargoBcp.Obtener(300, validarVigencia: true, validarSub: "sub-test", validarIdNegocio: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				CargoBcpTest.CargoDummy(id: 300, sub: "sub-test", idNegocio: 10)
			);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => normaSuscritaUseCase.ActualizarNormaSuscrita(
				"sub-test",
				999,
				10,
				"otro-nombre-test",
				"otra-descripcion-test",
				"otra-multa-test",
				100,
				200,
				300,
				true,
				FECHA_DUMMY.AddDays(-1),
				[1000, 2000],
				[(5000, 1)]
			));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.DidNotReceive().Actualizar(Arg.Any<NormaSuscrita>(), Arg.Any<NpgsqlTransaction?>());
			await fiscalizadorNormaSuscritaBcp.DidNotReceive().ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<long>>(), Arg.Any<NpgsqlTransaction?>());
			await notificacionNormaSuscritaBcp.DidNotReceive().ActualizarPorNormaSuscrita(999, Arg.Any<HashSet<(long, int)>>(), Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaUseCase.DidNotReceive().EliminarPorNormaSuscrita(999, true, Arg.Any<NpgsqlTransaction>());
			await historialNormaSuscritaBcp.DidNotReceive().Crear(999, FECHA_DUMMY.AddDays(14), Arg.Any<NpgsqlTransaction?>());
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task CompletarNormaValidandoPertenenciaTest() {
			normaSuscritaBcp.Obtener(10, validarVigencia: true, validarSub: "sub-test", transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 10)
			);
			historialNormaSuscritaBcp.Obtener(100, validarVigencia: true, validarIdNormaSuscrita: 10, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100, idNormaSuscrita: 10)
			);
			historialNormaSuscritaUseCase.CompletarHistorialNormaSuscrita(Arg.Is<HistorialNormaSuscrita>(v => v.Id == 100), Arg.Any<NpgsqlTransaction>()).Returns(
				FECHA_DUMMY
			);

			HistorialNormaSuscrita retorno = await normaSuscritaUseCase.CompletarNormaValidandoPertenencia("sub-test", 10, 100);
			Assert.Equal(FECHA_DUMMY, retorno.FechaCompletitud);
			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.Received(1).Obtener(10, validarVigencia: true, validarSub: "sub-test", transaction: Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaBcp.Received(1).Obtener(100, validarVigencia: true, validarIdNormaSuscrita: 10, transaction: Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaUseCase.Received(1).CompletarHistorialNormaSuscrita(Arg.Is<HistorialNormaSuscrita>(v => v.Id == 100), Arg.Any<NpgsqlTransaction>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task CompletarNormaValidandoPertenenciaTest_Rollback() {
			normaSuscritaBcp.Obtener(10, validarVigencia: true, validarSub: "sub-test", transaction: Arg.Any<NpgsqlTransaction?>()).ThrowsAsync<Exception>();

			await Assert.ThrowsAsync<Exception>(() => normaSuscritaUseCase.CompletarNormaValidandoPertenencia("sub-test", 10, 100));

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.Received(1).Obtener(10, validarVigencia: true, validarSub: "sub-test", transaction: Arg.Any<NpgsqlTransaction?>());
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task CompletarNormaPorCodigoTest() {
			historialNotificacionBcp.ObtenerPorCodigoAccesoValidandoVigencia("codigo-acceso-test", Arg.Any<NpgsqlTransaction?>()).Returns(
				HistorialNotificacionBcpTest.HistorialNotificacionDummy(id: 1000, idHistorialNormaSuscrita: 100, idDestinatarioNotificacion: 500, codigoAcceso: "hash-codigo-acceso-test")
			);
			historialNormaSuscritaBcp.Obtener(100, validarVigencia: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 100, idNormaSuscrita: 10)
			);
			normaSuscritaBcp.Obtener(10, validarVigencia: true, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 10)
			);
			historialNormaSuscritaUseCase.CompletarHistorialNormaSuscrita(Arg.Is<HistorialNormaSuscrita>(v => v.Id == 100), Arg.Any<NpgsqlTransaction>()).Returns(
				FECHA_DUMMY
			);
			
			HistorialNormaSuscrita retorno = await normaSuscritaUseCase.CompletarNormaPorCodigoAcceso("codigo-acceso-test");
			Assert.Equal(FECHA_DUMMY, retorno.FechaCompletitud);
			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await historialNotificacionBcp.Received(1).ObtenerPorCodigoAccesoValidandoVigencia("codigo-acceso-test", Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaBcp.Received(1).Obtener(100, validarVigencia: true, transaction: Arg.Any<NpgsqlTransaction?>());
			await normaSuscritaBcp.Received(1).Obtener(10, validarVigencia: true, transaction: Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaUseCase.Received(1).CompletarHistorialNormaSuscrita(Arg.Is<HistorialNormaSuscrita>(v => v.Id == 100), Arg.Any<NpgsqlTransaction>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task CompletarNormaPorCodigoTest_Rollback() {
			historialNotificacionBcp.ObtenerPorCodigoAccesoValidandoVigencia("codigo-acceso-test", Arg.Any<NpgsqlTransaction?>()).ThrowsAsync<Exception>();

			await Assert.ThrowsAsync<Exception>(() => normaSuscritaUseCase.CompletarNormaPorCodigoAcceso("codigo-acceso-test"));

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await historialNotificacionBcp.Received(1).ObtenerPorCodigoAccesoValidandoVigencia("codigo-acceso-test", Arg.Any<NpgsqlTransaction?>());
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task DesactivarNormaSuscritaTest() {
			normaSuscritaBcp.Obtener(999, validarVigencia: true, validarSub: "sub-test", transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, idTemplate: null, idNorma: null, activado: true)
			);
			normaSuscritaBcp.EstaActiva(Arg.Any<NormaSuscrita>()).Returns(true);

			// Para ActualizarProgramacionProcesosNormaSuscrita
			normaSuscritaBcp.Obtener(999, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, sub: "sub-test", idTemplate: null, idNorma: null, activado: false)
			);
			
			(NormaSuscrita obligacion, List<SalKairosIngresarProceso> programados, List<NormaSuscritaProcesoNotificacion> desprogramados) retorno = await normaSuscritaUseCase.DesactivarNormaSuscrita(999, "sub-test");
			Assert.NotNull(retorno.obligacion.HistorialesNormaSuscrita);
			Assert.Empty(retorno.obligacion.HistorialesNormaSuscrita);
			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.Received(1).Desactivar(Arg.Any<NormaSuscrita>(), Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaUseCase.Received(1).EliminarPorNormaSuscrita(999, false, Arg.Any<NpgsqlTransaction>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task DesactivarNormaSuscritaTest_Rollback() {
			normaSuscritaBcp.Obtener(999, validarVigencia: true, validarSub: "sub-test", transaction: Arg.Any<NpgsqlTransaction?>()).ThrowsAsync<Exception>();

			await Assert.ThrowsAsync<Exception>(() => normaSuscritaUseCase.DesactivarNormaSuscrita(999, "sub-test"));

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ActivarNormaSuscritaTest() {
			normaSuscritaBcp.Obtener(999, validarVigencia: true, validarSub: "sub-test", transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, idTemplate: null, idNorma: null, idTipoPeriodicidad: 1, activado: false)
			);
			normaSuscritaBcp.EstaActiva(Arg.Any<NormaSuscrita>()).Returns(false);
			tipoPeriodicidadBcp.ObtenerValidandoVigencia(1, Arg.Any<NpgsqlTransaction?>()).Returns(
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 1, deltaDias: 1)	
			);
			historialNormaSuscritaBcp.Crear(999, FECHA_DUMMY.AddDays(14), Arg.Any<NpgsqlTransaction?>()).Returns(
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 10_000, idNormaSuscrita: 999, fechaVencimiento: FECHA_DUMMY.AddDays(14))	
			);

			// Para ActualizarProgramacionProcesosNormaSuscrita
			normaSuscritaBcp.Obtener(999, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, sub: "sub-test", idTemplate: null, idNorma: null, activado: true)
			);
			
			(NormaSuscrita obligacion, List<SalKairosIngresarProceso> programados, List<NormaSuscritaProcesoNotificacion> desprogramados) retorno = await normaSuscritaUseCase.ActivarNormaSuscrita(999, "sub-test", FECHA_DUMMY.AddDays(14));
			Assert.NotNull(retorno.obligacion.HistorialesNormaSuscrita);
			Assert.Single(retorno.obligacion.HistorialesNormaSuscrita);
			Assert.Equal(FECHA_DUMMY.AddDays(14), retorno.obligacion.HistorialesNormaSuscrita.FirstOrDefault()?.FechaVencimiento);
			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.Received(1).Activar(Arg.Any<NormaSuscrita>(), Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaBcp.Received(1).Crear(999, FECHA_DUMMY.AddDays(14), Arg.Any<NpgsqlTransaction>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ActivarNormaSuscritaTest_ProximoVencimientoPasado() {
			normaSuscritaBcp.Obtener(999, validarVigencia: true, validarSub: "sub-test", transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, idTemplate: null, idNorma: null, idTipoPeriodicidad: 1, activado: false)
			);
			normaSuscritaBcp.EstaActiva(Arg.Any<NormaSuscrita>()).Returns(false);
			tipoPeriodicidadBcp.ObtenerValidandoVigencia(1, Arg.Any<NpgsqlTransaction?>()).Returns(
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 1, deltaDias: 1)
			);
			historialNormaSuscritaUseCase.CalcularVencimientoFuturo(FECHA_DUMMY.AddDays(-1), Arg.Any<TipoPeriodicidad>()).Returns(FECHA_DUMMY.AddDays(14));
			historialNormaSuscritaBcp.Crear(999, FECHA_DUMMY.AddDays(14), Arg.Any<NpgsqlTransaction?>()).Returns(
				HistorialNormaSuscritaBcpTest.HistorialNormaSuscritaDummy(id: 10_000, idNormaSuscrita: 999, fechaVencimiento: FECHA_DUMMY.AddDays(14))
			);

			// Para ActualizarProgramacionProcesosNormaSuscrita
			normaSuscritaBcp.Obtener(999, transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, sub: "sub-test", idTemplate: null, idNorma: null, activado: true)
			);
			
			(NormaSuscrita obligacion, List<SalKairosIngresarProceso> programados, List<NormaSuscritaProcesoNotificacion> desprogramados) retorno = await normaSuscritaUseCase.ActivarNormaSuscrita(999, "sub-test", FECHA_DUMMY.AddDays(-1));
			Assert.NotNull(retorno.obligacion.HistorialesNormaSuscrita);
			Assert.Single(retorno.obligacion.HistorialesNormaSuscrita);
			Assert.Equal(FECHA_DUMMY.AddDays(14), retorno.obligacion.HistorialesNormaSuscrita.FirstOrDefault()?.FechaVencimiento);
			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await normaSuscritaBcp.Received(1).Activar(Arg.Any<NormaSuscrita>(), Arg.Any<NpgsqlTransaction?>());
			await historialNormaSuscritaBcp.Received(1).Crear(999, FECHA_DUMMY.AddDays(14), Arg.Any<NpgsqlTransaction>());
			await transaction.Received(1).CommitAsync();
			await transaction.DidNotReceive().RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ActivarNormaSuscritaTest_Rollback() {
			normaSuscritaBcp.Obtener(999, validarVigencia: true, validarSub: "sub-test", transaction: Arg.Any<NpgsqlTransaction?>()).ThrowsAsync<Exception>();

			await Assert.ThrowsAsync<Exception>(() => normaSuscritaUseCase.ActivarNormaSuscrita(999, "sub-test", FECHA_DUMMY.AddDays(14)));
			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}

		[Fact]
		public async Task ActivarNormaSuscritaTest_ProximoVencimientoPasadoSinDeltas() {
			normaSuscritaBcp.Obtener(999, validarVigencia: true, validarSub: "sub-test", transaction: Arg.Any<NpgsqlTransaction?>()).Returns(
				NormaSuscritaBcpTest.NormaSuscritaDummy(id: 999, idTemplate: null, idNorma: null, idTipoPeriodicidad: 1, activado: false)
			);
			normaSuscritaBcp.EstaActiva(Arg.Any<NormaSuscrita>()).Returns(false);
			tipoPeriodicidadBcp.ObtenerValidandoVigencia(1, Arg.Any<NpgsqlTransaction?>()).Returns(
				TipoPeriodicidadBcpTest.TipoPeriodicidadDummy(id: 1, deltaDias: null, deltaMeses: null, deltaAnnos: null)
			);

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => normaSuscritaUseCase.ActivarNormaSuscrita(999, "sub-test", FECHA_DUMMY.AddDays(-1)));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);

			await connectionHelper.Received(1).ObtenerConexionWrapper();
			await connection.Received(1).BeginTransactionAsync();
			await transaction.DidNotReceive().CommitAsync();
			await transaction.Received(1).RollbackAsync();
			await transaction.Received(1).DisposeAsync();
			await connection.Received(1).DisposeAsync();
		}
	}
}

