using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.UseCases {
	public interface IHistorialNormaSuscritaUseCase {
		public Task EliminarPorNormaSuscrita(long idNormaSuscrita, bool ignorarVencidos, NpgsqlTransaction transaction);
		public Task<DateTime> CompletarHistorialNormaSuscrita(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction transaction);
		public Task ProgramarSiguienteVencimiento(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null);
		public DateTime CalcularSiguienteVencimiento(DateTime vencimientoActual, TipoPeriodicidad tipoPeriodicidad, bool fechasChilenas = false);
		public DateTime CalcularVencimientoFuturo(DateTime fechaReferenciaUTC, TipoPeriodicidad tipoPeriodicidad);
	}
}
