using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
    public class TemplateNormaBcp(NormaSuscritaBcp normaSuscritaBcp, TemplateNormaDao templateNormaDao, TemplateNormaNotificacionDao templateNormaNotificacionDao, TemplateNormaFiscalizadorDao templateNormaFiscalizadorDao, NormaSuscritaDao normaSuscritaDao) {

        public async Task Eliminar(long idTemplate, long? idNorma, NpgsqlTransaction? transaction = null) {
            await templateNormaNotificacionDao.Eliminar(idTemplate, idNorma, null, null, transaction);
            await templateNormaFiscalizadorDao.Eliminar(idTemplate, idNorma, null, transaction);

            Dictionary<long, TemplateNorma> templateNormas = (await templateNormaDao.ObtenerPorTemplate(idTemplate, transaction)).ToDictionary(tn => tn.IdNorma, tn => tn);

            // Previo a eliminar el template norma, se desenlazan todas las normas suscritas relacionadas
            List<NormaSuscrita> normasSuscritasDependientes = await normaSuscritaDao.ObtenerPorTemplate(idTemplate, idNorma, null, transaction);
            foreach (NormaSuscrita normaSuscrita in normasSuscritasDependientes) {
                TemplateNorma templateNorma = templateNormas[normaSuscrita.IdNorma!.Value];

                normaSuscrita.Nombre ??= templateNorma.Nombre;
                normaSuscrita.Descripcion ??= templateNorma.Descripcion;
                normaSuscrita.IdTipoPeriodicidad ??= templateNorma.IdTipoPeriodicidad;
                normaSuscrita.Multa ??= templateNorma.Multa;
                normaSuscrita.IdCategoriaNorma ??= templateNorma.IdCategoriaNorma;
                normaSuscrita.IdTemplate = null;
                normaSuscrita.IdNorma = null;

                // Si la norma suscrita no está activada se elimina
                if (!normaSuscrita.Activado) {
                    await normaSuscritaBcp.EliminarNormaSuscrita(normaSuscrita, transaction);
                
                // Pero si está activada, solo se desenlaza del template
                } else {
                    await normaSuscritaDao.Actualizar(normaSuscrita, transaction);
                }
            }

            await templateNormaDao.Eliminar(idTemplate, idNorma, transaction);
        }
    }
}
