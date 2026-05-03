using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
    public class TemplateNormaBcp(NormaSuscritaBcp normaSuscritaBcp, NotificacionNormaSuscritaBcp notificacionNormaSuscritaBcp, FiscalizadorNormaSuscritaBcp fiscalizadorNormaSuscritaBcp, TemplateNormaDao templateNormaDao, TemplateNormaNotificacionDao templateNormaNotificacionDao, TemplateNormaFiscalizadorDao templateNormaFiscalizadorDao, NormaSuscritaDao normaSuscritaDao, NotificacionNormaSuscritaDao notificacionNormaSuscritaDao, FiscalizadorNormaSuscritaDao fiscalizadorNormaSuscritaDao) {

        public async Task Eliminar(long idTemplate, long? idNorma, NpgsqlTransaction? transaction = null) {
            Dictionary<long, TemplateNorma> templateNormas = (await templateNormaDao.ObtenerPorTemplate(idTemplate, transaction)).ToDictionary(tn => tn.IdNorma, tn => tn);
            Dictionary<long, HashSet<(long IdTipoUnidadTiempoAntelacion, int CantAntelacion)>> templateNormasNotificaciones = 
                (await templateNormaNotificacionDao.ObtenerPorTemplateNorma(idTemplate, idNorma, transaction))
                .GroupBy(tnn => tnn.IdNorma)
                .ToDictionary(tnn => tnn.Key, tnn => tnn.Select(x => (x.IdTipoUnidadTiempoAntelacion, x.CantAntelacion)).ToHashSet());
            Dictionary<long, HashSet<long>> templateNormasFiscalizadores = 
                (await templateNormaFiscalizadorDao.ObtenerPorTemplateNorma(idTemplate, idNorma, transaction))
                .GroupBy(tnf => tnf.IdNorma)
                .ToDictionary(tnf => tnf.Key, tnf => tnf.Select(x => x.IdTipoFiscalizador).ToHashSet());

            // Previo a eliminar el template norma, se desenlazan todas las normas suscritas relacionadas...
            List<NormaSuscrita> normasSuscritasDependientes = await normaSuscritaDao.ObtenerPorTemplate(idTemplate, idNorma, null, transaction);
            foreach (NormaSuscrita normaSuscrita in normasSuscritasDependientes) {
                // Si no tiene notificaciones, se dejan las del template norma...
                List<NotificacionNormaSuscrita> notificacionNormaSuscritas = await notificacionNormaSuscritaDao.ObtenerPorNormaSuscrita(normaSuscrita.Id, true, transaction);
                if (notificacionNormaSuscritas.Count == 0 && templateNormasNotificaciones.TryGetValue(normaSuscrita.IdNorma!.Value, out HashSet<(long IdTipoUnidadTiempoAntelacion, int CantAntelacion)>? templateNormaNotificacion)) {
                    await notificacionNormaSuscritaBcp.ActualizarPorNormaSuscrita(normaSuscrita, templateNormaNotificacion, transaction);
                }

                // Si no tiene fiscalizadores, se dejan los del template norma...
                List<FiscalizadorNormaSuscrita> fiscalizadorNormaSuscritas = await fiscalizadorNormaSuscritaDao.ObtenerPorNormaSuscrita(normaSuscrita.Id, true, transaction);
                if (fiscalizadorNormaSuscritas.Count == 0 && templateNormasFiscalizadores.TryGetValue(normaSuscrita.IdNorma!.Value, out HashSet<long>? templateNormaFiscalizador)) {
                    await fiscalizadorNormaSuscritaBcp.ActualizarPorNormaSuscrita(normaSuscrita, templateNormaFiscalizador, transaction);
                }

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

            await templateNormaNotificacionDao.Eliminar(idTemplate, idNorma, null, null, transaction);
            await templateNormaFiscalizadorDao.Eliminar(idTemplate, idNorma, null, transaction);
            await templateNormaDao.Eliminar(idTemplate, idNorma, transaction);
        }
    }
}
