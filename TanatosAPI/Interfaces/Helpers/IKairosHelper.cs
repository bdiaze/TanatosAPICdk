using TanatosAPI.Entities.Others.Kairos;

namespace TanatosAPI.Interfaces.Helpers {
	public interface IKairosHelper {
		public Task<SalKairosIngresarProceso> IngresarProceso(EntKairosIngresarProceso proceso);
		public Task EliminarProceso(string idProceso);
	}
}
