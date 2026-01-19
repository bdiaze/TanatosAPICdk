namespace TanatosAPI.Entities.Others {
	public class SalKairosIngresarProceso {
		public required string IdProceso { get; set; }
		public required string IdCalendarizacion { get; set; }
		public required string Nombre { get; set; }
		public required string ArnRol { get; set; }
		public required string ArnProceso { get; set; }
		public required string Parametros { get; set; }
		public required bool Habilitado { get; set; }
		public required DateTime FechaCreacion { get; set; }
	}
}
