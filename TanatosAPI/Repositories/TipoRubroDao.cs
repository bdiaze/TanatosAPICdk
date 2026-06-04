using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	public class TipoRubroDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<TipoRubro?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null) {
            string query =
                "SELECT ID, NOMBRE, DESCRIPCION, VIGENCIA FROM TANATOS.TIPO_RUBRO WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("ID", id);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                TipoRubro? retorno = null;
                if (await reader.ReadAsync()) {
                    retorno = new TipoRubro {
                        Id = reader.GetInt64(0),
                        Nombre = reader.GetString(1),
						Descripcion = await reader.IsDBNullAsync(2) ? null : reader.GetString(2),
                        Vigencia = reader.GetBoolean(3)
                    };
                }
                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task<List<TipoRubro>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null) {
			string query =
                "SELECT ID, NOMBRE, DESCRIPCION, VIGENCIA FROM TANATOS.TIPO_RUBRO " +
                "WHERE (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                List<TipoRubro> retorno = [];
                while (await reader.ReadAsync()) {
                    retorno.Add(new TipoRubro {
                        Id = reader.GetInt64(0),
                        Nombre = reader.GetString(1),
                        Descripcion = await reader.IsDBNullAsync(2) ? null : reader.GetString(2),
                        Vigencia = reader.GetBoolean(3)
                    });
                }
                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task Insertar(TipoRubro item, NpgsqlTransaction? transaction = null) {
			string query =
                "INSERT INTO TANATOS.TIPO_RUBRO(ID, NOMBRE, DESCRIPCION, VIGENCIA) " +
                "VALUES (@ID, @NOMBRE, @DESCRIPCION, @VIGENCIA)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("ID", item.Id);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
                command.Parameters.AddWithValue("DESCRIPCION", (object?)item.Descripcion ?? DBNull.Value);
                command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
                await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task Actualizar(TipoRubro item, NpgsqlTransaction? transaction = null) {
			string query =
                "UPDATE TANATOS.TIPO_RUBRO SET NOMBRE = @NOMBRE, DESCRIPCION = @DESCRIPCION, VIGENCIA = @VIGENCIA " +
                "WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
                command.Parameters.AddWithValue("DESCRIPCION", (object?)item.Descripcion ?? DBNull.Value);
                command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
                command.Parameters.AddWithValue("ID", item.Id);
                await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task Eliminar(long id, NpgsqlTransaction? transaction = null) {
            string query =
                "DELETE FROM TANATOS.TIPO_RUBRO WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("ID", id);
                await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }
	}
}
