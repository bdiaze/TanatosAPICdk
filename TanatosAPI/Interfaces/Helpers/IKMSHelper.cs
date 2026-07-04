namespace TanatosAPI.Interfaces.Helpers {
	public interface IKMSHelper {
		public Task<string> Desencriptar(string encryptedBase64);
	}
}
