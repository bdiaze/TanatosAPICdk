using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IUsuarioBcp {
		public Task<Usuario?> ObtenerPorFlowCustomerId(string flowCustomerId, NpgsqlTransaction? transaction = null);
		public Task<Usuario> ObtenerInformacionUsuario(string sub, NpgsqlTransaction? transaction = null);
		public Task<string> RegistrarUsuarioEnFlow(string sub, NpgsqlTransaction? transaction = null);
		public Task<string> RegistrarTarjetaEnFlow(string flowCustomerId);
	}
}
