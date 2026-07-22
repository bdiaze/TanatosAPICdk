using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
    public interface IHistorialNotificacionBcp {
        public bool EstaVigente(HistorialNotificacion? historialNotificacion);
        public bool CodigoAccesoVigente(HistorialNotificacion historialNotificacion);
        public Task<HistorialNotificacion?> ObtenerPorCodigoAcceso(string codigoAcceso, NpgsqlTransaction? transaction = null);
        public Task<HistorialNotificacion> ObtenerPorCodigoAccesoValidandoVigencia(string codigoAcceso, NpgsqlTransaction? transaction = null);

    }
}
