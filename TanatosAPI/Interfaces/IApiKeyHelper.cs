namespace TanatosAPI.Interfaces {
	public interface IApiKeyHelper {
		public Task<string> ObtenerApiKey(string apiKeyId);
	}
}
