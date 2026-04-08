using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("template", Schema = "tanatos")]
	[Comment("Tabla que contiene los templates de normas a inscribirse.")]
	public class Template {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Comment("Identificador del template.")]
		public required long Id { get; set; }

		[UseColumnAttribute]
		[Column("id_template_padre")]
		[Comment("Identificador del template padre.")]
		public long? IdTemplatePadre { get; set; }

		[UseColumnAttribute]
		[Column("nombre")]
		[Comment("Nombre del template.")]
		public required string Nombre { get; set; }

		[UseColumnAttribute]
		[Column("descripcion")]
		[Comment("Descripcion del template.")]
		public required string Descripcion { get; set; }

		[UseColumnAttribute]
		[Column("vigencia")]
		[Comment("Vigencia del template.")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdTemplatePadre))]
		public Template? TemplatePadre { get; set; }

		[JsonIgnore]
		public List<Template>? TemplatesHijos { get; set; }

		public List<TemplateNorma>? TemplateNormas { get; set; }

		[JsonIgnore]
		public List<InscripcionTemplate>? InscripcionesTemplate { get; set; }

		public List<TemplateActividad>? TemplateActividades { get; set; }

		public override int GetHashCode() {
			return HashCode.Combine(Id, IdTemplatePadre, Nombre, Descripcion, Vigencia);
		}

		public override bool Equals(object? obj) {
			if (obj is not Template other) {
				return false;
			}

			return Id == other.Id &&
					IdTemplatePadre == other.IdTemplatePadre &&
					Nombre == other.Nombre &&
					Descripcion == other.Descripcion &&
					Vigencia == other.Vigencia;
		}
	}
}
