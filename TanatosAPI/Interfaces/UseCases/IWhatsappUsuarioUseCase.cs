using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.Interfaces.UseCases {
	public interface IWhatsappUsuarioUseCase {
		public Task<WhatsappUsuario?> ObtenerUltimoVigente(string sub, NpgsqlTransaction? transaction = null);
		public Task ActualizarWhatsapp(string sub, string? whatsapp, IDatabaseTransaction? transaction = null);
		public Task ValidarWhatsapp(long idWhatsappUsuario, string codigoAcceso, IDatabaseTransaction? transaction = null);
	}
}
