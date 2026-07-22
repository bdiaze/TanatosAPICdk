using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IFiscalizadorNormaSuscritaBcp {
		public Task<List<FiscalizadorNormaSuscrita>> ObtenerVigentesPorNormaSuscrita(long idNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task Eliminar(FiscalizadorNormaSuscrita fiscalizadorNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task EliminarPorNormaSuscrita(long idNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task<List<FiscalizadorNormaSuscrita>> ActualizarPorNormaSuscrita(long idNormaSuscrita, HashSet<long> idTiposFiscalizadores, NpgsqlTransaction? transaction = null);
	}
}
