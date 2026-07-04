using Npgsql;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
    [ExcludeFromCodeCoverage]
    public class EventoPagoDao(IDatabaseConnectionHelper connectionHelper) : IEventoPagoDao {
		public async Task<long> Insertar(EventoPago item, NpgsqlTransaction? transaction = null) {
            string query =
                "INSERT INTO TANATOS.EVENTO_PAGO(PROVEEDOR, EVENTO, PAYLOAD, PROCESADO, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
                "VALUES (@PROVEEDOR, @EVENTO, @PAYLOAD::JSONB, @PROCESADO, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
                "RETURNING ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("PROVEEDOR", item.Proveedor);
                command.Parameters.AddWithValue("EVENTO", item.Evento);
                command.Parameters.AddWithValue("PAYLOAD", item.Payload);
                command.Parameters.AddWithValue("PROCESADO", item.Procesado);
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

		public async Task Actualizar(EventoPago item, NpgsqlTransaction? transaction = null) {
            string query =
                "UPDATE TANATOS.EVENTO_PAGO SET PROVEEDOR = @PROVEEDOR, EVENTO = @EVENTO, PAYLOAD = @PAYLOAD::JSONB, PROCESADO = @PROCESADO, " +
                "FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("PROVEEDOR", item.Proveedor);
                command.Parameters.AddWithValue("EVENTO", item.Evento);
                command.Parameters.AddWithValue("PAYLOAD", item.Payload);
                command.Parameters.AddWithValue("PROCESADO", item.Procesado);
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
