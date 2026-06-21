using System.Net.Http.Headers;

namespace TanatosAPI.Interfaces {
	public interface ICognitoHttpClient : IHttpClientWrapper { }
	public interface IHermesHttpClient : IHttpClientWrapper { }
	public interface IKairosHttpClient : IHttpClientWrapper { }
	public interface IFlowHttpClient : IHttpClientWrapper { }
	public interface IGoogleRecaptchaHttpClient : IHttpClientWrapper { }
	public interface IGoogleCredentialHttpClient: IHttpClientWrapper { }

	public interface IHttpClientWrapper {
		public HttpRequestHeaders DefaultRequestHeaders { get; }
		public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
		public Task<HttpResponseMessage> PostAsync(string? requestUri, HttpContent? content);
		public Task<HttpResponseMessage> DeleteAsync(string? requestUri);
		public Task<HttpResponseMessage> GetAsync(string? requestUri);
	}
}
