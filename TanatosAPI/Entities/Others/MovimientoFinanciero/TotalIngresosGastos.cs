namespace TanatosAPI.Entities.Others.MovimientoFinanciero {
	public class TotalIngresosGastos {
		public required decimal TotalIngresos { get; set; }
		public required decimal TotalGastos { get; set; }
		public required decimal Resultado { get; set; }
		public required int CantidadMovimientos { get; set; }
	}
}
