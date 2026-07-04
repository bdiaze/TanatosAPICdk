using Npgsql;
using System.Transactions;

namespace TanatosAPI.Interfaces.Helpers {
	public interface IDatabaseTransaction : IAsyncDisposable {
		public NpgsqlTransaction NpgsqlTransaction();
		public Task CommitAsync();
		public Task RollbackAsync();
	}
}
