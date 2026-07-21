using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Repositories;

namespace TanatosAPI.UseCases {
    public class TipoUnidadTiempoUseCase(ITipoUnidadTiempoBcp tipoUnidadTiempoBcp) {
        public async Task<List<TipoUnidadTiempo>> ObtenerVigentes(NpgsqlTransaction? transaction = null) {
            return await tipoUnidadTiempoBcp.ObtenerVigentes(transaction);
        }

        public async Task<List<TipoUnidadTiempo>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null) {
            return await tipoUnidadTiempoBcp.ObtenerPorVigencia(vigencia, transaction);
        }

        public async Task<TipoUnidadTiempo> Insertar(long id, string nombre, string? nombrePlural, long cantSegundos, long? cantMinutos, long? cantHoras, long? cantDias, bool vigencia, NpgsqlTransaction? transaction = null) {
            return await tipoUnidadTiempoBcp.Insertar(id, nombre, nombrePlural, cantSegundos, cantMinutos, cantHoras, cantDias, vigencia, transaction);
        }

        public async Task<TipoUnidadTiempo> Actualizar(long id, string nombre, string? nombrePlural, long cantSegundos, long? cantMinutos, long? cantHoras, long? cantDias, bool vigencia, NpgsqlTransaction? transaction = null) {
            return await tipoUnidadTiempoBcp.Actualizar(id, nombre, nombrePlural, cantSegundos, cantMinutos, cantHoras, cantDias, vigencia, transaction);
        }

        public async Task Eliminar(long idTipoUnidadTiempo, NpgsqlTransaction? transaction = null) {
            await tipoUnidadTiempoBcp.Eliminar(idTipoUnidadTiempo, transaction);
        }
    }
}
