using Npgsql;
using System.ComponentModel;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
    public class HistorialNotificacionBcp(IDateTimeProvider dateTimeProvider, IHistorialNotificacionDao historialNotificacionDao) : IHistorialNotificacionBcp {
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
    }
}
