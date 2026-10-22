namespace TanatosAPI.Entities.Others.CuentaFinanciera {
	public class SalCuentaFinanciera {
		public required long Id { get; set; }
		public required string Nombre { get; set; }
		public required long? IdTipoCuentaFinanciera { get; set; }
		public required string? NombreTipoCuentaFinanciera { get; set; }
		public required decimal SaldoActual { get; set; }
	}
}
