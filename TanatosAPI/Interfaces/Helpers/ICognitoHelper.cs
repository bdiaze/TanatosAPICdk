namespace TanatosAPI.Interfaces.Helpers {
	public interface ICognitoHelper {
		public Task<Dictionary<string, string>> ObtenerUsuario(string sub);
		public Task ConfirmarRegistro(string username, string confirmationCode);
		public Task ReenviarCodigoVerificacion(string username);
        public Task<(string accessToken, string refreshToken, int expiresIn, int refreshExpiresIn)> ObtenerConAuthorizationCode(string code, string codeVerifier, string redirectUri);
		public Task<(string accessToken, int expiresIn)> ObtenerConRefreshToken(string refreshToken);
	}
}
