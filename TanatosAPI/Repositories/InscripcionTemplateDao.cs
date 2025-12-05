using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class InscripcionTemplateDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<InscripcionTemplate>> ObtenerPorSub(string sub, bool vigencia = true) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<InscripcionTemplate>(
				"SELECT SUB, ID_TEMPLATE, FECHA_ACTIVACION, FECHA_DESACTIVACION, VIGENCIA FROM TANATOS.INSCRIPCION_TEMPLATE WHERE SUB = @SUB AND VIGENCIA = @VIGENCIA",
				new { sub, vigencia }
			)];
		}

		public async Task Insertar(InscripcionTemplate item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"INSERT INTO TANATOS.INSCRIPCION_TEMPLATE(SUB, ID_TEMPLATE, FECHA_ACTIVACION, FECHA_DESACTIVACION, VIGENCIA) VALUES (@SUB, @IDTEMPLATE, @FECHAACTIVACION, @FECHADESACTIVACION, @VIGENCIA)",
				new { item.Sub, item.IdTemplate, item.FechaActivacion, item.FechaDesactivacion, item.Vigencia }
			);
		}

		public async Task Actualizar(InscripcionTemplate item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"UPDATE TANATOS.INSCRIPCION_TEMPLATE SET FECHA_ACTIVACION = @FECHAACTIVACION, FECHA_DESACTIVACION = @FECHADESACTIVACION, VIGENCIA = @VIGENCIA WHERE SUB = @SUB AND ID_TEMPLATE = @IDTEMPLATE",
				new { item.FechaActivacion, item.FechaDesactivacion, item.Vigencia, item.Sub, item.IdTemplate }
			);
		}

		public async Task Eliminar(string sub, long idTemplate) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"DELETE FROM TANATOS.INSCRIPCION_TEMPLATE WHERE SUB = @SUB AND ID_TEMPLATE = @IDTEMPLATE",
				new { sub, idTemplate }
			);
		}
	}
}
