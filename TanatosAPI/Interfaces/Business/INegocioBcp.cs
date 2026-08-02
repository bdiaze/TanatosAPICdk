using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface INegocioBcp {
		public bool EstaVigente(Negocio? negocio);
		public bool Pertenece(Negocio negocio, string sub);
		public List<Negocio> FiltrarVigentes(List<Negocio> negocios);
		public Task<Negocio?> Obtener(long idNegocio, bool filtrarVigente = false, bool validarVigencia = false, string? validarSub = null, NpgsqlTransaction? transaction = null);
		public Task<List<Negocio>> ObtenerPorSub(string sub, bool filtrarVigentes = false, NpgsqlTransaction? transaction = null);
	}
}
