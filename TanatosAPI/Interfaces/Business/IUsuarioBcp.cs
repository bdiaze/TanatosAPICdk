using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IUsuarioBcp {
		public Task<Usuario?> ObtenerPorFlowCustomerId(string flowCustomerId, NpgsqlTransaction? transaction = null);
		public Task<Usuario> Crear(string sub, string userName, string? flowCustomerId, string? nombre, string? apellido, string? correoElectronico, NpgsqlTransaction? transaction = null);
		public Task<Usuario> CargarDesdeCognitoSiNoExiste(string userName, NpgsqlTransaction? transaction = null);
		public Task<Usuario?> Obtener(string sub, NpgsqlTransaction? transaction = null);
		public Task<string> RegistrarUsuarioEnFlow(string sub, NpgsqlTransaction? transaction = null);
		public Task<string> RegistrarTarjetaEnFlow(string flowCustomerId);
	}
}
