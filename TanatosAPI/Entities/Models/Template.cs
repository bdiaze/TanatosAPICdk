using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("template", Schema = "tanatos")]
	[Comment("Tabla que contiene los templates de normas a inscribirse.")]
	public class Template {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Comment("Identificador del template.")]
		public required long Id { get; set; }

		[Column("id_template_padre")]
		[Comment("Identificador del template padre.")]
		public long? IdTemplatePadre { get; set; }

		[Column("nombre")]
		[Comment("Nombre del template.")]
		public required string Nombre { get; set; }

		[Column("descripcion")]
		[Comment("Descripcion del template.")]
		public required string Descripcion { get; set; }

		[Column("vigencia")]
		[Comment("Vigencia del template.")]
		public required bool Vigencia { get; set; }

		[ForeignKey(nameof(IdTemplatePadre))]
		public Template? TemplatePadre { get; set; }

		public List<Template>? TemplatesHijos { get; set; }

		public List<TemplateNorma>? TemplateNormas { get; set; }

		public List<InscripcionTemplate>? InscripcionesTemplate { get; set; }
	}
}
