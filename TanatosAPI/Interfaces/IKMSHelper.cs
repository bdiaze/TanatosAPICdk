namespace TanatosAPI.Interfaces {
	public interface IKMSHelper {
		public Task<string> Desencriptar(string encryptedBase64);
	}
}
