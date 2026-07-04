namespace TanatosAPI.Interfaces.Helpers {
	public interface IApiKeyHelper {
		public Task<string> ObtenerApiKey(string apiKeyId);
	}
}
