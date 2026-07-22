using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface INegocioBcp {
		public bool EstaVigente(Negocio? negocio);
		public bool PerteneceAlUsuario(Negocio negocio, string sub);
		public Task<Negocio?> Obtener(long idNegocio, NpgsqlTransaction? transaction = null);
		public Task<Negocio> ObtenerValidandoVigencia(long idNegocio, NpgsqlTransaction? transaction = null);
        public Task<Negocio> ObtenerValidandoVigenciaYPertenencia(long idNegocio, string sub, NpgsqlTransaction? transaction = null);
		public Task<Negocio?> ObtenerVigentePorSubYNegocio(string sub, long idNegocio, NpgsqlTransaction? transaction = null);
		public Task<List<Negocio>> ObtenerVigentesPorSub(string sub, NpgsqlTransaction? transaction = null);
	}
}
