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

            modelBuilder.Entity<InscripcionTemplate>()
                .HasOne(o => o.Template)
                .WithMany(c => c.InscripcionesTemplate)
                .HasForeignKey(o => o.IdTemplate)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Template>()
                .HasOne(o => o.TemplatePadre)
                .WithMany(c => c.TemplatesHijos)
                .HasForeignKey(o => o.IdTemplatePadre)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TemplateNorma>()
                .HasOne(o => o.Template)
                .WithMany(c => c.TemplateNormas)
                .HasForeignKey(o => o.IdTemplate)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TemplateNorma>()
                .HasOne(o => o.TipoPeriodicidad)
                .WithMany(c => c.TemplateNormas)
                .HasForeignKey(o => o.IdTipoPeriodicidad)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<TemplateNorma>()
                .HasOne(o => o.CategoriaNorma)
                .WithMany(c => c.TemplateNormas)
                .HasForeignKey(o => o.IdCategoriaNorma)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TemplateNormaFiscalizador>()
                .HasOne(o => o.TemplateNorma)
                .WithMany(c => c.TemplateNormaFiscalizadores)
                .HasForeignKey(o => o.IdTemplateNorma)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TemplateNormaFiscalizador>()
                .HasOne(o => o.TipoFiscalizador)
                .WithMany(c => c.TemplateNormasFiscalizador)
                .HasForeignKey(o => o.IdTipoFiscalizador)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TemplateNormaNotificacion>()
                .HasOne(o => o.TemplateNorma)
                .WithMany(c => c.TemplateNormaNotificaciones)
                .HasForeignKey(o => o.IdTemplateNorma)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TemplateNormaNotificacion>()
                .HasOne(o => o.TipoUnidadTiempoAntelacion)
                .WithMany(c => c.TemplateNormasNotificacion)
                .HasForeignKey(o => o.IdTipoUnidadTiempoAntelacion)
                .OnDelete(DeleteBehavior.Restrict);
		}

        public DbSet<TipoReceptorNotificacion> TiposReceptoresNotificaciones { get; set; }

        public DbSet<DestinatarioNotificacion> DestinatariosNotificaciones { get; set; }

        public DbSet<CategoriaNorma> CategoriasNormas { get; set; }

        public DbSet<TipoFiscalizador> TiposFiscalizadores { get; set; }

        public DbSet<TipoPeriodicidad> TiposPeriodicidades { get; set; }

        public DbSet<TipoUnidadTiempo> TiposUnidadesTiempo { get; set; }

        public DbSet<InscripcionTemplate> InscripcionesTemplates { get; set; }

        public DbSet<Template> Templates { get; set; }

        public DbSet<TemplateNorma> TemplatesNormas { get; set; }

        public DbSet<TemplateNormaFiscalizador> TemplatesNormasFiscalizadores { get; set; }

        public DbSet<TemplateNormaNotificacion> TemplatesNormasNotificaciones { get; set; }
    }
}
