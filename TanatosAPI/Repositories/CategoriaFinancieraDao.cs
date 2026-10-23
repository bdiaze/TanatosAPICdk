using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
	public class CategoriaFinancieraDao(IDatabaseConnectionHelper connectionHelper) : ICategoriaFinancieraDao {
		public async Task<CategoriaFinanciera?> Obtener(long id, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, ID_NEGOCIO, NOMBRE, TIPO_MOVIMIENTO, " +
				"FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.CATEGORIA_FINANCIERA " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("ID", id);

				await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

				CategoriaFinanciera? retorno = null;
				if (await reader.ReadAsync()) {
					retorno = new CategoriaFinanciera {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						Sub = reader.GetString(reader.GetOrdinal("SUB")),
						IdNegocio = reader.GetInt64(reader.GetOrdinal("ID_NEGOCIO")),
						Nombre = reader.GetString(reader.GetOrdinal("NOMBRE")),
						TipoMovimiento = reader.GetInt16(reader.GetOrdinal("TIPO_MOVIMIENTO")),
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

		public async Task<List<CategoriaFinanciera>> ObtenerPorSub(string sub, long? idNegocio = null, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, ID_NEGOCIO, NOMBRE, TIPO_MOVIMIENTO, " +
				"FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.CATEGORIA_FINANCIERA " +
				"WHERE SUB = @SUB AND (ID_NEGOCIO = @IDNEGOCIO OR @IDNEGOCIO IS NULL)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("SUB", sub);
				command.Parameters.AddWithValue("IDNEGOCIO", (object?)idNegocio ?? DBNull.Value);

				await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

				List<CategoriaFinanciera> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new CategoriaFinanciera {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						Sub = reader.GetString(reader.GetOrdinal("SUB")),
						IdNegocio = reader.GetInt64(reader.GetOrdinal("ID_NEGOCIO")),
						Nombre = reader.GetString(reader.GetOrdinal("NOMBRE")),
						TipoMovimiento = reader.GetInt16(reader.GetOrdinal("TIPO_MOVIMIENTO")),
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

		public async Task<long> Insertar(CategoriaFinanciera item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.CATEGORIA_FINANCIERA(SUB, ID_NEGOCIO, NOMBRE, TIPO_MOVIMIENTO, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@SUB, @IDNEGOCIO, @NOMBRE, @TIPOMOVIMIENTO, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("SUB", item.Sub);
				command.Parameters.AddWithValue("IDNEGOCIO", item.IdNegocio);
				command.Parameters.AddWithValue("NOMBRE", item.Nombre);
				command.Parameters.AddWithValue("TIPOMOVIMIENTO", item.TipoMovimiento);
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

		public async Task Actualizar(CategoriaFinanciera item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.CATEGORIA_FINANCIERA SET " +
				"SUB = @SUB, ID_NEGOCIO = @IDNEGOCIO, NOMBRE = @NOMBRE, TIPO_MOVIMIENTO = @TIPOMOVIMIENTO, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("SUB", item.Sub);
				command.Parameters.AddWithValue("IDNEGOCIO", item.IdNegocio);
				command.Parameters.AddWithValue("NOMBRE", item.Nombre);
				command.Parameters.AddWithValue("TIPOMOVIMIENTO", item.TipoMovimiento);
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
