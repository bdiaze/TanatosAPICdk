using Google.Api.Gax.ResourceNames;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.RecaptchaEnterprise.V1;
using System.Text.Json;

namespace TanatosAPI.Helpers {
	public class GoogleRecaptchaHelper(VariableEntornoHelper variableEntorno, SecretManagerHelper secretManagerHelper) {
		public async Task<(bool valid, string invalidReason, string action, float score)> ObtenerAssesment(string recaptchaToken, string expectedAction) {
			Dictionary<string, string> secretApp = JsonSerializer.Deserialize(
				await secretManagerHelper.ObtenerSecreto(variableEntorno.Obtener("SECRET_ARN_APP")),
				AppJsonSerializerContext.Default.DictionaryStringString
			)!;

			GoogleCredential credential = CredentialFactory.FromJson<ServiceAccountCredential>(secretApp["GoogleRecaptchaCredential"]).ToGoogleCredential();
			RecaptchaEnterpriseServiceClient client = new RecaptchaEnterpriseServiceClientBuilder {
				Credential = credential
			}.Build();
			ProjectName projectName = new(variableEntorno.Obtener("GOOGLE_RECAPTCHA_PROJECT_ID"));
			CreateAssessmentRequest request = new() {
				ParentAsProjectName = projectName,
				Assessment = new Assessment {
					Event = new Event {
						SiteKey = variableEntorno.Obtener("GOOGLE_RECAPTCHA_SITE_KEY"),
						ExpectedAction = expectedAction,
						Token = recaptchaToken,
					}
				}
			};
			Assessment response = await client.CreateAssessmentAsync(request);

			return (response.TokenProperties.Valid, response.TokenProperties.InvalidReason.ToString(), response.TokenProperties.Action, response.RiskAnalysis.Score);
		}
	}
}
