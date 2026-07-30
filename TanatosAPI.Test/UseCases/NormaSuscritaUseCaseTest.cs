using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Business;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.UseCases;

namespace TanatosAPI.Test.UseCases {
    public class NormaSuscritaUseCaseTest {
        private readonly IDatabaseConnectionHelper connectionHelper = Substitute.For<IDatabaseConnectionHelper>();
        private readonly IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        private readonly HistorialNormaSuscritaUseCase historialNormaSuscritaUseCase = Substitute.For<HistorialNormaSuscritaUseCase>();
        private readonly NotificacionNormaSuscritaUseCase notificacionNormaSuscritaUseCase = Substitute.For<NotificacionNormaSuscritaUseCase>();
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

        private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

        public NormaSuscritaUseCaseTest() {
            dateTimeProvider.UtcNow.Returns(FECHA_DUMMY);

            connection.BeginTransactionAsync().Returns(transaction);
            connectionHelper.ObtenerConexionWrapper().Returns(connection);

            normaSuscritaUseCase = new(
                connectionHelper, dateTimeProvider, historialNormaSuscritaUseCase, notificacionNormaSuscritaUseCase,
                normaSuscritaBcp, historialNormaSuscritaBcp, historialNotificacionBcp, fiscalizadorNormaSuscritaBcp,
                notificacionNormaSuscritaBcp, templateBcp, templateNormaBcp, templateNormaNotificacionBcp, 
                templateNormaFiscalizadorBcp, tipoPeriodicidadBcp, categoriaNormaBcp, tipoFiscalizadorBcp, 
                tipoUnidadTiempoBcp, cargoBcp, negocioBcp, suscripcionBcp, documentoAdjuntoBcp
            );
        }
    }
}
