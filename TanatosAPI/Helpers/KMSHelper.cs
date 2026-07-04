using Amazon.KeyManagementService;
using AWS.Cryptography.EncryptionSDK;
using AWS.Cryptography.MaterialProviders;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.Helpers {
	[ExcludeFromCodeCoverage]
	public class KMSHelper(IAmazonKeyManagementService kmsClient, IVariableEntornoHelper variableEntorno) : IKMSHelper {
		private readonly string KMS_KEY_ARN = variableEntorno.Obtener("KMS_KEY_ARN");

		public async Task<string> Desencriptar(string encryptedBase64) {
			ESDK encryptionSDK = new(new AwsEncryptionSdkConfig() {
				CommitmentPolicy = ESDKCommitmentPolicy.REQUIRE_ENCRYPT_ALLOW_DECRYPT
			});
			MaterialProviders materialProviders = new(new MaterialProvidersConfig());
			CreateAwsKmsKeyringInput kmsKeyringInput = new() {
				KmsClient = kmsClient,
				KmsKeyId = KMS_KEY_ARN
			};

			IKeyring keyring = materialProviders.CreateAwsKmsKeyring(kmsKeyringInput);

			DecryptOutput output = encryptionSDK.Decrypt(new DecryptInput {
				Ciphertext = new MemoryStream(Convert.FromBase64String(encryptedBase64)),
				Keyring = keyring
			});

			using StreamReader reader = new(output.Plaintext);
			return await reader.ReadToEndAsync();
		}
	}
}
