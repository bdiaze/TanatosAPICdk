using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
	public class PermisoDao(IDatabaseConnectionHelper connectionHelper) : IPermisoDao {
		public async Task<List<Permiso>> ObtenerPorIdAcceso(long idAcceso, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, ID_ACCESO, ID_RECURSO, ACCION, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.PERMISO " +
				"WHERE ID_ACCESO = @IDACCESO";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("IDACCESO", idAcceso);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<Permiso> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new Permiso {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						IdAcceso = reader.GetInt64(reader.GetOrdinal("ID_ACCESO")),
						IdRecurso = reader.GetInt64(reader.GetOrdinal("ID_RECURSO")),
						Accion = reader.GetString(reader.GetOrdinal("ACCION")),
						FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FECHA_CREACION")),
						FechaEliminacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_ELIMINACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_ELIMINACION")),
						Vigencia = reader.GetBoolean(reader.GetOrdinal("VIGENCIA"))
					});
				}
				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<long> Insertar(Permiso item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.PERMISO(ID_ACCESO, ID_RECURSO, ACCION, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@IDACCESO, @IDRECURSO, @ACCION, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("IDACCESO", item.IdAcceso);
				command.Parameters.AddWithValue("IDRECURSO", item.IdRecurso);
				command.Parameters.AddWithValue("ACCION", item.Accion);
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

		public async Task Actualizar(Permiso item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.PERMISO SET ID_ACCESO = @IDACCESO, ID_RECURSO = @IDRECURSO, ACCION = @ACCION, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("IDACCESO", item.IdAcceso);
				command.Parameters.AddWithValue("IDRECURSO", item.IdRecurso);
				command.Parameters.AddWithValue("ACCION", item.Accion);
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
