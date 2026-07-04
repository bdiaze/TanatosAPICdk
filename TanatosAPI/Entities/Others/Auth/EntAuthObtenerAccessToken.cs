using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.Auth {
    [ExcludeFromCodeCoverage]
    public class EntAuthObtenerAccessToken {
		public required string Code { get; set; }
		public required string CodeVerifier { get; set; }
		public required string RedirectUri { get; set; }
	}
}
