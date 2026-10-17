using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface ITipoReceptorNotificacionBcp {
		public bool EstaVigente(TipoReceptorNotificacion? item);
		public bool RegexInvalido(TipoReceptorNotificacion item, string valor);
		public Task<TipoReceptorNotificacion?> Obtener(long id, bool validarVigencia = false, NpgsqlTransaction? transaction = null);
	}
}
