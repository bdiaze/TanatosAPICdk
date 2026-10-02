using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.UseCases;

namespace TanatosAPI.Interfaces.UseCases {
	public interface IAccesoUseCase {
		public Task<(Acceso, string CodigoAcceso)> HabilitarAcceso(List<RecursoSolicitado> recursosSolicitados, TimeSpan? duracion = null, IDatabaseTransaction? transaction = null);
		public Task ValidarAcceso(string codigoAcceso, string tipoRecurso, string idRecurso, string accion, NpgsqlTransaction? transaction = null);
	}
}
