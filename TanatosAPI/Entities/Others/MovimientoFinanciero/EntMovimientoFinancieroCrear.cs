namespace TanatosAPI.Entities.Others.MovimientoFinanciero {
	public class EntMovimientoFinancieroCrear {
		public required long IdCuentaFinanciera { get; set; }
		public required long? IdCategoriaFinanciera { get; set; }
		public required short TipoMovimiento { get; set; }
		public required decimal Monto { get; set; }
		public required DateTime FechaMovimientoUtc { get; set; }
		public required string? Descripcion { get; set; }
	}
}
