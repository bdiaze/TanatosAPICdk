namespace TanatosAPI.Interfaces {
	public interface IHttpClientWrapper {
		public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
	}
}
