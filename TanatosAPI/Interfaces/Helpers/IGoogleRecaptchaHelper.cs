namespace TanatosAPI.Interfaces.Helpers {
	public interface IGoogleRecaptchaHelper {
		public Task<(bool valid, string invalidReason, string action, float score)> ObtenerAssesment(string recaptchaToken, string expectedAction);
	}
}
