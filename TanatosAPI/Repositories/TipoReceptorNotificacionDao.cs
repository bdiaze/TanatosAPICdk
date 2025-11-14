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
				"SELECT ID, NOMBRE, VIGENCIA FROM TANATOS.TIPO_RECEPTOR_NOTIFICACION WHERE ID = @ID",
                new { id }
            );
        }

        public async Task Insertar(TipoReceptorNotificacion tipoReceptorNotificacion) {
            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await connection.ExecuteAsync(
                "INSERT INTO TANATOS.TIPO_RECEPTOR_NOTIFICACION(ID, NOMBRE, VIGENCIA) VALUES (@ID, @NOMBRE, @VIGENCIA)",
                new { tipoReceptorNotificacion.Id, tipoReceptorNotificacion.Nombre, tipoReceptorNotificacion.Vigencia }
            );
        }

    }
}
