using Npgsql;
using System.Reflection.Metadata.Ecma335;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;
using TanatosAPI.UseCases;

namespace TanatosAPI.Business {
    public class TemplateNormaBcp(ITemplateNormaDao templateNormaDao) : ITemplateNormaBcp {
        public async Task<List<TemplateNorma>> ObtenerPorTemplate(long idTemplate, NpgsqlTransaction? transaction = null) {
            return await templateNormaDao.ObtenerPorTemplate(idTemplate, transaction);
        }

        public async Task<TemplateNorma?> ObtenerPorTemplateNorma(long idTemplate, long idNorma, NpgsqlTransaction? transaction = null) {
            return (await templateNormaDao.ObtenerPorTemplate(idTemplate, transaction)).FirstOrDefault(n => n.IdNorma == idNorma);
        }
    }
}
