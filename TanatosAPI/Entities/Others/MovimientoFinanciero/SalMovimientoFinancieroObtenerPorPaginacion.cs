namespace TanatosAPI.Entities.Others.MovimientoFinanciero {
	public class SalMovimientoFinancieroObtenerPorPaginacion {
		public required List<SalMovimientoFinanciero> Items { get; set; }
		public required DateTime? SiguienteFecha { get; set; }
		public required long? SiguienteId { get; set; }
	}
}
