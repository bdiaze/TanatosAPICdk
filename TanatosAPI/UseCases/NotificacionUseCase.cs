using Amazon.Lambda.Core;
using Cronos;
using Npgsql;
using Scriban.Runtime;
using System.Diagnostics;
using System.Net;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Hermes;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Interfaces.UseCases;
using TanatosAPI.Repositories;

namespace TanatosAPI.UseCases {
	public class NotificacionUseCase(IDateTimeProvider dateTimeProvider, NormaSuscritaUseCase normaSuscritaUseCase, IHistorialNormaSuscritaUseCase historialNormaSuscritaUseCase, DestinatarioNotificacionUseCase destinatarioNotificacionUseCase, ITipoUnidadTiempoBcp tipoUnidadTiempoBcp, IHistorialNormaSuscritaBcp historialNormaSuscritaBcp, IHistorialNotificacionBcp historialNotificacionBcp) {
		public async Task<(HistorialNormaSuscrita?, DateTime fechaProgramacionNotificacion)> DeterminarVencimientoAsociadoNotificacion(long idNormaSuscrita, TipoUnidadTiempo? unidadAntelacion, int? cantAntelacion, bool esVencimiento, string? cron, int? frecuenciaDias, DateTime? inicioEjecucionUtc, NpgsqlTransaction? transaction = null) {
			DateTime masCercanaUTC;
			if (cron != null) {
				(_, _, masCercanaUTC) = CronHelper.ObtenerOcurrenciasCronAWS(cron, dateTimeProvider.UtcNow);
			} else if (frecuenciaDias != null && inicioEjecucionUtc != null) {
				(_, _, masCercanaUTC) = FrecuenciaDiasHelper.ObtenerOcurrenciasFrecuenciaDias(frecuenciaDias.Value, inicioEjecucionUtc.Value, dateTimeProvider.UtcNow);
			} else {
				throw new InvalidOperationException("No se incluyen configuración de cron ni de frecuencia en días.");
			};
			
			List<HistorialNormaSuscrita> vencimientos = await historialNormaSuscritaBcp.ObtenerPorNormaSuscrita(idNormaSuscrita, filtrarVigente: true, filtrarNoCompletadas: true, transaction: transaction);
			HistorialNormaSuscrita? vencimiento;
			if (cantAntelacion != null && unidadAntelacion != null) {
				// Si tenemos información de la notificación previa, se calculca la fecha de vencimiento...
				DateTime fechaVencimientoChile = NotificacionPreviaHelper.ObtenerFechaReferenciaChileSegunNotificacionPrevia(
					DateTimeHelper.TransformarFechaUTCATimezone(masCercanaUTC), 
					cantAntelacion.Value, 
					unidadAntelacion
				);
				vencimiento = vencimientos.FirstOrDefault(v => v.FechaVencimiento == DateTimeHelper.TransformarFechaTimezoneAUTC(fechaVencimientoChile));
			} else if (!esVencimiento) {
				// Si no estamos en una fecha de vencimiento, pero tampoco tenemos información de la notificación previa, se asume último vencimiento...
				vencimiento = vencimientos.OrderByDescending(v => v.FechaVencimiento).FirstOrDefault();
			} else {
				// Si estamos en una fecha de vencimiento, se busca el vencimiento que coincide con la fecha de programación...
				vencimiento = vencimientos.FirstOrDefault(v => v.FechaVencimiento == masCercanaUTC);
			}

			return (vencimiento, masCercanaUTC);
		}

