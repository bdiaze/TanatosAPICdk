using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface ICargoBcp {
		public bool EstaVigente(Cargo? cargo);
		public bool PerteneceAlNegocio(Cargo cargo, long idNegocio);
        public bool PerteneceAlUsuario(Cargo cargo, string sub);
		public Task<Cargo?> Obtener(long idCargo, NpgsqlTransaction? transaction = null);
		public Task<Cargo> ObtenerValidandoVigencia(long idCargo, NpgsqlTransaction? transaction = null);
        public Task<Cargo> ObtenerValidandoVigenciaYPertenencia(long idCargo, string sub, NpgsqlTransaction? transaction = null);
		public Task<Cargo> ObtenerValidandoVigenciaPertenenciaNegocio(long idCargo, long idNegocio, string sub, NpgsqlTransaction? transaction = null);
		public Task<List<Cargo>> ObtenerVigentes(string sub, long? idNegocio);
		public Task<Cargo> Crear(string sub, string nombre, long idNegocio);
		public Task Actualizar(Cargo cargo);
		public Task Eliminar(Cargo cargo, NpgsqlTransaction? transaction = null);
	}
}
