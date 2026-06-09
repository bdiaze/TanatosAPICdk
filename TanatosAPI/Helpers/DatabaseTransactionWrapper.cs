using Npgsql;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Helpers {
	public class DatabaseTransactionWrapper(NpgsqlTransaction transaction) : IDatabaseTransaction {
		public NpgsqlTransaction NpgsqlTransaction() => transaction;
		public Task CommitAsync() => transaction.CommitAsync();
		public Task RollbackAsync() => transaction.RollbackAsync();
		public ValueTask DisposeAsync() => transaction.DisposeAsync();
	}
}
