using Cronos;
using Npgsql;
using Scriban.Runtime;
using System.Net;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Hermes;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.UseCases {
	public class NotificacionUseCase(IDateTimeProvider dateTimeProvider, IVariableEntornoHelper variableEntornoHelper, HistorialNormaSuscritaUseCase historialNormaSuscritaUseCase, DestinatarioNotificacionUseCase destinatarioNotificacionUseCase, IHtmlRenderer renderer, IHermesHelper hermesHelper, IUsuarioBcp usuarioBcp, IDestinatarioNotificacionBcp destinatarioNotificacionBcp, ISuscripcionBcp suscripcionBcp, INormaSuscritaBcp normaSuscritaBcp, ITipoUnidadTiempoBcp tipoUnidadTiempoBcp, IHistorialNormaSuscritaBcp historialNormaSuscritaBcp, IHistorialNotificacionDao historialNotificacionDao, ITemplateNormaDao templateNormaDao, IDestinatarioNotificacionDao destinatarioNotificacionDao, ICargoDao cargoDao, IEmpleadoDao empleadoDao) {
		private const int DIAS_CADUCIDAD_CODIGO_ACCESO = 30;

		public async Task ProcesarNotificacion(long idNormaSuscrita, string cron, long? idTipoUnidadTiempoAntelacion, int? cantAntelacion, bool? esVencimiento, bool programarSiguienteEjecucion, NpgsqlTransaction? transaction = null) {
			esVencimiento ??= programarSiguienteEjecucion;

			// Se obtiene norma suscrita y/o template...
			NormaSuscrita normaSuscrita = await normaSuscritaBcp.ObtenerPorId(idNormaSuscrita, transaction) ?? throw new InvalidOperationException("ID norma suscrita inválida");
			TemplateNorma? templateNorma = null;
			if (normaSuscrita.IdTemplate != null && normaSuscrita.IdNorma != null) {
				templateNorma = (await templateNormaDao.ObtenerPorTemplate(normaSuscrita.IdTemplate.Value, transaction)).FirstOrDefault(n => n.IdNorma == normaSuscrita.IdNorma);
			}

			// Se obtienen destinatarios vigentes...
			List<DestinatarioNotificacion> destinatariosVigentes = await destinatarioNotificacionDao.ObtenerPorSub(normaSuscrita.Sub, normaSuscrita.IdNegocio, true, transaction);
			List<DestinatarioNotificacion> destinatariosValidados = [.. destinatariosVigentes.Where(dn => dn.Validado)];

			// Se valida que exista un destinatario correspondiente a la cuenta del usuario...
			Usuario usuario = await usuarioBcp.ObtenerInformacionUsuario(normaSuscrita.Sub, transaction);
			if (usuario.CorreoElectronico != null && !destinatariosValidados.Any(d => d.IdEmpleado == null && d.IdTipoReceptor == 1 /* Correo electrónico */ && d.Destino == usuario.CorreoElectronico)) {
				(DestinatarioNotificacion nuevoDestinatario, _) = await destinatarioNotificacionBcp.Insertar(
					normaSuscrita.Sub,
					normaSuscrita.IdNegocio,
					null,
					1, // Correo electrónico
					"Mi correo electrónico",
					usuario.CorreoElectronico,
					true,
					transaction
				);
				destinatariosValidados.Add(nuevoDestinatario);
			}

			// Se sobreescribe el cargo responsable si el usuario no tiene plan empresa o si el cargo no está vigente...
			long? idCargoResponsable = normaSuscrita.IdCargo;

			bool tienePlanEmpresa = await suscripcionBcp.ConsultaTienePlanEmpresa(normaSuscrita.Sub, transaction);
			if (!tienePlanEmpresa) idCargoResponsable = null;

			if (idCargoResponsable != null) {
				Cargo? cargo = (await cargoDao.ObtenerPorSub(normaSuscrita.Sub, normaSuscrita.IdNegocio, true, transaction)).FirstOrDefault(c => c.Id == idCargoResponsable);
				if (cargo == null || !cargo.Vigencia) {
					idCargoResponsable = null;
				}
			}

			// Se filtra lista de destinatario habilitados según cargo responsable de la obligación...
			if (idCargoResponsable == null) {
				// Si no tiene un cargo responsable, solo se dejan los destinatarios que no son de un empleado...
				destinatariosValidados = [.. destinatariosValidados.Where(d => d.IdEmpleado == null)];
			} else {
				// Si tiene un cargo responsable, solo se dejan los destinatarios que posean dicho cargo...
				List<Empleado> empleadosResponsables = [.. (await empleadoDao.ObtenerPorSub(normaSuscrita.Sub, normaSuscrita.IdNegocio, true, transaction)).Where(e => e.IdCargo == idCargoResponsable)];
				List<DestinatarioNotificacion> destinatariosEmpleadosResponsables = [.. destinatariosValidados.Where(d => empleadosResponsables.Any(e => e.Id == d.IdEmpleado))];
				if (destinatariosEmpleadosResponsables.Count == 0) {
					// Si no tengo empleados responsables, se dejan los destinatarios que no son de un empleado...
					destinatariosValidados = [.. destinatariosValidados.Where(d => d.IdEmpleado == null)];
				} else {
					destinatariosValidados = destinatariosEmpleadosResponsables;
				}
			}

			// Se obtienen los tipos de unidades de tiempo...
			List<TipoUnidadTiempo> tiposUnidadesTiempo = await tipoUnidadTiempoBcp.ObtenerPorVigencia(true, transaction);
			TipoUnidadTiempo? unidadTiempo = tiposUnidadesTiempo.FirstOrDefault(ut => ut.Id == idTipoUnidadTiempoAntelacion);

			TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("America/Santiago");

			// Se calcula la fecha a la que corresponde la ejecución actual, según la ocurrencia del cron más cercana...
			CronExpression cronExpression = CronExpression.Parse(CronHelper.TransformarCronAWSAStandard(cron));
			DateTime utcNow = dateTimeProvider.UtcNow;
			DateTime? siguienteUTC = cronExpression.GetNextOccurrence(utcNow, timeZoneInfo);
			DateTime? anteriorUTC = cronExpression.GetPreviousOccurrence(utcNow, timeZoneInfo, true);
			DateTime masCercanaUTC = (siguienteUTC, anteriorUTC) switch {
				(null, null) => throw new InvalidOperationException($"El cron '{cron}' no tiene ocurrencias válidas."),
				(null, _) => anteriorUTC!.Value,
				(_, null) => siguienteUTC!.Value,
				_ => (siguienteUTC!.Value - utcNow) <= (utcNow - anteriorUTC!.Value) ? siguienteUTC!.Value : anteriorUTC!.Value
			};

			// Se calcula el vencimiento al que corresponde la ejecución actual...
			HistorialNormaSuscrita? vencimiento = null;
			if (cantAntelacion != null && idTipoUnidadTiempoAntelacion != null && unidadTiempo != null) {
				// Si tenemos información de la notificación previa, se calculca la fecha de vencimiento...
				DateTime masCercanaChile = DateTimeHelper.TransformarFechaUTCATimezone(masCercanaUTC);
				DateTime fechaVencimientoChile = NotificacionPreviaHelper.ObtenerFechaReferenciaChileSegunNotificacionPrevia(masCercanaChile, cantAntelacion.Value, unidadTiempo);
				DateTime fechaVencimientoUTC = DateTimeHelper.TransformarFechaTimezoneAUTC(fechaVencimientoChile);

				vencimiento = (await historialNormaSuscritaBcp.ObtenerVigentesPorNormaSuscritaNoCompletadas(idNormaSuscrita, transaction)).FirstOrDefault(v => v.FechaVencimiento == fechaVencimientoUTC);
			} else if (!esVencimiento.Value) {
				// Si no estamos en una fecha de vencimiento, pero tampoco tenemos información de la notificación previa, se asume último vencimiento...
				vencimiento = (await historialNormaSuscritaBcp.ObtenerVigentesPorNormaSuscritaNoCompletadas(idNormaSuscrita, transaction)).OrderByDescending(v => v.FechaVencimiento).FirstOrDefault();

			} else {
				// Si estamos en una fecha de vencimiento, se busca el vencimiento que coincide con la fecha del cron...
				vencimiento = (await historialNormaSuscritaBcp.ObtenerVigentesPorNormaSuscritaNoCompletadas(idNormaSuscrita, transaction)).FirstOrDefault(v => v.FechaVencimiento == masCercanaUTC);
			}

			if (vencimiento != null) {
				// Se definen textos a incluirse en la notificación...
				string? tiempoFaltante = null;
				string? deLosProximos = null;
				if (cantAntelacion != null && idTipoUnidadTiempoAntelacion != null && unidadTiempo != null) {
					if (cantAntelacion > 1) tiempoFaltante = $"{cantAntelacion} {unidadTiempo.NombrePlural?.ToLower()}";
					else tiempoFaltante = $"{cantAntelacion} {unidadTiempo.Nombre.ToLower()}";

					if (cantAntelacion > 1) {
						if (idTipoUnidadTiempoAntelacion == 1 || idTipoUnidadTiempoAntelacion == 3) {
							deLosProximos = $"de los próximos {cantAntelacion} {unidadTiempo.NombrePlural?.ToLower()}";
						} else {
							deLosProximos = $"de las próximas {cantAntelacion} {unidadTiempo.NombrePlural?.ToLower()}";
						}
					} else {
						if (idTipoUnidadTiempoAntelacion == 1) {
							deLosProximos = $"del próximo {unidadTiempo.Nombre.ToLower()}";
						} else if (idTipoUnidadTiempoAntelacion == 3) {
							deLosProximos = $"de mañana";
						} else {
							deLosProximos = $"de la próxima {unidadTiempo.Nombre.ToLower()}";
						}
					}
				} else if (!esVencimiento.Value) {
					tiempoFaltante = "poco tiempo";
					deLosProximos = $"del {vencimiento.FechaVencimiento:dd 'de' MMMM}";
				}

				// Se procesan las notificaciones de todos los destinatarios validados...
				foreach (DestinatarioNotificacion destinatario in destinatariosValidados) {
					HistorialNotificacion historialNotificacion = new() {
						Id = 0,
						IdHistorialNormaSuscrita = vencimiento.Id,
						IdDestinatarioNotificacion = destinatario.Id,
						IdTipoUnidadTiempoAntelacion = idTipoUnidadTiempoAntelacion,
						CantAntelacion = cantAntelacion,
						FechaProgramacion = masCercanaUTC,
						FechaCreacion = dateTimeProvider.UtcNow,
						Vigencia = true
					};
					historialNotificacion.Id = await historialNotificacionDao.Insertar(historialNotificacion, transaction);

					// Se valida que según suscripción el destinatario esté habilitado, si no lo esta entonces no se manda la notificación...
					if (!await destinatarioNotificacionUseCase.DestinatarioHabilitado(normaSuscrita.Sub, normaSuscrita.IdNegocio, destinatario.Id, transaction)) {
						historialNotificacion.FechaEjecucion = dateTimeProvider.UtcNow;
						historialNotificacion.Estado = 2; // Omitido
						historialNotificacion.Observacion = "El destinatario no está habilitado según la suscripción del usuario.";
						await historialNotificacionDao.Actualizar(historialNotificacion, transaction);

						continue;
					}

					// Se valida que la unidad de tiempo este vigente, solo si viene como entrada...
					if (idTipoUnidadTiempoAntelacion != null && unidadTiempo == null) {
						historialNotificacion.FechaEjecucion = dateTimeProvider.UtcNow;
						historialNotificacion.Estado = 2; // Omitido
						historialNotificacion.Observacion = "El tipo de unidad de tiempo no está vigente.";
						await historialNotificacionDao.Actualizar(historialNotificacion, transaction);

						continue;
					}

					// Se genera código de acceso para notificación...
					string codigoAcceso = CryptoHelper.GenerarToken();
					HistorialNotificacion? mismoCodigo = await historialNotificacionDao.ObtenerPorCodigoAcceso(CryptoHelper.HashSHA256(codigoAcceso), true, transaction);
					while (mismoCodigo != null) {
						codigoAcceso = CryptoHelper.GenerarToken();
						mismoCodigo = await historialNotificacionDao.ObtenerPorCodigoAcceso(CryptoHelper.HashSHA256(codigoAcceso), true, transaction);
					}

					// Si el destinatario es email, se manda correo electrónico...
					if (destinatario.IdTipoReceptor == 1) {
						string asunto;
						string cuerpoCorreo;
						if (!esVencimiento.Value) {
							asunto = $"¡Tu obligación vence en {tiempoFaltante ?? ""}!";
							cuerpoCorreo = await renderer.GenerarHtml("NotificacionPrevia.html", new ScriptObject() {
								["NOMBRE_NORMA"] = WebUtility.HtmlEncode(normaSuscrita.Nombre ?? templateNorma?.Nombre ?? "Sin nombre registrado"),
								["MULTA_NORMA"] = WebUtility.HtmlEncode(normaSuscrita.Multa ?? templateNorma?.Multa ?? "Sin multa registrada"),
								["CODIGO_ACCESO"] = Uri.EscapeDataString(codigoAcceso),
								["TIEMPO_FALTANTE"] = WebUtility.HtmlEncode(tiempoFaltante ?? ""),
								["DE_LOS_PROXIMOS"] = WebUtility.HtmlEncode(deLosProximos ?? ""),
							});
						} else {
							asunto = "¡Tu obligación venció!";
							cuerpoCorreo = await renderer.GenerarHtml("NormaVencida.html", new ScriptObject() {
								["NOMBRE_NORMA"] = WebUtility.HtmlEncode(normaSuscrita.Nombre ?? templateNorma?.Nombre ?? "Sin nombre registrado"),
								["MULTA_NORMA"] = WebUtility.HtmlEncode(normaSuscrita.Multa ?? templateNorma?.Multa ?? "Sin multa registrada"),
								["CODIGO_ACCESO"] = Uri.EscapeDataString(codigoAcceso),
							});
						}

						SalHermesEnviar response = await hermesHelper.EnviarCorreo(new EntHermesCorreoEnviar() {
							De = new DireccionCorreo() {
								Nombre = variableEntornoHelper.Obtener("HERMES_DE_NOMBRE"),
								Correo = variableEntornoHelper.Obtener("HERMES_DE_CORREO"),
							},
							Para = [
								new DireccionCorreo() {
									Correo = destinatario.Destino
								}
							],
							Asunto = asunto,
							Cuerpo = cuerpoCorreo
						});

						historialNotificacion.FechaEjecucion = dateTimeProvider.UtcNow;
						historialNotificacion.Estado = 1; // Enviado
						historialNotificacion.CodigoAcceso = CryptoHelper.HashSHA256(codigoAcceso);
						historialNotificacion.FechaCaducidadCodigoAcceso = dateTimeProvider.UtcNow.AddDays(DIAS_CADUCIDAD_CODIGO_ACCESO);
						historialNotificacion.HermesIdMensaje = response.IdMensaje;
						await historialNotificacionDao.Actualizar(historialNotificacion, transaction);

						// Si el destinatario es Whatsapp, se manda mensaje de Whatsapp...
					} else if (destinatario.IdTipoReceptor == 2) {
						string nombreTemplate;
						string[]? parametrosTitulo;
						string[]? parametrosCuerpo;
						if (!esVencimiento.Value) {
							nombreTemplate = "notificacion_previa";
							parametrosTitulo = [
								tiempoFaltante!
							];
							parametrosCuerpo = [
								normaSuscrita.Nombre ?? templateNorma?.Nombre ?? "Sin nombre registrado",
								deLosProximos!,
								normaSuscrita.Multa ?? templateNorma?.Multa ?? "Sin multa registrada"
							];
						} else {
							nombreTemplate = "norma_vencida";
							parametrosTitulo = null;
							parametrosCuerpo = [
								normaSuscrita.Nombre ?? templateNorma?.Nombre ?? "Sin nombre registrado",
								normaSuscrita.Multa ?? templateNorma?.Multa ?? "Sin multa registrada"
							];
						}

						SalHermesEnviar response = await hermesHelper.EnviarWhatsapp(new EntHermesWhatsappEnviar() {
							De = variableEntornoHelper.Obtener("HERMES_DE_WHATSAPP"),
							Para = destinatario.Destino,
							NombreTemplate = nombreTemplate,
							ParametrosTitulo = parametrosTitulo,
							ParametrosCuerpo = parametrosCuerpo,
							ParametrosBoton = [Uri.EscapeDataString(codigoAcceso)]
						});

						historialNotificacion.FechaEjecucion = dateTimeProvider.UtcNow;
						historialNotificacion.Estado = 1; // Enviado
						historialNotificacion.CodigoAcceso = CryptoHelper.HashSHA256(codigoAcceso);
						historialNotificacion.FechaCaducidadCodigoAcceso = dateTimeProvider.UtcNow.AddDays(DIAS_CADUCIDAD_CODIGO_ACCESO);
						historialNotificacion.HermesIdMensaje = response.IdMensaje;
						await historialNotificacionDao.Actualizar(historialNotificacion, transaction);

						// En cualquier otro caso, se omite la notificación por falta de implementación...
					} else {
						historialNotificacion.FechaEjecucion = dateTimeProvider.UtcNow;
						historialNotificacion.Estado = 2; // Omitido
						historialNotificacion.Observacion = "El tipo de receptor asociado al destinatario no tiene lógica de notificación implementada.";
						await historialNotificacionDao.Actualizar(historialNotificacion, transaction);
					}
				}

				if (programarSiguienteEjecucion) {
					await historialNormaSuscritaUseCase.ProgramarSiguienteVencimiento(vencimiento, transaction);
				}
			}
		}
	}
}
