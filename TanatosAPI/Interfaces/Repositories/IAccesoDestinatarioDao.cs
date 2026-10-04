using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IAccesoDestinatarioDao {
		public Task<List<AccesoDestinatario>> ObtenerPorIdAcceso(long idAcceso, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(AccesoDestinatario item, NpgsqlTransaction? transaction = null);
	}
}
