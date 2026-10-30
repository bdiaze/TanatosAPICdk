namespace TanatosAPI.Entities.Others.MovimientoFinanciero {
	public class EntMovimientoFinancieroActualizar {
		public required long IdNegocio { get; set; }
		public required long IdCuentaFinanciera { get; set; }
		public required long IdMovimientoFinanciero { get; set; }
		public required long? IdCategoriaFinanciera { get; set; }
		public required short TipoMovimiento { get; set; }
		public required decimal Monto { get; set; }
		public required DateTime FechaMovimiento { get; set; }
		public required string? Descripcion { get; set; }
		public short Estado { get; set; } = 1; // 0: Preliminar - 1: Confirmado
	}
}
