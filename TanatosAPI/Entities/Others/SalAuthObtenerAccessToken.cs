namespace TanatosAPI.Entities.Others {
	public class SalAuthObtenerAccessToken {
		public required string AccessToken { get; set; }
		public required int ExpiresIn { get; set; }
		public required string CsrfToken { get; set; }
		public required DateTimeOffset CsrfTokenExpiration { get; set; }
	}
}
