using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface ICargoBcp {
		public bool EstaVigente(Cargo? cargo);
		public bool PerteneceAlNegocio(Cargo cargo, long idNegocio);
        public bool PerteneceAlUsuario(Cargo cargo, string sub);
		public Task<Cargo?> Obtener(long idCargo, bool filtrarVigente = false, string? filtrarSub = null, long? filtrarIdNegocio = null, bool validarVigencia = false, string? validarSub = null, long? validarIdNegocio = null, NpgsqlTransaction? transaction = null);
		public Task<List<Cargo>> ObtenerPorSubYNegocio(string sub, long? idNegocio, bool filtrarVigente = false, NpgsqlTransaction? transaction = null);
		public Task<Cargo> Crear(string sub, string nombre, long idNegocio, NpgsqlTransaction? transaction = null);
		public Task Actualizar(Cargo cargo, NpgsqlTransaction? transaction = null);
		public Task Eliminar(Cargo cargo, NpgsqlTransaction? transaction = null);
	}
}
