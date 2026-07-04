using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface ISuscripcionBcp {
		public bool EstaVigente(Suscripcion? suscripcion);
		public bool PerteneceAlUsuario(Suscripcion suscripcion, string sub);
		public List<Suscripcion> FiltrarEnCurso(List<Suscripcion> suscripciones, DateTime? fechaReferencia = null);
		public List<Suscripcion> FiltrarEnCursoConFlow(List<Suscripcion> suscripciones, DateTime? fechaReferencia = null);
		public List<Suscripcion> FiltrarFuturas(List<Suscripcion> suscripciones, DateTime? fechaReferencia = null);
		public List<Suscripcion> FiltrarPagosEnCurso(List<Suscripcion> suscripciones, DateTime? fechaReferencia = null);
		public bool AlgunaConPagoEnCurso(List<Suscripcion> suscripciones, DateTime? fechaReferencia = null);
		public DateTime? ProximaFechaExpiracion(List<Suscripcion> suscripciones, DateTime? fechaReferencia = null);
		public DateTime ProximaFechaSinSuscripcion(List<Suscripcion> suscripciones, DateTime? fechaReferencia = null);
		public Task<List<Suscripcion>> ObtenerVigentesPorSub(string sub, NpgsqlTransaction? transaction = null);
		public Task<Suscripcion?> ObtenerPorFlowSubscriptionId(string flowSubscriptionId, NpgsqlTransaction? transaction = null);
		public Task<bool> TienePlanEmpresa(string sub, NpgsqlTransaction? transaction = null);
		public Task Cancelar(Suscripcion suscripcion, NpgsqlTransaction? transaction = null);
		public Task Eliminar(Suscripcion suscripcion, NpgsqlTransaction? transaction = null);
		public Task EliminarCreacionNoConfirmada(List<Suscripcion> suscripciones, NpgsqlTransaction? transaction = null);
		public Task<Suscripcion> Crear(string sub, long idPlan, DateTime? fechaInicio, DateTime? fechaExpiracion, short estado, NpgsqlTransaction? transaction = null);
		public Task Modificar(Suscripcion suscripcion, NpgsqlTransaction? transaction = null);
	}
}
