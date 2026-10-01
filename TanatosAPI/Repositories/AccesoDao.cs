using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
	public class AccesoDao(IDatabaseConnectionHelper connectionHelper) : IAccesoDao {
		public async Task<Acceso?> ObtenerPorHashCodigo(string hashCodigo, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, HASH_CODIGO, FECHA_EXPIRACION, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.ACCESO " +
				"WHERE HASH_CODIGO = @HASHCODIGO";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("HASHCODIGO", hashCodigo);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				Acceso? retorno = null;
				if (await reader.ReadAsync()) {
					retorno = new Acceso {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						HashCodigo = reader.GetString(reader.GetOrdinal("HASH_CODIGO")),
						FechaExpiracion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_EXPIRACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_EXPIRACION")),
						FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FECHA_CREACION")),
						FechaEliminacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_ELIMINACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_ELIMINACION")),
						Vigencia = reader.GetBoolean(reader.GetOrdinal("VIGENCIA"))
					};
				}
				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<long> Insertar(Acceso item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.ACCESO(HASH_CODIGO, FECHA_EXPIRACION, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@HASHCODIGO, @FECHAEXPIRACION, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("HASHCODIGO", item.HashCodigo);
				command.Parameters.AddWithValue("FECHAEXPIRACION", (object?)item.FechaExpiracion ?? DBNull.Value);
				command.Parameters.AddWithValue("FECHACREACION", item.FechaCreacion);
				command.Parameters.AddWithValue("FECHAELIMINACION", (object?)item.FechaEliminacion ?? DBNull.Value);
				command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
				return Convert.ToInt64(await command.ExecuteScalarAsync());
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task Actualizar(Acceso item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.ACCESO SET HASH_CODIGO = @HASHCODIGO, FECHA_EXPIRACION = @FECHAEXPIRACION, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("HASHCODIGO", item.HashCodigo);
				command.Parameters.AddWithValue("FECHAEXPIRACION", (object?)item.FechaExpiracion ?? DBNull.Value);
				command.Parameters.AddWithValue("FECHACREACION", item.FechaCreacion);
				command.Parameters.AddWithValue("FECHAELIMINACION", (object?)item.FechaEliminacion ?? DBNull.Value);
				command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
				command.Parameters.AddWithValue("ID", item.Id);
				await command.ExecuteNonQueryAsync();
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}
	}
}
