using Npgsql;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
    [ExcludeFromCodeCoverage]
    public class UsuarioDao(IDatabaseConnectionHelper connectionHelper) : IUsuarioDao {
		public async Task<Usuario?> Obtener(string sub, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT SUB, USER_NAME, FLOW_CUSTOMER_ID, NOMBRE, APELLIDO, CORREO_ELECTRONICO, FECHA_CREACION FROM TANATOS.USUARIO " +
                "WHERE SUB = @SUB";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("SUB", sub);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				Usuario? retorno = null;
				if (await reader.ReadAsync()) {
					retorno = new Usuario {
						Sub = reader.GetString(reader.GetOrdinal("SUB")),
                        UserName = await reader.IsDBNullAsync(reader.GetOrdinal("USER_NAME")) ? null : reader.GetString(reader.GetOrdinal("USER_NAME")),
						FlowCustomerId = await reader.IsDBNullAsync(reader.GetOrdinal("FLOW_CUSTOMER_ID")) ? null : reader.GetString(reader.GetOrdinal("FLOW_CUSTOMER_ID")),
						Nombre = await reader.IsDBNullAsync(reader.GetOrdinal("NOMBRE")) ? null : reader.GetString(reader.GetOrdinal("NOMBRE")),
						Apellido = await reader.IsDBNullAsync(reader.GetOrdinal("APELLIDO")) ? null : reader.GetString(reader.GetOrdinal("APELLIDO")),
						CorreoElectronico = await reader.IsDBNullAsync(reader.GetOrdinal("CORREO_ELECTRONICO")) ? null : reader.GetString(reader.GetOrdinal("CORREO_ELECTRONICO")),
                        FechaCreacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_CREACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_CREACION")),
					};
				}

				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<Usuario?> ObtenerPorUserName(string userName, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT SUB, USER_NAME, FLOW_CUSTOMER_ID, NOMBRE, APELLIDO, CORREO_ELECTRONICO, FECHA_CREACION FROM TANATOS.USUARIO " +
				"WHERE USER_NAME = @USERNAME";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("USERNAME", userName);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				Usuario? retorno = null;
				if (await reader.ReadAsync()) {
					retorno = new Usuario {
						Sub = reader.GetString(reader.GetOrdinal("SUB")),
						UserName = await reader.IsDBNullAsync(reader.GetOrdinal("USER_NAME")) ? null : reader.GetString(reader.GetOrdinal("USER_NAME")),
						FlowCustomerId = await reader.IsDBNullAsync(reader.GetOrdinal("FLOW_CUSTOMER_ID")) ? null : reader.GetString(reader.GetOrdinal("FLOW_CUSTOMER_ID")),
						Nombre = await reader.IsDBNullAsync(reader.GetOrdinal("NOMBRE")) ? null : reader.GetString(reader.GetOrdinal("NOMBRE")),
						Apellido = await reader.IsDBNullAsync(reader.GetOrdinal("APELLIDO")) ? null : reader.GetString(reader.GetOrdinal("APELLIDO")),
						CorreoElectronico = await reader.IsDBNullAsync(reader.GetOrdinal("CORREO_ELECTRONICO")) ? null : reader.GetString(reader.GetOrdinal("CORREO_ELECTRONICO")),
						FechaCreacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_CREACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_CREACION")),
					};
				}

				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<Usuario?> ObtenerPorFlowCustomerId(string flowCustomerId, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT SUB, USER_NAME, FLOW_CUSTOMER_ID, NOMBRE, APELLIDO, CORREO_ELECTRONICO, FECHA_CREACION FROM TANATOS.USUARIO " +
				"WHERE FLOW_CUSTOMER_ID = @FLOWCUSTOMERID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("FLOWCUSTOMERID", flowCustomerId);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                Usuario? retorno = null;
                if (await reader.ReadAsync()) {
                    retorno = new Usuario {
						Sub = reader.GetString(reader.GetOrdinal("SUB")),
						UserName = await reader.IsDBNullAsync(reader.GetOrdinal("USER_NAME")) ? null : reader.GetString(reader.GetOrdinal("USER_NAME")),
						FlowCustomerId = await reader.IsDBNullAsync(reader.GetOrdinal("FLOW_CUSTOMER_ID")) ? null : reader.GetString(reader.GetOrdinal("FLOW_CUSTOMER_ID")),
						Nombre = await reader.IsDBNullAsync(reader.GetOrdinal("NOMBRE")) ? null : reader.GetString(reader.GetOrdinal("NOMBRE")),
						Apellido = await reader.IsDBNullAsync(reader.GetOrdinal("APELLIDO")) ? null : reader.GetString(reader.GetOrdinal("APELLIDO")),
						CorreoElectronico = await reader.IsDBNullAsync(reader.GetOrdinal("CORREO_ELECTRONICO")) ? null : reader.GetString(reader.GetOrdinal("CORREO_ELECTRONICO")),
						FechaCreacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_CREACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_CREACION")),
					};
                }

                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task Insertar(Usuario item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.USUARIO(SUB, USER_NAME, FLOW_CUSTOMER_ID, NOMBRE, APELLIDO, CORREO_ELECTRONICO, FECHA_CREACION) " +
				"VALUES (@SUB, @USERNAME, @FLOWCUSTOMERID, @NOMBRE, @APELLIDO, @CORREOELECTRONICO, @FECHACREACION)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("SUB", item.Sub);
				command.Parameters.AddWithValue("USERNAME", (object?)item.UserName ?? DBNull.Value);
				command.Parameters.AddWithValue("FLOWCUSTOMERID", (object?)item.FlowCustomerId ?? DBNull.Value);
                command.Parameters.AddWithValue("NOMBRE", (object?)item.Nombre ?? DBNull.Value);
                command.Parameters.AddWithValue("APELLIDO", (object?)item.Apellido ?? DBNull.Value);
                command.Parameters.AddWithValue("CORREOELECTRONICO", (object?)item.CorreoElectronico ?? DBNull.Value);
				command.Parameters.AddWithValue("FECHACREACION", (object?)item.FechaCreacion ?? DBNull.Value);
				await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
		}

		public async Task Actualizar(Usuario item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.USUARIO SET USER_NAME = @USERNAME, FLOW_CUSTOMER_ID = @FLOWCUSTOMERID, NOMBRE = @NOMBRE, " +
                "APELLIDO = @APELLIDO, CORREO_ELECTRONICO = @CORREOELECTRONICO, FECHA_CREACION = @FECHACREACION " +
				"WHERE SUB = @SUB";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("USERNAME", (object?)item.UserName ?? DBNull.Value);
				command.Parameters.AddWithValue("FLOWCUSTOMERID", (object?)item.FlowCustomerId ?? DBNull.Value);
                command.Parameters.AddWithValue("NOMBRE", (object?)item.Nombre ?? DBNull.Value);
                command.Parameters.AddWithValue("APELLIDO", (object?)item.Apellido ?? DBNull.Value);
                command.Parameters.AddWithValue("CORREOELECTRONICO", (object?)item.CorreoElectronico ?? DBNull.Value);
				command.Parameters.AddWithValue("FECHACREACION", (object?)item.FechaCreacion ?? DBNull.Value);
				command.Parameters.AddWithValue("SUB", item.Sub);
                await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
		}
	}
}
