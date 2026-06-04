using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	public class TipoFiscalizadorDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<TipoFiscalizador?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null) {
			string query =
                "SELECT ID, NOMBRE, NOMBRE_CORTO, VIGENCIA FROM TANATOS.TIPO_FISCALIZADOR WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("ID", id);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                TipoFiscalizador? retorno = null;
                if (await reader.ReadAsync()) {
                    retorno = new TipoFiscalizador {
                        Id = reader.GetInt64(0),
                        Nombre = reader.GetString(1),
						NombreCorto = await reader.IsDBNullAsync(2) ? null : reader.GetString(2),
                        Vigencia = reader.GetBoolean(3),
                    };
                }
                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task<List<TipoFiscalizador>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null) {
			string query =
                "SELECT ID, NOMBRE, NOMBRE_CORTO, VIGENCIA FROM TANATOS.TIPO_FISCALIZADOR " +
                "WHERE (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                List<TipoFiscalizador> retorno = [];
                while (await reader.ReadAsync()) {
                    retorno.Add(new TipoFiscalizador {
                        Id = reader.GetInt64(0),
                        Nombre = reader.GetString(1),
                        NombreCorto = await reader.IsDBNullAsync(2) ? null : reader.GetString(2),
                        Vigencia = reader.GetBoolean(3),
                    });
                }
                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task Insertar(TipoFiscalizador item, NpgsqlTransaction? transaction = null) {
			string query =
                "INSERT INTO TANATOS.TIPO_FISCALIZADOR(ID, NOMBRE, NOMBRE_CORTO, VIGENCIA) " +
                "VALUES (@ID, @NOMBRE, @NOMBRECORTO, @VIGENCIA)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("ID", item.Id);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
                command.Parameters.AddWithValue("NOMBRECORTO", (object?)item.NombreCorto ?? DBNull.Value);
                command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
                await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task Actualizar(TipoFiscalizador item, NpgsqlTransaction? transaction = null) {
			string query =
                "UPDATE TANATOS.TIPO_FISCALIZADOR SET NOMBRE = @NOMBRE, NOMBRE_CORTO = @NOMBRECORTO, " +
                "VIGENCIA = @VIGENCIA WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
                command.Parameters.AddWithValue("NOMBRECORTO", (object?)item.NombreCorto ?? DBNull.Value);
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
                "DELETE FROM TANATOS.TIPO_FISCALIZADOR WHERE ID = @ID";

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
