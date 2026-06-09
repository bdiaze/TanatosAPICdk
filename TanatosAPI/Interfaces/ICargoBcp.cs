using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces {
	public interface ICargoBcp {
		public bool EstaVigente(Cargo? cargo);
		public bool PerteneceAlUsuario(Cargo cargo, string sub);
		public Task<Cargo?> ObtenerPorId(long idCargo, NpgsqlTransaction? transaction = null);
		public Task<Cargo> ObtenerPorIdValidandoVigenciaYPertenencia(long idCargo, string sub, NpgsqlTransaction? transaction = null);
		public Task<List<Cargo>> ObtenerVigentes(string sub, long? idNegocio);
		public Task<Cargo> Insertar(string sub, string nombre, long idNegocio);
		public Task Modificar(Cargo cargo);
		public Task Eliminar(Cargo cargo, NpgsqlTransaction? transaction = null);
	}
}
