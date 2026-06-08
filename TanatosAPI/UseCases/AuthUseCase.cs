using Amazon.Lambda.Core;
using System.Diagnostics;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;

namespace TanatosAPI.UseCases {
	public class AuthUseCase(IVariableEntornoHelper variableEntorno, ICognitoHelper cognitoHelper) {
		public async Task<(string accessToken, string refreshToken, int expiresIn, int refreshExpiresIn)> ObtenerAccessToken(string code, string codeVerifier, string redirectUri) {
			// Se valida que el redirect uri se encuentre entre los permitidos...
			if (!variableEntorno.Obtener("COGNITO_CALLBACK_URLS").Split(',').Contains(redirectUri)) {
				throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "La URI de redirección es inválida", "Los parámetros para autorización son inválidos.");
			}

			return await cognitoHelper.ObtenerConAuthorizationCode(code, codeVerifier, redirectUri);
		}

		public async Task<(string accessToken, int expiresIn)> RefreshAccessToken(string? refreshToken) {
			if (string.IsNullOrWhiteSpace(refreshToken)) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "El refresh token viene vacío", "Los parámetros para refrescar la autorización son inválidos.");
			}

			return await cognitoHelper.ObtenerConRefreshToken(refreshToken);
		}
	}
}
