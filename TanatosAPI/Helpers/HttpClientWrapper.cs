using TanatosAPI.Interfaces;

namespace TanatosAPI.Helpers {
	public class HttpClientWrapper(HttpClient httpClient) : IHttpClientWrapper {
		public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request) {
			return await httpClient.SendAsync(request);
		}
	}
}
