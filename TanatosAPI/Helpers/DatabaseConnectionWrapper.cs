using Npgsql;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Helpers {
	[ExcludeFromCodeCoverage]
	public class DatabaseConnectionWrapper(NpgsqlConnection connection) : IDatabaseConnection {

		public NpgsqlConnection NpgsqlConnection() => connection;

		public async Task<IDatabaseTransaction> BeginTransactionAsync() {
			return new DatabaseTransactionWrapper(await connection.BeginTransactionAsync());
		}

		public ValueTask DisposeAsync() => connection.DisposeAsync();
	}
}