        public async Task ProcesarNotificacion(long idNormaSuscrita, string? cron, int? frecuenciaDias, DateTime? inicioEjecucionUtc, long? idTipoUnidadTiempoAntelacion, int? cantAntelacion, bool? esVencimiento, bool programarSiguienteEjecucion, NpgsqlTransaction? transaction = null) {
			esVencimiento ??= programarSiguienteEjecucion;

			// Se obtiene norma suscrita y/o template...
			NormaSuscrita normaSuscrita = (await normaSuscritaUseCase.Obtener(idNormaSuscrita, validarVigencia: true, incluirTemplate: true, transaction: transaction))!;

			// Se obtienen destinatarios asociados a norma suscrita...
			List<DestinatarioNotificacion> destinatariosValidados = await destinatarioNotificacionUseCase.ObtenerDestinatariosNormaSuscrita(normaSuscrita, transaction);

			// Se obtienen los tipos de unidades de tiempo...
			TipoUnidadTiempo? unidadTiempo = idTipoUnidadTiempoAntelacion == null ? null : await tipoUnidadTiempoBcp.Obtener(idTipoUnidadTiempoAntelacion.Value, filtrarVigente: true, transaction: transaction);

            // Se calcula el vencimiento al que corresponde la ejecución actual...
            (HistorialNormaSuscrita? vencimiento, DateTime fechaProgramacionNotificacion) = await DeterminarVencimientoAsociadoNotificacion(
				idNormaSuscrita, 
				unidadTiempo, 
				cantAntelacion, 
				esVencimiento.Value, 
				cron, 
				frecuenciaDias, 
				inicioEjecucionUtc, 
				transaction
			);

			if (vencimiento == null) {
				LambdaLogger.Log(
					$"No se logra identificar el vencimieto al que pertenece el proceso de notificación - " +
					$"ID Norma Suscrita: {idNormaSuscrita} - " +
					$"Cron: {cron} - " +
					$"Frecuencia en Días: {frecuenciaDias} - " +
					$"Inicio Ejecución UTC: {inicioEjecucionUtc:o}"
				);

				return;
			}

			// Se procesan las notificaciones de todos los destinatarios validados...
			foreach (DestinatarioNotificacion destinatario in destinatariosValidados) {
				(HistorialNotificacion historialNotificacion, string codigoAcceso) = await historialNotificacionBcp.Registrar(
					vencimiento.Id,
                    destinatario.Id,
                    idTipoUnidadTiempoAntelacion,
                    cantAntelacion,
                    fechaProgramacionNotificacion,
					transaction
                );

				// Se valida que según suscripción el destinatario esté habilitado, si no lo esta entonces no se manda la notificación...
				if (!await destinatarioNotificacionUseCase.DestinatarioHabilitado(normaSuscrita.Sub, normaSuscrita.IdNegocio, destinatario.Id, transaction)) {
					await historialNotificacionBcp.MarcarOmitido(historialNotificacion, "El destinatario no está habilitado según la suscripción del usuario.", transaction);
					continue;
				}

				// Se valida que la unidad de tiempo este vigente, solo si viene como entrada...
				if (idTipoUnidadTiempoAntelacion != null && unidadTiempo == null) {
                    await historialNotificacionBcp.MarcarOmitido(historialNotificacion, "El tipo de unidad de tiempo no está vigente.", transaction);
                    continue;
				}
										
				if (destinatario.IdTipoReceptor == 1) {
                    // Si el destinatario es email, se manda correo electrónico...
                    string hermesIdMensaje;
					if (!esVencimiento.Value) {
						hermesIdMensaje = await historialNotificacionBcp.EnviarCorreoNotificacionPrevia(
                            destinatario.Destino,
                            vencimiento.FechaVencimiento, 
                            unidadTiempo, 
                            cantAntelacion,
                            normaSuscrita.Nombre ?? normaSuscrita.TemplateNorma?.Nombre,
                            normaSuscrita.Multa ?? normaSuscrita.TemplateNorma?.Multa,
                            codigoAcceso
                        );
					} else {
						hermesIdMensaje = await historialNotificacionBcp.EnviarCorreoNotificacionVencido(
                            destinatario.Destino,
                            normaSuscrita.Nombre ?? normaSuscrita.TemplateNorma?.Nombre,
                            normaSuscrita.Multa ?? normaSuscrita.TemplateNorma?.Multa,
                            codigoAcceso
                        );
					}

					await historialNotificacionBcp.MarcarEnviado(historialNotificacion, hermesIdMensaje, transaction);
				} else if (destinatario.IdTipoReceptor == 2) {
                    // Si el destinatario es Whatsapp, se manda mensaje de Whatsapp...
                    string hermesIdMensaje;
					if (!esVencimiento.Value) {
                        hermesIdMensaje = await historialNotificacionBcp.EnviarWhatsappNotificacionPrevia(
                            destinatario.Destino,
                            vencimiento.FechaVencimiento, 
                            unidadTiempo, 
                            cantAntelacion,
                            normaSuscrita.Nombre ?? normaSuscrita.TemplateNorma?.Nombre,
                            normaSuscrita.Multa ?? normaSuscrita.TemplateNorma?.Multa,
                            codigoAcceso
                        );
					} else {
                        hermesIdMensaje = await historialNotificacionBcp.EnviarWhatsappNotificacionVencido(
                            destinatario.Destino,
                            normaSuscrita.Nombre ?? normaSuscrita.TemplateNorma?.Nombre,
                            normaSuscrita.Multa ?? normaSuscrita.TemplateNorma?.Multa,
                            codigoAcceso
                        );
                    }
                    await historialNotificacionBcp.MarcarEnviado(historialNotificacion, hermesIdMensaje, transaction);
				} else {
                    // En cualquier otro caso, se omite la notificación por falta de implementación...
                    await historialNotificacionBcp.MarcarOmitido(historialNotificacion, "El tipo de receptor asociado al destinatario no tiene lógica de notificación implementada.", transaction);
				}
			}

			if (programarSiguienteEjecucion) {
				await historialNormaSuscritaUseCase.ProgramarSiguienteVencimiento(vencimiento, transaction);
			}
		}
	}
}
