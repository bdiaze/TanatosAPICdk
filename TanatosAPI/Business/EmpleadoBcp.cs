using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class EmpleadoBcp(EmpleadoDao empleadoDao) {
		public async Task DesasociarCargo(string sub, long idNegocio, long idCargo, NpgsqlTransaction? transaction = null) {
			List<Empleado> empleados = await empleadoDao.ObtenerPorSub(sub, idNegocio, true, transaction);
			foreach (Empleado empleado in empleados.Where(e => e.IdCargo == idCargo)) {
				empleado.IdCargo = null;
				await empleadoDao.Actualizar(empleado, transaction);
			}
		}
	}
}
