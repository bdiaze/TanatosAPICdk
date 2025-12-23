using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class InscripcionTemplateDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<InscripcionTemplate>> ObtenerPorSub(string sub, long idNegocio, bool? vigencia = true) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<InscripcionTemplate>(
				"SELECT SUB, ID_NEGOCIO, ID_TEMPLATE, FECHA_ACTIVACION, FECHA_DESACTIVACION, VIGENCIA FROM TANATOS.INSCRIPCION_TEMPLATE " +
				"WHERE SUB = @SUB AND ID_NEGOCIO = @IDNEGOCIO AND (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)",
				new { sub, idNegocio, vigencia }
			)];
		}

		public async Task Insertar(InscripcionTemplate item, NpgsqlTransaction? transaction = null) {
			string query = 
				"INSERT INTO TANATOS.INSCRIPCION_TEMPLATE(SUB, ID_NEGOCIO, ID_TEMPLATE, FECHA_ACTIVACION, FECHA_DESACTIVACION, VIGENCIA) " +
				"VALUES (@SUB, @IDNEGOCIO, @IDTEMPLATE, @FECHAACTIVACION, @FECHADESACTIVACION, @VIGENCIA)";
			DynamicParameters param = new();
			param.Add("SUB", item.Sub);
			param.Add("IDNEGOCIO", item.IdNegocio);
			param.Add("IDTEMPLATE", item.IdTemplate);
			param.Add("FECHAACTIVACION", item.FechaActivacion);
			param.Add("FECHADESACTIVACION", item.FechaDesactivacion);
			param.Add("VIGENCIA", item.Vigencia);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}

		public async Task Actualizar(InscripcionTemplate item, NpgsqlTransaction? transaction = null) {
			string query = 
				"UPDATE TANATOS.INSCRIPCION_TEMPLATE SET FECHA_ACTIVACION = @FECHAACTIVACION, FECHA_DESACTIVACION = @FECHADESACTIVACION, VIGENCIA = @VIGENCIA " +
				"WHERE SUB = @SUB AND ID_NEGOCIO = @IDNEGOCIO AND ID_TEMPLATE = @IDTEMPLATE";
			DynamicParameters param = new();
			param.Add("FECHAACTIVACION", item.FechaActivacion);
			param.Add("FECHADESACTIVACION", item.FechaDesactivacion);
			param.Add("VIGENCIA", item.Vigencia);
			param.Add("SUB", item.Sub);
			param.Add("IDNEGOCIO", item.IdNegocio);
			param.Add("IDTEMPLATE", item.IdTemplate);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}
	}
}
