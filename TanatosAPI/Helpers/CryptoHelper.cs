using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace TanatosAPI.Helpers {
	[ExcludeFromCodeCoverage]
	public static class CryptoHelper {
		public static string GenerarToken(int bytes = 32) {
			return Convert.ToBase64String(RandomNumberGenerator.GetBytes(bytes));
		}

		public static string HashSHA256(string input) {
			return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
		}
	}
}
