using Dapper;
using Npgsql;
using System.Data;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class HistorialNotificacionDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<HistorialNotificacion>> ObtenerPorHistorial(long idHistorialNormaSuscrita, DateTime? fechaEjecucion = null) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<HistorialNotificacion>(
				"SELECT ID, ID_HISTORIAL_NORMA_SUSCRITA, ID_DESTINATARIO_NOTIFICACION, FECHA_PROGRAMACION, FECHA_EJECUCION FROM TANATOS.HISTORIAL_NOTIFICACION " +
				"WHERE ID_HISTORIAL_NORMA_SUSCRITA = @IDHISTORIALNORMASUSCRITA AND FECHA_EJECUCION IS NOT DISTINCT FROM @FECHAEJECUCION",
				new { idHistorialNormaSuscrita, fechaEjecucion }
			)];
		}

		public async Task<long> Insertar(HistorialNotificacion item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.HISTORIAL_NOTIFICACION(ID_HISTORIAL_NORMA_SUSCRITA, ID_DESTINATARIO_NOTIFICACION, FECHA_PROGRAMACION, FECHA_EJECUCION) " +
				"VALUES (@IDHISTORIALNORMASUSCRITA, @IDDESTINATARIONOTIFICACION, @FECHAPROGRAMACION, @FECHAEJECUCION) " +
				"RETURNING ID";
			DynamicParameters param = new();
			param.Add("IDHISTORIALNORMASUSCRITA", item.IdHistorialNormaSuscrita);
			param.Add("IDDESTINATARIONOTIFICACION", item.IdDestinatarioNotificacion);
			param.Add("FECHAPROGRAMACION", item.FechaProgramacion);
			param.Add("FECHAEJECUCION", item.FechaEjecucion);

			if (transaction?.Connection != null) {
				return await transaction!.Connection!.ExecuteScalarAsync<long>(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				return await connection.ExecuteScalarAsync<long>(query, param);
			}
		}

		public async Task Actualizar(HistorialNotificacion item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.HISTORIAL_NOTIFICACION SET ID_HISTORIAL_NORMA_SUSCRITA = @IDHISTORIALNORMASUSCRITA, ID_DESTINATARIO_NOTIFICACION = @IDDESTINATARIONOTIFICACION, " +
				"FECHA_PROGRAMACION = @FECHAPROGRAMACION, FECHA_EJECUCION = @FECHAEJECUCION " +
				"WHERE ID = @ID";
			DynamicParameters param = new();
			param.Add("IDHISTORIALNORMASUSCRITA", item.IdHistorialNormaSuscrita);
			param.Add("IDDESTINATARIONOTIFICACION", item.IdDestinatarioNotificacion);
			param.Add("FECHAPROGRAMACION", item.FechaProgramacion);
			param.Add("FECHAEJECUCION", item.FechaEjecucion);
			param.Add("ID", item.Id);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}

		public async Task Eliminar(long id, NpgsqlTransaction? transaction = null) {
			string query = "DELETE FROM TANATOS.HISTORIAL_NOTIFICACION WHERE ID = @ID";
			DynamicParameters param = new();
			param.Add("ID", id);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}
	}
}
