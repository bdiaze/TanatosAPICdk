namespace TanatosAPI.Entities.Others {
	public class EntProfileEnviarCodigoVerificacion {
		public string? Nombre { get; set; }
		public required string CorreoElectronico { get; set; }
		public required string CodigoEncriptado { get; set; }
	}
}
