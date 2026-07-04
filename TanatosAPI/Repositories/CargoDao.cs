using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
    [ExcludeFromCodeCoverage]
    public class CargoDao(IDatabaseConnectionHelper connectionHelper) : ICargoDao {

		public async Task<Cargo?> Obtener(long id, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, ID_NEGOCIO, NOMBRE, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.CARGO " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("ID", id);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				Cargo? retorno = null;
				if (await reader.ReadAsync()) {
					retorno = new Cargo {
						Id = reader.GetInt64(0),
						Sub = reader.GetString(1),
						IdNegocio = reader.GetInt64(2),
						Nombre = reader.GetString(3),
						FechaCreacion = reader.GetDateTime(4),
						FechaEliminacion = await reader.IsDBNullAsync(5) ? null : reader.GetDateTime(5),
						Vigencia = reader.GetBoolean(6)
					};
				}
				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<List<Cargo>> ObtenerPorSub(string sub, long? idNegocio = null, bool? vigencia = true, NpgsqlTransaction? transaction = null) {
            string query =
                "SELECT ID, SUB, ID_NEGOCIO, NOMBRE, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
                "FROM TANATOS.CARGO " +
                "WHERE SUB = @SUB AND (ID_NEGOCIO = @IDNEGOCIO OR @IDNEGOCIO IS NULL) AND (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);

                command.Parameters.AddWithValue("SUB", sub);
                command.Parameters.AddWithValue("IDNEGOCIO", (object?)idNegocio ?? DBNull.Value);
                command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                List<Cargo> retorno = [];

                while (await reader.ReadAsync()) {
                    retorno.Add(new Cargo {
                        Id = reader.GetInt64(0),
                        Sub = reader.GetString(1),
                        IdNegocio = reader.GetInt64(2),
                        Nombre = reader.GetString(3),
                        FechaCreacion = reader.GetDateTime(4),
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

        public async Task<long> Insertar(Cargo item, NpgsqlTransaction? transaction = null) {
            string query =
                "INSERT INTO TANATOS.CARGO(SUB, ID_NEGOCIO, NOMBRE, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
                "VALUES (@SUB, @IDNEGOCIO, @NOMBRE, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
                "RETURNING ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);

                command.Parameters.AddWithValue("SUB", item.Sub);
                command.Parameters.AddWithValue("IDNEGOCIO", item.IdNegocio);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
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

        public async Task Actualizar(Cargo item, NpgsqlTransaction? transaction = null) {
            string query =
                "UPDATE TANATOS.CARGO SET SUB = @SUB, ID_NEGOCIO = @IDNEGOCIO, NOMBRE = @NOMBRE, " +
                "FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
                "WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);

                command.Parameters.AddWithValue("SUB", item.Sub);
                command.Parameters.AddWithValue("IDNEGOCIO", item.IdNegocio);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
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
