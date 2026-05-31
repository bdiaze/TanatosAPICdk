using Dapper;
using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
    [DapperAot]
    public class CargoDao(DatabaseConnectionHelper connectionHelper) {
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
            DynamicParameters param = new();
            param.Add("SUB", item.Sub);
            param.Add("IDNEGOCIO", item.IdNegocio);
            param.Add("NOMBRE", item.Nombre);
            param.Add("FECHACREACION", item.FechaCreacion);
            param.Add("FECHAELIMINACION", item.FechaEliminacion);
            param.Add("VIGENCIA", item.Vigencia);

            if (transaction?.Connection != null) {
                return await transaction!.Connection!.ExecuteScalarAsync<long>(query, param, transaction);
            } else {
                await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
                return await connection.ExecuteScalarAsync<long>(query, param);
            }
        }

        public async Task Actualizar(Cargo item, NpgsqlTransaction? transaction = null) {
            string query =
                "UPDATE TANATOS.CARGO SET SUB = @SUB, ID_NEGOCIO = @IDNEGOCIO, NOMBRE = @NOMBRE, " +
                "FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
                "WHERE ID = @ID";
            DynamicParameters param = new();
            param.Add("SUB", item.Sub);
            param.Add("IDNEGOCIO", item.IdNegocio);
            param.Add("NOMBRE", item.Nombre);
            param.Add("FECHACREACION", item.FechaCreacion);
            param.Add("FECHAELIMINACION", item.FechaEliminacion);
            param.Add("VIGENCIA", item.Vigencia);
            param.Add("ID", item.Id);

            if (transaction?.Connection != null) {
                await transaction!.Connection!.ExecuteAsync(query, param, transaction);
            } else {
                await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
                await connection.ExecuteAsync(query, param);
            }
        }
    }
}
