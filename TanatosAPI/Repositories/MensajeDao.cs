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
				"SELECT ID, SUB, NOMBRE, CORREO, CONTENIDO, FECHA_CREACION FROM TANATOS.MENSAJE " +
				"WHERE (FECHA_CREACION >= @FECHAINICIAL OR @FECHAINICIAL IS NULL) AND (FECHA_CREACION <= @FECHAFINAL OR @FECHAFINAL IS NULL)",
				new { fechaInicial, fechaFinal }
			)];
		}

		public async Task<long> Insertar(Mensaje item) {
			await using var connection = await connectionHelper.ObtenerConexion();
			return await connection.ExecuteScalarAsync<long>(
				"INSERT INTO TANATOS.MENSAJE(SUB, NOMBRE, CORREO, CONTENIDO, FECHA_CREACION) " +
				"VALUES (@SUB, @NOMBRE, @CORREO, @CONTENIDO, @FECHACREACION) " +
				"RETURNING ID",
				new { item.Sub, item.Nombre, item.Correo, item.Contenido, item.FechaCreacion }
			);
		}
	}
}
