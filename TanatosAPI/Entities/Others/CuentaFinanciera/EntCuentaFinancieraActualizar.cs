namespace TanatosAPI.Entities.Others.CuentaFinanciera {
	public class EntCuentaFinancieraActualizar {
		public required long Id { get; set; }
		public required string Nombre { get; set; }
		public required long IdTipoCuentaFinanciera { get; set; }
	}
}
