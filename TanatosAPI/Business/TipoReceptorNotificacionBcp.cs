using Npgsql;
using System.Text.RegularExpressions;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
	public class TipoReceptorNotificacionBcp(ITipoReceptorNotificacionDao tipoReceptorNotificacionDao) : ITipoReceptorNotificacionBcp {
		public bool EstaVigente(TipoReceptorNotificacion? item) {
			return item != null && item.Vigencia;
		}

		public bool RegexInvalido(TipoReceptorNotificacion item, string valor) {
			return !string.IsNullOrWhiteSpace(item.RegexValidacion) && !Regex.IsMatch(valor, item.RegexValidacion, RegexOptions.NonBacktracking);
		}

		public async Task<TipoReceptorNotificacion?> Obtener(long id, bool validarVigencia = false, NpgsqlTransaction? transaction = null) {
			TipoReceptorNotificacion? item = await tipoReceptorNotificacionDao.ObtenerPorId(id, transaction);
			if (validarVigencia && !EstaVigente(item)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El tipo de receptor no existe o no está vigente", "El tipo de receptor no es válido.");
			return item;
		}
	}
}
