using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
    [ExcludeFromCodeCoverage]
    public class MensajeDao(IDatabaseConnectionHelper connectionHelper) : IMensajeDao {
		public async Task<List<Mensaje>> ObtenerPorRangoFechas(DateTime? fechaInicial, DateTime? fechaFinal, NpgsqlTransaction? transaction = null) {
			string query =
                "SELECT ID, SUB, NOMBRE, CORREO, CONTENIDO, HERMES_ID_MENSAJE, FECHA_CREACION FROM TANATOS.MENSAJE " +
                "WHERE (FECHA_CREACION >= @FECHAINICIAL OR @FECHAINICIAL IS NULL) " +
                "AND (FECHA_CREACION <= @FECHAFINAL OR @FECHAFINAL IS NULL)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);

                command.Parameters.AddWithValue("FECHAINICIAL", (object?)fechaInicial ?? DBNull.Value);
                command.Parameters.AddWithValue("FECHAFINAL", (object?)fechaFinal ?? DBNull.Value);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                List<Mensaje> retorno = [];
                while (await reader.ReadAsync()) {
                    retorno.Add(new Mensaje {
						Id = reader.GetInt64(0),
						Sub = await reader.IsDBNullAsync(1) ? null : reader.GetString(1),
						Nombre = reader.GetString(2),
						Correo = reader.GetString(3),
						Contenido = reader.GetString(4),
						HermesIdMensaje = await reader.IsDBNullAsync(5) ? null : reader.GetString(5),
						FechaCreacion = reader.GetDateTime(6)
                    });
                }
                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task<long> Insertar(Mensaje item, NpgsqlTransaction? transaction = null) {
            string query =
                "INSERT INTO TANATOS.MENSAJE(SUB, NOMBRE, CORREO, CONTENIDO, HERMES_ID_MENSAJE, FECHA_CREACION) " +
                "VALUES (@SUB, @NOMBRE, @CORREO, @CONTENIDO, @HERMESIDMENSAJE, @FECHACREACION) " +
                "RETURNING ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("SUB", (object?)item.Sub ?? DBNull.Value);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
                command.Parameters.AddWithValue("CORREO", item.Correo);
                command.Parameters.AddWithValue("CONTENIDO", item.Contenido);
                command.Parameters.AddWithValue("HERMESIDMENSAJE", (object?)item.HermesIdMensaje ?? DBNull.Value);
                command.Parameters.AddWithValue("FECHACREACION", item.FechaCreacion);
                return Convert.ToInt64(await command.ExecuteScalarAsync());
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task Actualizar(Mensaje item, NpgsqlTransaction? transaction = null) {
            string query =
                "UPDATE TANATOS.MENSAJE SET SUB = @SUB, NOMBRE = @NOMBRE, CORREO = @CORREO, CONTENIDO = @CONTENIDO, " +
                "HERMES_ID_MENSAJE = @HERMESIDMENSAJE, FECHA_CREACION = @FECHACREACION " +
                "WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("SUB", (object?)item.Sub ?? DBNull.Value);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
                command.Parameters.AddWithValue("CORREO", item.Correo);
                command.Parameters.AddWithValue("CONTENIDO", item.Contenido);
                command.Parameters.AddWithValue("HERMESIDMENSAJE", (object?)item.HermesIdMensaje ?? DBNull.Value);
                command.Parameters.AddWithValue("FECHACREACION", item.FechaCreacion);
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
