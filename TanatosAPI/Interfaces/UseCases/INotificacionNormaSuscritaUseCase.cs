using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.UseCases {
	public interface INotificacionNormaSuscritaUseCase {
		public Task<List<(TipoUnidadTiempo UnidadTiempoAntelacion, int CantAntelacion)>> ObtenerAntelacionesConsiderandoTemplate(long idNormaSuscrita, long? idTemplate, long? idNormaTemplate, NpgsqlTransaction? transaction = null);
		public Task<List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)>> GenerarCrons(DateTime proximoVencimientoUtc, string baseCronAws, List<(TipoUnidadTiempo TipoUnidadTiempo, int CantAntelacion)> antelaciones);
		public Task<List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)>> GenerarFrecuenciasDias(DateTime proximoVencimientoUtc, int frecuenciaDias, List<(TipoUnidadTiempo TipoUnidadTiempo, int CantAntelacion)> antelaciones);
	}
}
