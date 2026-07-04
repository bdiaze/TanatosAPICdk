using Npgsql;

namespace TanatosAPI.Interfaces.Business {
	public interface IEmpleadoBcp {
		public Task DesasociarCargo(string sub, long idNegocio, long idCargo, NpgsqlTransaction? transaction = null);
	}
}
