using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others {
    [ExcludeFromCodeCoverage]
    public class SalAuthRefreshAccessToken {
		public required string AccessToken { get; set; }
		public required int ExpiresIn { get; set; }
	}
}
