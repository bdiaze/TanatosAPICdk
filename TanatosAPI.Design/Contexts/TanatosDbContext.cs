using Microsoft.EntityFrameworkCore;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Design.Contexts {
    // Solo usar el context para migrations del modelo de base de datos
    public class TanatosDbContext : DbContext {
		public TanatosDbContext(DbContextOptions<TanatosDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<InscripcionTemplate>(entity => {
                entity.HasKey(o => new { o.Sub, o.IdNegocio, o.IdTemplate });
                entity.HasIndex(o => new { o.IdTemplate });
                entity.ToTable(o => o.HasComment("Tabla que contiene los templates a los que un usuario está inscrito."));
                entity.Property(o => o.Sub).HasComment("Usuario al que está asociada la inscripción.");
                entity.Property(o => o.IdNegocio).HasComment("Identificador del negocio del usuario.");
                entity.Property(o => o.IdTemplate).HasComment("Identificador del template al que está inscrito el usuario.");
                entity.Property(o => o.FechaActivacion).HasComment("Fecha en que se activa la inscripción.");
                entity.Property(o => o.FechaDesactivacion).HasComment("Fecha en que se desactiva la inscripción.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia de la inscripción.");

                entity
                    .HasOne(o => o.Template)
                    .WithMany(c => c.InscripcionesTemplate)
                    .HasForeignKey(o => o.IdTemplate)
                    .OnDelete(DeleteBehavior.Restrict);
                entity
                    .HasOne(o => o.Negocio)
                    .WithMany(c => c.InscripcionesTemplates)
                    .HasForeignKey(o => o.IdNegocio)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Template>(entity => {
                entity.ToTable(o => o.HasComment("Tabla que contiene los templates de normas a inscribirse."));
                entity.Property(o => o.Id).HasComment("Identificador del template.");
                entity.Property(o => o.IdTemplatePadre).HasComment("Identificador del template padre.");
                entity.Property(o => o.Nombre).HasComment("Nombre del template.");
                entity.Property(o => o.Descripcion).HasComment("Descripcion del template.");
                entity.Property(o => o.RequierePlanEmpresa).HasComment("Indicador de si el template requiere de que el usuario tenga plan Empresa.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia del template.");

                entity
                    .HasOne(o => o.TemplatePadre)
                    .WithMany(c => c.TemplatesHijos)
                    .HasForeignKey(o => o.IdTemplatePadre)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TemplateNorma>(entity => {
                entity.HasKey(o => new { o.IdTemplate, o.IdNorma });
                entity.ToTable(o => o.HasComment("Tabla que contiene las normas asociadas a un template."));
                entity.Property(o => o.IdTemplate).HasComment("Identificador del template al que pertenece la norma.");
                entity.Property(o => o.IdNorma).HasComment("Identificador de la norma asociada al template.");
                entity.Property(o => o.Nombre).HasComment("Nombre de la norma.");
                entity.Property(o => o.Descripcion).HasComment("Descripcion de la norma.");
                entity.Property(o => o.IdTipoPeriodicidad).HasComment("Identificador del tipo de periodicidad asociado a la norma.");
                entity.Property(o => o.Multa).HasComment("Multa de no cumplir con la norma");
                entity.Property(o => o.IdCategoriaNorma).HasComment("Identificador de la categoría a la que pertenece la norma.");
                entity.Property(o => o.CronActivacionAutomatica).HasComment("Cron que define el próximo vencimiento de la obligación al momento de la inscripción.");
				entity.Property(o => o.DiasActivacionAutomatica).HasComment("Días que define el próximo vencimiento de la obligación al momento de la inscripción.");

				entity
                    .HasOne(o => o.Template)
                    .WithMany(c => c.TemplateNormas)
                    .HasForeignKey(o => o.IdTemplate)
                    .OnDelete(DeleteBehavior.Restrict);
                entity
                    .HasOne(o => o.TipoPeriodicidad)
                    .WithMany(c => c.TemplateNormas)
                    .HasForeignKey(o => o.IdTipoPeriodicidad)
                    .OnDelete(DeleteBehavior.Restrict);
                entity
                    .HasOne(o => o.CategoriaNorma)
                    .WithMany(c => c.TemplateNormas)
                    .HasForeignKey(o => o.IdCategoriaNorma)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TemplateNormaFiscalizador>(entity => {
                entity.HasKey(o => new { o.IdTemplate, o.IdNorma, o.IdTipoFiscalizador });
                entity.HasIndex(o => new { o.IdTipoFiscalizador });
                entity.ToTable(o => o.HasComment("Tabla que contiene la relación entre un template norma y un fiscalizador."));
                entity.Property(o => o.IdTemplate).HasComment("Identificador del template al que pertenece la norma.");
                entity.Property(o => o.IdNorma).HasComment("Identificador de la norma asociada al template.");
                entity.Property(o => o.IdTipoFiscalizador).HasComment("Identificador del tipo de fiscalizador.");

                entity
                    .HasOne(o => o.TemplateNorma)
                    .WithMany(c => c.TemplateNormaFiscalizadores)
                    .HasForeignKey(o => new { o.IdTemplate, o.IdNorma })
                    .OnDelete(DeleteBehavior.Restrict);
                entity
                    .HasOne(o => o.TipoFiscalizador)
                    .WithMany(c => c.TemplateNormasFiscalizador)
                    .HasForeignKey(o => o.IdTipoFiscalizador)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TemplateNormaNotificacion>(entity => {
                entity.HasKey(o => new { o.IdTemplate, o.IdNorma, o.IdTipoUnidadTiempoAntelacion, o.CantAntelacion });
                entity.HasIndex(o => new { o.IdTipoUnidadTiempoAntelacion });
                entity.ToTable(o => o.HasComment("Tabla que contiene las notificaciones asociadas a una template norma."));
                entity.Property(o => o.IdTemplate).HasComment("Identificador del template al que pertenece la norma.");
                entity.Property(o => o.IdNorma).HasComment("Identificador de la norma asociada al template.");
                entity.Property(o => o.IdTipoUnidadTiempoAntelacion).HasComment("Identificador del tipo de unidad de tiempo a usar para la notificación.");
                entity.Property(o => o.CantAntelacion).HasComment("Cantidad de unidades de tiempo a usar para la notificación.");

                entity
                    .HasOne(o => o.TemplateNorma)
                    .WithMany(c => c.TemplateNormaNotificaciones)
                    .HasForeignKey(o => new { o.IdTemplate, o.IdNorma })
                    .OnDelete(DeleteBehavior.Restrict);
                entity
                    .HasOne(o => o.TipoUnidadTiempoAntelacion)
                    .WithMany(c => c.TemplateNormasNotificacion)
                    .HasForeignKey(o => o.IdTipoUnidadTiempoAntelacion)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DestinatarioNotificacion>(entity => {
                entity.HasIndex(o => new { o.Sub, o.IdNegocio, o.IdEmpleado });
                entity.HasIndex(o => new { o.IdTipoReceptor });
                entity.HasIndex(o => new { o.CodigoValidacion }).IsUnique();
                entity.ToTable(o => o.HasComment("Tabla que contiene los destinatarios de las notificaciones de un usuario."));
                entity.Property(o => o.Id).HasComment("Identificador del destinatario de notificación.");
                entity.Property(o => o.Sub).HasComment("Usuario al que pertenece el destinatario de notificación.");
                entity.Property(o => o.IdNegocio).HasComment("Identificador del negocio del usuario.");
                entity.Property(o => o.IdEmpleado).HasComment("Identificador del empleado al que pertenece el destino.");
                entity.Property(o => o.IdTipoReceptor).HasComment("Identificador del tipo de receptor asociado al destinatario.");
                entity.Property(o => o.Alias).HasComment("Alias del destinatario.");
                entity.Property(o => o.Destino).HasComment("Destino de la notificación. Puede ser un correo o un número de Whatsapp.");
                entity.Property(o => o.CodigoValidacion).HasComment("Código generado para validar que el destinatario se suscribe a la notificación.");
                entity.Property(o => o.FechaCaducidadCodigoValidacion).HasComment("Fecha en que caduca el código de validación.");
                entity.Property(o => o.FechaValidacion).HasComment("Fecha en que se validó el destinatario.");
                entity.Property(o => o.Validado).HasComment("Identifica si el destinatario ya fue validado.");
                entity.Property(o => o.HermesIdMensaje).HasComment("ID del mensaje en Hermes.");
                entity.Property(o => o.FechaCreacion).HasComment("Fecha en que se creó el destinatario.");
                entity.Property(o => o.FechaEliminacion).HasComment("Fecha en que se eliminó el destinatario.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia del destinatario.");
                
                entity
                    .Property(x => x.FechaCaducidadCodigoValidacion)
                    .HasDefaultValueSql($"NOW() + INTERVAL '{DestinatarioNotificacionBcp.HORAS_CADUCIDAD_CODIGO_VALIDACION} hours'");


                entity
                    .HasOne(o => o.Negocio)
                    .WithMany(c => c.DestinatariosNotificaciones)
                    .HasForeignKey(o => o.IdNegocio)
                    .OnDelete(DeleteBehavior.Restrict);
                entity
                    .HasOne(o => o.Empleado)
                    .WithMany(c => c.DestinatariosNotificaciones)
                    .HasForeignKey(o => o.IdEmpleado)
                    .OnDelete(DeleteBehavior.Restrict);
                entity
                    .HasOne(o => o.TipoReceptorNotificacion)
                    .WithMany(c => c.DestinatariosNotificaciones)
                    .HasForeignKey(o => o.IdTipoReceptor)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<NormaSuscrita>(entity => {
                entity.HasIndex(o => new { o.Sub, o.IdNegocio });
                entity.ToTable(o => o.HasComment("Tabla que contiene las normas a las que está suscrita un negocio del usuario."));
                entity.Property(o => o.Id).HasComment("Identificador de la norma suscrita.");
                entity.Property(o => o.Sub).HasComment("Usuario al que pertenece la norma suscrita.");
                entity.Property(o => o.IdNegocio).HasComment("Identificador del negocio del usuario.");
                entity.Property(o => o.IdTemplate).HasComment("Identificador del template al que pertenece la norma suscrita.");
                entity.Property(o => o.IdNorma).HasComment("Identificador del template norma al que pertenece la norma suscrita.");
                entity.Property(o => o.Nombre).HasComment("Nombre de la norma.");
                entity.Property(o => o.Descripcion).HasComment("Descripcion de la norma.");
                entity.Property(o => o.IdTipoPeriodicidad).HasComment("Identificador del tipo de periodicidad asociado a la norma.");
                entity.Property(o => o.Multa).HasComment("Multa de no cumplir con la norma.");
                entity.Property(o => o.IdCategoriaNorma).HasComment("Identificador de la categoría a la que pertenece la norma.");
                entity.Property(o => o.IdCargo).HasComment("Identificador del cargo responsable de la norma.");
                entity.Property(o => o.OrdenVisual).HasComment("Orden en que se deben presentar las normas.");
                entity.Property(o => o.Editable).HasComment("Indicador de si es editable la norma.");
                entity.Property(o => o.FechaActivacion).HasComment("Fecha en que se activó el cumplimiento de la norma.");
                entity.Property(o => o.FechaDesactivacion).HasComment("Fecha en que se desactivó el cumplimiento de la norma.");
                entity.Property(o => o.Activado).HasComment("Estado de activación de la norma.");
                entity.Property(o => o.ProcesosNotificaciones).HasComment("Procesos de notificaciones asociados a la norma suscrita.");
                entity.Property(o => o.FechaCreacion).HasComment("Fecha en que se creó la norma.");
                entity.Property(o => o.FechaEliminacion).HasComment("Fecha en que se eliminó la norma.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia de la norma.");

                entity
                    .HasOne(o => o.Negocio)
                    .WithMany(o => o.NormasSuscritas)
                    .HasForeignKey(o => o.IdNegocio)
                    .OnDelete(DeleteBehavior.Restrict);
                entity
                    .HasOne(o => o.CategoriaNorma)
                    .WithMany(o => o.NormasSuscritas)
                    .HasForeignKey(o => o.IdCategoriaNorma)
                    .OnDelete(DeleteBehavior.Restrict);
                entity
                    .HasOne(o => o.TipoPeriodicidad)
                    .WithMany(o => o.NormasSuscritas)
                    .HasForeignKey(o => o.IdTipoPeriodicidad)
                    .OnDelete(DeleteBehavior.Restrict);
                entity
                    .HasOne(o => o.TemplateNorma)
                    .WithMany(o => o.NormasSuscritas)
                    .HasForeignKey(o => new { o.IdTemplate, o.IdNorma })
                    .OnDelete(DeleteBehavior.Restrict);
                entity
                    .HasOne(o => o.Cargo)
                    .WithMany(o => o.NormasSuscritas)
                    .HasForeignKey(o => o.IdCargo)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<FiscalizadorNormaSuscrita>(entity => {
                entity.HasIndex(o => new { o.IdNormaSuscrita, o.Vigencia });
                entity.ToTable(o => o.HasComment("Tabla que contiene los fiscalizadores asociados a una norma suscrita."));
                entity.Property(o => o.Id).HasComment("Identificador del fiscalizador asociado a una norma suscrita.");
                entity.Property(o => o.IdNormaSuscrita).HasComment("Identificador de la norma suscrita.");
                entity.Property(o => o.IdTipoFiscalizador).HasComment("Identificador del fiscalizador asociado.");
                entity.Property(o => o.FechaCreacion).HasComment("Fecha en que se creó al fiscalizador asociado.");
                entity.Property(o => o.FechaEliminacion).HasComment("Fecha en que se eliminó al fiscalizador asociado.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia del fiscalizador asociado.");

                entity
                    .HasOne(o => o.NormaSuscrita)
                    .WithMany(o => o.FiscalizadoresNormaSuscrita)
                    .HasForeignKey(o => o.IdNormaSuscrita)
                    .OnDelete(DeleteBehavior.Restrict);
                entity
                    .HasOne(o => o.TipoFiscalizador)
                    .WithMany(o => o.FiscalizadoresNormaSuscrita)
                    .HasForeignKey(o => o.IdTipoFiscalizador)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<NotificacionNormaSuscrita>(entity => {
                entity.HasIndex(o => new { o.IdNormaSuscrita, o.Vigencia });
                entity.ToTable(o => o.HasComment("Tabla que contiene las notificaciones asociados a una norma suscrita."));
                entity.Property(o => o.Id).HasComment("Identificador de la notificación asociada a una norma suscrita.");
                entity.Property(o => o.IdNormaSuscrita).HasComment("Identificador de la norma suscrita.");
                entity.Property(o => o.IdTipoUnidadTiempoAntelacion).HasComment("Identificador del tipo de unidad de tiempo a usar para la notificación.");
                entity.Property(o => o.CantAntelacion).HasComment("Cantidad de unidades de tiempo a usar para la notificación.");
                entity.Property(o => o.FechaCreacion).HasComment("Fecha en que se creó la notificación asociada.");
                entity.Property(o => o.FechaEliminacion).HasComment("Fecha en que se eliminó la notificación asociada.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia de la notificación asociada.");

                entity
                    .HasOne(o => o.NormaSuscrita)
                    .WithMany(o => o.NotificacionesNormaSuscrita)
                    .HasForeignKey(o => o.IdNormaSuscrita)
                    .OnDelete(DeleteBehavior.Restrict);
                entity
                    .HasOne(o => o.TipoUnidadTiempo)
                    .WithMany(o => o.NotificacionesNormaSuscrita)
                    .HasForeignKey(o => o.IdTipoUnidadTiempoAntelacion)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<HistorialNormaSuscrita>(entity => {
                entity.HasIndex(o => new { o.IdNormaSuscrita, o.FechaVencimiento });
                entity.HasIndex(o => new { o.FechaVencimiento });
                entity.ToTable(o => o.HasComment("Tabla que contiene el historial de ejecución de una norma suscrita."));
                entity.Property(o => o.Id).HasComment("Identificador del historial de ejecución de una norma suscrita.");
                entity.Property(o => o.IdNormaSuscrita).HasComment("Identificador de la norma suscrita.");
                entity.Property(o => o.FechaVencimiento).HasComment("Fecha en que vencerá la ejecución de la norma suscrita");
                entity.Property(o => o.FechaCompletitud).HasComment("Fecha en que se completó la ejecución de la norma suscrita.");
                entity.Property(o => o.FechaCreacion).HasComment("Fecha en que se creó el registro.");
                entity.Property(o => o.FechaEliminacion).HasComment("Fecha en que se eliminó el registro.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia del registro.");

                entity
                    .HasOne(o => o.NormaSuscrita)
                    .WithMany(o => o.HistorialesNormaSuscrita)
                    .HasForeignKey(o => o.IdNormaSuscrita)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TipoActividad>(entity => {
                entity.ToTable(o => o.HasComment("Tabla que contiene las actividades que puede hacer un negocio."));
                entity.Property(o => o.Id).HasComment("Identificador de la actividad.");
                entity.Property(o => o.IdTipoRubro).HasComment("Identificador del rubro al que pertenece la actividad.");
                entity.Property(o => o.Nombre).HasComment("Nombre de la actividad.");
                entity.Property(o => o.Descripcion).HasComment("Descripción de la actividad.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia de la actividad.");

                entity
                    .HasOne(o => o.TipoRubro)
                    .WithMany(o => o.TiposActividades)
                    .HasForeignKey(o => o.IdTipoRubro)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Negocio>(entity => {
                entity.HasIndex(o => new { o.Sub, o.Nombre });
                entity.ToTable(o => o.HasComment("Tabla que contiene los negocios de un usuario."));
                entity.Property(o => o.Id).HasComment("Identificador del negocio.");
                entity.Property(o => o.Sub).HasComment("Usuario al que pertenece el negocio.");
                entity.Property(o => o.Nombre).HasComment("Nombre del negocio.");
                entity.Property(o => o.Direccion).HasComment("Dirección del negocio.");
                entity.Property(o => o.IdTipoActividad).HasComment("Identificador de la actividad que efectúa el negocio.");
                entity.Property(o => o.FechaCreacion).HasComment("Fecha en que se creó el negocio.");
                entity.Property(o => o.FechaEliminacion).HasComment("Fecha en que se eliminó el negocio.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia del negocio.");

                entity
                    .HasOne(o => o.TipoActividad)
                    .WithMany(o => o.Negocios)
                    .HasForeignKey(o => o.IdTipoActividad)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TemplateActividad>(entity => {
                entity.HasKey(o => new { o.IdTemplate, o.IdTipoActividad });
                entity.ToTable(o => o.HasComment("Tabla que contiene la recomendación de templates según tipo de actividad de un negocio."));
                entity.Property(o => o.IdTemplate).HasComment("Identificador del template.");
                entity.Property(o => o.IdTipoActividad).HasComment("Identificador del tipo de actividad del negocio.");

                entity
                    .HasOne(o => o.Template)
                    .WithMany(o => o.TemplateActividades)
                    .HasForeignKey(o => o.IdTemplate)
                    .OnDelete(DeleteBehavior.Restrict);
                entity
                    .HasOne(o => o.TipoActividad)
                    .WithMany(o => o.TemplatesActividad)
                    .HasForeignKey(o => o.IdTipoActividad)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<HistorialNotificacion>(entity => {
                entity.HasIndex(o => new { o.CodigoAcceso }).IsUnique();
                entity.ToTable(o => o.HasComment("Tabla que contiene el historial de notificaciones de una norma suscrita."));
                entity.Property(o => o.Id).HasComment("Identificador del historial de notificación de una norma suscrita.");
                entity.Property(o => o.IdHistorialNormaSuscrita).HasComment("Identificador del historial de ejecución de una norma suscrita.");
                entity.Property(o => o.IdDestinatarioNotificacion).HasComment("Identificador del destinatario de la notificación.");
                entity.Property(o => o.IdTipoUnidadTiempoAntelacion).HasComment("Identificador del tipo de unidad de tiempo correspondiente a la notificación.");
                entity.Property(o => o.CantAntelacion).HasComment("Cantidad de unidades de tiempo correspondientes a la notificación.");
                entity.Property(o => o.FechaProgramacion).HasComment("Fecha en que se programó el envío de la notificación.");
                entity.Property(o => o.FechaEjecucion).HasComment("Fecha en que se ejecutó el envío de la notificación.");
                entity.Property(o => o.Estado).HasComment("Estado de la notificación - 0: Pendiente - 1: Enviado - 2: Omitido.");
                entity.Property(o => o.Observacion).HasComment("Observación relacionada a la notificación.");
                entity.Property(o => o.CodigoAcceso).HasComment("Código generado para acceder al vencimiento desde la notificación.");
                entity.Property(o => o.FechaCaducidadCodigoAcceso).HasComment("Fecha en que caduca el código de acceso.");
                entity.Property(o => o.HermesIdMensaje).HasComment("ID del mensaje en Hermes.");
                entity.Property(o => o.FechaCreacion).HasComment("Fecha en que se creó el registro.");
                entity.Property(o => o.FechaEliminacion).HasComment("Fecha en que se eliminó el registro.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia del registro.");

                entity
                    .HasOne(o => o.DestinatarioNotificacion)
                    .WithMany(o => o.HistorialNotificaciones)
                    .HasForeignKey(o => o.IdDestinatarioNotificacion)
                    .OnDelete(DeleteBehavior.Restrict);
                entity
                    .HasOne(o => o.HistorialNormaSuscrita)
                    .WithMany(o => o.HistorialNotificaciones)
                    .HasForeignKey(o => o.IdHistorialNormaSuscrita)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DocumentoAdjunto>(entity => {
                entity.HasIndex(o => new { o.BucketName, o.BucketKey });
                entity.ToTable(o => o.HasComment("Tabla que contiene la metadata de los documentos adjuntos asociados al historial de ejecución de una norma suscrita."));
                entity.Property(o => o.Id).HasComment("Identificador del documento adjunto.");
                entity.Property(o => o.IdHistorialNormaSuscrita).HasComment("Identificador del historial de ejecución de una norma suscrita.");
                entity.Property(o => o.BucketName).HasComment("Nombre del bucket donde está almacenado el documento.");
                entity.Property(o => o.BucketKey).HasComment("Identificador del objeto dentro del bucket.");
                entity.Property(o => o.NombreArchivo).HasComment("Nombre original del archivo.");
                entity.Property(o => o.MimeEsperado).HasComment("Mime esperado del archivo.");
                entity.Property(o => o.TamannoEsperado).HasComment("Tamaño esperado del archivo en bytes.");
                entity.Property(o => o.MimeReal).HasComment("Mime real del archivo.");
                entity.Property(o => o.TamannoReal).HasComment("Tamaño real del archivo en bytes.");
                entity.Property(o => o.EstadoSubida).HasComment("Estado de subida del documento adjunto. 0: Generada URL prefirmada para PUT - 1: Documento recepcionado.");
                entity.Property(o => o.FechaEmisionUrlPrefirmadaPut).HasComment("Fecha en que se emitió la URL prefirmada para método PUT.");
                entity.Property(o => o.FechaConfirmacionSubida).HasComment("Fecha en que se confirmó la subida del archivo.");
                entity.Property(o => o.FechaCreacion).HasComment("Fecha en que se creó el registro.");
                entity.Property(o => o.FechaEliminacion).HasComment("Fecha en que se eliminó el registro.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia del registro.");

                entity
                    .HasOne(o => o.HistorialNormaSuscrita)
                    .WithMany(o => o.DocumentosAdjuntos)
                    .HasForeignKey(o => o.IdHistorialNormaSuscrita)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Suscripcion>(entity => {
                entity.HasIndex(o => new { o.Sub });
                entity.HasIndex(o => new { o.FlowSubscriptionId }).IsUnique();
                entity.ToTable(o => o.HasComment("Tabla que contiene las suscripciones de los usuarios."));
                entity.Property(o => o.Id).HasComment("Identificador de la suscripción.");
                entity.Property(o => o.Sub).HasComment("Usuario al que pertenece la suscripción.");
                entity.Property(o => o.IdPlan).HasComment("Identificador del plan al que el usuario está suscrito.");
                entity.Property(o => o.FechaInicio).HasComment("Fecha en que se inició la suscripción.");
                entity.Property(o => o.FechaExpiracion).HasComment("Fecha en que expira la suscripción.");
                entity.Property(o => o.FechaCancelacion).HasComment("Fecha en que se cancela la suscripción.");
                entity.Property(o => o.Estado).HasComment("Estado de la suscripción. 1: Activa - 2: Cancelada - 3: Expirada - 4: Pago Pendiente.");
                entity.Property(o => o.FlowCustomerId).HasComment("ID del cliente en la plataforma Flow.");
                entity.Property(o => o.FlowSubscriptionId).HasComment("ID de la suscripción en la plataforma Flow.");
                entity.Property(o => o.FechaCreacion).HasComment("Fecha en que se creó la suscripción.");
                entity.Property(o => o.FechaEliminacion).HasComment("Fecha en que se eliminó la suscripción.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia de la suscripción.");

                entity
                    .HasOne(o => o.Plan)
                    .WithMany(o => o.Suscripciones)
                    .HasForeignKey(o => o.IdPlan)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Pago>(entity => {
                entity.HasIndex(o => new { o.Sub });
                entity.HasIndex(o => new { o.FlowSubscriptionId, o.FlowInvoiceId }).IsUnique();
                entity.ToTable(o => o.HasComment("Tabla que contiene los pagos de los usuarios."));
                entity.Property(o => o.Id).HasComment("Identificador del pago.");
                entity.Property(o => o.Sub).HasComment("Usuario al que pertenece el pago.");
                entity.Property(o => o.IdSuscripcion).HasComment("Identificador de la suscripción a la que pertenece el pago.");
                entity.Property(o => o.Monto).HasComment("Monto del pago efectuado.");
                entity.Property(o => o.Moneda).HasComment("Moneda en que se efectuó el pago.");
                entity.Property(o => o.FechaPago).HasComment("Fecha en que se efectuó el pago.");
                entity.Property(o => o.Estado).HasComment("Estado del pago. 0: Pendiente - 1: Pagado - 2: Fallido - 3: Reembolsado.");
                entity.Property(o => o.FlowSubscriptionId).HasComment("ID de la suscripción en la plataforma Flow.");
                entity.Property(o => o.FlowInvoiceId).HasComment("ID del invoice en la plataforma Flow.");
                entity.Property(o => o.FechaCreacion).HasComment("Fecha en que se creó el pago.");
                entity.Property(o => o.FechaEliminacion).HasComment("Fecha en que se eliminó el pago.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia del pago.");

                entity
                    .HasOne(o => o.Suscripcion)
                    .WithMany(o => o.Pagos)
                    .HasForeignKey(o => o.IdSuscripcion)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EventoPago>(entity => {
                entity.ToTable(o => o.HasComment("Tabla que contiene los eventos de pagos recepcionados."));
                entity.Property(o => o.Id).HasComment("Identificador del evento de pago.");
                entity.Property(o => o.Proveedor).HasComment("Proveedor que informa el evento de pago.");
                entity.Property(o => o.Evento).HasComment("Tipo de evento.");
                entity.Property(o => o.Payload).HasComment("Payload del evento recepcionado desde el proveedor.");
                entity.Property(o => o.Procesado).HasComment("Indicador de si evento fue procesado.");
                entity.Property(o => o.FechaCreacion).HasComment("Fecha en que se creó el evento de pago.");
                entity.Property(o => o.FechaEliminacion).HasComment("Fecha en que se eliminó el evento de pago.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia del evento de pago.");
            });

            modelBuilder.Entity<CategoriaNorma>(entity => {
                entity.ToTable(o => o.HasComment("Tabla que contiene las categorías de las normas"));
                entity.Property(o => o.Id).HasComment("Identificador de la categoría.");
                entity.Property(o => o.Nombre).HasComment("Nombre de la categoría.");
                entity.Property(o => o.NombreCorto).HasComment("Nombre corto de la categoría.");
                entity.Property(o => o.Descripcion).HasComment("Descripción de la categoría.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia de la categoría.");
            });

            modelBuilder.Entity<Cargo>(entity => {
                entity.HasIndex(o => new { o.Sub, o.IdNegocio });
                entity.ToTable(o => o.HasComment("Tabla que contiene los cargos asociados a un negocio."));
                entity.Property(o => o.Id).HasComment("Identificador del cargo.");
                entity.Property(o => o.Sub).HasComment("Usuario al que pertenece el cargo.");
                entity.Property(o => o.IdNegocio).HasComment("Identificador del negocio del usuario.");
                entity.Property(o => o.Nombre).HasComment("Nombre del cargo.");
                entity.Property(o => o.FechaCreacion).HasComment("Fecha en que se creó el cargo.");
                entity.Property(o => o.FechaEliminacion).HasComment("Fecha en que se eliminó el cargo.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia del cargo.");
                entity
                    .HasOne(o => o.Negocio)
                    .WithMany(o => o.Cargos)
                    .HasForeignKey(o => o.IdNegocio)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Empleado>(entity => {
                entity.HasIndex(o => new { o.Sub, o.IdNegocio });
                entity.ToTable(o => o.HasComment("Tabla que contiene los empleados asociados a un negocio."));
                entity.Property(o => o.Id).HasComment("Identificador del empleado.");
                entity.Property(o => o.Sub).HasComment("Usuario al que pertenece el empleado.");
                entity.Property(o => o.IdNegocio).HasComment("Identificador del negocio del usuario.");
                entity.Property(o => o.Nombre).HasComment("Nombre del empleado.");
                entity.Property(o => o.IdCargo).HasComment("Identificador del cargo del empleado.");
                entity.Property(o => o.FechaCreacion).HasComment("Fecha en que se creó el empleado.");
                entity.Property(o => o.FechaEliminacion).HasComment("Fecha en que se eliminó el empleado.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia del empleado.");

                entity
                    .HasOne(o => o.Negocio)
                    .WithMany(o => o.Empleados)
                    .HasForeignKey(o => o.IdNegocio)
                    .OnDelete(DeleteBehavior.Restrict);
                entity
                    .HasOne(o => o.Cargo)
                    .WithMany(o => o.Empleados)
                    .HasForeignKey(o => o.IdCargo)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Mensaje>(entity => {
                entity.HasIndex(o => new { o.Sub });
                entity.HasIndex(o => new { o.Correo });
                entity.HasIndex(o => new { o.FechaCreacion });
                entity.ToTable(o => o.HasComment("Tabla que contiene los mensajes ingresados por formulario de contacto."));
                entity.Property(o => o.Id).HasComment("Identificador de la notificación asociada a una norma suscrita.");
                entity.Property(o => o.Sub).HasComment("Usuario que ingresó el mensaje.");
                entity.Property(o => o.Nombre).HasComment("Nombre del usuario que ingresó el mensaje.");
                entity.Property(o => o.Correo).HasComment("Correo electrónico del usuario que ingresó el mensaje.");
                entity.Property(o => o.Contenido).HasComment("Contenido del mensaje.");
                entity.Property(o => o.HermesIdMensaje).HasComment("ID del mensaje en Hermes.");
                entity.Property(o => o.FechaCreacion).HasComment("Fecha en que se creó el mensaje.");
            });

            modelBuilder.Entity<Plan>(entity => {
                entity.HasIndex(o => new { o.FlowPlanId }).IsUnique();
                entity.ToTable(o => o.HasComment("Tabla que contiene los planes de suscripción."));
                entity.Property(o => o.Id).HasComment("Identificador del plan.");
                entity.Property(o => o.Nombre).HasComment("Nombre del plan.");
                entity.Property(o => o.Precio).HasComment("Precio del plan.");
                entity.Property(o => o.DuracionMeses).HasComment("Duración del plan en meses.");
                entity.Property(o => o.SuscripcionUnica).HasComment("Indicador de si el plan solo permite una suscripción única por usuario.");
                entity.Property(o => o.FlowPlanId).HasComment("ID del plan en la plataforma Flow.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia del plan.");
            });

            modelBuilder.Entity<TipoFiscalizador>(entity => {
                entity.ToTable(o => o.HasComment("Tabla que contiene los tipos de fiscalizadores de las normas."));
                entity.Property(o => o.Id).HasComment("Identificador del tipo de fiscalizador.");
                entity.Property(o => o.Nombre).HasComment("Nombre del tipo de fiscalizador.");
                entity.Property(o => o.NombreCorto).HasComment("Nombre corto del tipo de fiscalizador.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia del tipo de fiscalizador.");
            });

            modelBuilder.Entity<TipoPeriodicidad>(entity => {
                entity.ToTable(o => o.HasComment("Tabla que contiene los tipos de periodicidad."));
                entity.Property(o => o.Id).HasComment("Identificador del tipo de periodicidad.");
                entity.Property(o => o.Nombre).HasComment("Nombre del tipo de periodicidad.");
                entity.Property(o => o.Descripcion).HasComment("Descripción del tipo de periodicidad.");
                entity.Property(o => o.Cron).HasComment("Cron del tipo de periodicidad.");
                entity.Property(o => o.DeltaDias).HasComment("Delta en días de la periodicidad.");
                entity.Property(o => o.DeltaMeses).HasComment("Delta en meses de la periodicidad.");
                entity.Property(o => o.DeltaAnnos).HasComment("Delta en años de la periodicidad.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia del tipo de periodicidad.");
            });

            modelBuilder.Entity<TipoReceptorNotificacion>(entity => {
                entity.ToTable(o => o.HasComment("Tabla que contiene los tipos de receptores de notificación."));
                entity.Property(o => o.Id).HasComment("Identificador del tipo de receptor de notificación.");
                entity.Property(o => o.Nombre).HasComment("Nombre del tipo de receptor de notificación.");
                entity.Property(o => o.RegexValidacion).HasComment("Regex para validar el tipo de receptor.");
                entity.Property(o => o.RequierePlanEmpresa).HasComment("Indicador de si el tipo de receptor requiere de que el usuario tenga plan Empresa.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia del tipo de receptor de notificación.");
            });

            modelBuilder.Entity<TipoRubro>(entity => {
                entity.ToTable(o => o.HasComment("Tabla que contiene los rubros a los que puede pertenecer un negocio."));
                entity.Property(o => o.Id).HasComment("Identificador del rubro.");
                entity.Property(o => o.Nombre).HasComment("Nombre del rubro.");
                entity.Property(o => o.Descripcion).HasComment("Descripción del rubro.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia del rubro.");
            });

            modelBuilder.Entity<TipoUnidadTiempo>(entity => {
                entity.ToTable(o => o.HasComment("Tabla que contiene los tipos de unidades de tiempo."));
                entity.Property(o => o.Id).HasComment("Identificador del tipo de unidad de tiempo.");
                entity.Property(o => o.Nombre).HasComment("Nombre del tipo de unidad de tiempo.");
                entity.Property(o => o.NombrePlural).HasComment("Nombre plural del tipo de unidad de tiempo.");
                entity.Property(o => o.CantSegundos).HasComment("Cantidad de segundos que representan a la unidad de tiempo.");
                entity.Property(o => o.CantMinutos).HasComment("Cantidad de minutos que representan a la unidad de tiempo.");
                entity.Property(o => o.CantHoras).HasComment("Cantidad de horas que representan a la unidad de tiempo.");
                entity.Property(o => o.CantDias).HasComment("Cantidad de días que representan a la unidad de tiempo.");
                entity.Property(o => o.Vigencia).HasComment("Vigencia del tipo de unidad de tiempo.");
            });

            modelBuilder.Entity<Usuario>(entity => {
                entity.HasIndex(o => new { o.FlowCustomerId }).IsUnique();
                entity.ToTable(o => o.HasComment("Tabla que contiene la información del usuario."));
                entity.Property(o => o.Sub).HasComment("Identificador del usuario.");
                entity.Property(o => o.FlowCustomerId).HasComment("ID del cliente en Flow.");
                entity.Property(o => o.Nombre).HasComment("Nombre del usuario.");
                entity.Property(o => o.Apellido).HasComment("Apellido del usuario.");
                entity.Property(o => o.CorreoElectronico).HasComment("Correo electrónico del usuario.");
            });

			modelBuilder.Entity<PreguntaFrecuente>(entity => {
				entity.ToTable(o => o.HasComment("Tabla que contiene las preguntas frecuentes con sus respectivas respuestas."));
				entity.Property(o => o.Id).HasComment("Identificador de la pregunta frecuente.");
				entity.Property(o => o.Pregunta).HasComment("Título de la pregunta frecuente.");
				entity.Property(o => o.Respuesta).HasComment("Respuesta a la pregunta frecuente.");
				entity.Property(o => o.Habilitado).HasComment("Indicador de si la pregunta frecuente está habilitada.");
				entity.Property(o => o.Orden).HasComment("Orden en que se presenta la pregunta frecuente.");
				entity.Property(o => o.FechaCreacion).HasComment("Fecha en que se creó la pregunta frecuente.");
				entity.Property(o => o.FechaEliminacion).HasComment("Fecha en que se eliminó la pregunta frecuente.");
				entity.Property(o => o.Vigencia).HasComment("Vigencia de la pregunta frecuente.");
			});

			modelBuilder.Entity<VideoTutorial>(entity => {
				entity.ToTable(o => o.HasComment("Tabla que contiene los videos tutoriales."));
				entity.Property(o => o.Id).HasComment("Identificador del video tutorial.");
				entity.Property(o => o.Titulo).HasComment("Título del video tutorial.");
				entity.Property(o => o.Descripcion).HasComment("Descripción del video tutorial.");
				entity.Property(o => o.Url).HasComment("URL del video tutorial.");
				entity.Property(o => o.Habilitado).HasComment("Indicador de si el video tutorial está habilitado.");
				entity.Property(o => o.Orden).HasComment("Orden en que se presenta el video tutorial.");
				entity.Property(o => o.FechaCreacion).HasComment("Fecha en que se creó el video tutorial.");
				entity.Property(o => o.FechaEliminacion).HasComment("Fecha en que se eliminó el video tutorial.");
				entity.Property(o => o.Vigencia).HasComment("Vigencia del video tutorial.");
			});
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

        public DbSet<HistorialNotificacion> HistorialNotificaciones { get; set; }

        public DbSet<TipoRubro> TiposRubros { get; set; }

        public DbSet<TipoActividad> TiposActividades { get; set; }

		public DbSet<TemplateActividad> TemplatesActividades  { get; set; }

        public DbSet<DocumentoAdjunto> DocumentosAdjuntos { get; set; }

        public DbSet<Mensaje> Mensajes { get; set; }

        public DbSet<Plan> Planes { get; set; }

        public DbSet<Suscripcion> Suscripciones { get; set; }

        public DbSet<Pago> Pagos { get; set; }

        public DbSet<EventoPago> EventosPagos { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Cargo> Cargos { get; set; }

		public DbSet<Empleado> Empleados { get; set; }

        public DbSet<PreguntaFrecuente> PreguntasFrecuentes { get; set; }

        public DbSet<VideoTutorial> VideosTutoriales { get; set; }
	}
}
