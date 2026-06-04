using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("template", Schema = "tanatos")]
	public class Template {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public required long Id { get; set; }

		[Column("id_template_padre")]
		public long? IdTemplatePadre { get; set; }

		[Column("nombre")]
		public required string Nombre { get; set; }

		[Column("descripcion")]
		public required string Descripcion { get; set; }

		[Required]
		[DefaultValue(false)]
		[Column("requiere_plan_empresa")]
		public required bool RequierePlanEmpresa { get; set; } = false;

		[Column("vigencia")]
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
			return HashCode.Combine(Id, IdTemplatePadre, Nombre, Descripcion, RequierePlanEmpresa, Vigencia);
		}

		public override bool Equals(object? obj) {
			if (obj is not Template other) {
				return false;
			}

			return Id == other.Id &&
					IdTemplatePadre == other.IdTemplatePadre &&
					Nombre == other.Nombre &&
					Descripcion == other.Descripcion &&
					RequierePlanEmpresa == other.RequierePlanEmpresa &&
					Vigencia == other.Vigencia;
		}
	}
}
