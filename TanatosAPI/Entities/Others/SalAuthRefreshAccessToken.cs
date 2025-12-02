namespace TanatosAPI.Entities.Others {
	public class SalAuthRefreshAccessToken {
		public required string AccessToken { get; set; }
		public required int ExpiresIn { get; set; }
	}
}
