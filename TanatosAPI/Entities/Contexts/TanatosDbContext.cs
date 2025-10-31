using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TanatosAPI.Entities.Contexts {
    public class TanatosDbContext : DbContext {

        public TanatosDbContext(DbContextOptions<TanatosDbContext> options) : base(options) { }

        public DbSet<TipoReceptorNotificacion> TipoReceptorNotificaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.UseIdentityAlwaysColumns();
        }
    }
}
