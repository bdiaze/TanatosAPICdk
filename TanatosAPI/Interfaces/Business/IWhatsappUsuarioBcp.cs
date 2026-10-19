using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IWhatsappUsuarioBcp {
		public bool EstaVigente(WhatsappUsuario? item);
		public bool FueValidado(WhatsappUsuario item);
		public List<WhatsappUsuario> FiltrarVigentes(List<WhatsappUsuario> items);
		public List<WhatsappUsuario> FiltrarValidados(List<WhatsappUsuario> items);
		public Task<WhatsappUsuario?> Obtener(long id, bool validarVigencia = false, bool validarValidacion = false, NpgsqlTransaction? transaction = null);
		public Task<List<WhatsappUsuario>> ObtenerPorSub(string sub, bool filtrarVigentes = false, bool filtrarValidados = false, NpgsqlTransaction? transaction = null);
		public Task<WhatsappUsuario> Crear(string sub, string whatsapp, NpgsqlTransaction? transaction = null);
		public Task Actualizar(WhatsappUsuario item, NpgsqlTransaction? transaction = null);
		public Task Validar(WhatsappUsuario item, NpgsqlTransaction? transaction = null);
		public Task Eliminar(WhatsappUsuario item, NpgsqlTransaction? transaction = null);
		public Task<string> EnviarWhatsappValidacion(string codigoAcceso, string numeroWhatsapp, string nombreUsuario, long idWhatsappUsuario);
	}
}
