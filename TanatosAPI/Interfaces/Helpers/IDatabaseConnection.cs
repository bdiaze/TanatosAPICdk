
using Npgsql;

namespace TanatosAPI.Interfaces.Helpers {
	public interface IDatabaseConnection : IAsyncDisposable {
		public NpgsqlConnection NpgsqlConnection();
		public Task<IDatabaseTransaction> BeginTransactionAsync();
	}
}
