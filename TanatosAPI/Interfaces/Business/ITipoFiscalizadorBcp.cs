using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
    public interface ITipoFiscalizadorBcp {
        public bool EstaVigente(TipoFiscalizador? tipoFiscalizador);
        public Task<List<TipoFiscalizador>> ValidarTodosVigentes(HashSet<long> ids, NpgsqlTransaction? transaction = null);
        public Task<TipoFiscalizador?> Obtener(long id, NpgsqlTransaction? transaction = null);
        public Task<List<TipoFiscalizador>> ObtenerVigentes(NpgsqlTransaction? transaction = null);
        public Task<List<TipoFiscalizador>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null);
        public Task<TipoFiscalizador> Crear(long id, string nombre, string? nombreCorto, bool vigencia, NpgsqlTransaction? transaction = null);
        public Task Actualizar(TipoFiscalizador tipoFiscalizador, NpgsqlTransaction? transaction = null);
        public Task Eliminar(long id, NpgsqlTransaction? transaction = null);
    }
}
