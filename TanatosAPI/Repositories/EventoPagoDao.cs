using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class EventoPagoDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<long> Insertar(EventoPago item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return await connection.ExecuteScalarAsync<long>(
				"INSERT INTO TANATOS.EVENTO_PAGO(PROVEEDOR, EVENTO, PAYLOAD, PROCESADO, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@PROVEEDOR, @EVENTO, @PAYLOAD::JSONB, @PROCESADO, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID",
				new { item.Proveedor, item.Evento, item.Payload, item.Procesado, item.FechaCreacion, item.FechaEliminacion, item.Vigencia }
			);
		}

		public async Task Actualizar(EventoPago item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"UPDATE TANATOS.EVENTO_PAGO SET PROVEEDOR = @PROVEEDOR, EVENTO = @EVENTO, PAYLOAD = @PAYLOAD::JSONB, PROCESADO = @PROCESADO, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA WHERE ID = @ID",
				new { item.Proveedor, item.Evento, item.Payload, item.Procesado, item.FechaCreacion, item.FechaEliminacion, item.Vigencia, item.Id }
			);
		}
	}
}
