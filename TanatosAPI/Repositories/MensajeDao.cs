using Dapper;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class MensajeDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<Mensaje>> ObtenerPorRangoFechas(DateTime? fechaInicial, DateTime? fechaFinal) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<Mensaje>(
				"SELECT ID, SUB, NOMBRE, CORREO, CONTENIDO, HERMES_ID_MENSAJE, FECHA_CREACION FROM TANATOS.MENSAJE " +
				"WHERE (FECHA_CREACION >= @FECHAINICIAL OR @FECHAINICIAL IS NULL) AND (FECHA_CREACION <= @FECHAFINAL OR @FECHAFINAL IS NULL)",
				new { fechaInicial, fechaFinal }
			)];
		}

		public async Task<long> Insertar(Mensaje item) {
			await using var connection = await connectionHelper.ObtenerConexion();
			return await connection.ExecuteScalarAsync<long>(
				"INSERT INTO TANATOS.MENSAJE(SUB, NOMBRE, CORREO, CONTENIDO, HERMES_ID_MENSAJE, FECHA_CREACION) " +
				"VALUES (@SUB, @NOMBRE, @CORREO, @CONTENIDO, @HERMESIDMENSAJE, @FECHACREACION) " +
				"RETURNING ID",
				new { item.Sub, item.Nombre, item.Correo, item.Contenido, item.HermesIdMensaje, item.FechaCreacion }
			);
		}

		public async Task Actualizar(Mensaje item) {
			await using var connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"UPDATE TANATOS.MENSAJE SET SUB = @SUB, NOMBRE = @NOMBRE, CORREO = @CORREO, CONTENIDO = @CONTENIDO, " +
				"HERMES_ID_MENSAJE = @HERMESIDMENSAJE, FECHA_CREACION = @FECHACREACION " +
				"WHERE ID = @ID",
				new { item.Sub, item.Nombre, item.Correo, item.Contenido, item.HermesIdMensaje, item.FechaCreacion, item.Id }
			);
		}
	}
}
