using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Repositories {
    [ExcludeFromCodeCoverage]
    public class CategoriaNormaDao(IDatabaseConnectionHelper connectionHelper) : ICategoriaNormaDao {
		public async Task<CategoriaNorma?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null) {
            string query =
                "SELECT ID, NOMBRE, NOMBRE_CORTO, DESCRIPCION, VIGENCIA FROM TANATOS.CATEGORIA_NORMA WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("ID", id);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                CategoriaNorma? retorno = null;
                if (await reader.ReadAsync()) {
                    retorno = new CategoriaNorma {
                        Id = reader.GetInt64(0),
                        Nombre = reader.GetString(1),
                        NombreCorto = await reader.IsDBNullAsync(2) ? null : reader.GetString(2),
						Descripcion = await reader.IsDBNullAsync(3) ? null : reader.GetString(3),
                        Vigencia = reader.GetBoolean(4)
                    };
                }
                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
		}

		public async Task<List<CategoriaNorma>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null) {
            string query =
                "SELECT ID, NOMBRE, NOMBRE_CORTO, DESCRIPCION, VIGENCIA FROM TANATOS.CATEGORIA_NORMA WHERE (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                List<CategoriaNorma> retorno = [];
                while (await reader.ReadAsync()) {
                    retorno.Add(new CategoriaNorma {
                        Id = reader.GetInt64(0),
                        Nombre = reader.GetString(1),
                        NombreCorto = await reader.IsDBNullAsync(2) ? null : reader.GetString(2),
                        Descripcion = await reader.IsDBNullAsync(3) ? null : reader.GetString(3),
                        Vigencia = reader.GetBoolean(4)
                    });
                }
                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task Insertar(CategoriaNorma item, NpgsqlTransaction? transaction = null) {
            string query =
                "INSERT INTO TANATOS.CATEGORIA_NORMA(ID, NOMBRE, NOMBRE_CORTO, DESCRIPCION, VIGENCIA) " +
                "VALUES (@ID, @NOMBRE, @NOMBRECORTO, @DESCRIPCION, @VIGENCIA)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("ID", item.Id);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
                command.Parameters.AddWithValue("NOMBRECORTO", (object?)item.NombreCorto ?? DBNull.Value);
                command.Parameters.AddWithValue("DESCRIPCION", (object ?)item.Descripcion ?? DBNull.Value);
                command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
                await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
		}

		public async Task Actualizar(CategoriaNorma item, NpgsqlTransaction? transaction = null) {
            string query =
                "UPDATE TANATOS.CATEGORIA_NORMA SET NOMBRE = @NOMBRE, NOMBRE_CORTO = @NOMBRECORTO, DESCRIPCION = @DESCRIPCION, " +
                "VIGENCIA = @VIGENCIA WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
                command.Parameters.AddWithValue("NOMBRECORTO", (object?)item.NombreCorto ?? DBNull.Value);
                command.Parameters.AddWithValue("DESCRIPCION", (object?)item.Descripcion ?? DBNull.Value);
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
                "DELETE FROM TANATOS.CATEGORIA_NORMA WHERE ID = @ID";

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
