using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IEmpleadoDao {
		public Task<List<Empleado>> ObtenerPorSub(string sub, long? idNegocio = null, bool? vigencia = true, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(Empleado item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(Empleado item, NpgsqlTransaction? transaction = null);
	}
}
