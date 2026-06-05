using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Helpers {
    [ExcludeFromCodeCoverage]
    public class CognitoHelper(IAmazonCognitoIdentityProvider client, IVariableEntornoHelper variableEntorno) {

		private readonly Dictionary<string, Dictionary<string, string>> atributosUsuarios = [];

		public async Task<Dictionary<string, string>> ObtenerUsuario(string sub) {
			if (!atributosUsuarios.TryGetValue(sub, out Dictionary<string, string>? atributos)) {

			 	AdminGetUserResponse response = await client.AdminGetUserAsync(new AdminGetUserRequest {
					UserPoolId = variableEntorno.Obtener("COGNITO_USER_POOL_ID"),
					Username = sub
				});

				if (response == null || response.UserAttributes == null) {
					throw new InvalidOperationException($"No se pudo rescatar correctamente los atributos del usuario: {sub}");
				}

				atributos = response.UserAttributes.ToDictionary(a => a.Name, a => a.Value);
				atributosUsuarios[sub] = atributos;
			}

			return atributos;
		}
	}
}
