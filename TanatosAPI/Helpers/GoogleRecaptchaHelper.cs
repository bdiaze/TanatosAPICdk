using Google.Api.Gax.ResourceNames;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.RecaptchaEnterprise.V1;

namespace TanatosAPI.Helpers {
	public class GoogleRecaptchaHelper(VariableEntornoHelper variableEntorno) {
		public async Task<(bool valid, string invalidReason, string action, float score)> ObtenerAssesment(string recaptchaToken, string expectedAction) {
			GoogleCredential credential = CredentialFactory.FromJson<AwsExternalAccountCredential>(variableEntorno.Obtener("GOOGLE_AWS_EXTERNAL_ACCOUNT_JSON")).ToGoogleCredential();
			credential = credential.CreateScoped(["https://www.googleapis.com/auth/recaptchaenterprise"]);
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
