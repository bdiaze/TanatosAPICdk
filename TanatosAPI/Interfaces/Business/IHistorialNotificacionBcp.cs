using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
    public interface IHistorialNotificacionBcp {
        public bool EstaVigente(HistorialNotificacion? historialNotificacion);
        public bool CodigoAccesoVigente(HistorialNotificacion historialNotificacion);
        public Task<HistorialNotificacion?> ObtenerPorCodigoAcceso(string codigoAcceso, NpgsqlTransaction? transaction = null);
        public Task<HistorialNotificacion> ObtenerPorCodigoAccesoValidandoVigencia(string codigoAcceso, NpgsqlTransaction? transaction = null);
        public Task<string> GenerarCodigoAcceso(NpgsqlTransaction? transaction = null);
        public Task<(HistorialNotificacion nuevaNotificacion, string codigoAcceso)> Registrar(long idHistorialNormaSuscrita, long idDestinatarioNotificacion, long? idTipoUnidadTiempoAntelacion, int? cantAntelacion, DateTime fechaProgramacion, NpgsqlTransaction? transaction = null);
        public Task MarcarOmitido(HistorialNotificacion historialNotificacion, string observacion, NpgsqlTransaction? transaction = null);
        public Task MarcarEnviado(HistorialNotificacion historialNotificacion, string hermesIdMensaje, NpgsqlTransaction? transaction = null);
        public (string tiempoFaltante, string deLosProximos) DeterminarTextosNotificacionPrevia(DateTime fechaVencimiento, TipoUnidadTiempo? unidadTiempoAntelacion, int? cantAntelacion);
        public Task<string> EnviarCorreoNotificacionPrevia(string correoDestino, DateTime fechaVencimiento, TipoUnidadTiempo? unidadTiempoAntelacion, int? cantAntelacion, string? nombreNorma, string? multaNorma, string codigoAcceso);
        public Task<string> EnviarCorreoNotificacionVencido(string correoDestino, string? nombreNorma, string? multaNorma, string codigoAcceso);
        public Task<string> EnviarWhatsappNotificacionPrevia(string whatsappDestino, DateTime fechaVencimiento, TipoUnidadTiempo? unidadTiempoAntelacion, int? cantAntelacion, string? nombreNorma, string? multaNorma, string codigoAcceso);
        public Task<string> EnviarWhatsappNotificacionVencido(string whatsappDestino, string? nombreNorma, string? multaNorma, string codigoAcceso);
    }
}
