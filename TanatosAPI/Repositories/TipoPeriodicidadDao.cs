using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
    [ExcludeFromCodeCoverage]
    public class TipoPeriodicidadDao(IDatabaseConnectionHelper connectionHelper) : ITipoPeriodicidadDao {
		public async Task<TipoPeriodicidad?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null) {
			string query = "SELECT ID, NOMBRE, DESCRIPCION, CRON, FRECUENCIA_DIAS, DELTA_DIAS, DELTA_MESES, DELTA_ANNOS, VIGENCIA FROM TANATOS.TIPO_PERIODICIDAD WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("ID", id);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				TipoPeriodicidad? retorno = null;

				if (await reader.ReadAsync()) {
					retorno = new TipoPeriodicidad {
						Id = reader.GetInt64(0),
						Nombre = reader.GetString(1),
						Descripcion = await reader.IsDBNullAsync(2) ? null : reader.GetString(2),
						Cron = await reader.IsDBNullAsync(3) ? null : reader.GetString(3),
						FrecuenciaDias = await reader.IsDBNullAsync(4) ? null : reader.GetInt32(4),
						DeltaDias = await reader.IsDBNullAsync(5) ? null : reader.GetInt32(5),
						DeltaMeses = await reader.IsDBNullAsync(6) ? null : reader.GetInt32(6),
                        DeltaAnnos = await reader.IsDBNullAsync(7) ? null : reader.GetInt32(7),
                        Vigencia = reader.GetBoolean(8),
					};
				}

				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<List<TipoPeriodicidad>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null) {
			string query = "SELECT ID, NOMBRE, DESCRIPCION, CRON, FRECUENCIA_DIAS, DELTA_DIAS, DELTA_MESES, DELTA_ANNOS, VIGENCIA " +
                "FROM TANATOS.TIPO_PERIODICIDAD WHERE (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                List<TipoPeriodicidad> retorno = [];
                while (await reader.ReadAsync()) {
                    retorno.Add(new TipoPeriodicidad {
                        Id = reader.GetInt64(0),
                        Nombre = reader.GetString(1),
                        Descripcion = await reader.IsDBNullAsync(2) ? null : reader.GetString(2),
                        Cron = await reader.IsDBNullAsync(3) ? null : reader.GetString(3),
						FrecuenciaDias = await reader.IsDBNullAsync(4) ? null : reader.GetInt32(4),
						DeltaDias = await reader.IsDBNullAsync(5) ? null : reader.GetInt32(5),
						DeltaMeses = await reader.IsDBNullAsync(6) ? null : reader.GetInt32(6),
						DeltaAnnos = await reader.IsDBNullAsync(7) ? null : reader.GetInt32(7),
						Vigencia = reader.GetBoolean(8),
					});
                }
                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task Insertar(TipoPeriodicidad item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.TIPO_PERIODICIDAD(ID, NOMBRE, DESCRIPCION, CRON, FRECUENCIA_DIAS, DELTA_DIAS, DELTA_MESES, DELTA_ANNOS, VIGENCIA) " +
				"VALUES (@ID, @NOMBRE, @DESCRIPCION, @CRON, @FRECUENCIADIAS, @DELTADIAS, @DELTAMESES, @DELTAANNOS, @VIGENCIA)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("ID", item.Id);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
                command.Parameters.AddWithValue("DESCRIPCION", (object?)item.Descripcion ?? DBNull.Value);
                command.Parameters.AddWithValue("CRON", (object?)item.Cron ?? DBNull.Value);
				command.Parameters.AddWithValue("FRECUENCIADIAS", (object?)item.FrecuenciaDias ?? DBNull.Value);
				command.Parameters.AddWithValue("DELTADIAS", (object?)item.DeltaDias ?? DBNull.Value);
                command.Parameters.AddWithValue("DELTAMESES", (object?)item.DeltaMeses ?? DBNull.Value);
                command.Parameters.AddWithValue("DELTAANNOS", (object?)item.DeltaAnnos ?? DBNull.Value);
                command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
                await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task Actualizar(TipoPeriodicidad item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.TIPO_PERIODICIDAD SET NOMBRE = @NOMBRE, DESCRIPCION = @DESCRIPCION, CRON = @CRON, FRECUENCIA_DIAS = @FRECUENCIADIAS, " +
                "DELTA_DIAS = @DELTADIAS, DELTA_MESES = @DELTAMESES, DELTA_ANNOS = @DELTAANNOS, VIGENCIA = @VIGENCIA " +
                "WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
                command.Parameters.AddWithValue("DESCRIPCION", (object?)item.Descripcion ?? DBNull.Value);
                command.Parameters.AddWithValue("CRON", (object?)item.Cron ?? DBNull.Value);
				command.Parameters.AddWithValue("FRECUENCIADIAS", (object?)item.FrecuenciaDias ?? DBNull.Value);
				command.Parameters.AddWithValue("DELTADIAS", (object?)item.DeltaDias ?? DBNull.Value);
                command.Parameters.AddWithValue("DELTAMESES", (object?)item.DeltaMeses ?? DBNull.Value);
                command.Parameters.AddWithValue("DELTAANNOS", (object?)item.DeltaAnnos ?? DBNull.Value);
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
                "DELETE FROM TANATOS.TIPO_PERIODICIDAD WHERE ID = @ID";

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
