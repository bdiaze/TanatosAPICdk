using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.Helpers {
	[ExcludeFromCodeCoverage]
	public class FileHelper : IFileHelper {
		public async Task<string> ReadAllTextAsync(string path) {
			return await File.ReadAllTextAsync(path);
		}

		public bool Exists(string path) {
			return File.Exists(path);
		}
	}
}
