using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
    [ExcludeFromCodeCoverage]
    public class UsuarioDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<Usuario?> Obtener(string sub, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT SUB, FLOW_CUSTOMER_ID, NOMBRE, APELLIDO, CORREO_ELECTRONICO FROM TANATOS.USUARIO WHERE SUB = @SUB";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("SUB", sub);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				Usuario? retorno = null;
				if (await reader.ReadAsync()) {
					retorno = new Usuario {
						Sub = reader.GetString(0),
						FlowCustomerId = await reader.IsDBNullAsync(1) ? null : reader.GetString(1),
						Nombre = await reader.IsDBNullAsync(2) ? null : reader.GetString(2),
						Apellido = await reader.IsDBNullAsync(3) ? null : reader.GetString(3),
						CorreoElectronico = await reader.IsDBNullAsync(4) ? null : reader.GetString(4)
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
				"SELECT SUB, FLOW_CUSTOMER_ID, NOMBRE, APELLIDO, CORREO_ELECTRONICO FROM TANATOS.USUARIO " +
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
                        Sub = reader.GetString(0),
                        FlowCustomerId = await reader.IsDBNullAsync(1) ? null : reader.GetString(1),
                        Nombre = await reader.IsDBNullAsync(2) ? null : reader.GetString(2),
                        Apellido = await reader.IsDBNullAsync(3) ? null : reader.GetString(3),
                        CorreoElectronico = await reader.IsDBNullAsync(4) ? null : reader.GetString(4)
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
				"INSERT INTO TANATOS.USUARIO(SUB, FLOW_CUSTOMER_ID, NOMBRE, APELLIDO, CORREO_ELECTRONICO) " +
				"VALUES (@SUB, @FLOWCUSTOMERID, @NOMBRE, @APELLIDO, @CORREOELECTRONICO)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("SUB", item.Sub);
                command.Parameters.AddWithValue("FLOWCUSTOMERID", (object?)item.FlowCustomerId ?? DBNull.Value);
                command.Parameters.AddWithValue("NOMBRE", (object?)item.Nombre ?? DBNull.Value);
                command.Parameters.AddWithValue("APELLIDO", (object?)item.Apellido ?? DBNull.Value);
                command.Parameters.AddWithValue("CORREOELECTRONICO", (object?)item.CorreoElectronico ?? DBNull.Value);
                await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
		}

		public async Task Actualizar(Usuario item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.USUARIO SET FLOW_CUSTOMER_ID = @FLOWCUSTOMERID, NOMBRE = @NOMBRE, " +
                "APELLIDO = @APELLIDO, CORREO_ELECTRONICO = @CORREOELECTRONICO " +
				"WHERE SUB = @SUB";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("FLOWCUSTOMERID", (object?)item.FlowCustomerId ?? DBNull.Value);
                command.Parameters.AddWithValue("NOMBRE", (object?)item.Nombre ?? DBNull.Value);
                command.Parameters.AddWithValue("APELLIDO", (object?)item.Apellido ?? DBNull.Value);
                command.Parameters.AddWithValue("CORREOELECTRONICO", (object?)item.CorreoElectronico ?? DBNull.Value);
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
