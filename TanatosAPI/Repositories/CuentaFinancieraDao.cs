using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
	public class CuentaFinancieraDao(IDatabaseConnectionHelper connectionHelper) : ICuentaFinancieraDao {
		public async Task<CuentaFinanciera?> Obtener(long id, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, ID_NEGOCIO, NOMBRE, ID_TIPO_CUENTA_FINANCIERA, " +
				"SALDO_INICIAL, FECHA_SALDO_INICIAL, SALDO_ACTUAL, " +
				"FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.CUENTA_FINANCIERA " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("ID", id);

				await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

				CuentaFinanciera? retorno = null;
				if (await reader.ReadAsync()) {
					retorno = new CuentaFinanciera {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						Sub = reader.GetString(reader.GetOrdinal("SUB")),
						IdNegocio = reader.GetInt64(reader.GetOrdinal("ID_NEGOCIO")),
						Nombre = reader.GetString(reader.GetOrdinal("NOMBRE")),
						IdTipoCuentaFinanciera = reader.GetInt64(reader.GetOrdinal("ID_TIPO_CUENTA_FINANCIERA")),
						SaldoInicial = reader.GetDecimal(reader.GetOrdinal("SALDO_INICIAL")),
						FechaSaldoInicial = reader.GetDateTime(reader.GetOrdinal("FECHA_SALDO_INICIAL")),
						SaldoActual = reader.GetDecimal(reader.GetOrdinal("SALDO_ACTUAL")),
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

		public async Task<List<CuentaFinanciera>> ObtenerPorSub(string sub, long? idNegocio = null, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, ID_NEGOCIO, NOMBRE, ID_TIPO_CUENTA_FINANCIERA, " +
				"SALDO_INICIAL, FECHA_SALDO_INICIAL, SALDO_ACTUAL, " +
				"FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.CUENTA_FINANCIERA " +
				"WHERE SUB = @SUB AND (ID_NEGOCIO = @IDNEGOCIO OR @IDNEGOCIO IS NULL)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("SUB", sub);
				command.Parameters.AddWithValue("IDNEGOCIO", (object?)idNegocio ?? DBNull.Value);

				await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

				List<CuentaFinanciera> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new CuentaFinanciera {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						Sub = reader.GetString(reader.GetOrdinal("SUB")),
						IdNegocio = reader.GetInt64(reader.GetOrdinal("ID_NEGOCIO")),
						Nombre = reader.GetString(reader.GetOrdinal("NOMBRE")),
						IdTipoCuentaFinanciera = reader.GetInt64(reader.GetOrdinal("ID_TIPO_CUENTA_FINANCIERA")),
						SaldoInicial = reader.GetDecimal(reader.GetOrdinal("SALDO_INICIAL")),
						FechaSaldoInicial = reader.GetDateTime(reader.GetOrdinal("FECHA_SALDO_INICIAL")),
						SaldoActual = reader.GetDecimal(reader.GetOrdinal("SALDO_ACTUAL")),
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

		public async Task<long> Insertar(CuentaFinanciera item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.CUENTA_FINANCIERA(SUB, ID_NEGOCIO, NOMBRE, ID_TIPO_CUENTA_FINANCIERA, SALDO_INICIAL, FECHA_SALDO_INICIAL, SALDO_ACTUAL, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@SUB, @IDNEGOCIO, @NOMBRE, @IDTIPOCUENTAFINANCIERA, @SALDOINICIAL, @FECHASALDOINICIAL, @SALDOACTUAL, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("SUB", item.Sub);
				command.Parameters.AddWithValue("IDNEGOCIO", item.IdNegocio);
				command.Parameters.AddWithValue("NOMBRE", item.Nombre);
				command.Parameters.AddWithValue("IDTIPOCUENTAFINANCIERA", item.IdTipoCuentaFinanciera);
				command.Parameters.AddWithValue("SALDOINICIAL", item.SaldoInicial);
				command.Parameters.AddWithValue("FECHASALDOINICIAL", item.FechaSaldoInicial);
				command.Parameters.AddWithValue("SALDOACTUAL", item.SaldoActual);
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

		public async Task Actualizar(CuentaFinanciera item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.CUENTA_FINANCIERA SET " +
				"SUB = @SUB, ID_NEGOCIO = @IDNEGOCIO, NOMBRE = @NOMBRE, ID_TIPO_CUENTA_FINANCIERA = @IDTIPOCUENTAFINANCIERA, " +
				"SALDO_INICIAL = @SALDOINICIAL, FECHA_SALDO_INICIAL = @FECHASALDOINICIAL, SALDO_ACTUAL = @SALDOACTUAL, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("SUB", item.Sub);
				command.Parameters.AddWithValue("IDNEGOCIO", item.IdNegocio);
				command.Parameters.AddWithValue("NOMBRE", item.Nombre);
				command.Parameters.AddWithValue("IDTIPOCUENTAFINANCIERA", item.IdTipoCuentaFinanciera);
				command.Parameters.AddWithValue("SALDOINICIAL", item.SaldoInicial);
				command.Parameters.AddWithValue("FECHASALDOINICIAL", item.FechaSaldoInicial);
				command.Parameters.AddWithValue("SALDOACTUAL", item.SaldoActual);
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
