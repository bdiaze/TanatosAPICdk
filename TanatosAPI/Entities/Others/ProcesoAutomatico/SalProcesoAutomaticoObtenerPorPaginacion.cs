namespace TanatosAPI.Entities.Others.ProcesoAutomatico {
	public class SalProcesoAutomaticoObtenerPorPaginacion {
		public required List<Models.ProcesoAutomatico> Items { get; set; }
		public required long? SiguienteId { get; set; }
	}
}
