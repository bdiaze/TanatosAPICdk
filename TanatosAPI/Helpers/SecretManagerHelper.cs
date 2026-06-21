using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Helpers {
    public class SecretManagerHelper(IAmazonSecretsManager client) : ISecretManagerHelper {

        private readonly Dictionary<string, string> secretsValues = [];

        public async Task<string> ObtenerSecreto(string secretArn) {
            if (!secretsValues.TryGetValue(secretArn, out string? value)) {
                GetSecretValueResponse response = await client.GetSecretValueAsync(new GetSecretValueRequest {
                    SecretId = secretArn
                });

                if (response == null || response.SecretString == null) {
                    throw new InvalidOperationException($"No se pudo rescatar correctamente el secreto: {secretArn}");
                }

                value = response.SecretString;
                secretsValues[secretArn] = value;
            }

            return value;
        }
    }
}
