using Npgsql;
using System.Data;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Helpers {
	public class DatabaseConnectionWrapper(NpgsqlConnection connection) : IDatabaseConnection {

		public NpgsqlConnection NpgsqlConnection() => connection;

		public async Task<IDatabaseTransaction> BeginTransactionAsync() {
			return new DatabaseTransactionWrapper(await connection.BeginTransactionAsync());
		}

		public ValueTask DisposeAsync() => connection.DisposeAsync();
	}
}
