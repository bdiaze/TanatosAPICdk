using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IHistorialNormaSuscritaBcp {
		public bool EstaVigente(HistorialNormaSuscrita? historialNormaSuscrita);
		public bool EstaCompletada(HistorialNormaSuscrita historialNormaSuscrita);
		public bool VigenteOCompletada(HistorialNormaSuscrita? historialNormaSuscrita);
		public Task<HistorialNormaSuscrita?> ObtenerPorId(long idHistorialNormaSuscrita);
		public Task<List<HistorialNormaSuscrita>> ObtenerVigentesPorNormaSuscritaNoCompletadas(long idNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task<DateTime> ObtenerProximoVencimiento(long idNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task<HistorialNormaSuscrita> Crear(long idNormaSuscrita, DateTime fechaVencimiento, NpgsqlTransaction? transaction = null);
		public Task Eliminar(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task Completar(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null);
		public DateTime CalcularVencimientoFuturo(DateTime fechaReferenciaUTC, TipoPeriodicidad tipoPeriodicidad);
	}
}
