namespace TanatosAPI.Interfaces.Helpers {
	public interface ISecretManagerHelper {
		public Task<string> ObtenerSecreto(string secretArn);
	}
}
