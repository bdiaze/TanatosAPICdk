using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	public class NotificacionNormaSuscritaDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<NotificacionNormaSuscrita>> ObtenerPorNormaSuscrita(long idNormaSuscrita, bool? vigencia = true, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, ID_NORMA_SUSCRITA, ID_TIPO_UNIDAD_TIEMPO_ANTELACION, CANT_ANTELACION, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.NOTIFICACION_NORMA_SUSCRITA " +
				"WHERE ID_NORMA_SUSCRITA = @IDNORMASUSCRITA AND (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("IDNORMASUSCRITA", idNormaSuscrita);
				command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<NotificacionNormaSuscrita> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new NotificacionNormaSuscrita {
						Id = reader.GetInt64(0),
						IdNormaSuscrita = reader.GetInt64(1),
						IdTipoUnidadTiempoAntelacion = reader.GetInt64(2),
						CantAntelacion = reader.GetInt32(3),
						FechaCreacion = await reader.IsDBNullAsync(4) ? null : reader.GetDateTime(4),
						FechaEliminacion = await reader.IsDBNullAsync(5) ? null : reader.GetDateTime(5),
						Vigencia = reader.GetBoolean(6)
					});
				}
				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<long> Insertar(NotificacionNormaSuscrita item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.NOTIFICACION_NORMA_SUSCRITA(ID_NORMA_SUSCRITA, ID_TIPO_UNIDAD_TIEMPO_ANTELACION, CANT_ANTELACION, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@IDNORMASUSCRITA, @IDTIPOUNIDADTIEMPOANTELACION, @CANTANTELACION, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("IDNORMASUSCRITA", item.IdNormaSuscrita);
                command.Parameters.AddWithValue("IDTIPOUNIDADTIEMPOANTELACION", item.IdTipoUnidadTiempoAntelacion);
                command.Parameters.AddWithValue("CANTANTELACION", item.CantAntelacion);
                command.Parameters.AddWithValue("FECHACREACION", (object?)item.FechaCreacion ?? DBNull.Value);
                command.Parameters.AddWithValue("FECHAELIMINACION", (object?)item.FechaEliminacion ?? DBNull.Value);
                command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
                return Convert.ToInt64(await command.ExecuteScalarAsync());
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
		}

		public async Task Actualizar(NotificacionNormaSuscrita item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.NOTIFICACION_NORMA_SUSCRITA SET ID_NORMA_SUSCRITA = @IDNORMASUSCRITA, " +
				"ID_TIPO_UNIDAD_TIEMPO_ANTELACION = @IDTIPOUNIDADTIEMPOANTELACION, CANT_ANTELACION = @CANTANTELACION, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("IDNORMASUSCRITA", item.IdNormaSuscrita);
                command.Parameters.AddWithValue("IDTIPOUNIDADTIEMPOANTELACION", item.IdTipoUnidadTiempoAntelacion);
                command.Parameters.AddWithValue("CANTANTELACION", item.CantAntelacion);
                command.Parameters.AddWithValue("FECHACREACION", (object?)item.FechaCreacion ?? DBNull.Value);
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
