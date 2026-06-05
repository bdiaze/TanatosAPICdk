using System.ComponentModel;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
    public class HistorialNotificacionBcp(IDateTimeProvider dateTimeProvider, CryptoHelper cryptoHelper, HistorialNotificacionDao historialNotificacionDao) {
        public async Task<HistorialNotificacion?> ObtenerPorCodigoAcceso(string codigoAcceso) {
            return await historialNotificacionDao.ObtenerPorCodigoAcceso(cryptoHelper.HashSHA256(codigoAcceso), null);
        }

        public bool EstaVigente(HistorialNotificacion? historialNotificacion) {
            return historialNotificacion != null && historialNotificacion.Vigencia;
        }

        public bool CodigoAccesoVigente(HistorialNotificacion historialNotificacion) {
            return historialNotificacion.FechaCaducidadCodigoAcceso == null || historialNotificacion.FechaCaducidadCodigoAcceso >= dateTimeProvider.UtcNow;
        }
    }
}
