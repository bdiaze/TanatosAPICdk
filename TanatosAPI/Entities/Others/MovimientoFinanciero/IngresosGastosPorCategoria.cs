namespace TanatosAPI.Entities.Others.MovimientoFinanciero {
	public class IngresosGastosPorCategoria {
		public required short TipoMovimiento { get; set; }
		public required long? IdCategoriaFinanciera { get; set; }
		public string? NombreCategoriaFinanciera { get; set; }
		public required decimal MontoTotal { get; set; }
		public required int CantidadMovimientos { get; set; }
	}
}
