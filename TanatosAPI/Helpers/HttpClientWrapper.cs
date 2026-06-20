using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Helpers {
	[ExcludeFromCodeCoverage]
	public class HttpClientWrapper(HttpClient httpClient) : ICognitoHttpClient, IHermesHttpClient, IKairosHttpClient, IFlowHttpClient, IGoogleRecaptchaHttpClient {
		public HttpRequestHeaders DefaultRequestHeaders => httpClient.DefaultRequestHeaders;

		public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request) {
			return await httpClient.SendAsync(request);
		}

		public async Task<HttpResponseMessage> PostAsync(string? requestUri, HttpContent? content) {
			return await httpClient.PostAsync(requestUri, content);
		}

		public async Task<HttpResponseMessage> DeleteAsync(string? requestUri) {
			return await httpClient.DeleteAsync(requestUri);
		}

		public async Task<HttpResponseMessage> GetAsync(string? requestUri) {
			return await httpClient.GetAsync(requestUri);
		}
	}
}
