using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
    public class TipoFiscalizadorBcp(ITipoFiscalizadorDao tipoFiscalizadorDao) : ITipoFiscalizadorBcp {
        public bool EstaVigente(TipoFiscalizador? tipoFiscalizador) {
            return tipoFiscalizador != null && tipoFiscalizador.Vigencia;
        }

        public async Task<List<TipoFiscalizador>> ValidarTodosVigentes(HashSet<long> ids, NpgsqlTransaction? transaction = null) {
            if (ids.Count == 0) return [];

            List<TipoFiscalizador> vigentes = await ObtenerVigentes(transaction);

            HashSet<long> idsVigentes = [.. vigentes.Select(f => f.Id)];
            foreach (long id in ids) {
                if (!idsVigentes.Contains(id)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, $"El tipo de fiscalizador con ID {id} no está vigente", "Tipo de fiscalizador inválido.");
            }

            return [.. vigentes.Where(f => ids.Contains(f.Id))];
        }

        public async Task<TipoFiscalizador?> Obtener(long id, NpgsqlTransaction? transaction = null) {
            return await tipoFiscalizadorDao.ObtenerPorId(id, transaction);
        }

        public async Task<List<TipoFiscalizador>> ObtenerVigentes(NpgsqlTransaction? transaction = null) {
            return await ObtenerPorVigencia(true, transaction);
        }

        public async Task<List<TipoFiscalizador>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null) {
            return await tipoFiscalizadorDao.ObtenerPorVigencia(vigencia, transaction);
        }

        public async Task<TipoFiscalizador> Crear(long id, string nombre, string? nombreCorto, bool vigencia, NpgsqlTransaction? transaction = null) {
            TipoFiscalizador nuevo = new() { 
                Id = id,
                Nombre = nombre,
                NombreCorto = nombreCorto,
                Vigencia = vigencia
            };
            await tipoFiscalizadorDao.Insertar(nuevo, transaction);
            return nuevo;
        }

        public async Task Actualizar(TipoFiscalizador tipoFiscalizador, NpgsqlTransaction? transaction = null) {
            await tipoFiscalizadorDao.Actualizar(tipoFiscalizador, transaction);
        }

        public async Task Eliminar(long id, NpgsqlTransaction? transaction = null) {
            await tipoFiscalizadorDao.Eliminar(id, transaction);
        }
    }
}
