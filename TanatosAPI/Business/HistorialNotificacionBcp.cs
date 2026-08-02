using Microsoft.AspNetCore.Components.RenderTree;
using Npgsql;
using Scriban.Runtime;
using System.ComponentModel;
using System.Net;
using System.Transactions;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Hermes;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
    public class HistorialNotificacionBcp(IVariableEntornoHelper variableEntorno, IDateTimeProvider dateTimeProvider, IHermesHelper hermesHelper, IHtmlRenderer renderer, IHistorialNotificacionDao historialNotificacionDao) : IHistorialNotificacionBcp {
        public const short DIAS_CADUCIDAD_CODIGO_ACCESO = 30;

        public bool EstaVigente(HistorialNotificacion? historialNotificacion) {
            return historialNotificacion != null && historialNotificacion.Vigencia;
        }

        public bool CodigoAccesoVigente(HistorialNotificacion historialNotificacion) {
            return historialNotificacion.FechaCaducidadCodigoAcceso == null || historialNotificacion.FechaCaducidadCodigoAcceso >= dateTimeProvider.UtcNow;
        }

        public async Task<HistorialNotificacion?> ObtenerPorCodigoAcceso(string codigoAcceso, NpgsqlTransaction? transaction = null) {
            return await historialNotificacionDao.ObtenerPorCodigoAcceso(CryptoHelper.HashSHA256(codigoAcceso), null, transaction);
        }

        public async Task<HistorialNotificacion> ObtenerPorCodigoAccesoValidandoVigencia(string codigoAcceso, NpgsqlTransaction? transaction = null) {
            HistorialNotificacion? historialNotificacion = await ObtenerPorCodigoAcceso(codigoAcceso, transaction);
            if (!EstaVigente(historialNotificacion)) {
                throw new ErrorValidacion(TipoErrorValidacion.AccesoCaducado, "La notificación no está vigente", "El código de acceso es inválido.");
            }

            if (!CodigoAccesoVigente(historialNotificacion!)) {
                throw new ErrorValidacion(TipoErrorValidacion.AccesoCaducado, "El código de acceso ha caducado", "El código de acceso es inválido.");
            }

            return historialNotificacion!;
        }

        public async Task<string> GenerarCodigoAcceso(NpgsqlTransaction? transaction = null) {
            string codigoAcceso = CryptoHelper.GenerarToken();
            HistorialNotificacion? mismoCodigo = await ObtenerPorCodigoAcceso(codigoAcceso, transaction);
            while (mismoCodigo != null) {
                codigoAcceso = CryptoHelper.GenerarToken();
                mismoCodigo = await ObtenerPorCodigoAcceso(codigoAcceso, transaction);
            }
            return codigoAcceso;
        }

        public async Task<(HistorialNotificacion nuevaNotificacion, string codigoAcceso)> Registrar(long idHistorialNormaSuscrita, long idDestinatarioNotificacion, long? idTipoUnidadTiempoAntelacion, int? cantAntelacion, DateTime fechaProgramacion, NpgsqlTransaction? transaction = null) {
            string codigoAcceso = await GenerarCodigoAcceso(transaction);
            DateTime nowUtc = dateTimeProvider.UtcNow;

            HistorialNotificacion historialNotificacion = new() {
                Id = 0,
                IdHistorialNormaSuscrita = idHistorialNormaSuscrita,
                IdDestinatarioNotificacion = idDestinatarioNotificacion,
                IdTipoUnidadTiempoAntelacion = idTipoUnidadTiempoAntelacion,
                CantAntelacion = cantAntelacion,
                FechaProgramacion = fechaProgramacion,
                Estado = 0 /* Pendiente */,
                CodigoAcceso = CryptoHelper.HashSHA256(codigoAcceso),
                FechaCaducidadCodigoAcceso = nowUtc.AddDays(DIAS_CADUCIDAD_CODIGO_ACCESO),
                FechaCreacion = nowUtc,
                Vigencia = true
            };
            historialNotificacion.Id = await historialNotificacionDao.Insertar(historialNotificacion, transaction);
            return (historialNotificacion, codigoAcceso);
        }

        public async Task MarcarOmitido(HistorialNotificacion historialNotificacion, string observacion, NpgsqlTransaction? transaction = null) {
            if (historialNotificacion.Estado != 2 /* Omitido */) {
                historialNotificacion.FechaEjecucion = dateTimeProvider.UtcNow;
                historialNotificacion.Estado = 2; // Omitido
                historialNotificacion.Observacion = observacion;
                await historialNotificacionDao.Actualizar(historialNotificacion, transaction);
            }
        }

        public async Task MarcarEnviado(HistorialNotificacion historialNotificacion, string hermesIdMensaje, NpgsqlTransaction? transaction = null) {
            if (historialNotificacion.Estado != 1 /* Enviado */) {
                historialNotificacion.FechaEjecucion = dateTimeProvider.UtcNow;
                historialNotificacion.Estado = 1; // Enviado
                historialNotificacion.HermesIdMensaje = hermesIdMensaje;
                await historialNotificacionDao.Actualizar(historialNotificacion, transaction);
            }
        }

        public (string tiempoFaltante, string deLosProximos) DeterminarTextosNotificacionPrevia(DateTime fechaVencimiento, TipoUnidadTiempo? unidadTiempoAntelacion, int? cantAntelacion) {
            string tiempoFaltante;
            string deLosProximos;
            if (cantAntelacion != null && unidadTiempoAntelacion != null) {
                if (cantAntelacion > 1) tiempoFaltante = $"{cantAntelacion} {unidadTiempoAntelacion.NombrePlural?.ToLower()}";
                else tiempoFaltante = $"{cantAntelacion} {unidadTiempoAntelacion.Nombre.ToLower()}";

                if (cantAntelacion > 1) {
                    if (unidadTiempoAntelacion.Id == 1 || unidadTiempoAntelacion.Id == 3) {
                        deLosProximos = $"de los próximos {cantAntelacion} {unidadTiempoAntelacion.NombrePlural?.ToLower()}";
                    } else {
                        deLosProximos = $"de las próximas {cantAntelacion} {unidadTiempoAntelacion.NombrePlural?.ToLower()}";
                    }
                } else {
                    if (unidadTiempoAntelacion.Id == 1) {
                        deLosProximos = $"del próximo {unidadTiempoAntelacion.Nombre.ToLower()}";
                    } else if (unidadTiempoAntelacion.Id == 3) {
                        deLosProximos = $"de mañana";
                    } else {
                        deLosProximos = $"de la próxima {unidadTiempoAntelacion.Nombre.ToLower()}";
                    }
                }
            } else {
                tiempoFaltante = "poco tiempo";
                deLosProximos = $"del {fechaVencimiento:dd 'de' MMMM}";
            }
            return (tiempoFaltante, deLosProximos);
        }

        public async Task<string> EnviarCorreoNotificacionPrevia(string correoDestino, DateTime fechaVencimiento, TipoUnidadTiempo? unidadTiempoAntelacion, int? cantAntelacion, string? nombreNorma, string? multaNorma, string codigoAcceso) {
            (string tiempoFaltante, string deLosProximos) = DeterminarTextosNotificacionPrevia(fechaVencimiento, unidadTiempoAntelacion, cantAntelacion);

            SalHermesEnviar retorno = await hermesHelper.EnviarCorreo(new EntHermesCorreoEnviar() {
                De = new DireccionCorreo() {
                    Nombre = variableEntorno.Obtener("HERMES_DE_NOMBRE"),
                    Correo = variableEntorno.Obtener("HERMES_DE_CORREO"),
                },
                Para = [
                    new DireccionCorreo() {
                        Correo = correoDestino
                    }
                ],
                Asunto = $"¡Tu obligación vence en {tiempoFaltante ?? ""}!",
                Cuerpo = await renderer.GenerarHtml("NotificacionPrevia.html", new ScriptObject() {
                    ["NOMBRE_NORMA"] = WebUtility.HtmlEncode(nombreNorma ?? "Sin nombre registrado"),
                    ["MULTA_NORMA"] = WebUtility.HtmlEncode(multaNorma ?? "Sin multa registrada"),
                    ["CODIGO_ACCESO"] = Uri.EscapeDataString(codigoAcceso),
                    ["TIEMPO_FALTANTE"] = WebUtility.HtmlEncode(tiempoFaltante ?? ""),
                    ["DE_LOS_PROXIMOS"] = WebUtility.HtmlEncode(deLosProximos ?? ""),
                })
            });

            return retorno.IdMensaje;
        }

        public async Task<string> EnviarCorreoNotificacionVencido(string correoDestino, string? nombreNorma, string? multaNorma, string codigoAcceso) {
            SalHermesEnviar retorno = await hermesHelper.EnviarCorreo(new EntHermesCorreoEnviar() {
                De = new DireccionCorreo() {
                    Nombre = variableEntorno.Obtener("HERMES_DE_NOMBRE"),
                    Correo = variableEntorno.Obtener("HERMES_DE_CORREO"),
                },
                Para = [
                    new DireccionCorreo() {
                        Correo = correoDestino
                    }
                ],
                Asunto = "¡Tu obligación venció!",
                Cuerpo = await renderer.GenerarHtml("NormaVencida.html", new ScriptObject() {
                    ["NOMBRE_NORMA"] = WebUtility.HtmlEncode(nombreNorma ?? "Sin nombre registrado"),
                    ["MULTA_NORMA"] = WebUtility.HtmlEncode(multaNorma ?? "Sin multa registrada"),
                    ["CODIGO_ACCESO"] = Uri.EscapeDataString(codigoAcceso),
                })
            });

            return retorno.IdMensaje;
        }

        public async Task<string> EnviarWhatsappNotificacionPrevia(string whatsappDestino, DateTime fechaVencimiento, TipoUnidadTiempo? unidadTiempoAntelacion, int? cantAntelacion, string? nombreNorma, string? multaNorma, string codigoAcceso) {
            (string tiempoFaltante, string deLosProximos) = DeterminarTextosNotificacionPrevia(fechaVencimiento, unidadTiempoAntelacion, cantAntelacion);

            SalHermesEnviar retorno = await hermesHelper.EnviarWhatsapp(new EntHermesWhatsappEnviar() {
                De = variableEntorno.Obtener("HERMES_DE_WHATSAPP"),
                Para = whatsappDestino,
                NombreTemplate = "notificacion_previa",
                ParametrosTitulo = [tiempoFaltante!],
                ParametrosCuerpo = [
                    nombreNorma ?? "Sin nombre registrado",
                    deLosProximos!,
                    multaNorma ?? "Sin multa registrada"
                ],
                ParametrosBoton = [Uri.EscapeDataString(codigoAcceso)]
            });

            return retorno.IdMensaje;
        }

        public async Task<string> EnviarWhatsappNotificacionVencido(string whatsappDestino, string? nombreNorma, string? multaNorma, string codigoAcceso) {
            SalHermesEnviar retorno = await hermesHelper.EnviarWhatsapp(new EntHermesWhatsappEnviar() {
                De = variableEntorno.Obtener("HERMES_DE_WHATSAPP"),
                Para = whatsappDestino,
                NombreTemplate = "norma_vencida",
                ParametrosTitulo = null,
                ParametrosCuerpo = [
                    nombreNorma ?? "Sin nombre registrado",
                    multaNorma ?? "Sin multa registrada"
                ],
                ParametrosBoton = [Uri.EscapeDataString(codigoAcceso)]
            });

            return retorno.IdMensaje;
        }
    }
}
