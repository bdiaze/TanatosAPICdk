namespace TanatosAPI.Entities.Others {
	public class EntMensajeIngresar {
		public required string Nombre { get; set; }
		public required string Correo { get; set; }
		public required string Contenido { get; set; }
		public required string RecaptchaToken { get; set; }
	}
}
