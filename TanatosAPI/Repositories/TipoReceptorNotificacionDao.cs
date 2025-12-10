using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {

    [DapperAot]
    public class TipoReceptorNotificacionDao(DatabaseConnectionHelper connectionHelper) {

        public async Task<TipoReceptorNotificacion?> ObtenerPorId(long id) {
            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            return await connection.QueryFirstOrDefaultAsync<TipoReceptorNotificacion>(
				"SELECT ID, NOMBRE, REGEX_VALIDACION, VIGENCIA FROM TANATOS.TIPO_RECEPTOR_NOTIFICACION WHERE ID = @ID",
                new { id }
            );
        }

        public async Task<List<TipoReceptorNotificacion>> ObtenerPorVigencia(bool? vigencia) {
            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            return [.. await connection.QueryAsync<TipoReceptorNotificacion>(
				"SELECT ID, NOMBRE, REGEX_VALIDACION, VIGENCIA FROM TANATOS.TIPO_RECEPTOR_NOTIFICACION WHERE (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)",
                new { vigencia }
			)];

		}

        public async Task Insertar(TipoReceptorNotificacion tipoReceptorNotificacion) {
            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await connection.ExecuteAsync(
				"INSERT INTO TANATOS.TIPO_RECEPTOR_NOTIFICACION(ID, NOMBRE, REGEX_VALIDACION, VIGENCIA) VALUES (@ID, @NOMBRE, @REGEXVALIDACION, @VIGENCIA)",
                new { tipoReceptorNotificacion.Id, tipoReceptorNotificacion.Nombre, tipoReceptorNotificacion.RegexValidacion, tipoReceptorNotificacion.Vigencia }
            );
        }

        public async Task Actualizar(TipoReceptorNotificacion tipoReceptorNotificacion) {
            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await connection.ExecuteAsync(
				"UPDATE TANATOS.TIPO_RECEPTOR_NOTIFICACION SET NOMBRE = @NOMBRE, REGEX_VALIDACION = @REGEXVALIDACION, VIGENCIA = @VIGENCIA WHERE ID = @ID",
                new { tipoReceptorNotificacion.Nombre, tipoReceptorNotificacion.RegexValidacion, tipoReceptorNotificacion.Vigencia, tipoReceptorNotificacion.Id }
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
