using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others {
    [ExcludeFromCodeCoverage]
    public class EntEmpleadoCrear {
		public required long IdNegocio { get; set; }
		public required string Nombre { get; set; }
		public required long IdCargo { get; set; }
		public List<EntEmpleadoCrearDestinatario> Destinatarios { get; set; } = [];
	}

    [ExcludeFromCodeCoverage]
    public class EntEmpleadoCrearDestinatario {
		public required long IdTipoReceptor { get; set; }
		public required string Destino { get; set; }
	}
}
