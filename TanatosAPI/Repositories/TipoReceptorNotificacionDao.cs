using Dapper;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {

    [DapperAot]
    public class TipoReceptorNotificacionDao(DatabaseConnectionHelper connectionHelper) {

        public async Task<TipoReceptorNotificacion?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null) {
            string query =
                "SELECT ID, NOMBRE, REGEX_VALIDACION, REQUIERE_PLAN_EMPRESA, VIGENCIA FROM TANATOS.TIPO_RECEPTOR_NOTIFICACION WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("ID", id);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				TipoReceptorNotificacion? retorno = null;

				if (await reader.ReadAsync()) {
					retorno = new TipoReceptorNotificacion {
						Id = reader.GetInt64(0),
                        Nombre = reader.GetString(1),
                        RegexValidacion = reader.IsDBNull(2) ? null : reader.GetString(2),
                        RequierePlanEmpresa = reader.GetBoolean(3),
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

        public async Task<List<TipoReceptorNotificacion>> ObtenerPorVigencia(bool? vigencia) {
            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            return [.. await connection.QueryAsync<TipoReceptorNotificacion>(
				"SELECT ID, NOMBRE, REGEX_VALIDACION, REQUIERE_PLAN_EMPRESA, VIGENCIA FROM TANATOS.TIPO_RECEPTOR_NOTIFICACION WHERE (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)",
                new { vigencia }
			)];

		}

        public async Task Insertar(TipoReceptorNotificacion tipoReceptorNotificacion) {
            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await connection.ExecuteAsync(
				"INSERT INTO TANATOS.TIPO_RECEPTOR_NOTIFICACION(ID, NOMBRE, REGEX_VALIDACION, REQUIERE_PLAN_EMPRESA, VIGENCIA) VALUES (@ID, @NOMBRE, @REGEXVALIDACION, @REQUIEREPLANEMPRESA, @VIGENCIA)",
                new { tipoReceptorNotificacion.Id, tipoReceptorNotificacion.Nombre, tipoReceptorNotificacion.RegexValidacion, tipoReceptorNotificacion.RequierePlanEmpresa, tipoReceptorNotificacion.Vigencia }
            );
        }

        public async Task Actualizar(TipoReceptorNotificacion tipoReceptorNotificacion) {
            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await connection.ExecuteAsync(
				"UPDATE TANATOS.TIPO_RECEPTOR_NOTIFICACION SET NOMBRE = @NOMBRE, REGEX_VALIDACION = @REGEXVALIDACION, REQUIERE_PLAN_EMPRESA = @REQUIEREPLANEMPRESA, VIGENCIA = @VIGENCIA WHERE ID = @ID",
                new { tipoReceptorNotificacion.Nombre, tipoReceptorNotificacion.RegexValidacion, tipoReceptorNotificacion.RequierePlanEmpresa, tipoReceptorNotificacion.Vigencia, tipoReceptorNotificacion.Id }
            );
		}

        public async Task Eliminar(long id) {
            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await connection.ExecuteAsync(
                "DELETE FROM TANATOS.TIPO_RECEPTOR_NOTIFICACION WHERE ID = @ID",
                new { id }
            );
		}

	}
}
