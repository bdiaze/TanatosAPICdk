using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	public class TipoActividadDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<TipoActividad?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null) {
            string query =
                "SELECT ID, ID_TIPO_RUBRO, NOMBRE, DESCRIPCION, VIGENCIA FROM TANATOS.TIPO_ACTIVIDAD WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("ID", id);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                TipoActividad? retorno = null;
                if (await reader.ReadAsync()) {
                    retorno = new TipoActividad {
						Id = reader.GetInt64(0),
						IdTipoRubro = reader.GetInt64(1),
						Nombre = reader.GetString(2),
						Descripcion = await reader.IsDBNullAsync(3) ? null : reader.GetString(3),
						Vigencia = reader.GetBoolean(4),
                    };
                }
                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
		}

		public async Task<List<TipoActividad>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null) {
			string query =
                "SELECT ID, ID_TIPO_RUBRO, NOMBRE, DESCRIPCION, VIGENCIA FROM TANATOS.TIPO_ACTIVIDAD " +
                "WHERE (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                List<TipoActividad> retorno = [];
                while (await reader.ReadAsync()) {
                    retorno.Add(new TipoActividad {
                        Id = reader.GetInt64(0),
                        IdTipoRubro = reader.GetInt64(1),
                        Nombre = reader.GetString(2),
                        Descripcion = await reader.IsDBNullAsync(3) ? null : reader.GetString(3),
                        Vigencia = reader.GetBoolean(4),
                    });
                }
                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task Insertar(TipoActividad item, NpgsqlTransaction? transaction = null) {
			string query =
                "INSERT INTO TANATOS.TIPO_ACTIVIDAD(ID, ID_TIPO_RUBRO, NOMBRE, DESCRIPCION, VIGENCIA) " +
                "VALUES (@ID, @IDTIPORUBRO, @NOMBRE, @DESCRIPCION, @VIGENCIA)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("ID", item.Id);
                command.Parameters.AddWithValue("IDTIPORUBRO", item.IdTipoRubro);
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

		public async Task Actualizar(TipoActividad item, NpgsqlTransaction? transaction = null) {
			string query =
                "UPDATE TANATOS.TIPO_ACTIVIDAD SET ID_TIPO_RUBRO = @IDTIPORUBRO, NOMBRE = @NOMBRE, " +
                "DESCRIPCION = @DESCRIPCION, VIGENCIA = @VIGENCIA WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("IDTIPORUBRO", item.IdTipoRubro);
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
                "DELETE FROM TANATOS.TIPO_ACTIVIDAD WHERE ID = @ID";

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
