namespace TanatosAPI.Entities.Others.MovimientoFinanciero {
	public class IngresosGastosPorPeriodo {
		public required string Periodo { get; set; }
		public required decimal TotalIngresos { get; set; }
		public required decimal TotalGastos { get; set; }
		public required decimal Resultado { get; set; }
	}
}
