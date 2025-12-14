using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;

namespace TanatosAPI.Helpers {
	public class CognitoHelper(IAmazonCognitoIdentityProvider client, VariableEntornoHelper variableEntorno) {

		private readonly Dictionary<string, Dictionary<string, string>> atributosUsuarios = [];

		public async Task<Dictionary<string, string>> ObtenerUsuario(string sub) {
			if (!atributosUsuarios.TryGetValue(sub, out Dictionary<string, string>? atributos)) {

			 	AdminGetUserResponse response = await client.AdminGetUserAsync(new AdminGetUserRequest {
					UserPoolId = variableEntorno.Obtener("COGNITO_USER_POOL_ID"),
					Username = sub
				});

				if (response == null || response.UserAttributes == null) {
					throw new Exception("No se pudo rescatar correctamente los atributos del usuario");
				}

				atributos = response.UserAttributes.ToDictionary(a => a.Name, a => a.Value);
				atributosUsuarios[sub] = atributos;
			}

			return atributos;
		}
	}
}
