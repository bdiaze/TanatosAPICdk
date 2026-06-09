
using Npgsql;

namespace TanatosAPI.Interfaces {
	public interface IDatabaseConnection : IAsyncDisposable {
		public NpgsqlConnection NpgsqlConnection();
		public Task<IDatabaseTransaction> BeginTransactionAsync();
	}
}
