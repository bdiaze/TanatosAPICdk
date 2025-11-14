using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TanatosAPI.Entities.Contexts {
    // Solo usar el context para migrations del modelo de base de datos
    public class TanatosDbContext : DbContext {

        public TanatosDbContext(DbContextOptions<TanatosDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<DestinatarioNotificacion>()
                .HasOne(o => o.TipoReceptorNotificacion)
                .WithMany(c => c.DestinatariosNotificaciones)
                .HasForeignKey(o => o.IdTipoReceptor)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public DbSet<TipoReceptorNotificacion> TiposReceptoresNotificaciones { get; set; }

        public DbSet<DestinatarioNotificacion> DestinatariosNotificaciones { get; set; }

        public DbSet<CategoriaNorma> CategoriasNormas { get; set; }

        public DbSet<TipoFiscalizador> TiposFiscalizadores { get; set; }

        public DbSet<TipoPeriodicidad> TiposPeriodicidades { get; set; }

        public DbSet<TipoUnidadTiempo> TiposUnidadesTiempo { get; set; }
    }
}
