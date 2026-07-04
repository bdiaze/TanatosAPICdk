namespace TanatosAPI.Interfaces.Helpers {
	public interface IFileHelper {
		public Task<string> ReadAllTextAsync(string path);
		public bool Exists(string path);
	}
}
