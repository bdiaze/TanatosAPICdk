using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IAccesoDestinatarioBcp {
		public bool EstaVigente(AccesoDestinatario? item);
		public List<AccesoDestinatario> FiltrarVigentes(List<AccesoDestinatario> items);
		public Task<List<AccesoDestinatario>> ObtenerPorIdAcceso(long idAcceso, bool filtrarVigentes = false, NpgsqlTransaction? transaction = null);
		public Task<AccesoDestinatario> Crear(long idAcceso, long idDestinatarioNotificacion, NpgsqlTransaction? transaction = null);
	}
}
