using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IHistorialNormaSuscritaBcp {
		public bool EstaVigente(HistorialNormaSuscrita? historialNormaSuscrita);
		public bool EstaCompletada(HistorialNormaSuscrita historialNormaSuscrita);
		public bool VigenteOCompletada(HistorialNormaSuscrita? historialNormaSuscrita);
		public bool Pertenece(HistorialNormaSuscrita historialNormaSuscrita, long idNormaSuscrita);
        public Task<HistorialNormaSuscrita?> ObtenerPorId(long idHistorialNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task<HistorialNormaSuscrita> ObtenerValidandoVigencia(long idHistorialNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task<HistorialNormaSuscrita> ObtenerValidandoVigenciaYPertenencia(long idHistorialNormaSuscrita, long idNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task<List<HistorialNormaSuscrita>> ObtenerVigentesPorNormaSuscrita(long idNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task<HistorialNormaSuscrita?> ObtenerUltimoVigentePorNormaSuscrita(long idNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task<List<HistorialNormaSuscrita>> ObtenerVigentesPorNormaSuscritaNoCompletadas(long idNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task<List<HistorialNormaSuscrita>> ObtenerVigentesPorNormaSuscritaCompletadas(long idNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task<DateTime> ObtenerProximoVencimiento(long idNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task<bool> TieneVencimientoFuturoNoCompletado(long idNormaSuscrita, List<long>? idVencimientoIgnorar = null, NpgsqlTransaction? transaction = null);
        public Task<HistorialNormaSuscrita> Crear(long idNormaSuscrita, DateTime fechaVencimiento, NpgsqlTransaction? transaction = null);
		public Task Eliminar(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task<DateTime> Completar(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null);
	}
}
