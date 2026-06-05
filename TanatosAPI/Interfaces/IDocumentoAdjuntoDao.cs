using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces {
    public interface IDocumentoAdjuntoDao {
        public Task<List<DocumentoAdjunto>> ObtenerPorHistorial(long idHistorialNormaSuscrita, bool? vigencia = true, NpgsqlTransaction? transaction = null);
        public Task<DocumentoAdjunto?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null);
        public Task<long> Insertar(DocumentoAdjunto item, NpgsqlTransaction? transaction = null);
        public Task Actualizar(DocumentoAdjunto item, NpgsqlTransaction? transaction = null);
    }
}
