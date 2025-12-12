using Amazon.APIGateway;
using Amazon.APIGateway.Model;

namespace TanatosAPI.Helpers {
	public class ApiKeyHelper(IAmazonAPIGateway apiClient) {
		private readonly Dictionary<string, string> apiKeys = [];

		public async Task<string> ObtenerApiKey(string apiKeyId) {
			if (!apiKeys.TryGetValue(apiKeyId, out string? value)) {
				GetApiKeyResponse response = await apiClient.GetApiKeyAsync(new GetApiKeyRequest {
					ApiKey = apiKeyId,
					IncludeValue = true
				});

				if (response == null || response.Value == null) {
					throw new Exception("No se pudo rescatar correctamente el api key");
				}

				value = response.Value;
				apiKeys[apiKeyId] = value;
			}

			return value;
		}
	}
}
