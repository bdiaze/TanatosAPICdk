using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces {
	public interface INegocioBcp {
		public bool EstaVigente(Negocio? negocio);
		public bool PerteneceAlUsuario(Negocio negocio, string sub);
		public Task<Negocio?> ObtenerPorId(long idNegocio, NpgsqlTransaction? transaction = null);
		public Task<Negocio> ObtenerPorIdValidandoVigenciaYPertenencia(long idNegocio, string sub, NpgsqlTransaction? transaction = null);
		public Task<Negocio?> ObtenerVigentePorSubYNegocio(string sub, long idNegocio, NpgsqlTransaction? transaction = null);
		public Task EliminarNegocio(Negocio negocio, NpgsqlTransaction? transaction = null);
	}
}
