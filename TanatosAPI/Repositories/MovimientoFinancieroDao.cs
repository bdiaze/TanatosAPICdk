using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
	public class MovimientoFinancieroDao(IDatabaseConnectionHelper connectionHelper) : IMovimientoFinancieroDao {
		public async Task<MovimientoFinanciero?> Obtener(long id, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, ID_CUENTA_FINANCIERA, ID_CATEGORIA_FINANCIERA, TIPO_MOVIMIENTO, " +
				"MONTO, SALDO, FECHA_MOVIMIENTO, DESCRIPCION, " +
				"FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.MOVIMIENTO_FINANCIERO " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("ID", id);

				await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

				MovimientoFinanciero? retorno = null;
				if (await reader.ReadAsync()) {
					retorno = new MovimientoFinanciero {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						IdCuentaFinanciera = reader.GetInt64(reader.GetOrdinal("ID_CUENTA_FINANCIERA")),
						IdCategoriaFinanciera = await reader.IsDBNullAsync(reader.GetOrdinal("ID_CATEGORIA_FINANCIERA")) ? null : reader.GetInt64(reader.GetOrdinal("ID_CATEGORIA_FINANCIERA")),
						TipoMovimiento = reader.GetInt16(reader.GetOrdinal("TIPO_MOVIMIENTO")),
						Monto = reader.GetDecimal(reader.GetOrdinal("MONTO")),
						Saldo = reader.GetDecimal(reader.GetOrdinal("SALDO")),
						FechaMovimiento = reader.GetDateTime(reader.GetOrdinal("FECHA_MOVIMIENTO")),
						Descripcion = await reader.IsDBNullAsync(reader.GetOrdinal("DESCRIPCION")) ? null : reader.GetString(reader.GetOrdinal("DESCRIPCION")),
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

		public async Task<List<MovimientoFinanciero>> ObtenerConPaginacion(long idCuentaFinanciera, DateTime? fechaDesde = null, DateTime? fechaHasta = null, short estado = 1, bool? vigencia = true, DateTime? primeraFecha = null, long? primerId = null, int cantidad = 50, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, ID_CUENTA_FINANCIERA, ID_CATEGORIA_FINANCIERA, TIPO_MOVIMIENTO, " +
				"MONTO, SALDO, FECHA_MOVIMIENTO, DESCRIPCION, ESTADO, " +
				"FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.MOVIMIENTO_FINANCIERO " +
				"WHERE ID_CUENTA_FINANCIERA = @IDCUENTAFINANCIERA AND " +
				"(FECHA_MOVIMIENTO >= @FECHADESDE OR @FECHADESDE IS NULL) AND " +
				"(FECHA_MOVIMIENTO <= @FECHAHASTA OR @FECHAHASTA IS NULL) AND " +
				"ESTADO = @ESTADO AND " +
				"(VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL) ";
			
			if (primeraFecha != null && primerId != null)
				query += "AND (FECHA_MOVIMIENTO, ID) <= (@PRIMERAFECHA, @PRIMERID) ";

			query += "ORDER BY FECHA_MOVIMIENTO DESC, ID DESC " +
					 "LIMIT @CANTIDAD";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("IDCUENTAFINANCIERA", idCuentaFinanciera);
				command.Parameters.AddWithValue("FECHADESDE", (object?)fechaDesde ?? DBNull.Value);
				command.Parameters.AddWithValue("FECHAHASTA", (object?)fechaHasta ?? DBNull.Value);
				command.Parameters.AddWithValue("ESTADO", estado);
				command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);
				if (primeraFecha != null && primerId != null) {
					command.Parameters.AddWithValue("PRIMERAFECHA", primeraFecha);
					command.Parameters.AddWithValue("PRIMERID", primerId.Value);
				}
				command.Parameters.AddWithValue("CANTIDAD", cantidad);

				await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

				List<MovimientoFinanciero> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new MovimientoFinanciero {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						IdCuentaFinanciera = reader.GetInt64(reader.GetOrdinal("ID_CUENTA_FINANCIERA")),
						IdCategoriaFinanciera = await reader.IsDBNullAsync(reader.GetOrdinal("ID_CATEGORIA_FINANCIERA")) ? null : reader.GetInt64(reader.GetOrdinal("ID_CATEGORIA_FINANCIERA")),
						TipoMovimiento = reader.GetInt16(reader.GetOrdinal("TIPO_MOVIMIENTO")),
						Monto = reader.GetDecimal(reader.GetOrdinal("MONTO")),
						Saldo = reader.GetDecimal(reader.GetOrdinal("SALDO")),
						FechaMovimiento = reader.GetDateTime(reader.GetOrdinal("FECHA_MOVIMIENTO")),
						Descripcion = await reader.IsDBNullAsync(reader.GetOrdinal("DESCRIPCION")) ? null : reader.GetString(reader.GetOrdinal("DESCRIPCION")),
						Estado = reader.GetInt16(reader.GetOrdinal("ESTADO")),
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

		public async Task<List<MovimientoFinanciero>> ObtenerPrevios(long idCuentaFinanciera, DateTime fechaReferencia, long? idReferencia = null, short estado = 1, bool? vigencia = true, int cantidad = 1, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, ID_CUENTA_FINANCIERA, ID_CATEGORIA_FINANCIERA, TIPO_MOVIMIENTO, " +
				"MONTO, SALDO, FECHA_MOVIMIENTO, DESCRIPCION, " +
				"FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.MOVIMIENTO_FINANCIERO " +
				"WHERE ID_CUENTA_FINANCIERA = @IDCUENTAFINANCIERA AND " +
				"ESTADO = @ESTADO AND " +
				"(VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL) ";

			if (idReferencia != null) query += "AND (FECHA_MOVIMIENTO, ID) < (@FECHAREFERENCIA, @IDREFERENCIA) ";
			else query += "AND FECHA_MOVIMIENTO <= @FECHAREFERENCIA ";

			query += "ORDER BY FECHA_MOVIMIENTO DESC, ID DESC " +
						"LIMIT @CANTIDAD";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("IDCUENTAFINANCIERA", idCuentaFinanciera);
				command.Parameters.AddWithValue("FECHAREFERENCIA", fechaReferencia);
				if (idReferencia != null) {
					command.Parameters.AddWithValue("IDREFERENCIA", idReferencia.Value);
				}
				command.Parameters.AddWithValue("ESTADO", estado);
				command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);
				command.Parameters.AddWithValue("CANTIDAD", cantidad);

				await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

				List<MovimientoFinanciero> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new MovimientoFinanciero {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						IdCuentaFinanciera = reader.GetInt64(reader.GetOrdinal("ID_CUENTA_FINANCIERA")),
						IdCategoriaFinanciera = await reader.IsDBNullAsync(reader.GetOrdinal("ID_CATEGORIA_FINANCIERA")) ? null : reader.GetInt64(reader.GetOrdinal("ID_CATEGORIA_FINANCIERA")),
						TipoMovimiento = reader.GetInt16(reader.GetOrdinal("TIPO_MOVIMIENTO")),
						Monto = reader.GetDecimal(reader.GetOrdinal("MONTO")),
						Saldo = reader.GetDecimal(reader.GetOrdinal("SALDO")),
						FechaMovimiento = reader.GetDateTime(reader.GetOrdinal("FECHA_MOVIMIENTO")),
						Descripcion = await reader.IsDBNullAsync(reader.GetOrdinal("DESCRIPCION")) ? null : reader.GetString(reader.GetOrdinal("DESCRIPCION")),
						Estado = reader.GetInt16(reader.GetOrdinal("ESTADO")),
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

		public async Task<List<MovimientoFinanciero>> ObtenerPosteriores(long idCuentaFinanciera, DateTime fechaReferencia, long idReferencia, short estado = 1, bool? vigencia = true, bool incluirReferencia = false, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, ID_CUENTA_FINANCIERA, ID_CATEGORIA_FINANCIERA, TIPO_MOVIMIENTO, " +
				"MONTO, SALDO, FECHA_MOVIMIENTO, DESCRIPCION, " +
				"FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.MOVIMIENTO_FINANCIERO " +
				"WHERE ID_CUENTA_FINANCIERA = @IDCUENTAFINANCIERA AND " +
				"ESTADO = @ESTADO AND " +
				"(VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL) ";

			if (incluirReferencia) query += "AND (FECHA_MOVIMIENTO, ID) >= (@FECHAREFERENCIA, @IDREFERENCIA) ";
			else query += "AND (FECHA_MOVIMIENTO, ID) > (@FECHAREFERENCIA, @IDREFERENCIA) ";

			query += "ORDER BY FECHA_MOVIMIENTO, ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("IDCUENTAFINANCIERA", idCuentaFinanciera);
				command.Parameters.AddWithValue("FECHAREFERENCIA", fechaReferencia);
				command.Parameters.AddWithValue("IDREFERENCIA", idReferencia);
				command.Parameters.AddWithValue("ESTADO", estado);
				command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

				await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

				List<MovimientoFinanciero> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new MovimientoFinanciero {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						IdCuentaFinanciera = reader.GetInt64(reader.GetOrdinal("ID_CUENTA_FINANCIERA")),
						IdCategoriaFinanciera = await reader.IsDBNullAsync(reader.GetOrdinal("ID_CATEGORIA_FINANCIERA")) ? null : reader.GetInt64(reader.GetOrdinal("ID_CATEGORIA_FINANCIERA")),
						TipoMovimiento = reader.GetInt16(reader.GetOrdinal("TIPO_MOVIMIENTO")),
						Monto = reader.GetDecimal(reader.GetOrdinal("MONTO")),
						Saldo = reader.GetDecimal(reader.GetOrdinal("SALDO")),
						FechaMovimiento = reader.GetDateTime(reader.GetOrdinal("FECHA_MOVIMIENTO")),
						Descripcion = await reader.IsDBNullAsync(reader.GetOrdinal("DESCRIPCION")) ? null : reader.GetString(reader.GetOrdinal("DESCRIPCION")),
						Estado = reader.GetInt16(reader.GetOrdinal("ESTADO")),
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

		public async Task<long> Insertar(MovimientoFinanciero item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.MOVIMIENTO_FINANCIERO(ID_CUENTA_FINANCIERA, ID_CATEGORIA_FINANCIERA, TIPO_MOVIMIENTO, MONTO, SALDO, FECHA_MOVIMIENTO, DESCRIPCION, ESTADO, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@IDCUENTAFINANCIERA, @IDCATEGORIAFINANCIERA, @TIPOMOVIMIENTO, @MONTO, @SALDO, @FECHAMOVIMIENTO, @DESCRIPCION, @ESTADO, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("IDCUENTAFINANCIERA", item.IdCuentaFinanciera);
				command.Parameters.AddWithValue("IDCATEGORIAFINANCIERA", (object?)item.IdCategoriaFinanciera ?? DBNull.Value);
				command.Parameters.AddWithValue("TIPOMOVIMIENTO", item.TipoMovimiento);
				command.Parameters.AddWithValue("MONTO", item.Monto);
				command.Parameters.AddWithValue("SALDO", item.Saldo);
				command.Parameters.AddWithValue("FECHAMOVIMIENTO", item.FechaMovimiento);
				command.Parameters.AddWithValue("DESCRIPCION", (object?)item.Descripcion ?? DBNull.Value);
				command.Parameters.AddWithValue("FECHACREACION", item.FechaCreacion);
				command.Parameters.AddWithValue("FECHAELIMINACION", (object?)item.FechaEliminacion ?? DBNull.Value);
				command.Parameters.AddWithValue("ESTADO", item.Estado);
				command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
				return Convert.ToInt64(await command.ExecuteScalarAsync());
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task Actualizar(MovimientoFinanciero item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.MOVIMIENTO_FINANCIERO SET " +
				"ID_CUENTA_FINANCIERA = @IDCUENTAFINANCIERA, ID_CATEGORIA_FINANCIERA = @IDCATEGORIAFINANCIERA, TIPO_MOVIMIENTO = @TIPOMOVIMIENTO, " +
				"MONTO = @MONTO, SALDO = @SALDO, FECHA_MOVIMIENTO = @FECHAMOVIMIENTO, DESCRIPCION = @DESCRIPCION, ESTADO = @ESTADO, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("IDCUENTAFINANCIERA", item.IdCuentaFinanciera);
				command.Parameters.AddWithValue("IDCATEGORIAFINANCIERA", (object?)item.IdCategoriaFinanciera ?? DBNull.Value);
				command.Parameters.AddWithValue("TIPOMOVIMIENTO", item.TipoMovimiento);
				command.Parameters.AddWithValue("MONTO", item.Monto);
				command.Parameters.AddWithValue("SALDO", item.Saldo);
				command.Parameters.AddWithValue("FECHAMOVIMIENTO", item.FechaMovimiento);
				command.Parameters.AddWithValue("DESCRIPCION", (object?)item.Descripcion ?? DBNull.Value);
				command.Parameters.AddWithValue("ESTADO", item.Estado);
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
