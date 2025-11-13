using Microsoft.EntityFrameworkCore;
using TanatosAPI.Entities.Contexts;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Entities.Queries {
    public static class TipoReceptorNotificacionQueries {
        public static readonly Func<TanatosDbContext, long, Task<TipoReceptorNotificacion?>> GetByIdAsync = EF.CompileAsyncQuery(
            (TanatosDbContext db, long id) => db.TipoReceptorNotificaciones.FirstOrDefault(x => x.Id == id)
        );
    }
}
