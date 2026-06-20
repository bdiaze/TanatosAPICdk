using Npgsql;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Helpers {
	[ExcludeFromCodeCoverage]
	public class DatabaseTransactionWrapper(NpgsqlTransaction transaction) : IDatabaseTransaction {
		public NpgsqlTransaction NpgsqlTransaction() => transaction;
		public Task CommitAsync() => transaction.CommitAsync();
		public Task RollbackAsync() => transaction.RollbackAsync();
		public ValueTask DisposeAsync() => transaction.DisposeAsync();
	}
}
