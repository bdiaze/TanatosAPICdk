using TanatosAPI.Entities.Others.Kairos;

namespace TanatosAPI.Interfaces.Helpers {
	public interface IKairosHelper {
		public Task<SalKairosIngresarProceso> IngresarProceso(EntKairosIngresarProceso proceso);
		public Task<List<SalKairosIngresarProceso>> IngresarVariosProcesos(List<EntKairosIngresarProceso> procesos);
		public Task EliminarProceso(string idProceso);
		public Task EliminarVariosProcesos(List<string> idsProcesos);
	}
}
