using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Repositories {
    [ExcludeFromCodeCoverage]
    public class TipoUnidadTiempoDao(IDatabaseConnectionHelper connectionHelper) {
		public async Task<TipoUnidadTiempo?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, NOMBRE, NOMBRE_PLURAL, CANT_SEGUNDOS, CANT_MINUTOS, CANT_HORAS, CANT_DIAS, VIGENCIA FROM TANATOS.TIPO_UNIDAD_TIEMPO " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("ID", id);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				TipoUnidadTiempo? retorno = null;

				if (await reader.ReadAsync()) {
					retorno = new TipoUnidadTiempo {
						Id = reader.GetInt64(0),
						Nombre = reader.GetString(1),
						NombrePlural = await reader.IsDBNullAsync(2) ? null : reader.GetString(2),
						CantSegundos = reader.GetInt64(3),
						CantMinutos = await reader.IsDBNullAsync(4) ? null : reader.GetInt64(4),
						CantHoras = await reader.IsDBNullAsync(5) ? null : reader.GetInt64(5),
						CantDias = await reader.IsDBNullAsync(6) ? null : reader.GetInt64(6),
						Vigencia = reader.GetBoolean(7)
					};
				}

				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<List<TipoUnidadTiempo>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, NOMBRE, NOMBRE_PLURAL, CANT_SEGUNDOS, CANT_MINUTOS, CANT_HORAS, CANT_DIAS, VIGENCIA FROM TANATOS.TIPO_UNIDAD_TIEMPO " +
				"WHERE (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<TipoUnidadTiempo> retorno = [];

				while (await reader.ReadAsync()) {
					retorno.Add(new TipoUnidadTiempo {
						Id = reader.GetInt64(0),
						Nombre = reader.GetString(1),
                        NombrePlural = await reader.IsDBNullAsync(2) ? null : reader.GetString(2),
                        CantSegundos = reader.GetInt64(3),
						CantMinutos = await reader.IsDBNullAsync(4) ? null : reader.GetInt64(4),
						CantHoras = await reader.IsDBNullAsync(5) ? null : reader.GetInt64(5),
						CantDias = await reader.IsDBNullAsync(6) ? null : reader.GetInt64(6),
						Vigencia = reader.GetBoolean(7)
					});
				}

				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task Insertar(TipoUnidadTiempo item, NpgsqlTransaction? transaction = null) {
			string query =
                "INSERT INTO TANATOS.TIPO_UNIDAD_TIEMPO(ID, NOMBRE, NOMBRE_PLURAL, CANT_SEGUNDOS, CANT_MINUTOS, CANT_HORAS, CANT_DIAS, VIGENCIA) " +
                "VALUES (@ID, @NOMBRE, @NOMBREPLURAL, @CANTSEGUNDOS, @CANTMINUTOS, @CANTHORAS, @CANTDIAS, @VIGENCIA)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("ID", item.Id);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
                command.Parameters.AddWithValue("NOMBREPLURAL", (object?)item.NombrePlural ?? DBNull.Value);
                command.Parameters.AddWithValue("CANTSEGUNDOS", item.CantSegundos);
                command.Parameters.AddWithValue("CANTMINUTOS", (object?)item.CantMinutos ?? DBNull.Value);
                command.Parameters.AddWithValue("CANTHORAS", (object?)item.CantHoras ?? DBNull.Value);
                command.Parameters.AddWithValue("CANTDIAS", (object?)item.CantDias ?? DBNull.Value);
                command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
                await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task Actualizar(TipoUnidadTiempo item, NpgsqlTransaction? transaction = null) {
			string query =
                "UPDATE TANATOS.TIPO_UNIDAD_TIEMPO SET NOMBRE = @NOMBRE, NOMBRE_PLURAL = @NOMBREPLURAL, CANT_SEGUNDOS = @CANTSEGUNDOS, CANT_MINUTOS = @CANTMINUTOS, " +
                "CANT_HORAS = @CANTHORAS, CANT_DIAS = @CANTDIAS, VIGENCIA = @VIGENCIA WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
                command.Parameters.AddWithValue("NOMBREPLURAL", (object?)item.NombrePlural ?? DBNull.Value);
                command.Parameters.AddWithValue("CANTSEGUNDOS", item.CantSegundos);
                command.Parameters.AddWithValue("CANTMINUTOS", (object?)item.CantMinutos ?? DBNull.Value);
                command.Parameters.AddWithValue("CANTHORAS", (object?)item.CantHoras ?? DBNull.Value);
                command.Parameters.AddWithValue("CANTDIAS", (object?)item.CantDias ?? DBNull.Value);
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
                "DELETE FROM TANATOS.TIPO_UNIDAD_TIEMPO WHERE ID = @ID";

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
