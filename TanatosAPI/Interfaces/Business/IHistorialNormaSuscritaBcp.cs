using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IHistorialNormaSuscritaBcp {
		public bool EstaVigente(HistorialNormaSuscrita? historialNormaSuscrita);
		public bool EstaCompletada(HistorialNormaSuscrita historialNormaSuscrita);
		public bool Pertenece(HistorialNormaSuscrita historialNormaSuscrita, long idNormaSuscrita);
		public List<HistorialNormaSuscrita> FiltrarVigentes(List<HistorialNormaSuscrita> vencimientos);
		public List<HistorialNormaSuscrita> FiltrarNoCompletadas(List<HistorialNormaSuscrita> vencimientos);
		public List<HistorialNormaSuscrita> FiltrarCompletadas(List<HistorialNormaSuscrita> vencimientos);
		public HistorialNormaSuscrita? FiltrarUltimoVencimiento(List<HistorialNormaSuscrita> vencimientos);
		public Task<HistorialNormaSuscrita?> Obtener(long idHistorialNormaSuscrita, bool validarVigencia = false, long? validarIdNormaSuscrita = null, NpgsqlTransaction? transaction = null);
		public Task<List<HistorialNormaSuscrita>> ObtenerPorNormaSuscrita(long idNormaSuscrita, bool filtrarVigente = false, bool filtrarNoCompletadas = false, bool filtrarCompletadas = false, NpgsqlTransaction? transaction = null);
		public Task<DateTime> ObtenerProximoVencimiento(long idNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task<bool> TieneVencimientoFuturoNoCompletado(long idNormaSuscrita, List<long>? idVencimientoIgnorar = null, NpgsqlTransaction? transaction = null);
        public Task<HistorialNormaSuscrita> Crear(long idNormaSuscrita, DateTime fechaVencimiento, NpgsqlTransaction? transaction = null);
		public Task Eliminar(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task<DateTime> Completar(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null);
	}
}
