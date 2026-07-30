using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IEmpleadoBcp {
        public bool EstaVigente(Empleado? empleado);
        public List<Empleado> FiltrarVigentes(List<Empleado> empleados);
        public Task<List<Empleado>> ObtenerPorSubYNegocio(string sub, long idNegocio, bool filtrarVigente = false, long? filtrarIdCargo = null, NpgsqlTransaction? transaction = null);
        public Task DesasociarCargo(string sub, long idNegocio, long idCargo, NpgsqlTransaction? transaction = null);
	}
}
