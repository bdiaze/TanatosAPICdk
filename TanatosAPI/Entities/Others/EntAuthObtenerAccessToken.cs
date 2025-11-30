namespace TanatosAPI.Entities.Others {
	public class EntAuthObtenerAccessToken {
		public required string Code { get; set; }
		public required string CodeVerifier { get; set; }
	}
}
