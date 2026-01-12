using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using TanatosAPI.Endpoints;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TanatosAPI.Entities.Contexts {
    // Solo usar el context para migrations del modelo de base de datos
    public class TanatosDbContext : DbContext {
		[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
		[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
		public TanatosDbContext(DbContextOptions<TanatosDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<DestinatarioNotificacion>()
                .HasOne(o => o.TipoReceptorNotificacion)
                .WithMany(c => c.DestinatariosNotificaciones)
                .HasForeignKey(o => o.IdTipoReceptor)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DestinatarioNotificacion>()
                .Property(x => x.FechaCaducidadCodigoValidacion)
                .HasDefaultValueSql($"NOW() + INTERVAL '{DestinatarioNotificacionEndpoints.HORAS_CADUCIDAD_CODIGO_VALIDACION} hours'");

			modelBuilder.Entity<InscripcionTemplate>()
                .HasOne(o => o.Template)
                .WithMany(c => c.InscripcionesTemplate)
                .HasForeignKey(o => o.IdTemplate)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InscripcionTemplate>()
                .HasOne(o => o.Negocio)
                .WithMany(c => c.InscripcionesTemplates)
                .HasForeignKey(o => o.IdNegocio)
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
                .HasForeignKey(o => new { o.IdTemplate, o.IdNorma })
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TemplateNormaFiscalizador>()
                .HasOne(o => o.TipoFiscalizador)
                .WithMany(c => c.TemplateNormasFiscalizador)
                .HasForeignKey(o => o.IdTipoFiscalizador)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TemplateNormaNotificacion>()
                .HasOne(o => o.TemplateNorma)
                .WithMany(c => c.TemplateNormaNotificaciones)
                .HasForeignKey(o => new { o.IdTemplate, o.IdNorma })
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TemplateNormaNotificacion>()
                .HasOne(o => o.TipoUnidadTiempoAntelacion)
                .WithMany(c => c.TemplateNormasNotificacion)
                .HasForeignKey(o => o.IdTipoUnidadTiempoAntelacion)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DestinatarioNotificacion>()
                .HasOne(o => o.Negocio)
                .WithMany(c => c.DestinatariosNotificaciones)
                .HasForeignKey(o => o.IdNegocio)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NormaSuscrita>()
                .HasOne(o => o.Negocio)
                .WithMany(o => o.NormasSuscritas)
                .HasForeignKey(o => o.IdNegocio)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NormaSuscrita>()
                .HasOne(o => o.CategoriaNorma)
                .WithMany(o => o.NormasSuscritas)
                .HasForeignKey(o => o.IdCategoriaNorma)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NormaSuscrita>()
                .HasOne(o => o.TipoPeriodicidad)
                .WithMany(o => o.NormasSuscritas)
                .HasForeignKey(o => o.IdTipoPeriodicidad)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NormaSuscrita>()
                .HasOne(o => o.TemplateNorma)
                .WithMany(o => o.NormasSuscritas)
                .HasForeignKey(o => new { o.IdTemplate, o.IdNorma })
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FiscalizadorNormaSuscrita>()
                .HasOne(o => o.NormaSuscrita)
                .WithMany(o => o.FiscalizadoresNormaSuscrita)
                .HasForeignKey(o => o.IdNormaSuscrita)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FiscalizadorNormaSuscrita>()
                .HasOne(o => o.TipoFiscalizador)
                .WithMany(o => o.FiscalizadoresNormaSuscrita)
                .HasForeignKey(o => o.IdTipoFiscalizador)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NotificacionNormaSuscrita>()
                .HasOne(o => o.NormaSuscrita)
                .WithMany(o => o.NotificacionesNormaSuscrita)
                .HasForeignKey(o => o.IdNormaSuscrita)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NotificacionNormaSuscrita>()
                .HasOne(o => o.TipoUnidadTiempo)
                .WithMany(o => o.NotificacionesNormaSuscrita)
                .HasForeignKey(o => o.IdTipoUnidadTiempoAntelacion)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HistorialNormaSuscrita>()
                .HasOne(o => o.NormaSuscrita)
                .WithMany(o => o.HistorialesNormaSuscrita)
                .HasForeignKey(o => o.IdNormaSuscrita)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TipoActividad>()
                .HasOne(o => o.TipoRubro)
                .WithMany(o => o.TiposActividades)
                .HasForeignKey(o => o.IdTipoRubro)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Negocio>()
                .HasOne(o => o.TipoActividad)
                .WithMany(o => o.Negocios)
                .HasForeignKey(o => o.IdTipoActividad)
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

        public DbSet<Negocio> Negocios { get; set; }

        public DbSet<NormaSuscrita> NormasSuscritas { get; set; }

        public DbSet<FiscalizadorNormaSuscrita> FiscalizadoresNormasSuscritas { get; set; }

        public DbSet<NotificacionNormaSuscrita> NotificacionesNormasSuscritas { get; set; }

        public DbSet<HistorialNormaSuscrita> HistorialesNormasSuscritas { get; set; }

        public DbSet<TipoRubro> TiposRubros { get; set; }

        public DbSet<TipoActividad> TiposActividades { get; set; }
	}
}
