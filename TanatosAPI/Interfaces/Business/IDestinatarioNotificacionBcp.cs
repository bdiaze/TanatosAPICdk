using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
    public interface IDestinatarioNotificacionBcp {
        public bool EstaVigente(DestinatarioNotificacion? destinatarioNotificacion);
        public bool EstaValidado(DestinatarioNotificacion destinatarioNotificacion);
        public bool CodigoValidacionVigente(DestinatarioNotificacion destinatarioNotificacion);
        public Task<string> GenerarCodigoValidacion(NpgsqlTransaction? transaction = null);
        public Task<DestinatarioNotificacion?> ObtenerPorCodigoValidacion(string codigoValidacion, NpgsqlTransaction? transaction = null);
        public Task<List<DestinatarioNotificacion>> ObtenerVigentesPorSubYNegocio(string sub, long idNegocio, NpgsqlTransaction? transaction = null);
        public Task<(DestinatarioNotificacion nuevoDestinatario, string codigoValidacion)> Insertar(string sub, long idNegocio, long? idEmpleado, long idTipoReceptor, string? alias, string destino, bool yaValidado = false, NpgsqlTransaction? transaction = null);
        public Task RegistrarHermesIdMensaje(DestinatarioNotificacion destinatarioNotificacion, string hermesIdMensaje, NpgsqlTransaction? transaction = null);
        public Task<string> EnviarCorreoValidacionDestinatario(string correoDestino, string nombreUsuario, string nombreNegocio, string codigoValidacion);
        public Task<string> EnviarWhatsappValidacionDestinatario(string whatsappDestino, string nombreUsuario, string nombreNegocio, string codigoValidacion);
        public Task Validar(DestinatarioNotificacion destinatarioNotificacion, NpgsqlTransaction? transaction = null);
        public Task Eliminar(DestinatarioNotificacion destinatarioNotificacion, NpgsqlTransaction? transaction = null);
    }
}
