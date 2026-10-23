namespace TanatosAPI.Entities.Others.MovimientoFinanciero {
	public class SalMovimientoFinanciero {
		public required long Id { get; set; }
		public required long? IdCategoriaFinanciera { get; set; }
		public required string? NombreCategoriaFinanciera { get; set; }
		public required short TipoMovimiento { get; set; }
		public required decimal Monto { get; set; }
		public required decimal Saldo { get; set; }
		public required DateTime FechaMovimiento { get; set; }
		public required string? Descripcion { get; set; }
	}
}
