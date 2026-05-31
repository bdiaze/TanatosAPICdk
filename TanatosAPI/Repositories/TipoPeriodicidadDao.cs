using Dapper;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using System.Data.Common;
using System.Text.Json;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class TipoPeriodicidadDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<TipoPeriodicidad?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null) {
			string query = "SELECT ID, NOMBRE, DESCRIPCION, CRON, DELTA_DIAS, DELTA_MESES, DELTA_ANNOS, VIGENCIA FROM TANATOS.TIPO_PERIODICIDAD WHERE ID = @ID";

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
						DeltaDias = await reader.IsDBNullAsync(4) ? null : reader.GetInt32(4),
						DeltaMeses = await reader.IsDBNullAsync(5) ? null : reader.GetInt32(5),
                        DeltaAnnos = await reader.IsDBNullAsync(6) ? null : reader.GetInt32(6),
                        Vigencia = reader.GetBoolean(7),
					};
				}

				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<List<TipoPeriodicidad>> ObtenerPorVigencia(bool? vigencia) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<TipoPeriodicidad>(
                "SELECT ID, NOMBRE, DESCRIPCION, CRON, DELTA_DIAS, DELTA_MESES, DELTA_ANNOS, VIGENCIA FROM TANATOS.TIPO_PERIODICIDAD WHERE (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)",
				new { vigencia }
			)];
		}

		public async Task Insertar(TipoPeriodicidad item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
                "INSERT INTO TANATOS.TIPO_PERIODICIDAD(ID, NOMBRE, DESCRIPCION, CRON, DELTA_DIAS, DELTA_MESES, DELTA_ANNOS, VIGENCIA) VALUES (@ID, @NOMBRE, @DESCRIPCION, @CRON, @DELTADIAS, @DELTAMESES, @DELTAANNOS, @VIGENCIA)",
				new { item.Id, item.Nombre, item.Descripcion, item.Cron, item.DeltaDias, item.DeltaMeses, item.DeltaAnnos, item.Vigencia }
			);
		}

		public async Task Actualizar(TipoPeriodicidad item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
                "UPDATE TANATOS.TIPO_PERIODICIDAD SET NOMBRE = @NOMBRE, DESCRIPCION = @DESCRIPCION, CRON = @CRON, DELTA_DIAS = @DELTADIAS, DELTA_MESES = @DELTAMESES, DELTA_ANNOS = @DELTAANNOS, VIGENCIA = @VIGENCIA WHERE ID = @ID",
				new { item.Nombre, item.Descripcion, item.Cron, item.DeltaDias, item.DeltaMeses, item.DeltaAnnos, item.Vigencia, item.Id }
			);
		}

		public async Task Eliminar(long id) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"DELETE FROM TANATOS.TIPO_PERIODICIDAD WHERE ID = @ID",
				new { id }
			);
		}
	}
}
