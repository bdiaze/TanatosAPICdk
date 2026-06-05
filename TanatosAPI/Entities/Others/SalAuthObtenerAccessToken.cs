using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others {
    [ExcludeFromCodeCoverage]
    public class SalAuthObtenerAccessToken {
		public required string AccessToken { get; set; }
		public required int ExpiresIn { get; set; }
	}
}
