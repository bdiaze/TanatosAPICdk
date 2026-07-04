using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others.Flow {
    [ExcludeFromCodeCoverage]
    public class SalFlowUrlToken : ISalFlow {
		[JsonPropertyName("token")]
		public required string Token { get; set; }

		[JsonPropertyName("url")]
		public required string Url { get; set; }
	}
}
