using System.Security.Cryptography;
using System.Text;

namespace TanatosAPI.Helpers {
	public class CryptoHelper {
		public string GenerarToken(int bytes = 32) {
			return Convert.ToBase64String(RandomNumberGenerator.GetBytes(bytes));
		}

		public string HashSHA256(string input) {
			return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
		}
	}
}
