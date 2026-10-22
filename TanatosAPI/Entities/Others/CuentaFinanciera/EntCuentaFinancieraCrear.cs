namespace TanatosAPI.Entities.Others.CuentaFinanciera {
	public class EntCuentaFinancieraCrear {
		public required long IdNegocio { get; set; }
		public required string Nombre { get; set; }
		public required long IdTipoCuentaFinanciera { get; set; }
		public required decimal SaldoInicial { get; set; }
		public required DateTime FechaSaldoInicialUtc { get; set; }
	}
}
