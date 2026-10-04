using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.UseCases;

namespace TanatosAPI.Interfaces.UseCases {
	public interface IAccesoDestinatarioUseCase {
		public Task<(AccesoDestinatario, string CodigoAcceso)> HabilitarAccesoDestinatario(long idDestinatario, List<RecursoSolicitado> recursosSolicitados, TimeSpan? duracion = null, IDatabaseTransaction? transaction = null);
		public Task<DestinatarioNotificacion> ValidarAccesoDestinatario(string codigoAcceso, string tipoRecurso, string idRecurso, string accion, NpgsqlTransaction? transaction = null);
	}
}
