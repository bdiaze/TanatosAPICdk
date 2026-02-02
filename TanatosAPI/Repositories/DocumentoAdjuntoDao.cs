using Amazon.S3.Model;
using Dapper;
using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class DocumentoAdjuntoDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<DocumentoAdjunto>> ObtenerPorHistorial(long idHistorialNormaSuscrita, bool? vigencia = true, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, ID_HISTORIAL_NORMA_SUSCRITA, BUCKET_NAME, BUCKET_KEY, NOMBRE_ARCHIVO, MIME_ESPERADO, TAMANNO_ESPERADO, MIME_REAL, TAMANNO_REAL, " +
				"ESTADO_SUBIDA, FECHA_EMISION_URL_PREFIRMADA_PUT, FECHA_CONFIRMACION_SUBIDA, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.DOCUMENTO_ADJUNTO " +
				"WHERE ID_HISTORIAL_NORMA_SUSCRITA = @IDHISTORIALNORMASUSCRITA AND (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("IDHISTORIALNORMASUSCRITA", idHistorialNormaSuscrita);
				command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<DocumentoAdjunto> retorno = [];

				while (await reader.ReadAsync()) {
					retorno.Add(new DocumentoAdjunto {
						Id = reader.GetInt64(0),
						IdHistorialNormaSuscrita = reader.GetInt64(1),
						BucketName = reader.GetString(2),
						BucketKey = reader.GetString(3),
						NombreArchivo = reader.GetString(4),
						MimeEsperado = reader.GetString(5),
						TamannoEsperado = reader.GetInt64(6),
						MimeReal = reader.IsDBNull(7) ? null : reader.GetString(7),
						TamannoReal = reader.IsDBNull(8) ? null : reader.GetInt64(8),
						EstadoSubida = reader.GetInt16(9),
						FechaEmisionUrlPrefirmadaPut = reader.GetDateTime(10),
						FechaConfirmacionSubida = reader.IsDBNull(11) ? null : reader.GetDateTime(11),
						FechaCreacion = reader.GetDateTime(12),
						FechaEliminacion = reader.IsDBNull(13) ? null : reader.GetDateTime(13),
						Vigencia = reader.GetBoolean(14)
					});
				}

				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<long> Insertar(DocumentoAdjunto item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.DOCUMENTO_ADJUNTO(ID_HISTORIAL_NORMA_SUSCRITA, BUCKET_NAME, BUCKET_KEY, NOMBRE_ARCHIVO, MIME_ESPERADO, TAMANNO_ESPERADO, " +
				"MIME_REAL, TAMANNO_REAL, ESTADO_SUBIDA, FECHA_EMISION_URL_PREFIRMADA_PUT, FECHA_CONFIRMACION_SUBIDA, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@IDHISTORIALNORMASUSCRITA, @BUCKETNAME, @BUCKETKEY, @NOMBREARCHIVO, @MIMEESPERADO, @TAMANNOESPERADO, " +
				"@MIMEREAL, @TAMANNOREAL, @ESTADOSUBIDA, @FECHAEMISIONURLPREFIRMADAPUT, @FECHACONFIRMACIONSUBIDA, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";
			DynamicParameters param = new();
			param.Add("IDHISTORIALNORMASUSCRITA", item.IdHistorialNormaSuscrita);
			param.Add("BUCKETNAME", item.BucketName);
			param.Add("BUCKETKEY", item.BucketKey);
			param.Add("NOMBREARCHIVO", item.NombreArchivo);
			param.Add("MIMEESPERADO", item.MimeEsperado);
			param.Add("TAMANNOESPERADO", item.TamannoEsperado);
			param.Add("MIMEREAL", item.MimeReal);
			param.Add("TAMANNOREAL", item.TamannoReal);
			param.Add("ESTADOSUBIDA", item.EstadoSubida);
			param.Add("FECHAEMISIONURLPREFIRMADAPUT", item.FechaEmisionUrlPrefirmadaPut);
			param.Add("FECHACONFIRMACIONSUBIDA", item.FechaConfirmacionSubida);
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

		public async Task Actualizar(DocumentoAdjunto item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.DOCUMENTO_ADJUNTO SET ID_HISTORIAL_NORMA_SUSCRITA = @IDHISTORIALNORMASUSCRITA, BUCKET_NAME = @BUCKETNAME, BUCKET_KEY = @BUCKETKEY, " +
				"NOMBRE_ARCHIVO = @NOMBREARCHIVO, MIME_ESPERADO = @MIMEESPERADO, TAMANNO_ESPERADO = @TAMANNOESPERADO, MIME_REAL = @MIMEREAL, TAMANNO_REAL = @TAMANNOREAL, " +
				"ESTADO_SUBIDA = @ESTADOSUBIDA, FECHA_EMISION_URL_PREFIRMADA_PUT = @FECHAEMISIONURLPREFIRMADAPUT, FECHA_CONFIRMACION_SUBIDA = @FECHACONFIRMACIONSUBIDA, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";
			DynamicParameters param = new();
			param.Add("IDHISTORIALNORMASUSCRITA", item.IdHistorialNormaSuscrita);
			param.Add("BUCKETNAME", item.BucketName);
			param.Add("BUCKETKEY", item.BucketKey);
			param.Add("NOMBREARCHIVO", item.NombreArchivo);
			param.Add("MIMEESPERADO", item.MimeEsperado);
			param.Add("TAMANNOESPERADO", item.TamannoEsperado);
			param.Add("MIMEREAL", item.MimeReal);
			param.Add("TAMANNOREAL", item.TamannoReal);
			param.Add("ESTADOSUBIDA", item.EstadoSubida);
			param.Add("FECHAEMISIONURLPREFIRMADAPUT", item.FechaEmisionUrlPrefirmadaPut);
			param.Add("FECHACONFIRMACIONSUBIDA", item.FechaConfirmacionSubida);
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
