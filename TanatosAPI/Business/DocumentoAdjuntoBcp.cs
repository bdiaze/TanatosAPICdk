using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class DocumentoAdjuntoBcp(DocumentoAdjuntoDao documentoAdjuntoDao, DocumentoAdjuntoHelper documentoAdjuntoHelper) {
		public async Task Eliminar(DocumentoAdjunto documentoAdjunto, NpgsqlTransaction? transaction = null) {
			if (documentoAdjunto.Vigencia) {
				documentoAdjunto.Vigencia = false;
				documentoAdjunto.FechaEliminacion = DateTime.UtcNow;

				await documentoAdjuntoDao.Actualizar(documentoAdjunto, transaction);
				await documentoAdjuntoHelper.AgregarTagEstadoEliminado(documentoAdjunto.BucketKey);
			}
		}

		public async Task EliminarPorHistorialNormaSuscrita(long idHistorialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			List<DocumentoAdjunto> documentosAdjuntosEliminar = await documentoAdjuntoDao.ObtenerPorHistorial(idHistorialNormaSuscrita, true, transaction);
			foreach (DocumentoAdjunto documentoAdjuntoEliminar in documentosAdjuntosEliminar) {
				await Eliminar(documentoAdjuntoEliminar, transaction);
			}
		}
	}
}
