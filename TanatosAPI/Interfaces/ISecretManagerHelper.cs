namespace TanatosAPI.Interfaces {
	public interface ISecretManagerHelper {
		public Task<string> ObtenerSecreto(string secretArn);
	}
}
