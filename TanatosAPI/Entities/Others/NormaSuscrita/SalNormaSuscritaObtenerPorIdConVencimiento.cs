using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Others.DocumentoAdjunto;

namespace TanatosAPI.Entities.Others.NormaSuscrita {
    [ExcludeFromCodeCoverage]
    public class SalNormaSuscritaObtenerPorIdConVencimiento {
		public required bool TienePlanEmpresa { get; set; }
		public long? IdNegocio { get; set; }
		public string? NombreNegocio { get; set; }
		public required long Id { get; set; }
		public string? Nombre { get; set; }
		public string? Descripcion { get; set; }
		public long? IdTipoPeriodicidad { get; set; }
		public string? NombreTipoPeriodicidad { get; set; }
		public string? Multa { get; set; }
		public long? IdCategoriaNorma { get; set; }
		public string? NombreCategoriaNorma { get; set; }
		public long? IdCargo { get; set; }
		public string? NombreCargo { get; set; }
		public List<SalFiscalizadorNormaSuscrita>? Fiscalizadores { get; set; }
		public SalTemplateNormaObtenerPorIdConVencimiento? TemplateNorma { get; set; }
		public required DateTime FechaVencimiento { get; set; }
		public DateTime? FechaCompletitud { get; set; }
		public List<SalDocumentoAdjunto>? DocumentosAdjuntos { get; set; }
	}

    [ExcludeFromCodeCoverage]
    public class SalTemplateNormaObtenerPorIdConVencimiento {
		public required long IdTemplate { get; set; }
		public required string NombreTemplate { get; set; }
		public required string Nombre { get; set; }
		public string? Descripcion { get; set; }
		public long? IdTipoPeriodicidad { get; set; }
		public string? NombreTipoPeriodicidad { get; set; }
		public string? Multa { get; set; }
		public long? IdCategoriaNorma { get; set; }
		public string? NombreCategoriaNorma { get; set; }
		public List<SalFiscalizadorNormaSuscrita>? Fiscalizadores { get; set; }
	}
}
