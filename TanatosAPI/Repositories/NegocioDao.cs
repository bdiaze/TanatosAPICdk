using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
    [ExcludeFromCodeCoverage]
    public class NegocioDao(IDatabaseConnectionHelper connectionHelper) : INegocioDao {

		public async Task<Negocio?> Obtener(long id, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, NOMBRE, DIRECCION, ID_TIPO_ACTIVIDAD, MISION, VISION, VALORES, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.NEGOCIO " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("ID", id);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				Negocio? retorno = null;
				if (await reader.ReadAsync()) {
					retorno = new Negocio {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						Sub = reader.GetString(reader.GetOrdinal("SUB")),
						Nombre = reader.GetString(reader.GetOrdinal("NOMBRE")),
						Direccion = await reader.IsDBNullAsync(reader.GetOrdinal("DIRECCION")) ? null : reader.GetString(reader.GetOrdinal("DIRECCION")),
						IdTipoActividad = await reader.IsDBNullAsync(reader.GetOrdinal("ID_TIPO_ACTIVIDAD")) ? null : reader.GetInt64(reader.GetOrdinal("ID_TIPO_ACTIVIDAD")),
						Mision = await reader.IsDBNullAsync(reader.GetOrdinal("MISION")) ? null : reader.GetString(reader.GetOrdinal("MISION")),
						Vision = await reader.IsDBNullAsync(reader.GetOrdinal("VISION")) ? null : reader.GetString(reader.GetOrdinal("VISION")),
						Valores = await reader.IsDBNullAsync(reader.GetOrdinal("VALORES")) ? null : reader.GetString(reader.GetOrdinal("VALORES")),
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

		public async Task<List<Negocio>> ObtenerPorSub(string sub, bool? vigencia = true, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, NOMBRE, DIRECCION, ID_TIPO_ACTIVIDAD, MISION, VISION, VALORES, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.NEGOCIO " +
				"WHERE SUB = @SUB AND (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("SUB", sub);
				command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<Negocio> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new Negocio {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						Sub = reader.GetString(reader.GetOrdinal("SUB")),
						Nombre = reader.GetString(reader.GetOrdinal("NOMBRE")),
						Direccion = await reader.IsDBNullAsync(reader.GetOrdinal("DIRECCION")) ? null : reader.GetString(reader.GetOrdinal("DIRECCION")),
						IdTipoActividad = await reader.IsDBNullAsync(reader.GetOrdinal("ID_TIPO_ACTIVIDAD")) ? null : reader.GetInt64(reader.GetOrdinal("ID_TIPO_ACTIVIDAD")),
						Mision = await reader.IsDBNullAsync(reader.GetOrdinal("MISION")) ? null : reader.GetString(reader.GetOrdinal("MISION")),
						Vision = await reader.IsDBNullAsync(reader.GetOrdinal("VISION")) ? null : reader.GetString(reader.GetOrdinal("VISION")),
						Valores = await reader.IsDBNullAsync(reader.GetOrdinal("VALORES")) ? null : reader.GetString(reader.GetOrdinal("VALORES")),
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

		public async Task<long> Insertar(Negocio item, NpgsqlTransaction? transaction = null) {
            string query =
				"INSERT INTO TANATOS.NEGOCIO(SUB, NOMBRE, DIRECCION, ID_TIPO_ACTIVIDAD, MISION, VISION, VALORES, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@SUB, @NOMBRE, @DIRECCION, @IDTIPOACTIVIDAD, @MISION, @VISION, @VALORES, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
                "RETURNING ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("SUB", item.Sub);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
                command.Parameters.AddWithValue("DIRECCION", (object?)item.Direccion ?? DBNull.Value);
                command.Parameters.AddWithValue("IDTIPOACTIVIDAD", (object?)item.IdTipoActividad ?? DBNull.Value);
				command.Parameters.AddWithValue("MISION", (object?)item.Mision ?? DBNull.Value);
				command.Parameters.AddWithValue("VISION", (object?)item.Vision ?? DBNull.Value);
				command.Parameters.AddWithValue("VALORES", (object?)item.Valores ?? DBNull.Value);
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

		public async Task Actualizar(Negocio item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.NEGOCIO SET SUB = @SUB, NOMBRE = @NOMBRE, DIRECCION = @DIRECCION, " +
				"ID_TIPO_ACTIVIDAD = @IDTIPOACTIVIDAD, MISION = @MISION, VISION = @VISION, VALORES = @VALORES, FECHA_CREACION = @FECHACREACION, " +
                "FECHA_ELIMINACION = @FECHAELIMINACION, " +
				"VIGENCIA = @VIGENCIA WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("SUB", item.Sub);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
                command.Parameters.AddWithValue("DIRECCION", (object?)item.Direccion ?? DBNull.Value);
                command.Parameters.AddWithValue("IDTIPOACTIVIDAD", (object?)item.IdTipoActividad ?? DBNull.Value);
				command.Parameters.AddWithValue("MISION", (object?)item.Mision ?? DBNull.Value);
				command.Parameters.AddWithValue("VISION", (object?)item.Vision ?? DBNull.Value);
				command.Parameters.AddWithValue("VALORES", (object?)item.Valores ?? DBNull.Value);
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
