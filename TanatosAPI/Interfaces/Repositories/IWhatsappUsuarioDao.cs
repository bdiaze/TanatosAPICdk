using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IWhatsappUsuarioDao {
		public Task<WhatsappUsuario?> Obtener(long id, NpgsqlTransaction? transaction = null);
		public Task<List<WhatsappUsuario>> ObtenerPorSub(string sub, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(WhatsappUsuario item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(WhatsappUsuario item, NpgsqlTransaction? transaction = null);
	}
}
