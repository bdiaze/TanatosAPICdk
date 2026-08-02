using Npgsql;
using Org.BouncyCastle.Crypto.Digests;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class EmpleadoBcp(IEmpleadoDao empleadoDao) : IEmpleadoBcp {
        public bool EstaVigente(Empleado? empleado) {
            return empleado != null && empleado.Vigencia;
        }

		public bool TieneCargo(Empleado empleado, long idCargo) {
			return empleado.IdCargo == idCargo;
		}

        public List<Empleado> FiltrarVigentes(List<Empleado> empleados) {
            return [.. empleados.Where(e => EstaVigente(e))];
        }

		public List<Empleado> FiltrarCargo(List<Empleado> empleados, long idCargo) {
			return [.. empleados.Where(e => TieneCargo(e, idCargo))];
		}

        public async Task<List<Empleado>> ObtenerPorSubYNegocio(string sub, long idNegocio, bool filtrarVigente = false, long? filtrarIdCargo = null, NpgsqlTransaction? transaction = null) {
			List<Empleado> empleados = await empleadoDao.ObtenerPorSub(sub, idNegocio, null, transaction);
			if (filtrarVigente) empleados = FiltrarVigentes(empleados);
			if (filtrarIdCargo != null) empleados = FiltrarCargo(empleados, filtrarIdCargo.Value);
			return empleados;
		}

		public async Task DesasociarCargo(string sub, long idNegocio, long idCargo, NpgsqlTransaction? transaction = null) {
			List<Empleado> empleados = await empleadoDao.ObtenerPorSub(sub, idNegocio, true, transaction);
			foreach (Empleado empleado in empleados.Where(e => e.IdCargo == idCargo)) {
				empleado.IdCargo = null;
				await empleadoDao.Actualizar(empleado, transaction);
			}
		}
	}
}
